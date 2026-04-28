import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import type { User, LoginCredentials, RegisterData } from '../types';
import { loginStudent, registerStudent } from '../api/registration';
import { getStudent, updateStudent, type UpdateStudentPayload } from '../api/students';
import { updateUser } from '../api/users';
import { isValidEmailFormat } from '../utils/emailValidation';
import { isRuPhoneComplete, ruPhoneToStoredString } from '../utils/phoneMask';
import { publicApiFileUrl } from '../utils/publicUrl';

export interface SaveProfileInput {
  email: string;
  firstName?: string;
  lastName?: string;
  phoneDigits?: string;
  parentPhoneDigits?: string;
  dateOfBirth?: string;
  classNumber?: number;
  parentEmail?: string;
  avatarBase64?: string | null;
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  saveProfile: (data: SaveProfileInput) => Promise<void>;
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
        studentId: res.studentId ?? undefined,
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
        studentId: res.studentId ?? undefined,
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

  const saveProfile = async (data: SaveProfileInput) => {
    if (user == null) {
      throw new Error('Нет данных пользователя.');
    }
    const emailTrim = data.email.trim();
    if (!isValidEmailFormat(emailTrim)) {
      throw new Error('Введите корректный email учётной записи.');
    }

    const sid = user.studentId;
    let syncedAvatarUrl: string | undefined = user.avatarUrl;

    if (sid != null) {
      const fn = data.firstName?.trim() ?? '';
      const ln = data.lastName?.trim() ?? '';
      if (!fn || !ln) {
        throw new Error('Укажите имя и фамилию.');
      }
      if (!data.phoneDigits || !isRuPhoneComplete(data.phoneDigits)) {
        throw new Error('Введите полный номер телефона ученика.');
      }
      if (!data.parentPhoneDigits || !isRuPhoneComplete(data.parentPhoneDigits)) {
        throw new Error('Введите полный номер телефона родителя.');
      }
      const parentEm = data.parentEmail?.trim() ?? '';
      if (!parentEm || !isValidEmailFormat(parentEm)) {
        throw new Error('Введите корректную почту родителя.');
      }
      const dob = data.dateOfBirth?.trim() ?? '';
      if (!dob) {
        throw new Error('Укажите дату рождения.');
      }
      const classNum = data.classNumber;
      if (classNum == null || classNum < 1 || classNum > 11) {
        throw new Error('Класс должен быть от 1 до 11.');
      }

      const studentPayload: UpdateStudentPayload = {
        firstName: fn,
        lastName: ln,
        phone: ruPhoneToStoredString(data.phoneDigits),
        parentPhone: ruPhoneToStoredString(data.parentPhoneDigits),
        dateOfBirth: dob,
        classNumber: classNum,
        parentEmail: parentEm,
      };
      const av = data.avatarBase64?.trim();
      if (av) {
        studentPayload.avatarBase64 = av;
      }
      await updateStudent(sid, studentPayload);
      const refreshed = await getStudent(sid);
      syncedAvatarUrl = publicApiFileUrl(refreshed.avatarUrl ?? undefined);
    }

    if (emailTrim.toLowerCase() !== user.email.toLowerCase()) {
      await updateUser(user.id, { email: emailTrim });
    }

    const nextUser: User = {
      ...user,
      email: emailTrim,
    };

    if (sid != null) {
      nextUser.firstName = data.firstName?.trim();
      nextUser.lastName = data.lastName?.trim();
      nextUser.phone = ruPhoneToStoredString(data.phoneDigits!);
      nextUser.parentPhone = ruPhoneToStoredString(data.parentPhoneDigits!);
      nextUser.dateOfBirth = data.dateOfBirth?.trim();
      nextUser.classNumber = data.classNumber;
      nextUser.parentEmail = data.parentEmail?.trim();
      nextUser.avatarUrl = syncedAvatarUrl;
    }

    localStorage.setItem('user', JSON.stringify(nextUser));
    setUser(nextUser);
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
    saveProfile,
    logout,
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

