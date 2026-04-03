import axiosInstance from './axiosInstance';
import type { Review } from '../types';

type ReviewDto = {
  // Backend может отдать как camelCase, так и PascalCase
  reviewId?: number;
  ReviewId?: number;

  studentId?: number;
  StudentId?: number;

  courseId?: number;
  CourseId?: number;

  rating?: number | null;
  Rating?: number | null;

  comment?: string | null;
  Comment?: string | null;

  isPublished?: boolean | null;
  IsPublished?: boolean | null;

  createdAt?: string | null;
  CreatedAt?: string | null;
};

export async function getReviews(): Promise<Review[]> {
  const res = await axiosInstance.get<ReviewDto[]>('/Reviews');

  return res.data.map((dto, index) => ({
    id: dto.reviewId ?? dto.ReviewId ?? index,
    courseId: dto.courseId ?? dto.CourseId ?? undefined,
    rating: dto.rating ?? dto.Rating ?? undefined,
    comment: dto.comment ?? dto.Comment ?? undefined,
    isPublished: dto.isPublished ?? dto.IsPublished ?? undefined,
    createdAt: dto.createdAt ?? dto.CreatedAt ?? undefined,
  }));
}

