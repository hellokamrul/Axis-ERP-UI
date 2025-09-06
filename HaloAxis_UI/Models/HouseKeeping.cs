using System.Text.Json.Serialization;

namespace HaloAxis_UI.Models
{
    // ---- Department ----
    public class DepartmentDto
    {
        public string Id { get; set; } = "";           // guid
        public string ComId { get; set; } = "";        // company id
        public string DeptCode { get; set; } = "";
        public string DeptName { get; set; } = "";
        public string? DeptLocalName { get; set; }
        public int Slno { get; set; }
    }

    // ---- Designation ----
    public class DesignationDto
    {
        public string Id { get; set; } = "";         // guid
        public string ComId { get; set; } = "";      // company id
        public string DesigName { get; set; } = "";
        public string? DesigLocalName { get; set; }
        public string? SalaryRange { get; set; }
        public int Slno { get; set; }
        public decimal Gsmin { get; set; }
        public decimal Attbonus { get; set; }
        public decimal Holidaybonus { get; set; }
        public decimal Nightallow { get; set; }
        public int Ttlmanpower { get; set; }
        public int Proposedmanpower { get; set; }
        public string? DeptId { get; set; }
    }
}
