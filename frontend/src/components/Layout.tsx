import { useEffect, type ReactNode } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Header from './Header';
import Footer from './Footer';

interface LayoutProps {
  children?: ReactNode;
}

const Layout = ({ children }: LayoutProps) => {
  const location = useLocation();

  useEffect(() => {
    // Чтобы при переходах по ссылкам всегда попадать в начало страницы.
    window.scrollTo(0, 0);
  }, [location.pathname]);

  const hideFooter = location.pathname === '/profile' || location.pathname.startsWith('/learn');

  return (
    <div className="layout">
      <Header />
      <main className="main-content">
        {children || <Outlet />}
      </main>
      {!hideFooter && <Footer />}
    </div>
  );
};

export default Layout;

