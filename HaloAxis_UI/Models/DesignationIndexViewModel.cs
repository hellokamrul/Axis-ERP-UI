// Models/DesignationIndexViewModel.cs
namespace HaloAxis_UI.Models;

public sealed class DesignationIndexViewModel
{
    public List<Row> Items { get; set; } = new();

    public sealed class Row
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DeptId { get; set; } = "";
        public string DeptName { get; set; } = "";
    }
}
