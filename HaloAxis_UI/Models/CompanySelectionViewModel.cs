using System.Collections.Generic;

namespace HaloAxis_UI.Models
{
    public class CompanySelectionViewModel
    {
        // For rendering the list
        public List<CompanyLite> Companies { get; set; } = new();

        // For binding the selected radio value on POST
        public string? CompanyId { get; set; }
    }
}
