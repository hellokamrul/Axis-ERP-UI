using System.Security.Claims;
using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaloAxis_UI.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ICompanyApi _companyApi;
        private readonly IUserCompanyApi _userCompanyApi;

        public CompanyController(ICompanyApi companyApi, IUserCompanyApi userCompanyApi)
        {
            _companyApi = companyApi;
            _userCompanyApi = userCompanyApi;
        }


        [HttpGet]
        public IActionResult Index() => RedirectToAction(nameof(Select));

        // ========= SELECT (list user companies) =========
        // GET /Company/Select
        [HttpGet]
        public async Task<IActionResult> Select(CancellationToken ct)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var companies = await _userCompanyApi.GetByUserAsync(userId, token, ct);
            if (companies.Count == 0)
                return RedirectToAction(nameof(Create)); // no company yet → create page

            return View(new CompanySelectionViewModel { Companies = companies });
        }

        // POST /Company/Select
        [ValidateAntiForgeryToken, HttpPost]
        public IActionResult Select(CompanySelectionViewModel vm)
        {
            var companyId = vm.CompanyId ?? Request.Form["CompanyId"];
            if (string.IsNullOrWhiteSpace(companyId))
            {
                ModelState.AddModelError("", "Please select a company.");
                return View(vm);
            }

            HttpContext.Session.SetString("CompanyId", companyId);
            return RedirectToAction("Index", "Dashboard");
        }

        // ========= CREATE (make first company) =========
        // GET /Company/Create
        [HttpGet]
        public IActionResult Create() => View(new CompanyCreateViewModel());

        // POST /Company/Create
        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Create(CompanyCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var token = HttpContext.Session.GetString("AccessToken") ?? "";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var comId = Guid.NewGuid().ToString("N"); // ERP expects "comid"

            var req = new CompanyCreateRequest
            {
                ComId = comId,
                Name = vm.Name,
                CountryId = vm.CountryId,
                UserId = userId,
                Address = vm.Address,
                City = vm.City,
                State = vm.State,
                Division = vm.Division,
                District = vm.District,
                Thana = vm.Thana,
                Upazila = vm.Upazila,
                Union = vm.Union,
                ZipOrPostalCode = vm.ZipOrPostalCode,
                ContactPerson = vm.ContactPerson,
                Phone = vm.Phone,
                Email = vm.Email,
                EIN = vm.EIN,
                UsCompanyType = vm.UsCompanyType,
                BIN = vm.BIN,
                TradeLicenseNumber = vm.TradeLicenseNumber,
                BdCompanyType = vm.BdCompanyType,
                Currency = vm.Currency,
                Locale = vm.Locale
            };

            CompanyCreatedDto? created;
            try
            {
                created = await _companyApi.CreateAsync(req, token, ct);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError("", "Couldn’t create company in ERP.");
                return View(vm);
            }

            // If ERP doesn’t auto-link creator, attach explicitly
            try
            {
                await _userCompanyApi.AttachAsync(userId, comId, vm.Name, token, ct);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                // optional: show a warning instead of failing creation
            }

            HttpContext.Session.SetString("CompanyId", created?.ComId ?? comId);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
