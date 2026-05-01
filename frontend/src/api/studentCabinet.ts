import axiosInstance from './axiosInstance';
import { publicApiFileUrl } from '../utils/publicUrl';

export type StudentCabinetCourseSummary = {
  courseId: number;
  title: string;
  description?: string | null;
  shortDescription?: string | null;
  coverImgUrl?: string | null;
  totalHours?: number | null;
  whatYouGet?: string | null;
};

export type StudentCabinetInstanceSummary = {
  instanceId: number;
  instanceName: string;
  startDate: string;
  endDate?: string | null;
  totalWeeks?: number | null;
  lessonsPerWeek?: number | null;
  scheduleDescription?: string | null;
};

export type StudentCabinetEnrollmentSummary = {
  enrollmentId: number;
  enrolledAt?: string | null;
  enrollmentStatusId?: number | null;
  enrollmentStatusName?: string | null;
  course: StudentCabinetCourseSummary;
  instance: StudentCabinetInstanceSummary;
};

export type StudentCabinetLessonOutline = {
  lessonId: number;
  moduleId: number;
  title: string;
  lessonOrder: number;
  lessonTypeId: number;
  durationMinutes?: number | null;
};

export type StudentCabinetModuleOutline = {
  moduleId: number;
  title: string;
  description?: string | null;
  moduleOrder: number;
  lessons: StudentCabinetLessonOutline[];
};

export type StudentCabinetEnrollmentDetail = {
  enrollmentId: number;
  enrolledAt?: string | null;
  enrollmentStatusId?: number | null;
  enrollmentStatusName?: string | null;
  course: StudentCabinetCourseSummary;
  instance: StudentCabinetInstanceSummary;
  modules: StudentCabinetModuleOutline[];
};

export type StudentCabinetLessonMaterial = {
  materialId: number;
  fileName: string;
  fileUrl: string;
  fileType?: string | null;
  fileSizeKb?: number | null;
  downloadCount?: number | null;
  uploadedAt?: string | null;
};

export type StudentCabinetAssignment = {
  assignmentId: number;
  title: string;
  description?: string | null;
  assignmentTypeId: number;
  assignmentTypeName?: string | null;
  maxScore: number;
  dueDaysAfterLesson?: number | null;
  calculatedDueDate?: string | null;
  createdAt?: string | null;
};

export type StudentCabinetLessonAccess = {
  accessId: number;
  plannedAccessDate: string;
  plannedAccessTime?: string | null;
  actualOpenDatetime?: string | null;
  isAvailable?: boolean | null;
};

export type StudentCabinetLessonProgress = {
  progressId: number;
  isCompleted?: boolean | null;
  completedAt?: string | null;
  watchTimeSeconds?: number | null;
  lastAccessed?: string | null;
};

export type StudentCabinetSubmission = {
  submissionId: number;
  assignmentId: number;
  studentAnswerText?: string | null;
  submittedAt?: string | null;
  score?: number | null;
  submissionStatusName?: string | null;
  teacherComment?: string | null;
};

export type StudentCabinetQuestion = {
  questionId: number;
  questionOrder: number;
  questionText: string;
  questionType?: string | null;
  maxPoints: number;
  studentAnswer?: string | null;
  pointsAwarded?: number | null;
};

export type StudentCabinetAssignmentResult = {
  assignmentId: number;
  totalScore: number;
  maxScore: number;
};

export type StudentCabinetLessonDetail = {
  lessonId: number;
  moduleId: number;
  moduleTitle: string;
  title: string;
  lessonTypeId: number;
  lessonTypeName?: string | null;
  content?: string | null;
  videoUrl?: string | null;
  durationMinutes?: number | null;
  lessonOrder: number;
  isFreePreview?: boolean | null;
  createdAt?: string | null;
  access?: StudentCabinetLessonAccess | null;
  progress?: StudentCabinetLessonProgress | null;
  materials: StudentCabinetLessonMaterial[];
  assignments: StudentCabinetAssignment[];
  submissions: StudentCabinetSubmission[];
};

export type StudentCabinetHomeworkRow = {
  enrollmentId: number;
  /** С сервера после обновления API; без него ссылка «к уроку» скрыта */
  lessonId?: number;
  courseTitle: string;
  moduleTitle: string;
  lessonTitle: string;
  assignmentId: number;
  assignmentTitle: string;
  assignmentDescription?: string | null;
  assignmentTypeId: number;
  assignmentTypeName?: string | null;
  maxScore: number;
  dueDaysAfterLesson?: number | null;
  plannedLessonAccessDate?: string | null;
  calculatedDueDate?: string | null;
  submittedAt?: string | null;
  submissionScore?: number | null;
  submissionStatusName?: string | null;
};

export type StudentCabinetProgressRow = {
  enrollmentId: number;
  lessonId: number;
  courseTitle: string;
  moduleTitle: string;
  moduleOrder: number;
  lessonTitle: string;
  lessonOrder: number;
  isCompleted?: boolean | null;
  completedAt?: string | null;
  watchTimeSeconds?: number | null;
  lastAccessed?: string | null;
};

