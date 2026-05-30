using GadgetFix.Orders.DAL;
using GadgetFix.Orders.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Orders.BLL;

public record CreateOrderRequest(
    string CustomerName,
    string Phone,
    int DeviceTypeId,
    int? ServiceId,
    string ProblemDescription,
    decimal? EstimatedPrice);

public record UpdateStatusRequest(OrderStatus Status);

public record OrderDto(
    Guid Id,
    string CustomerName,
    string Phone,
    int DeviceTypeId,
    int? ServiceId,
    string ProblemDescription,
    decimal? EstimatedPrice,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static OrderDto From(Order o) => new(o.Id, o.CustomerName, o.Phone, o.DeviceTypeId,
        o.ServiceId, o.ProblemDescription, o.EstimatedPrice, o.Status, o.CreatedAt, o.UpdatedAt);
}

/// <summary>Клієнт сервісу нотифікацій (Telegram). Реалізація — в Api-шарі.</summary>
public interface IOrderNotifier
{
    Task NotifyReadyAsync(OrderDto order, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default);
}

public class OrderService(OrdersDbContext db, IOrderNotifier notifier) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default) =>
        await db.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt)
            .Select(o => OrderDto.From(o)).ToListAsync(ct);

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null ? null : OrderDto.From(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            Phone = request.Phone.Trim(),
            DeviceTypeId = request.DeviceTypeId,
            ServiceId = request.ServiceId,
            ProblemDescription = request.ProblemDescription.Trim(),
            EstimatedPrice = request.EstimatedPrice,
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return OrderDto.From(order);
    }

    public async Task<OrderDto?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return null;

        var wasReady = order.Status == OrderStatus.Ready;
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var dto = OrderDto.From(order);
        // Пуш у Telegram, коли замовлення щойно стало "Готово"
        if (status == OrderStatus.Ready && !wasReady)
            await notifier.NotifyReadyAsync(dto, ct);

        return dto;
    }
}
