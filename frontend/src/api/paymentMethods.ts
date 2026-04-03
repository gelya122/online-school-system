import axiosInstance from './axiosInstance';

export async function getPaymentMethods(): Promise<string[]> {
  const res = await axiosInstance.get<string[]>('/PaymentMethods');
  return res.data;
}

