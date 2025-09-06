using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaloAxis_UI.Models
{
    public class DesignationEditVm
    {
        public string? Id { get; set; }
        public string ComId { get; set; } = "";
        public string? DeptId { get; set; }
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

        public List<SelectListItem> DepartmentOptions { get; set; } = new();
    }
}
