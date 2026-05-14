using System.Security.Claims;

namespace OnlineSchoolAPI.Services;

public static class AuthClaims
{
    public static string GetRole(ClaimsPrincipal user)
        => (user.FindFirstValue(ClaimTypes.Role) ?? "").Trim();

    public static int? GetUserId(ClaimsPrincipal user)
    {
        var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(uid, out var id) && id > 0 ? id : null;
    }

    public static int? GetEmployeeId(ClaimsPrincipal user)
    {
        var eid = user.FindFirstValue("employeeId");
        return int.TryParse(eid, out var id) && id > 0 ? id : null;
    }
}

