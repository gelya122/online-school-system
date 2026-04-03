import { ReactNode, useEffect } from 'react';
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

  return (
    <div className="layout">
      <Header />
      <main className="main-content">
        {children || <Outlet />}
      </main>
      <Footer />
    </div>
  );
};

export default Layout;

