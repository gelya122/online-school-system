import { useAuth } from '../contexts/AuthContext';
import './ProfilePage.css';

const ProfilePage = () => {
  const { user } = useAuth();

  return (
    <div className="profile-page">
      <h1>Профиль пользователя</h1>
      <div className="profile-card">
        <div className="profile-info">
          <div className="info-item">
            <label>Email:</label>
            <span>{user?.email}</span>
          </div>
          {user?.firstName && (
            <div className="info-item">
              <label>Имя:</label>
              <span>{user.firstName}</span>
            </div>
          )}
          {user?.lastName && (
            <div className="info-item">
              <label>Фамилия:</label>
              <span>{user.lastName}</span>
            </div>
          )}
          {user?.role && (
            <div className="info-item">
              <label>Роль:</label>
              <span>{user.role}</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;

