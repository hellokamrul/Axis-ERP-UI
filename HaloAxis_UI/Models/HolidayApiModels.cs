using System.Text.Json.Serialization;
using HaloAxis_UI.Models.Json;

namespace HaloAxis_UI.Models
{
    // Paged response from GET /api/v1/Holiday/paged
    public class HolidayPagedResponse
    {
        [JsonConverter(typeof(FlexibleListConverter<HolidayApiItem>))]
        [JsonPropertyName("items")]
        public List<HolidayApiItem> Items { get; set; } = new();

        [JsonPropertyName("pageNumber")] public int PageNumber { get; set; }
        [JsonPropertyName("pageSize")] public int PageSize { get; set; }
        [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
        [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
    }

    // An item inside the paged response and for GET /api/v1/Holiday/{id}
    public class HolidayApiItem
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("color")] public string? Color { get; set; }
        [JsonPropertyName("fromDate")] public DateTime? FromDate { get; set; }
        [JsonPropertyName("toDate")] public DateTime? ToDate { get; set; }

        [JsonPropertyName("weekday1")] public string? Weekday1 { get; set; }
        [JsonPropertyName("weekday2")] public string? Weekday2 { get; set; }
        [JsonPropertyName("weekday3")] public string? Weekday3 { get; set; }
        [JsonPropertyName("weekday4")] public string? Weekday4 { get; set; }
        [JsonPropertyName("weekday5")] public string? Weekday5 { get; set; }
        [JsonPropertyName("weekday6")] public string? Weekday6 { get; set; }
        [JsonPropertyName("weekday7")] public string? Weekday7 { get; set; }

        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }

        [JsonConverter(typeof(FlexibleListConverter<HolidayApiLine>))]
        [JsonPropertyName("holidayLists")]
        public List<HolidayApiLine> HolidayLists { get; set; } = new();

        [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
    }

    public class HolidayApiLine
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("serial")] public int? Serial { get; set; }
        [JsonPropertyName("holidayId")] public string? HolidayId { get; set; }

        // API shows "YYYY-MM-DD" (string)
        [JsonPropertyName("date")] public string? Date { get; set; }
        // enum as int (0,1,...)
        [JsonPropertyName("type")] public int? Type { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}
