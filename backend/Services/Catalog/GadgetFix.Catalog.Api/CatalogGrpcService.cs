using GadgetFix.Catalog.DAL;
using GadgetFix.Catalog.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace GadgetFix.Catalog.Api;

/// <summary>gRPC-сервіс каталогу — використовується іншими мікросервісами (напр. Orders).</summary>
public class CatalogGrpcService(CatalogDbContext db) : Grpc.CatalogGrpc.CatalogGrpcBase
{
    public override async Task<DeviceTypeReply> GetDeviceType(DeviceTypeRequest request, ServerCallContext context)
    {
        var device = await db.DeviceTypes.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DeviceTypeId, context.CancellationToken);

        return new DeviceTypeReply
        {
            Exists = device is not null,
            Name = device?.Name ?? string.Empty,
        };
    }
}
