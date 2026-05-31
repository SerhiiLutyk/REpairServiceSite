using GadgetFix.Orders.BLL;
using GadgetFix.Orders.DAL;
using GadgetFix.Orders.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GadgetFix.Tests;

public class OrderServiceTests
{
    private static OrdersDbContext NewDb() =>
        new(new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class FakeNotifier : IOrderNotifier
    {
        public int Calls { get; private set; }
        public Task NotifyReadyAsync(OrderDto order, CancellationToken ct = default)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Create_PersistsOrder_WithNewStatus()
    {
        var db = NewDb();
        var notifier = new FakeNotifier();
        var service = new OrderService(db, notifier);

        var dto = await service.CreateAsync(new CreateOrderRequest(
            "Тест Клієнт", "+380501112233", 1, 1, "розбитий екран", 1800m));

        Assert.Equal(OrderStatus.New, dto.Status);
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task SettingStatusReady_TriggersNotificationOnce()
    {
        var db = NewDb();
        var notifier = new FakeNotifier();
        var service = new OrderService(db, notifier);

        var dto = await service.CreateAsync(new CreateOrderRequest(
            "Тест", "+380501112233", 1, null, "не вмикається", null));

        await service.UpdateStatusAsync(dto.Id, OrderStatus.Ready);
        await service.UpdateStatusAsync(dto.Id, OrderStatus.Ready); // повторно — без дубля

        Assert.Equal(1, notifier.Calls);
    }
}
