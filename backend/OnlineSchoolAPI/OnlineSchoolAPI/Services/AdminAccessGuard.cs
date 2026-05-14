using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace OnlineSchoolAPI.Services;

public static class AdminAccessGuard
{
    public static bool HasAccess(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true;
    }

    public static ActionResult? ForbidIfNoAccess(ControllerBase controller)
    {
        if (!HasAccess(controller.User))
            return controller.Unauthorized("Недостаточно прав.");
        return null;
    }

    public static ActionResult? ForbidIfRoleNotIn(ControllerBase controller, params string[] roles)
    {
        // Временно отключено: права будем настраивать позже и по-другому.
        return null;
    }
}

