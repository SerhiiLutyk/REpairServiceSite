using System.Security.Claims;

namespace NeoFix.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(ClaimTypes.Name)
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(id);
    }

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole("Admin");
}
