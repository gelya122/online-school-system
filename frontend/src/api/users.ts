import axiosInstance from './axiosInstance';

export type UpdateUserPayload = {
  email?: string | null;
};

export async function updateUser(userId: number, body: UpdateUserPayload): Promise<void> {
  await axiosInstance.put(`/Users/${userId}`, body);
}
