using HaloAxis_UI.Models;
using HaloAxis_UI.Services;
using HaloAxis_UI.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class DesignationController : Controller
{
    private readonly IDesignationApi _desigApi;
    private readonly IDepartmentApi _deptApi;

    public DesignationController(IDesignationApi desigApi, IDepartmentApi deptApi)
    {
        _desigApi = desigApi;
        _deptApi = deptApi;
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new DesignationEditVm();
        vm.ComId = HttpContext.Session.GetString("CompanyId") ?? "";

        // Populate departments for the dropdown
        var token = HttpContext.Session.GetString("AccessToken") ?? "";
        var depts = await _deptApi.ListByCompanyAsync(vm.ComId, token, ct);
        vm.DepartmentOptions = depts.Select(d => new SelectListItem { Value = d.Id, Text = d.DeptName }).ToList();

        return View(vm); // looks for Views/Designation/Create.cshtml
    }

    [ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> Create(DesignationEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            // rebind dropdowns
            var token = HttpContext.Session.GetString("AccessToken") ?? "";
            var depts = await _deptApi.ListByCompanyAsync(vm.ComId, token, ct);
            vm.DepartmentOptions = depts.Select(d => new SelectListItem { Value = d.Id, Text = d.DeptName }).ToList();
            return View(vm);
        }

        var token2 = HttpContext.Session.GetString("AccessToken") ?? "";

        // Map to your API DTO
        var dto = new DesignationDto
        {
            ComId = vm.ComId,
            DeptId = vm.DeptId!,
            DesigName = vm.DesigName,
            DesigLocalName = vm.DesigLocalName,
            SalaryRange = vm.SalaryRange,
            Slno = vm.Slno,
            Gsmin = vm.Gsmin,
            Attbonus = vm.Attbonus,
            Holidaybonus = vm.Holidaybonus,
            Nightallow = vm.Nightallow,
            Ttlmanpower = vm.Ttlmanpower,
            Proposedmanpower = vm.Proposedmanpower
        };

        await _desigApi.CreateAsync(dto, token2, ct);
        TempData["Flash"] = "Designation created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("AccessToken") ?? "";
        var comId = HttpContext.Session.GetString("CompanyId") ?? "";

        // Pull lists
        var designations = await _desigApi.ListByCompanyAsync(comId, token, ct);
        var departments = await _deptApi.ListByCompanyAsync(comId, token, ct);

        // Build lookup: DeptId -> DeptName
        var deptNameById = departments
            .GroupBy(d => d.Id ?? d.Id) // whichever your DTO uses
            .ToDictionary(g => g.Key ?? "", g => g.First().DeptName ?? g.First().DeptLocalName ?? g.First().DeptCode ?? "");

        // Project to VM
        var vm = new DesignationIndexViewModel
        {
            Items = designations.Select(d => new DesignationIndexViewModel.Row
            {
                Id = d.Id ?? "",
                Name = d.DesigName ?? d.DesigLocalName ?? "",
                DeptId = d.DeptId ?? "",
                DeptName = (d.DeptId != null && deptNameById.TryGetValue(d.DeptId, out var name))
                           ? name
                           : d.DeptId ?? "—"
            }).ToList()
        };

        return View(vm);
    }
}
