using System.Security.Claims;
using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaloAxis_UI.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentApi _api;
        public DepartmentController(IDepartmentApi api) => _api = api;

        private (string token, string comId) MustContext()
        {
            var token = HttpContext.Session.GetString("AccessToken") ?? "";
            var comId = HttpContext.Session.GetString("CompanyId") ?? "";
            if (string.IsNullOrEmpty(token)) throw new InvalidOperationException("No access token.");
            if (string.IsNullOrEmpty(comId)) throw new InvalidOperationException("No company selected.");
            return (token, comId);
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var (token, comId) = MustContext();
            var items = await _api.ListByCompanyAsync(comId, token, ct);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create() => View(new DepartmentDto());

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Create(DepartmentDto dto, CancellationToken ct)
        {
            var (token, comId) = MustContext();
            if (!ModelState.IsValid) return View(dto);
            dto.Id = Guid.NewGuid().ToString();
            dto.ComId = comId;
            await _api.CreateAsync(dto, token, ct);
            TempData["Flash"] = "Department created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken ct)
        {
            var (token, _) = MustContext();
            var item = await _api.GetAsync(id, token, ct);
            if (item is null) return NotFound();
            return View(item);
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Edit(DepartmentDto dto, CancellationToken ct)
        {
            var (token, comId) = MustContext();
            if (!ModelState.IsValid) return View(dto);
            dto.ComId = comId;
            await _api.UpdateAsync(dto, token, ct);
            TempData["Flash"] = "Department updated.";
            return RedirectToAction(nameof(Index));
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var (token, _) = MustContext();
            await _api.DeleteAsync(id, token, ct);
            TempData["Flash"] = "Department deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
