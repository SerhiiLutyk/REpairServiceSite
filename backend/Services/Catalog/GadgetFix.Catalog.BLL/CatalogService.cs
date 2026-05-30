using GadgetFix.Catalog.DAL;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Catalog.BLL;

public record DeviceTypeDto(int Id, string Name, string Slug, string? Icon);
public record RepairServiceDto(int Id, int DeviceTypeId, string Name, decimal BasePrice, int EstimatedDays);

public interface ICatalogService
{
    Task<IReadOnlyList<DeviceTypeDto>> GetDeviceTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RepairServiceDto>> GetServicesAsync(int? deviceTypeId, CancellationToken ct = default);
    Task<RepairServiceDto?> GetServiceAsync(int id, CancellationToken ct = default);
}

public class CatalogService(CatalogDbContext db) : ICatalogService
{
    public async Task<IReadOnlyList<DeviceTypeDto>> GetDeviceTypesAsync(CancellationToken ct = default) =>
        await db.DeviceTypes.AsNoTracking()
            .Select(d => new DeviceTypeDto(d.Id, d.Name, d.Slug, d.Icon))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RepairServiceDto>> GetServicesAsync(int? deviceTypeId, CancellationToken ct = default) =>
        await db.RepairServices.AsNoTracking()
            .Where(s => deviceTypeId == null || s.DeviceTypeId == deviceTypeId)
            .OrderBy(s => s.DeviceTypeId).ThenBy(s => s.Name)
            .Select(s => new RepairServiceDto(s.Id, s.DeviceTypeId, s.Name, s.BasePrice, s.EstimatedDays))
            .ToListAsync(ct);

    public async Task<RepairServiceDto?> GetServiceAsync(int id, CancellationToken ct = default) =>
        await db.RepairServices.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new RepairServiceDto(s.Id, s.DeviceTypeId, s.Name, s.BasePrice, s.EstimatedDays))
            .FirstOrDefaultAsync(ct);
}
