import axiosInstance from './axiosInstance';
import type { Course } from '../types';

// DTO из backend (поля с PascalCase)
type CourseDto = {
  // Backend может отдавать как camelCase, так и PascalCase.
  courseId?: number;
  CourseId?: number;

  title?: string;
  Title?: string;

  description?: string | null;
  Description?: string | null;

  shortDescription?: string | null;
  ShortDescription?: string | null;

  categoryId?: number;
  CategoryId?: number;

  subjectId?: number;
  SubjectId?: number;

  examId?: number;
  ExamId?: number;

  coverImgUrl?: string | null;
  CoverImgUrl?: string | null;

  price?: number;
  Price?: number;

  discountPrice?: number | null;
  DiscountPrice?: number | null;

  totalHours?: number | null;
  TotalHours?: number | null;

  whatYouGet?: string | null;
  WhatYouGet?: string | null;

  isActive?: boolean | null;
  IsActive?: boolean | null;

  createdAt?: string | null;
  CreatedAt?: string | null;
};

type GetCoursesParams = {
  subjectId?: number;
  examId?: number;
};

export async function getCourses(params?: GetCoursesParams): Promise<Course[]> {
  const res = await axiosInstance.get<CourseDto[]>('/Courses', {
    params: {
      subjectId: params?.subjectId,
      examId: params?.examId,
    },
  });
  return res.data.map((dto) => ({
    id: dto.courseId ?? dto.CourseId ?? 0,
    title: dto.title ?? dto.Title ?? '',
    description: dto.shortDescription ?? dto.ShortDescription ?? dto.description ?? dto.Description ?? undefined,
    price: dto.discountPrice ?? dto.DiscountPrice ?? dto.price ?? dto.Price,
    categoryId: dto.categoryId ?? dto.CategoryId,
    subjectId: dto.subjectId ?? dto.SubjectId ?? undefined,
    examId: dto.examId ?? dto.ExamId ?? undefined,
    imageUrl: dto.coverImgUrl ?? dto.CoverImgUrl ?? undefined,
  }));
}

export async function getCourse(id: number): Promise<Course> {
  const res = await axiosInstance.get<CourseDto>(`/Courses/${id}`);
  const dto = res.data;
  return {
    id: dto.courseId ?? dto.CourseId ?? 0,
    title: dto.title ?? dto.Title ?? '',
    description: dto.shortDescription ?? dto.ShortDescription ?? dto.description ?? dto.Description ?? undefined,
    price: dto.discountPrice ?? dto.DiscountPrice ?? dto.price ?? dto.Price,
    categoryId: dto.categoryId ?? dto.CategoryId,
    subjectId: dto.subjectId ?? dto.SubjectId ?? undefined,
    examId: dto.examId ?? dto.ExamId ?? undefined,
    imageUrl: dto.coverImgUrl ?? dto.CoverImgUrl ?? undefined,
  };
}

