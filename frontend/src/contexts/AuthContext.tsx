import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { User, LoginCredentials, RegisterData } from '../types';
import { loginStudent, registerStudent } from '../api/registration';
import { publicApiFileUrl } from '../utils/publicUrl';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Проверяем наличие токена и пользователя в localStorage при загрузке
    const token = localStorage.getItem('token');
    const savedUser = localStorage.getItem('user');

    if (token && savedUser) {
      try {
        setUser(JSON.parse(savedUser));
      } catch (error) {
        console.error('Ошибка при загрузке пользователя:', error);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  const login = async (credentials: LoginCredentials) => {
    try {
      const res = await loginStudent({
        email: credentials.email,
        password: credentials.password,
      });

      const nextUser: User = {
        id: res.userId,
        email: res.email,
        firstName: res.firstName ?? undefined,
        lastName: res.lastName ?? undefined,
        roleLabel: res.roleLabel ?? undefined,
        avatarUrl: publicApiFileUrl(res.avatarUrl ?? undefined),
      };

      localStorage.setItem('token', 'session');
      localStorage.setItem('user', JSON.stringify(nextUser));
      setUser(nextUser);
    } catch (e: unknown) {
      const ax = e as { response?: { data?: unknown } };
      const d = ax.response?.data;
      if (typeof d === 'string') throw new Error(d);
      throw e;
    }
  };

  const register = async (data: RegisterData) => {
    try {
      const res = await registerStudent({
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        classNumber: data.classNumber,
        phone: data.phone.trim(),
        dateOfBirth: data.dateOfBirth,
        parentPhone: data.parentPhone.trim(),
        parentEmail: data.parentEmail.trim(),
        avatarBase64: data.avatarBase64 ?? undefined,
      });

      const nextUser: User = {
        id: res.userId,
        email: res.email,
        firstName: data.firstName,
        lastName: data.lastName,
        roleLabel: res.roleLabel ?? undefined,
        avatarUrl: publicApiFileUrl(res.avatarUrl ?? undefined),
      };

      localStorage.setItem('token', 'session');
      localStorage.setItem('user', JSON.stringify(nextUser));
      setUser(nextUser);
    } catch (e: unknown) {
      const ax = e as { response?: { data?: unknown } };
      const d = ax.response?.data;
      if (typeof d === 'string') throw new Error(d);
      throw e;
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    loading,
    login,
    register,
    logout,
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

