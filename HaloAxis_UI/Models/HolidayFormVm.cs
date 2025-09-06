namespace HaloAxis_UI.Models
{
    // View model for Create/Edit
    public class HolidayFormVm
    {
        public string? Id { get; set; }
        public string? ComId { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }

        // yyyy-MM-dd
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }

        public List<string> WeeklyOffDays { get; set; } = new();

        public string? Country { get; set; }
        public string? State { get; set; }
        public bool IsActive { get; set; } = true;

        public List<HolidayLineVm> Lines { get; set; } = new();
    }

    public class HolidayLineVm
    {
        public string? Id { get; set; }          // 👈 NEW: keep existing child id on edit
        public string? HolidayId { get; set; }
        public string? Date { get; set; }     // yyyy-MM-dd
        public int Type { get; set; } = 1;    // 0=Weekend, 1=Local
        public string? Description { get; set; }
    }
}
