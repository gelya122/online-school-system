using online_school_admin.Models;

namespace online_school_admin.Infrastructure;

public static class AdminSession
{
    public static LoginAdminResponse? Current { get; private set; }

    public static bool IsSignedIn => Current != null;

    public static void Set(LoginAdminResponse response) => Current = response;

    public static void Clear() => Current = null;

    public static string? DisplayName
    {
        get
        {
            if (Current == null) return null;
            var fn = Current.FirstName?.Trim();
            var ln = Current.LastName?.Trim();
            if (string.IsNullOrEmpty(fn) && string.IsNullOrEmpty(ln))
                return Current.Email;
            return $"{fn} {ln}".Trim();
        }
    }
}
