namespace GadgetFix.Orders.DAL.Entities;

public enum OrderStatus
{
    New = 0,         // Нова заявка
    Diagnostics = 1, // Діагностика
    InRepair = 2,    // В ремонті
    Ready = 3,       // Готово
    Done = 4,        // Видано клієнту
    Cancelled = 5,   // Скасовано
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int DeviceTypeId { get; set; }
    public int? ServiceId { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public decimal? EstimatedPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderStatusHistory> History { get; set; } = [];
}
