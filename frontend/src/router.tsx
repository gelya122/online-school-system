import { createBrowserRouter, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import CoursesPage from './pages/CoursesPage';
import LoginPage from './pages/LoginPage';
import ProfilePage from './pages/ProfilePage';
import PrivateRoute from './components/PrivateRoute';
import StudentCabinetLayout from './components/StudentCabinetLayout';
import MyCoursesPage from './pages/cabinet/MyCoursesPage';
import CourseWorkspacePage from './pages/cabinet/CourseWorkspacePage';
import LessonPage from './pages/cabinet/LessonPage';
import LessonAssignmentPage from './pages/cabinet/LessonAssignmentPage';
import HomeworkCabinetPage from './pages/cabinet/HomeworkCabinetPage';
import ProgressCabinetPage from './pages/cabinet/ProgressCabinetPage';
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
        path: 'learn',
        element: (
          <PrivateRoute>
            <StudentCabinetLayout />
          </PrivateRoute>
        ),
        children: [
          { index: true, element: <MyCoursesPage /> },
          { path: 'account', element: <ProfilePage /> },
          { path: 'homework', element: <HomeworkCabinetPage /> },
          { path: 'progress', element: <ProgressCabinetPage /> },
          { path: 'courses/:enrollmentId', element: <CourseWorkspacePage /> },
          { path: 'courses/:enrollmentId/lessons/:lessonId', element: <LessonPage /> },
          {
            path: 'courses/:enrollmentId/lessons/:lessonId/assignments/:assignmentId',
            element: <LessonAssignmentPage />,
          },
        ],
      },
      {
        path: 'profile',
        element: (
          <PrivateRoute>
            <Navigate to="/learn/account" replace />
          </PrivateRoute>
        ),
      },
      { path: '*', element: <div className="not-found">404 - Страница не найдена</div> },
    ],
  },
]);