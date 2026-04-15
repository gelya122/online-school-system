import axiosInstance from './axiosInstance';

export type RegisterStudentPayload = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone: string;
  dateOfBirth: string;
  classNumber: number;
  parentPhone: string;
  parentEmail: string;
  avatarBase64?: string | null;
};

export type RegisterStudentResponse = {
  userId: number;
  studentId: number;
  email: string;
  avatarUrl?: string | null;
  roleLabel?: string | null;
};

export type LoginStudentPayload = {
  email: string;
  password: string;
};

export type LoginStudentResponse = {
  userId: number;
  studentId: number | null;
  email: string;
  firstName: string | null;
  lastName: string | null;
  avatarUrl?: string | null;
  roleLabel?: string | null;
};

/** Серверная проверка (те же правила, что при регистрации). Не бросает при невалидной почте — только при сетевой ошибке. */
export async function checkEmailWithApi(email: string): Promise<boolean> {
  const trimmed = email.trim();
  if (!trimmed) return false;
  const res = await axiosInstance.get<{ valid: boolean }>('/Registration/validate-email', {
    params: { email: trimmed },
  });
  return res.data?.valid === true;
}

export async function registerStudent(body: RegisterStudentPayload): Promise<RegisterStudentResponse> {
  const res = await axiosInstance.post<RegisterStudentResponse>('/Registration/student', body);
  return res.data;
}

export async function loginStudent(body: LoginStudentPayload): Promise<LoginStudentResponse> {
  const res = await axiosInstance.post<LoginStudentResponse>('/Registration/login', body);
  return res.data;
}
