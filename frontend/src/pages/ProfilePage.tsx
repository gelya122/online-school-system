import { useAuth } from '../contexts/AuthContext';
import './ProfilePage.css';

const ProfilePage = () => {
  const { user } = useAuth();
  const roleText = user?.roleLabel ?? user?.role;

  return (
    <div className="profile-page">
      <div className="profile-hero">
        <div className="profile-hero-inner">
          <div className="profile-avatar-wrap">
            {user?.avatarUrl ? (
              <img
                className="profile-avatar"
                src={user.avatarUrl}
                alt={user.firstName ? `Фото: ${user.firstName}` : 'Аватар'}
              />
            ) : (
              <div className="profile-avatar profile-avatar-fallback" aria-hidden>
                {(user?.firstName?.[0] ?? user?.email?.[0] ?? '?').toUpperCase()}
              </div>
            )}
          </div>
          <div className="profile-hero-text">
            <h1>Аккаунт</h1>
            <p className="profile-email-lead">{user?.email}</p>
          </div>
        </div>
      </div>

      <div className="profile-card">
        <h2 className="profile-card-title">Данные профиля</h2>
        <div className="profile-info">
          <div className="info-item">
            <span className="info-label">Email</span>
            <span className="info-value">{user?.email}</span>
          </div>
          {user?.firstName && (
            <div className="info-item">
              <span className="info-label">Имя</span>
              <span className="info-value">{user.firstName}</span>
            </div>
          )}
          {user?.lastName && (
            <div className="info-item">
              <span className="info-label">Фамилия</span>
              <span className="info-value">{user.lastName}</span>
            </div>
          )}
          {roleText && (
            <div className="info-item">
              <span className="info-label">Роль</span>
              <span className="info-value">{roleText}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
