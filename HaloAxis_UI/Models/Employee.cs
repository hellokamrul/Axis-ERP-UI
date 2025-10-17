namespace HaloAxis_UI.Models
{
    public class Employee
    {
        public class EmployeeListItem
        {
            public string Id { get; set; } = string.Empty;

            // Display fields for the list grid
            public string EmployeeName { get; set; } = string.Empty;         // FirstName + LastName
            public string? Designation { get; set; }
            public string? Department { get; set; }
            public string? EmployeeType { get; set; }                         // FULL_TIME/CONTRACT/etc
            public DateTime? JoinDate { get; set; }
            public string? MobileNumber { get; set; }
            public string Status { get; set; } = "ACTIVE";                    // ACTIVE/INACTIVE
        }

        // Minimal DTO for Create
        public class EmployeeCreateDto
        {
            public string? ComId { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;

            public string? Department { get; set; }
            public string? Designation { get; set; }
            public string? EmployeeType { get; set; } // FULL_TIME / PART_TIME / CONTRACT / SEASONAL
            public DateTime? JoinDate { get; set; }
        }
    }
}
