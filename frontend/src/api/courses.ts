import axiosInstance from './axiosInstance';
import type { Course, CourseModuleOutline } from '../types';

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

  modules?: CourseModuleOutlineDto[] | null;
  Modules?: CourseModuleOutlineDto[] | null;

  reviewAverage?: number | null;
  ReviewAverage?: number | null;
  reviewCount?: number | null;
  ReviewCount?: number | null;
};

type CourseModuleOutlineDto = {
  moduleId?: number;
  ModuleId?: number;
  title?: string;
  Title?: string;
  description?: string | null;
  Description?: string | null;
  moduleOrder?: number;
  ModuleOrder?: number;
  lessonCount?: number;
  LessonCount?: number;
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

function mapModuleOutlines(dto: CourseDto): CourseModuleOutline[] | undefined {
  const raw = dto.modules ?? dto.Modules;
  if (!raw || !Array.isArray(raw) || raw.length === 0) return undefined;
  return raw.map((m) => ({
    id: m.moduleId ?? m.ModuleId ?? 0,
    title: m.title ?? m.Title ?? '',
    description: m.description ?? m.Description ?? undefined,
    order: m.moduleOrder ?? m.ModuleOrder ?? 0,
    lessonCount: m.lessonCount ?? m.LessonCount ?? 0,
  }));
}

export async function getCourse(id: number): Promise<Course> {
  const res = await axiosInstance.get<CourseDto>(`/Courses/${id}`);
  const dto = res.data;
  const shortDesc = dto.shortDescription ?? dto.ShortDescription ?? null;
  const longDesc = dto.description ?? dto.Description ?? null;
  const reviewAvgRaw = dto.reviewAverage ?? dto.ReviewAverage;
  const reviewCntRaw = dto.reviewCount ?? dto.ReviewCount;
  return {
    id: dto.courseId ?? dto.CourseId ?? 0,
    title: dto.title ?? dto.Title ?? '',
    description: shortDesc ?? longDesc ?? undefined,
    fullDescription: longDesc ?? shortDesc ?? undefined,
    price: dto.discountPrice ?? dto.DiscountPrice ?? dto.price ?? dto.Price,
    categoryId: dto.categoryId ?? dto.CategoryId,
    subjectId: dto.subjectId ?? dto.SubjectId ?? undefined,
    examId: dto.examId ?? dto.ExamId ?? undefined,
    imageUrl: dto.coverImgUrl ?? dto.CoverImgUrl ?? undefined,
    totalHours: dto.totalHours ?? dto.TotalHours ?? undefined,
    whatYouGet: dto.whatYouGet ?? dto.WhatYouGet ?? undefined,
    modules: mapModuleOutlines(dto),
    reviewAverage: reviewAvgRaw != null ? Number(reviewAvgRaw) : undefined,
    reviewCount: reviewCntRaw != null ? Number(reviewCntRaw) : undefined,
  };
}

