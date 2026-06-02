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

public record StatusHistoryDto(OrderStatus Status, DateTime ChangedAt);

public record OrderDto(
    Guid Id,
    Guid? UserId,
    string CustomerName,
    string Phone,
    int DeviceTypeId,
    int? ServiceId,
    string ProblemDescription,
    decimal? EstimatedPrice,
    OrderStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<StatusHistoryDto> History)
{
    public static OrderDto From(Order o) => new(o.Id, o.UserId, o.CustomerName, o.Phone, o.DeviceTypeId,
        o.ServiceId, o.ProblemDescription, o.EstimatedPrice, o.Status, o.CreatedAt, o.UpdatedAt,
        o.History.OrderBy(h => h.ChangedAt).Select(h => new StatusHistoryDto(h.Status, h.ChangedAt)).ToList());
}

/// <summary>Клієнт сервісу нотифікацій (Telegram). Реалізація — в Api-шарі.</summary>
public interface IOrderNotifier
{
    Task NotifyReadyAsync(OrderDto order, CancellationToken ct = default);
}

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid? userId, CancellationToken ct = default);
    Task<OrderDto?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default);
}

public class OrderService(OrdersDbContext db, IOrderNotifier notifier) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default) =>
        (await db.Orders.AsNoTracking().Include(o => o.History)
            .OrderByDescending(o => o.CreatedAt).ToListAsync(ct))
            .Select(OrderDto.From).ToList();

    public async Task<IReadOnlyList<OrderDto>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        (await db.Orders.AsNoTracking().Include(o => o.History).Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt).ToListAsync(ct))
            .Select(OrderDto.From).ToList();

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.History).FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null ? null : OrderDto.From(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid? userId, CancellationToken ct = default)
    {
        var order = new Order
        {
            UserId = userId,
            CustomerName = request.CustomerName.Trim(),
            Phone = request.Phone.Trim(),
            DeviceTypeId = request.DeviceTypeId,
            ServiceId = request.ServiceId,
            ProblemDescription = request.ProblemDescription.Trim(),
            EstimatedPrice = request.EstimatedPrice,
        };
        order.History.Add(new OrderStatusHistory { Status = OrderStatus.New });
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return OrderDto.From(order);
    }

    public async Task<OrderDto?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken ct = default)
    {
        var order = await db.Orders.Include(o => o.History).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return null;

        var wasReady = order.Status == OrderStatus.Ready;
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        order.History.Add(new OrderStatusHistory { OrderId = order.Id, Status = status });
        await db.SaveChangesAsync(ct);

        var dto = OrderDto.From(order);
        // Пуш у Telegram, коли замовлення щойно стало "Готово"
        if (status == OrderStatus.Ready && !wasReady)
            await notifier.NotifyReadyAsync(dto, ct);

        return dto;
    }
}
