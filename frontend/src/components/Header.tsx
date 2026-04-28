import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useCart } from '../contexts/CartContext';
import './Header.css';

const Header = () => {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const { items } = useCart();
  const cartCount = items.length;

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const handleLogoClick = () => {
    window.scrollTo(0, 0);
  };

  return (
    <header className="header">
      <div className="header-container">
        <div className="logo">
          <Link to="/" onClick={handleLogoClick}>EduSchool</Link>
        </div>
        <nav className="nav">
          <Link to="/courses" className="nav-link">
            Курсы
          </Link>
          <Link to="/about" className="nav-link">
            О школе
          </Link>

          {isAuthenticated ? (
            <>
              <Link to="/learn" className="nav-link">
                Личный кабинет
              </Link>

              <Link to="/cart" className="nav-cart" aria-label="Корзина">
                {cartCount > 0 && <span className="nav-cart-badge">{cartCount}</span>}
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                  <path
                    d="M7 18C7.55228 18 8 18.4477 8 19C8 19.5523 7.55228 20 7 20C6.44772 20 6 19.5523 6 19C6 18.4477 6.44772 18 7 18Z"
                    fill="currentColor"
                  />
                  <path
                    d="M17 18C17.5523 18 18 18.4477 18 19C18 19.5523 17.5523 20 17 20C16.4477 20 16 19.5523 16 19C16 18.4477 16.4477 18 17 18Z"
                    fill="currentColor"
                  />
                  <path
                    d="M3 3H5.2C5.64635 3 6.04521 3.2682 6.20565 3.68421L7 6H20C20.5523 6 21 6.44772 21 7C21 7.107 20.9815 7.21328 20.9455 7.31441L19.281 12.0809C19.1153 12.5494 18.6732 12.8626 18.1767 12.8626H8.2"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                  <path
                    d="M7 6L5.9 9.2C5.73956 9.616 5.3407 9.88421 4.89435 9.88421H3.5"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </Link>
              <button onClick={handleLogout} className="btn-logout">
                Выйти
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn-login-link">
                Войти
              </Link>

              <Link to="/cart" className="nav-cart" aria-label="Корзина">
                {cartCount > 0 && <span className="nav-cart-badge">{cartCount}</span>}
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                  <path
                    d="M7 18C7.55228 18 8 18.4477 8 19C8 19.5523 7.55228 20 7 20C6.44772 20 6 19.5523 6 19C6 18.4477 6.44772 18 7 18Z"
                    fill="currentColor"
                  />
                  <path
                    d="M17 18C17.5523 18 18 18.4477 18 19C18 19.5523 17.5523 20 17 20C16.4477 20 16 19.5523 16 19C16 18.4477 16.4477 18 17 18Z"
                    fill="currentColor"
                  />
                  <path
                    d="M3 3H5.2C5.64635 3 6.04521 3.2682 6.20565 3.68421L7 6H20C20.5523 6 21 6.44772 21 7C21 7.107 20.9815 7.21328 20.9455 7.31441L19.281 12.0809C19.1153 12.5494 18.6732 12.8626 18.1767 12.8626H8.2"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                  <path
                    d="M7 6L5.9 9.2C5.73956 9.616 5.3407 9.88421 4.89435 9.88421H3.5"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  );
};

export default Header;