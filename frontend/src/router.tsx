import { createBrowserRouter } from 'react-router-dom';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import CoursesPage from './pages/CoursesPage';
import LoginPage from './pages/LoginPage';
import ProfilePage from './pages/ProfilePage';
import PrivateRoute from './components/PrivateRoute';
import CourseDetailPage from './pages/CourseDetailPage.tsx';
import AboutPage from './pages/AboutPage.tsx';
import CartPage from './pages/CartPage.tsx';
import CheckoutPage from './pages/CheckoutPage.tsx';
import RegisterPage from './pages/RegisterPage.tsx';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'courses', element: <CoursesPage /> },
      { path: 'courses/:id', element: <CourseDetailPage /> },
      { path: 'about', element: <AboutPage /> },
      { path: 'cart', element: <CartPage /> },
      { path: 'checkout', element: <CheckoutPage /> },
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },
      {
        path: 'profile',
        element: (
          <PrivateRoute>
            <ProfilePage />
          </PrivateRoute>
        ),
      },
      { path: '*', element: <div className="not-found">404 - Страница не найдена</div> },
    ],
  },
]);