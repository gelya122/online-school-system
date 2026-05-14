using System.Net;
using online_school_admin.Models;

namespace online_school_admin.Services;

public sealed class AuthService
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    public AuthService(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    /// <summary>Вход по email и паролю: JWT сохраняется в <see cref="SessionService"/> и подставляется в запросы через <see cref="ApiClient"/>.</summary>
    public async Task SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var resp = await _api.PostAsync<AuthLoginRequest, AuthLoginResponse>(
            "api/auth/login",
            new AuthLoginRequest { Email = email, Password = password },
            cancellationToken);

        if (string.IsNullOrWhiteSpace(resp.AccessToken))
            throw new ApiException(HttpStatusCode.InternalServerError, "Сервер не вернул токен.");

        EmployeeDesktopAccess.Ensure(resp.User);

        _session.SetSession(resp.AccessToken, resp.User);

        try
        {
            var me = await GetMeAsync(cancellationToken);
            EmployeeDesktopAccess.Ensure(me);
            _session.SetSession(resp.AccessToken, me);
        }
        catch
        {
            _session.Clear();
            throw;
        }
    }

    public async Task<CurrentUserModel> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var me = await _api.GetAsync<CurrentUserModel>("api/auth/me", cancellationToken);
        return me;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _api.PostAsync<object, Dictionary<string, object>>("api/auth/logout", new { }, cancellationToken);
        }
        finally
        {
            _session.Clear();
        }
    }

    public Task<IReadOnlyList<UserRoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return _api.GetAsync<IReadOnlyList<UserRoleDto>>("api/UserRoles", cancellationToken);
    }

    public async Task RegisterEmployeeAsync(AuthRegisterEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        _ = await _api.PostAsync<AuthRegisterEmployeeRequest, AuthLoginResponse>(
            "api/auth/register-employee",
            request,
            cancellationToken);
        // После регистрации сервер возвращает токен, но здесь оставляем текущую сессию как есть.
    }
}

