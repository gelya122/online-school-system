// Типы для пользователя
export interface User {
  id: number;
  email: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  avatarUrl?: string;
}

// Типы для аутентификации
export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  classNumber: number;
  phone: string;
  dateOfBirth: string;
  parentPhone: string;
  parentEmail: string;
  avatarBase64?: string | null;
}

export interface AuthResponse {
  token: string;
  user: User;
}

// Типы для курсов
export interface Course {
  id: number;
  title: string;
  description?: string;
  price?: number;
  categoryId?: number;
  subjectId?: number;
  examId?: number;
  imageUrl?: string;
}

export interface CourseCategory {
  id: number;
  name: string;
  description?: string;
}

export interface Subject {
  id: number;
  name: string;
  description?: string;
  isActive?: boolean;
}

export interface Exam {
  id: number;
  name: string;
  description?: string;
  isActive?: boolean;
}

export interface SchoolSetting {
  id: number;
  schoolName: string;
  logoUrl?: string;
  contactPhone?: string;
  contactEmail?: string;
  address?: string;
  aboutSchoolText?: string;
  privacyPolicyUrl?: string;
  termsOfUseUrl?: string;
  updatedAt?: string;
}

// Типы для FAQ
export interface FaqItem {
  id: number;
  categoryId?: number;
  question: string;
  answer: string;
  isActive?: boolean;
  order?: number;
}

// Типы для отзывов
export interface Review {
  id: number;
  courseId?: number;
  rating?: number;
  comment?: string;
  isPublished?: boolean;
  createdAt?: string;
}

// Типы для преподавателей/сотрудников
export interface Employee {
  id: number;
  userId?: number;
  firstName: string;
  lastName: string;
  patronymic?: string;
  phone?: string;
  dateOfBirth?: string;
  avatarUrl?: string;
  workExperience?: number;
  isActive?: boolean;
  createdAt?: string;
}

// Типы для API ответов
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}

