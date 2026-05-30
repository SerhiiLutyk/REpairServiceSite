using GadgetFix.Catalog.BLL;
using Microsoft.AspNetCore.Mvc;

namespace GadgetFix.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController(ICatalogService catalog) : ControllerBase
{
    [HttpGet("device-types")]
    public async Task<IReadOnlyList<DeviceTypeDto>> GetDeviceTypes(CancellationToken ct) =>
        await catalog.GetDeviceTypesAsync(ct);

    [HttpGet("services")]
    public async Task<IReadOnlyList<RepairServiceDto>> GetServices([FromQuery] int? deviceTypeId, CancellationToken ct) =>
        await catalog.GetServicesAsync(deviceTypeId, ct);

    [HttpGet("services/{id:int}")]
    public async Task<ActionResult<RepairServiceDto>> GetService(int id, CancellationToken ct)
    {
        var service = await catalog.GetServiceAsync(id, ct);
        return service is null ? NotFound() : Ok(service);
    }
}
