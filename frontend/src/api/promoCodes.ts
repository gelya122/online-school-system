import axiosInstance from './axiosInstance';

export type ValidatePromoCodeResponse = {
  isValid: boolean;
  message?: string;
  discountAmount: number;
};

export async function validatePromoCode(code: string, cartTotal: number) {
  const res = await axiosInstance.post<ValidatePromoCodeResponse>('/PromoCodes/validate', {
    code,
    cartTotal,
  });
  return res.data;
}

