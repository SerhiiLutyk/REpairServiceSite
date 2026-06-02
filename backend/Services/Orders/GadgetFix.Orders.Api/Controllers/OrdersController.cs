using System.Security.Claims;
using GadgetFix.Orders.BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    /// <summary>Усі замовлення (адмін).</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IReadOnlyList<OrderDto>> GetAll(CancellationToken ct) =>
        await orders.GetAllAsync(ct);

    /// <summary>Замовлення поточного користувача.</summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> My(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await orders.GetByUserAsync(userId, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Створення заявки. Якщо користувач авторизований — прив'язується до акаунта.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        Guid? userId = TryGetUserId(out var id) ? id : null;
        var order = await orders.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateStatusRequest request, CancellationToken ct)
    {
        var order = await orders.UpdateStatusAsync(id, request.Status, ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Клієнт скасовує власне замовлення (поки воно не в роботі).</summary>
    [Authorize]
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        try
        {
            var order = await orders.CancelByUserAsync(id, userId, ct);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>Внутрішній ендпоінт для Telegram-бота (не проксується через gateway).</summary>
    [HttpGet("/internal/orders/by-user/{userId:guid}")]
    public async Task<IReadOnlyList<OrderDto>> ByUser(Guid userId, CancellationToken ct) =>
        await orders.GetByUserAsync(userId, ct);

    private bool TryGetUserId(out Guid id)
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out id);
    }
}
