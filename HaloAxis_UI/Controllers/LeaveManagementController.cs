using HaloAxis_UI.Models.LeaveManagementVM;
using HaloAxis_UI.Services; // ApiClient
using Microsoft.AspNetCore.Mvc;
using static HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM;
using System.Text.Json;

namespace HaloAxis_UI.Controllers
{
    public class LeaveManagementController : Controller
    {
        private readonly ApiClient _api;

        public LeaveManagementController(ApiClient api)
        {
            _api = api;
        }



    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 8, bool onlyActive = false)
        {
            var comId = HttpContext.Session.GetString("CompanyId") ?? string.Empty;

            var url = $"/api/v1/LeaveComponent/GetLeaveComponentList" +
                      $"?comid={Uri.EscapeDataString(comId)}" +
                      $"&pageIndex={page}&pageSize={pageSize}&onlyActive={onlyActive}";

            var doc = await _api.GetAsync<JsonDocument>(url);
            var root = doc.RootElement;

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // ---- items ----
            JsonElement itemsEl;
            List<LeaveComponentVM.LeaveComponentDTO> list = new();

            if (root.TryGetProperty("items", out itemsEl) || root.TryGetProperty("Items", out itemsEl))
            {
                if (itemsEl.ValueKind == JsonValueKind.Array)
                {
                    list = JsonSerializer.Deserialize<List<LeaveComponentVM.LeaveComponentDTO>>(itemsEl.GetRawText(), opts) ?? new();
                }
                else if (itemsEl.ValueKind == JsonValueKind.Object &&
                         itemsEl.TryGetProperty("$values", out var valuesEl) &&
                         valuesEl.ValueKind == JsonValueKind.Array)
                {
                    // Newtonsoft-style wrapper { "$id": "...", "$values": [ ... ] }
                    list = JsonSerializer.Deserialize<List<LeaveComponentVM.LeaveComponentDTO>>(valuesEl.GetRawText(), opts) ?? new();
                }
            }

            // ---- totalCount ----
            int total = 0;
            if (root.TryGetProperty("totalCount", out var t1) && t1.ValueKind == JsonValueKind.Number)
                total = t1.GetInt32();
            else if (root.TryGetProperty("TotalCount", out var t2) && t2.ValueKind == JsonValueKind.Number)
                total = t2.GetInt32();

            ViewBag.TotalCount = total;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            ViewBag.OnlyActive = onlyActive;

            return View(list);
        }


    // GET: /LeaveManagement/Create
    [HttpGet]
        public IActionResult Create()
        {
            var vm = new HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO
            {
                IsActive = true,
                UnitType = HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveUnitType.Daily
            };
            return View(vm);
        }

        // GET: /LeaveManagement/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var dto = await _api.GetAsync<HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO>(
                $"/api/v1/LeaveComponent/{Uri.EscapeDataString(id)}");
            if (dto is null) return NotFound();
            return View(dto);
        }

        // POST: /LeaveManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            dto.ComId = HttpContext.Session.GetString("CompanyId");

            var created = await _api.PostJsonAsync<
                HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO,
                HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO>(
                "/api/v1/LeaveComponent/CreateLeaveComponent", dto);

            if (created is null) { ModelState.AddModelError("", "Create failed."); return View(dto); }

            TempData["ok"] = "Leave type created.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /LeaveManagement/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);
            dto.ComId = HttpContext.Session.GetString("CompanyId");

            var updated = await _api.PutJsonAsync<
                HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO,
                HaloAxis_UI.Models.LeaveManagementVM.LeaveComponentVM.LeaveComponentDTO>(
                $"/api/v1/LeaveComponent/{Uri.EscapeDataString(id)}", dto);

            if (updated is null) { ModelState.AddModelError("", "Update failed."); return View(dto); }

            TempData["ok"] = "Leave type updated.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            try
            {
                await _api.DeleteAsync($"/api/v1/LeaveComponent/{Uri.EscapeDataString(id)}");
                TempData["ok"] = "Leave type deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // your ApiClient should throw on non-success; show a friendly message
                return Problem($"Delete failed: {ex.Message}");
            }
        }

        public class PagedResult<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
        }


    }
}
