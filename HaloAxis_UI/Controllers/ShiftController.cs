// Controllers/ShiftController.cs
using HaloAxis_UI.Models;
using HaloAxis_UI.Services;
using Microsoft.AspNetCore.Mvc;

public class ShiftController : Controller
{
    private readonly ApiClient _api;

    public ShiftController(ApiClient api) => _api = api;

    // Controllers/ShiftController.cs
    public async Task<IActionResult> Index(string? q = null)
    {
        var comId = HttpContext.Session.GetString("CompanyId");
        if (string.IsNullOrWhiteSpace(comId))
            return BadRequest("CompanyId not found in session.");

        var path = $"/api/v1/Shift/GetAll?comid={Uri.EscapeDataString(comId)}";

        // ✅ use the list-aware method
        var items = await _api.GetListAsync<ShiftDto>(path);

        if (!string.IsNullOrWhiteSpace(q))
            items = items.Where(x =>
                (x.ShiftName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (x.ShiftCode ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        return View(items);
    }


    private static string? HhmmToUtcIso(string? hhmm, DateTime? baseUtcDate = null)
    {
        if (string.IsNullOrWhiteSpace(hhmm)) return null;
        if (!TimeSpan.TryParse(hhmm, out var t)) return null;
        var d = (baseUtcDate ?? DateTime.UtcNow.Date);
        var dt = new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, 0, DateTimeKind.Utc);
        return dt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    }

    private static string? HhmmToDurationIso(string? hhmm)
    {
        if (string.IsNullOrWhiteSpace(hhmm)) return null;
        if (!TimeSpan.TryParse(hhmm, out var t)) return null;
        var dt = new DateTime(1, 1, 1, t.Hours, t.Minutes, 0, DateTimeKind.Utc);
        return dt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] ShiftDto dto)
    {
        var comId = HttpContext.Session.GetString("CompanyId");
        if (string.IsNullOrWhiteSpace(comId))
            return BadRequest("CompanyId missing in session.");

        // Construct the JSON exactly like the successful Swagger example
        var payload = new
        {
            comId = comId,
            shiftName = dto.ShiftName,
            shiftCode = dto.ShiftCode,
            shiftDesc = dto.ShiftDesc,

            shiftin = HhmmToUtcIso(dto.ShiftIn),
            shiftout = HhmmToUtcIso(dto.ShiftOut),
            shiftlate = HhmmToDurationIso(dto.ShiftLate),

            lunchtime = HhmmToDurationIso(dto.LunchTime),
            lunchin = HhmmToUtcIso(dto.LunchIn),
            lunchout = HhmmToUtcIso(dto.LunchOut),

            tiffintime = HhmmToDurationIso(dto.TiffinTime),
            tiffinin = HhmmToUtcIso(dto.TiffinIn),
            tiffinout = HhmmToUtcIso(dto.TiffinOut),

            reghour = HhmmToDurationIso(dto.RegularHour),

            shifttype = dto.ShiftType,      // "General" | "Shifting"
            shiftcat = dto.ShiftCat,       // optional
            isinactive = dto.IsInactive
        };

        // DEBUG (optional): log the exact JSON you send
        // var json = System.Text.Json.JsonSerializer.Serialize(payload);
        // Console.WriteLine(json);

        var res = await _api.PostAsync("/api/v1/Shift/CreateShift", payload);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            // Show the API validation details on the page instead of a bare 400
            ModelState.AddModelError(string.Empty, $"API 400: {body}");
            // Re-render the list page with error at the top
            var list = await _api.GetListAsync<ShiftDto>($"/api/v1/Shift/GetAll?comid={Uri.EscapeDataString(comId)}");
            return View("Index", list);
        }

        TempData["ok"] = "Shift created.";
        return RedirectToAction(nameof(Index));
    }


    // POST: /Shift/Update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] ShiftDto dto)
    {
        var res = await _api.PutAsync("/api/v1/Shift/Update", dto);
        if (!res.IsSuccessStatusCode)
            return BadRequest(await res.Content.ReadAsStringAsync());

        TempData["ok"] = "Shift updated.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Shift/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        //var res = await _api.DeleteAsync($"/api/v1/Shift/{Uri.EscapeDataString(id)}");
        //if (!res.IsSuccessStatusCode)
        //    return BadRequest(await res.Content.ReadAsStringAsync());

        TempData["ok"] = "Shift deleted.";
        return RedirectToAction(nameof(Index));
    }
}
