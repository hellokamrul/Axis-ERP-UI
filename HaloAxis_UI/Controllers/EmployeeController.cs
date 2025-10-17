using System.Text;
using System.Text.Json;
using HaloAxis_UI.Services;
using Microsoft.AspNetCore.Mvc;
using static HaloAxis_UI.Models.Employee;

namespace HaloAxis_UI.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApiClient _api;

        public EmployeeController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            var comId = HttpContext.Session.GetString("CompanyId") ?? string.Empty;

            var url = $"/api/v1/Employee/GetEmp?comid={Uri.EscapeDataString(comId)}&pageIndex={page}&pageSize={pageSize}";
            var doc = await _api.GetAsync<JsonDocument>(url);
            var root = doc.RootElement;

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // --- employees (supports "employees" or "Employees" and $values wrapper) ---
            List<EmployeeListItem> list = new();

            if (TryGetPropertyInsensitive(root, "employees", out var employeesEl))
            {
                if (employeesEl.ValueKind == JsonValueKind.Array)
                {
                    list = ParseEmployees(employeesEl, opts);
                }
                else if (employeesEl.ValueKind == JsonValueKind.Object &&
                         TryGetPropertyInsensitive(employeesEl, "$values", out var valuesEl) &&
                         valuesEl.ValueKind == JsonValueKind.Array)
                {
                    list = ParseEmployees(valuesEl, opts);
                }
            }

            // --- totals (supports "total"/"Total" and "totalDisplay"/"TotalDisplay") ---
            int total = 0;
            if (TryGetPropertyInsensitive(root, "total", out var t) && t.ValueKind == JsonValueKind.Number)
                total = t.GetInt32();
            if (total == 0 && TryGetPropertyInsensitive(root, "totalDisplay", out var td) && td.ValueKind == JsonValueKind.Number)
                total = td.GetInt32();
            if (total == 0) total = list.Count;

            ViewBag.TotalCount = total;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q ?? string.Empty;

            return View(list);
        }

        // -------- helpers --------
        private static bool TryGetPropertyInsensitive(JsonElement obj, string name, out JsonElement value)
        {
            // direct (exact) first
            if (obj.TryGetProperty(name, out value)) return true;

            // case-insensitive scan
            foreach (var prop in obj.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        private static List<EmployeeListItem> ParseEmployees(JsonElement arrayEl, JsonSerializerOptions opts)
        {
            var items = new List<EmployeeListItem>();

            foreach (var el in arrayEl.EnumerateArray())
            {
                // The API returns a rich object (your Core/Hr.Employee). We map to a flat list item.
                string id = el.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? "" : "";
                string first = el.TryGetProperty("firstName", out var fn) ? fn.GetString() ?? "" : "";
                string last = el.TryGetProperty("lastName", out var ln) ? ln.GetString() ?? "" : "";

                string? designation = TryString(el, "designation") ?? TryString(el, "Designation") ?? TryNestedString(el, "jobInformation", "designation");
                string? department = TryString(el, "department") ?? TryString(el, "Department") ?? TryNestedString(el, "jobInformation", "department");
                string? empType = TryString(el, "employeeType") ?? TryNestedString(el, "jobInformation", "employeeType");
                DateTime? joinDate = TryDate(el, "joinDate") ?? TryNestedDate(el, "jobInformation", "joinDate");
                string? phone = TryString(el, "phoneNumber") ?? TryNestedString(el, "contactInfos", "mobile"); // adjust if array

                // Status: if not provided, assume ACTIVE
                string status = TryString(el, "status") ?? "ACTIVE";

                items.Add(new EmployeeListItem
                {
                    Id = id,
                    EmployeeName = $"{first} {last}".Trim(),
                    Designation = designation,
                    Department = department,
                    EmployeeType = empType,
                    JoinDate = joinDate,
                    MobileNumber = phone,
                    Status = status
                });
            }

            return items;
        }

        private static string? TryString(JsonElement el, string name)
            => el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

        private static string? TryNestedString(JsonElement el, string obj, string prop)
            => el.TryGetProperty(obj, out var o) && o.ValueKind == JsonValueKind.Object && o.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static DateTime? TryDate(JsonElement el, string name)
            => el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String && DateTime.TryParse(p.GetString(), out var d) ? d : (DateTime?)null;

        private static DateTime? TryNestedDate(JsonElement el, string obj, string prop)
            => el.TryGetProperty(obj, out var o) && o.ValueKind == JsonValueKind.Object && o.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String && DateTime.TryParse(v.GetString(), out var d) ? d : (DateTime?)null;

        // GET: /Employee/Create
        [HttpGet]
        public IActionResult Create()
        {
            var dto = new EmployeeCreateDto
            {
                EmployeeType = "FULL_TIME",
                JoinDate = DateTime.Today
            };
            return View(dto);
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            dto.ComId = HttpContext.Session.GetString("CompanyId");

            var created = await _api.PostJsonAsync<EmployeeCreateDto, object>("/api/v1/Employee/CreateEmployee", dto);
            if (created is null)
            {
                ModelState.AddModelError("", "Create failed.");
                return View(dto);
            }

            TempData["ok"] = "Employee created.";
            return RedirectToAction(nameof(Index));
        }

        // Simple CSV export (client uses a GET link)
        [HttpGet]
        public async Task<IActionResult> ExportCsv(int page = 1, int pageSize = 500)
        {
            var comId = HttpContext.Session.GetString("CompanyId") ?? string.Empty;
            var url = $"/api/v1/Employee/GetEmp?comid={Uri.EscapeDataString(comId)}&pageIndex={page}&pageSize={pageSize}";
            var doc = await _api.GetAsync<JsonDocument>(url);
            var root = doc.RootElement;

            var items = new List<EmployeeListItem>();
            if (root.TryGetProperty("Employees", out var employeesEl))
            {
                if (employeesEl.ValueKind == JsonValueKind.Array)
                    items = ParseEmployees(employeesEl, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                else if (employeesEl.ValueKind == JsonValueKind.Object && employeesEl.TryGetProperty("$values", out var valuesEl))
                    items = ParseEmployees(valuesEl, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            var sb = new StringBuilder();
            sb.AppendLine("Employee Name,Designation,Department,Employee Type,Join Date,Mobile Number,Status");
            foreach (var e in items)
            {
                var date = e.JoinDate?.ToString("yyyy-MM-dd") ?? "";
                sb.AppendLine($"\"{e.EmployeeName}\",\"{e.Designation}\",\"{e.Department}\",\"{e.EmployeeType}\",\"{date}\",\"{e.MobileNumber}\",\"{e.Status}\"");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "employees.csv");
        }
    }
}
