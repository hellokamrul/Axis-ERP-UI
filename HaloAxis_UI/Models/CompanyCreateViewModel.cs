namespace HaloAxis_UI.Models
{
    public class CompanyCreateViewModel
    {
        // minimal required on the form
        public string Name { get; set; } = "";
        public string? CountryId { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipOrPostalCode { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Currency { get; set; }   // e.g., "USD"
        public string? Locale { get; set; }     // e.g., "en-US"

        // regional (optional)
        public string? Division { get; set; }
        public string? District { get; set; }
        public string? Thana { get; set; }
        public string? Upazila { get; set; }
        public string? Union { get; set; }
        public string? EIN { get; set; }
        public string? UsCompanyType { get; set; }
        public string? BIN { get; set; }
        public string? TradeLicenseNumber { get; set; }
        public string? BdCompanyType { get; set; }
    }
}
