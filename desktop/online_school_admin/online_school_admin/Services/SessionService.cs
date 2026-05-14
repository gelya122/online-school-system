using online_school_admin.Infrastructure;
using online_school_admin.Models;

namespace online_school_admin.Services;

/// <summary>JWT и профиль текущего пользователя в памяти процесса до выхода или очистки (истечение — см. таймер в главном окне).</summary>
public sealed class SessionService
{
    public string? AccessToken { get; private set; }
    public CurrentUserModel? CurrentUser { get; private set; }

    /// <summary>Время истечения access-токена (UTC), если удалось прочитать из JWT.</summary>
    public DateTime? AccessTokenExpiresUtc { get; private set; }

    public bool IsSignedIn => !string.IsNullOrWhiteSpace(AccessToken) && CurrentUser != null;

    public event Action? SignedOut;

    public void SetSession(string accessToken, CurrentUserModel user)
    {
        AccessToken = accessToken;
        CurrentUser = user;
        AccessTokenExpiresUtc = JwtExpiryReader.TryGetExpiryUtc(accessToken);
    }

    /// <summary>С запасом 30 секунд до формального истечения.</summary>
    public bool IsAccessTokenExpiredUtc()
    {
        if (!AccessTokenExpiresUtc.HasValue)
            return false;
        return DateTime.UtcNow >= AccessTokenExpiresUtc.Value.AddSeconds(-30);
    }

    public void Clear()
    {
        AccessToken = null;
        CurrentUser = null;
        AccessTokenExpiresUtc = null;
        SignedOut?.Invoke();
    }
}

