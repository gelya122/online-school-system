import axiosInstance from './axiosInstance';

export type PaymentMethodOption = {
  methodId: number;
  methodName: string;
  description?: string | null;
};

export async function getPaymentMethods(): Promise<PaymentMethodOption[]> {
  const res = await axiosInstance.get<PaymentMethodOption[]>('/PaymentMethods');
  return res.data;
}
