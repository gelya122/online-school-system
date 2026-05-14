namespace online_school_admin.Services;

public interface IAuthService
{
    Task<bool> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
}

