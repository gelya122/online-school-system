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
    const hash = location.hash.replace(/^#/, '');
    if (hash) {
      const t = window.setTimeout(() => {
        document.getElementById(hash)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }, 80);
      return () => window.clearTimeout(t);
    }
    window.scrollTo(0, 0);
  }, [location.pathname, location.hash]);

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

