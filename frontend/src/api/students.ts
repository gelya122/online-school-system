import axiosInstance from './axiosInstance';

export type UpdateStudentPayload = {
  firstName?: string | null;
  lastName?: string | null;
  phone?: string | null;
  dateOfBirth?: string | null;
  avatarUrl?: string | null;
  /** Data URL или base64 — сервер сохранит файл и обновит avatarUrl */
  avatarBase64?: string | null;
  classNumber?: number | null;
  parentPhone?: string | null;
  parentEmail?: string | null;
};

export type StudentRecord = {
  studentId: number;
  userId: number;
  firstName: string;
  lastName: string;
  phone?: string | null;
  dateOfBirth?: string | null;
  avatarUrl?: string | null;
  classNumber: number;
  parentPhone?: string | null;
  parentEmail?: string | null;
};

function pick<T>(raw: Record<string, unknown>, camel: string, pascal: string): T | undefined {
  const v = raw[camel] ?? raw[pascal];
  return v as T | undefined;
}

function normalizeDob(v: unknown): string | null {
  if (v == null) return null;
  const s = String(v);
  if (/^\d{4}-\d{2}-\d{2}/.test(s)) return s.slice(0, 10);
  return null;
}

export function normalizeStudentPayload(raw: unknown): StudentRecord {
  const r = raw as Record<string, unknown>;
  return {
    studentId: Number(pick(r, 'studentId', 'StudentId') ?? 0),
    userId: Number(pick(r, 'userId', 'UserId') ?? 0),
    firstName: String(pick(r, 'firstName', 'FirstName') ?? ''),
    lastName: String(pick(r, 'lastName', 'LastName') ?? ''),
    phone: (pick(r, 'phone', 'Phone') as string | null | undefined) ?? null,
    dateOfBirth: normalizeDob(pick(r, 'dateOfBirth', 'DateOfBirth')),
    avatarUrl: (pick(r, 'avatarUrl', 'AvatarUrl') as string | null | undefined) ?? null,
    classNumber: Number(pick(r, 'classNumber', 'ClassNumber') ?? 0),
    parentPhone: (pick(r, 'parentPhone', 'ParentPhone') as string | null | undefined) ?? null,
    parentEmail: (pick(r, 'parentEmail', 'ParentEmail') as string | null | undefined) ?? null,
  };
}

export async function getStudent(studentId: number): Promise<StudentRecord> {
  // Обходим кэш браузера/прокси: иначе после PUT с новым аватаром GET может вернуть старый JSON без avatarUrl.
  const res = await axiosInstance.get<unknown>(`/Students/${studentId}`, {
    params: { _: Date.now() },
  });
  return normalizeStudentPayload(res.data);
}

export async function updateStudent(studentId: number, body: UpdateStudentPayload): Promise<void> {
  await axiosInstance.put(`/Students/${studentId}`, body);
}

/** Загрузка аватара multipart (предпочтительно вместо base64 в PUT). */
export async function uploadStudentAvatar(studentId: number, file: File): Promise<string> {
  const fd = new FormData();
  fd.append('file', file);
  const res = await axiosInstance.post<unknown>(`/Students/${studentId}/avatar`, fd);
  const raw = res.data as Record<string, unknown>;
  const url = (raw.avatarUrl ?? raw.AvatarUrl) as string | undefined;
  if (!url || typeof url !== 'string') {
    throw new Error('Сервер не вернул адрес аватара.');
  }
  return url;
}
