import axiosInstance from './axiosInstance';
import type { SchoolSetting } from '../types';

type SchoolSettingDto = {
  // backend может отдать camelCase или PascalCase
  settingId?: number;
  SettingId?: number;

  schoolName?: string;
  SchoolName?: string;

  logoUrl?: string | null;
  LogoUrl?: string | null;

  contactPhone?: string | null;
  ContactPhone?: string | null;

  contactEmail?: string | null;
  ContactEmail?: string | null;

  address?: string | null;
  Address?: string | null;

  aboutSchoolText?: string | null;
  AboutSchoolText?: string | null;

  privacyPolicyUrl?: string | null;
  PrivacyPolicyUrl?: string | null;

  termsOfUseUrl?: string | null;
  TermsOfUseUrl?: string | null;

  updatedAt?: string | null;
  UpdatedAt?: string | null;
};

export async function getSchoolSettings(): Promise<SchoolSetting | null> {
  const res = await axiosInstance.get<SchoolSettingDto[]>('/SchoolSettings');
  const dto = res.data?.[0];
  if (!dto) return null;

  return {
    id: dto.settingId ?? dto.SettingId ?? 0,
    schoolName: dto.schoolName ?? dto.SchoolName ?? '',
    logoUrl: dto.logoUrl ?? dto.LogoUrl ?? undefined,
    contactPhone: dto.contactPhone ?? dto.ContactPhone ?? undefined,
    contactEmail: dto.contactEmail ?? dto.ContactEmail ?? undefined,
    address: dto.address ?? dto.Address ?? undefined,
    aboutSchoolText: dto.aboutSchoolText ?? dto.AboutSchoolText ?? undefined,
    privacyPolicyUrl: dto.privacyPolicyUrl ?? dto.PrivacyPolicyUrl ?? undefined,
    termsOfUseUrl: dto.termsOfUseUrl ?? dto.TermsOfUseUrl ?? undefined,
    updatedAt: dto.updatedAt ?? dto.UpdatedAt ?? undefined,
  };
}

