using online_school_admin.Infrastructure;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class MockAuthService : IAuthService
{
    public Task<bool> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        email = (email ?? "").Trim();
        password ??= "";

        // Тестовые данные входа (позже заменим на реальный API).
        if (string.Equals(email, "admin@school.local", StringComparison.OrdinalIgnoreCase) && password == "admin")
        {
            AdminSession.Set(new LoginAdminResponse
            {
                Email = email,
                FirstName = "Админ",
                LastName = "Школы",
                RoleLabel = "Администратор"
            });
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}

