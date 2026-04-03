import axiosInstance from './axiosInstance';
import type { FaqItem } from '../types';

type FaqItemDto = {
  // Backend может отдать как camelCase (по умолчанию в ASP.NET), так и PascalCase.
  faqId?: number;
  FaqId?: number;

  categoryId?: number | null;
  CategoryId?: number | null;

  question?: string;
  Question?: string;

  answer?: string;
  Answer?: string;

  isActive?: boolean | null;
  IsActive?: boolean | null;

  itemOrder?: number | null;
  ItemOrder?: number | null;

  createdAt?: string | null;
  CreatedAt?: string | null;
};

export async function getFaqItems(): Promise<FaqItem[]> {
  const res = await axiosInstance.get<FaqItemDto[]>('/FaqItems');
  return res.data.map((dto, index) => ({
    id: dto.faqId ?? dto.FaqId ?? index,
    categoryId: dto.categoryId ?? dto.CategoryId ?? undefined,
    question: dto.question ?? dto.Question ?? '',
    answer: dto.answer ?? dto.Answer ?? '',
    isActive: dto.isActive ?? dto.IsActive ?? undefined,
    order: dto.itemOrder ?? dto.ItemOrder ?? undefined,
  }));
}

