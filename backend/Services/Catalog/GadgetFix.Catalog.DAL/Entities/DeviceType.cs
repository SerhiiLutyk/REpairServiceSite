namespace GadgetFix.Catalog.DAL.Entities;

public class DeviceType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icon { get; set; }

    public List<RepairService> Services { get; set; } = [];
}
