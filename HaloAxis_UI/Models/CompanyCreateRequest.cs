using System.Text.Json.Serialization;

namespace HaloAxis_UI.Models
{
    public class CompanyCreateRequest
    {
        [JsonPropertyName("comid")] public string ComId { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("countryId")] public string? CountryId { get; set; }
        [JsonPropertyName("userId")] public string UserId { get; set; } = "";
        [JsonPropertyName("address")] public string? Address { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("division")] public string? Division { get; set; }
        [JsonPropertyName("district")] public string? District { get; set; }
        [JsonPropertyName("thana")] public string? Thana { get; set; }
        [JsonPropertyName("upazila")] public string? Upazila { get; set; }
        [JsonPropertyName("union")] public string? Union { get; set; }
        [JsonPropertyName("zipOrPostalCode")] public string? ZipOrPostalCode { get; set; }
        [JsonPropertyName("contactPerson")] public string? ContactPerson { get; set; }
        [JsonPropertyName("phone")] public string? Phone { get; set; }
        [JsonPropertyName("email")] public string? Email { get; set; }
        [JsonPropertyName("ein")] public string? EIN { get; set; }
        [JsonPropertyName("usCompanyType")] public string? UsCompanyType { get; set; }
        [JsonPropertyName("bin")] public string? BIN { get; set; }
        [JsonPropertyName("tradeLicenseNumber")] public string? TradeLicenseNumber { get; set; }
        [JsonPropertyName("bdCompanyType")] public string? BdCompanyType { get; set; }
        [JsonPropertyName("currency")] public string? Currency { get; set; }
        [JsonPropertyName("locale")] public string? Locale { get; set; }
    }

    // if ERP returns the created company, map what you need
    public class CompanyCreatedDto
    {
        [JsonPropertyName("comid")] public string ComId { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }
}
