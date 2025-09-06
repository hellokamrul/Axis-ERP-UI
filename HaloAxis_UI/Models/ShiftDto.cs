using System.Text.Json.Serialization;

namespace HaloAxis_UI.Models
{
    // Models/ShiftDto.cs  
    public class ShiftDto
    {
        public string? Id { get; set; }
        public string? ComId { get; set; }
        public string? ShiftName { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftDesc { get; set; }

        // form posts HH:mm
        public string? ShiftIn { get; set; }
        public string? ShiftOut { get; set; }
        public string? ShiftLate { get; set; }

        public string? LunchTime { get; set; }
        public string? LunchIn { get; set; }
        public string? LunchOut { get; set; }

        public string? TiffinTime { get; set; }
        public string? TiffinIn { get; set; }
        public string? TiffinOut { get; set; }

        public string? RegularHour { get; set; }

        public string? ShiftType { get; set; }
        public string? ShiftCat { get; set; }
        public bool IsInactive { get; set; }
    }


}
