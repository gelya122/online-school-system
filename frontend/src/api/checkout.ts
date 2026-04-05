import axiosInstance from './axiosInstance';

export type CheckoutLine = {
  courseId: number;
  instanceId?: number | null;
  quantity?: number;
};

export type CheckoutRequest = {
  userId: number;
  methodId?: number | null;
  promoCode?: string | null;
  /** Рассрочка: на бэкенде создаются installment_plan и installment_payment */
  useInstallment?: boolean;
  installmentCount?: number | null;
  items: CheckoutLine[];
};

export type CheckoutResponse = {
  orderId: number;
  orderNumber: string;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  promoCodeId?: number | null;
  promoMessage?: string | null;
};

export async function postCheckout(body: CheckoutRequest): Promise<CheckoutResponse> {
  const res = await axiosInstance.post<CheckoutResponse>('/Checkout', body);
  return res.data;
}
