namespace HaloAxis_UI.Models;

public record CompanyLite(string CompanyId, string CompanyName);

public class UserCompanyDto
{
    public string UserId { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public CompanyDto? Company { get; set; }
}
public class CompanyDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}
