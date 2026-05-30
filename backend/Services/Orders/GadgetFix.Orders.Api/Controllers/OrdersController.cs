using GadgetFix.Orders.BLL;
using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Orders.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<OrderDto>> GetAll(CancellationToken ct) =>
        await orders.GetAllAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var order = await orders.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, UpdateStatusRequest request, CancellationToken ct)
    {
        var order = await orders.UpdateStatusAsync(id, request.Status, ct);
        return order is null ? NotFound() : Ok(order);
    }
}
