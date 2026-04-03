import axiosInstance from './axiosInstance';
import type { Subject, Exam } from '../types';

type SubjectDto = {
  subjectId?: number;
  SubjectId?: number;
  subjectName?: string;
  SubjectName?: string;
  description?: string | null;
  Description?: string | null;
  isActive?: boolean | null;
  IsActive?: boolean | null;
};

type ExamDto = {
  examId?: number;
  ExamId?: number;
  examName?: string;
  ExamName?: string;
  description?: string | null;
  Description?: string | null;
  isActive?: boolean | null;
  IsActive?: boolean | null;
};

export async function getSubjects(): Promise<Subject[]> {
  const res = await axiosInstance.get<SubjectDto[]>('/Subjects');
  return res.data.map((dto, index) => ({
    id: dto.subjectId ?? dto.SubjectId ?? index,
    name: dto.subjectName ?? dto.SubjectName ?? '',
    description: dto.description ?? dto.Description ?? undefined,
    isActive: dto.isActive ?? dto.IsActive ?? undefined,
  }));
}

export async function getExams(): Promise<Exam[]> {
  const res = await axiosInstance.get<ExamDto[]>('/Exams');
  return res.data.map((dto, index) => ({
    id: dto.examId ?? dto.ExamId ?? index,
    name: dto.examName ?? dto.ExamName ?? '',
    description: dto.description ?? dto.Description ?? undefined,
    isActive: dto.isActive ?? dto.IsActive ?? undefined,
  }));
}

