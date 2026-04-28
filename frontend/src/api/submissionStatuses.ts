import axiosInstance from './axiosInstance';

export type SubmissionStatusRecord = {
  statusId: number;
  statusName: string;
  description?: string | null;
};

export async function getSubmissionStatuses(): Promise<SubmissionStatusRecord[]> {
  const res = await axiosInstance.get<SubmissionStatusRecord[]>('/SubmissionStatuses');
  return res.data ?? [];
}
