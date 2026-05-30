namespace GadgetFix.Catalog.DAL.Entities;

public class RepairService
{
    public int Id { get; set; }
    public int DeviceTypeId { get; set; }
    public DeviceType? DeviceType { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int EstimatedDays { get; set; }
}
