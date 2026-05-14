using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

/// <summary>Проверка доступа к desktop-админке: только записи сотрудника в <c>employee</c>, не роль ученика.</summary>
public static class EmployeeDesktopAccess
{
    /// <summary>Согласовано с backend: <c>AuthController</c>, <c>RegistrationController</c> (роль ученика).</summary>
    public const int StudentRoleId = 7;

    public static bool TryValidate(CurrentUserModel user, out string? errorMessage)
    {
        errorMessage = null;
        if (user.EmployeeId is null or <= 0)
        {
            errorMessage = "Вход доступен только сотрудникам школы (нужна запись в профиле сотрудника).";
            return false;
        }

        if (user.RoleId == StudentRoleId)
        {
            errorMessage = "Ученики не могут входить в приложение для сотрудников.";
            return false;
        }

        var r = (user.RoleName ?? "").Trim().ToLowerInvariant();
        if (r.Contains("student", StringComparison.Ordinal)
            || r.Contains("ученик", StringComparison.Ordinal)
            || r.Contains("студент", StringComparison.Ordinal))
        {
            errorMessage = "Эта учётная запись не предназначена для входа в приложение сотрудников.";
            return false;
        }

        return true;
    }

    public static void Ensure(CurrentUserModel user)
    {
        if (!TryValidate(user, out var msg))
            throw new ApiException(HttpStatusCode.Forbidden, msg!);
    }
}
