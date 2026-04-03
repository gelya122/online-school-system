import axiosInstance from './axiosInstance';

type CreateTrialApplicationPayload = {
  FirstName: string;
  LastName: string;
  Phone: string;
  Email?: string;
  ClassNumber: number;
  SelectedSubjects: string;
  ApplicationStatusId?: number;
};

export async function createTrialApplication(payload: CreateTrialApplicationPayload) {
  const res = await axiosInstance.post('/TrialApplications', payload);
  return res.data;
}

