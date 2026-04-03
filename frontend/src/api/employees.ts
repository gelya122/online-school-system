import axiosInstance from './axiosInstance';
import type { Employee } from '../types';

type EmployeeDto = {
  // Backend может вернуть PascalCase или camelCase.
  employeeId?: number;
  EmployeeId?: number;

  userId?: number;
  UserId?: number;

  firstName?: string;
  FirstName?: string;

  lastName?: string;
  LastName?: string;

  patronymic?: string | null;
  Patronymic?: string | null;

  phone?: string | null;
  Phone?: string | null;

  dateOfBirth?: string | null;
  DateOfBirth?: string | null;

  avatarUrl?: string | null;
  AvatarUrl?: string | null;

  workExperience?: number | null;
  WorkExperience?: number | null;

  isActive?: boolean | null;
  IsActive?: boolean | null;

  createdAt?: string | null;
  CreatedAt?: string | null;
};

export async function getEmployees(): Promise<Employee[]> {
  const res = await axiosInstance.get<EmployeeDto[]>('/Employees');

  return res.data.map((dto, index) => ({
    id: dto.employeeId ?? dto.EmployeeId ?? index,
    userId: dto.userId ?? dto.UserId ?? undefined,
    firstName: dto.firstName ?? dto.FirstName ?? '',
    lastName: dto.lastName ?? dto.LastName ?? '',
    patronymic: dto.patronymic ?? dto.Patronymic ?? undefined,
    phone: dto.phone ?? dto.Phone ?? undefined,
    dateOfBirth: dto.dateOfBirth ?? dto.DateOfBirth ?? undefined,
    avatarUrl: dto.avatarUrl ?? dto.AvatarUrl ?? undefined,
    workExperience: dto.workExperience ?? dto.WorkExperience ?? undefined,
    isActive: dto.isActive ?? dto.IsActive ?? undefined,
    createdAt: dto.createdAt ?? dto.CreatedAt ?? undefined,
  }));
}

