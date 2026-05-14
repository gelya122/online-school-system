namespace online_school_admin.Infrastructure;

public static class SimpleEmailValidator
{
    public static bool IsValid(string email)
    {
        email = email.Trim();
        if (email.Length < 5 || email.Length > 254)
            return false;
        var at = email.IndexOf('@');
        if (at <= 0 || at >= email.Length - 1)
            return false;
        return email.Contains('.', StringComparison.Ordinal);
    }
}
