using HaloAxis_UI.Models;
using HaloAxis_UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HaloAxis_UI.Controllers
{
    public class HolidayController : Controller
    {
        private readonly ApiClient _api;
        public HolidayController(ApiClient api) => _api = api;

        // GET: /Holiday
        public async Task<IActionResult> Index(int page = 1, int size = 8, string? search = null, string? country = null, bool? onlyActive = null)
        {
            var comId = HttpContext.Session.GetString("CompanyId");
            var qs = new List<string>
            {
                $"PageNumber={Math.Max(page,1)}",
                $"PageSize={Math.Clamp(size,1,100)}"
            };
            if (!string.IsNullOrWhiteSpace(comId)) qs.Add($"comid={Uri.EscapeDataString(comId)}"); // <-- match API name
            if (!string.IsNullOrWhiteSpace(search)) qs.Add($"Search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(country)) qs.Add($"Country={Uri.EscapeDataString(country)}");
            if (onlyActive.HasValue) qs.Add($"OnlyActive={(onlyActive.Value ? "true" : "false")}");

            var path = $"/api/v1/Holiday/paged?{string.Join("&", qs)}";
            var data = await _api.GetAsync<HolidayPagedResponse>(path) ?? new HolidayPagedResponse();

            ViewBag.Page = data.PageNumber;
            ViewBag.Size = data.PageSize;
            ViewBag.TotalPages = data.TotalPages;
            ViewBag.TotalCount = data.TotalCount;
            ViewBag.Search = search ?? "";
            ViewBag.Country = country ?? "";
            ViewBag.OnlyActive = onlyActive;

            return View(data.Items);
        }

        // GET: /Holiday/Create
        public IActionResult Create() => View("Edit", new HolidayFormVm());

        // POST: /Holiday/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HolidayFormVm vm)
        {
            var comId = HttpContext.Session.GetString("CompanyId");
            vm.ComId = comId;
            if (!ModelState.IsValid) return View("Edit", vm);

            var payload = BuildPayload(vm);
            var ok = await _api.PostJsonAsync<object, bool>("/api/v1/Holiday", payload);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "API returned false on create.");
                return View("Edit", vm);
            }

            TempData["ok"] = "Holiday list created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Holiday/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var apiItem = await _api.GetAsync<HolidayApiItem>($"/api/v1/Holiday/{Uri.EscapeDataString(id)}");
            if (apiItem is null) return NotFound();

            var vm = MapToForm(apiItem);
            return View(vm);
        }

        // POST: /Holiday/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, HolidayFormVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            vm.ComId = HttpContext.Session.GetString("CompanyId");
            vm.Id = id;
            var payload = BuildPayload(vm);
            var ok = await _api.PutJsonAsync<object, bool>($"/api/v1/Holiday/{Uri.EscapeDataString(id)}", payload);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "API returned false on update.");
                return View(vm);
            }

            TempData["ok"] = "Holiday list updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Holiday/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                // If your API exposes DELETE, you can call it directly:
                // await _api.DeleteAsync($"/api/v1/Holiday/{Uri.EscapeDataString(id)}");

                // fallback: deactivate
                await _api.PutJsonAsync<object, object>($"/api/v1/Holiday/{Uri.EscapeDataString(id)}", new { isActive = false });
                TempData["ok"] = "Holiday deleted.";
            }
            catch (HttpRequestException ex)
            {
                TempData["err"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("RemoveLine")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLine(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("id required");

            try
            {
                await _api.DeleteAsync($"/api/v1/Holiday/entries/{Uri.EscapeDataString(id)}");
                return Ok(new { ok = true });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Delete failed: {ex.Message}");
            }
        }


        // ---------------- helpers ----------------

        private static HolidayFormVm MapToForm(HolidayApiItem a)
        {
            var vm = new HolidayFormVm
            {
                Id = a.Id,
                Name = a.Name,
                Color = a.Color,
                FromDate = a.FromDate?.ToString("yyyy-MM-dd"),
                ToDate = a.ToDate?.ToString("yyyy-MM-dd"),
                Country = a.Country,
                State = a.State,
                IsActive = a.IsActive ?? true,
                Lines = a.HolidayLists.Select(l => new HolidayLineVm
                {
                    Id = l.Id,
                    HolidayId = l.HolidayId,
                    Date = l.Date,
                    Type = l.Type ?? 0,
                    Description = l.Description
                }).ToList()
            };

            vm.WeeklyOffDays = new[] { a.Weekday1, a.Weekday2, a.Weekday3, a.Weekday4, a.Weekday5, a.Weekday6, a.Weekday7 }
                               .Where(s => !string.IsNullOrWhiteSpace(s))
                               .ToList();
            return vm;
        }

        private static object BuildPayload(HolidayFormVm vm)
        {
            // Parent id from the form VM; for Create this will be null
            var parentId = string.IsNullOrWhiteSpace(vm.Id) ? null : vm.Id;

            var weekdays = vm.WeeklyOffDays?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new();
            string? wd(int i) => i < weekdays.Count ? weekdays[i] : null;

            var lines = (vm.Lines ?? new())
                .Where(l => !string.IsNullOrWhiteSpace(l.Date))
                .Select((l, idx) => new
                {
                    id = string.IsNullOrWhiteSpace(l.Id) ? null : l.Id,
                    serial = idx + 1,
                    // if the row doesn't have a holidayId, fall back to the parentId
                    holidayId = string.IsNullOrWhiteSpace(l.HolidayId) ? parentId : l.HolidayId,
                    date = NormalizeDate(l.Date!),   // ensure "yyyy-MM-dd"
                    type = l.Type,
                    description = l.Description
                })
                .ToList();

            return new
            {
                id = parentId,
                comId = vm.ComId,                    // include if your API expects it
                name = vm.Name,
                color = vm.Color,
                fromDate = ToUtcIsoStart(vm.FromDate),  // "yyyy-MM-ddT00:00:00Z"
                toDate = ToUtcIsoStart(vm.ToDate),

                weekday1 = wd(0),
                weekday2 = wd(1),
                weekday3 = wd(2),
                weekday4 = wd(3),
                weekday5 = wd(4),
                weekday6 = wd(5),
                weekday7 = wd(6),

                country = vm.Country,
                state = vm.State,
                holidayLists = lines,
                isActive = vm.IsActive
            };
        }


        private static string? ToUtcIsoStart(string? ymd)
        {
            if (string.IsNullOrWhiteSpace(ymd)) return null;
            if (!DateTime.TryParseExact(ymd, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return null;
            var utc = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
            return utc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }

        private static string NormalizeDate(string ymd)
        {
            if (DateTime.TryParse(ymd, out var d)) return d.ToString("yyyy-MM-dd");
            return ymd;
        }
    }
}
