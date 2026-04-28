import { NavLink, Outlet } from 'react-router-dom';
import './StudentCabinetLayout.css';

const StudentCabinetLayout = () => {
  return (
    <div className="student-cabinet">
      <aside className="student-cabinet-sidebar" aria-label="Меню личного кабинета">
        <nav className="student-cabinet-nav">
          <NavLink to="/learn" end className={({ isActive }) => (isActive ? 'sc-nav-link is-active' : 'sc-nav-link')}>
            Мои курсы
          </NavLink>
          <NavLink
            to="/learn/account"
            className={({ isActive }) => (isActive ? 'sc-nav-link is-active' : 'sc-nav-link')}
          >
            Аккаунт
          </NavLink>
          <NavLink
            to="/learn/homework"
            className={({ isActive }) => (isActive ? 'sc-nav-link is-active' : 'sc-nav-link')}
          >
            ДЗ
          </NavLink>
          <NavLink
            to="/learn/progress"
            className={({ isActive }) => (isActive ? 'sc-nav-link is-active' : 'sc-nav-link')}
          >
            Прогресс
          </NavLink>
        </nav>
      </aside>
      <div className="student-cabinet-body">
        <Outlet />
      </div>
    </div>
  );
};

export default StudentCabinetLayout;