function withCoverUrl(course: StudentCabinetCourseSummary): StudentCabinetCourseSummary {
  return {
    ...course,
    coverImgUrl: publicApiFileUrl(course.coverImgUrl ?? undefined) ?? course.coverImgUrl ?? null,
  };
}

function mapMaterials(materials: StudentCabinetLessonMaterial[]): StudentCabinetLessonMaterial[] {
  return materials.map((m) => ({
    ...m,
    fileUrl: publicApiFileUrl(m.fileUrl) ?? m.fileUrl,
  }));
}

export async function getCabinetCourses(studentId: number): Promise<StudentCabinetEnrollmentSummary[]> {
  const res = await axiosInstance.get<StudentCabinetEnrollmentSummary[]>(`/students/${studentId}/cabinet/courses`);
  return (res.data ?? []).map((row) => ({
    ...row,
    course: withCoverUrl(row.course),
  }));
}

export async function getCabinetEnrollment(
  studentId: number,
  enrollmentId: number
): Promise<StudentCabinetEnrollmentDetail> {
  const res = await axiosInstance.get<StudentCabinetEnrollmentDetail>(
    `/students/${studentId}/cabinet/enrollments/${enrollmentId}`
  );
  const d = res.data;
  return {
    ...d,
    course: withCoverUrl(d.course),
  };
}

export async function getCabinetLesson(
  studentId: number,
  enrollmentId: number,
  lessonId: number
): Promise<StudentCabinetLessonDetail> {
  const res = await axiosInstance.get<StudentCabinetLessonDetail>(
    `/students/${studentId}/cabinet/enrollments/${enrollmentId}/lessons/${lessonId}`
  );
  const d = res.data;
  return {
    ...d,
    materials: mapMaterials(d.materials ?? []),
    videoUrl: publicApiFileUrl(d.videoUrl ?? undefined) ?? d.videoUrl ?? null,
  };
}

function parsePositiveLessonId(raw: unknown): number | undefined {
  if (raw == null) return undefined;
  const n = typeof raw === 'number' ? raw : Number(String(raw).trim());
  if (!Number.isFinite(n) || n <= 0) return undefined;
  return Math.trunc(n);
}

/** Нормализует строку ДЗ из API: lessonId может прийти как LessonId или строкой. */
function normalizeHomeworkRow(item: unknown): StudentCabinetHomeworkRow {
  const r = item as Record<string, unknown>;
  const row = { ...(item as StudentCabinetHomeworkRow) };
  const lid = parsePositiveLessonId(r.lessonId ?? r.LessonId);
  if (lid != null) {
    row.lessonId = lid;
  }
  return row;
}

export async function getCabinetHomework(studentId: number): Promise<StudentCabinetHomeworkRow[]> {
  const res = await axiosInstance.get<unknown[]>(`/students/${studentId}/cabinet/homework`);
  return (res.data ?? []).map(normalizeHomeworkRow);
}

export async function getCabinetProgress(studentId: number): Promise<StudentCabinetProgressRow[]> {
  const res = await axiosInstance.get<StudentCabinetProgressRow[]>(`/students/${studentId}/cabinet/progress`);
  return res.data ?? [];
}

export async function submitCabinetAssignment(
  studentId: number,
  enrollmentId: number,
  lessonId: number,
  assignmentId: number,
  answerText: string,
): Promise<StudentCabinetSubmission> {
  const res = await axiosInstance.post<StudentCabinetSubmission>(
    `/students/${studentId}/cabinet/enrollments/${enrollmentId}/lessons/${lessonId}/assignments/${assignmentId}/submit`,
    { answerText },
  );
  return res.data;
}

export async function getCabinetAssignmentQuestions(
  studentId: number,
  enrollmentId: number,
  lessonId: number,
  assignmentId: number,
): Promise<StudentCabinetQuestion[]> {
  const res = await axiosInstance.get<StudentCabinetQuestion[]>(
    `/students/${studentId}/cabinet/enrollments/${enrollmentId}/lessons/${lessonId}/assignments/${assignmentId}/questions`,
  );
  return res.data ?? [];
}

export async function submitCabinetQuestionAnswer(
  studentId: number,
  enrollmentId: number,
  lessonId: number,
  assignmentId: number,
  questionId: number,
  answerText: string,
): Promise<StudentCabinetQuestion> {
  const res = await axiosInstance.post<StudentCabinetQuestion>(
    `/students/${studentId}/cabinet/enrollments/${enrollmentId}/lessons/${lessonId}/assignments/${assignmentId}/questions/${questionId}/answer`,
    { answerText },
  );
  return res.data;
}

export async function getCabinetAssignmentResult(
  studentId: number,
  enrollmentId: number,
  lessonId: number,
  assignmentId: number,
): Promise<StudentCabinetAssignmentResult> {
  const res = await axiosInstance.get<StudentCabinetAssignmentResult>(
    `/students/${studentId}/cabinet/enrollments/${enrollmentId}/lessons/${lessonId}/assignments/${assignmentId}/result`,
  );
  return res.data;
}
