import type { PaymentMethodOption } from '../api/paymentMethods';

export type PaymentMethodKind = 'card' | 'sbp' | 'installment' | 'other';

/** Определяет тип способа оплаты по названию и описанию из БД (без отдельного поля в таблице). */
export function getPaymentMethodKind(
  methodName: string,
  description?: string | null,
): PaymentMethodKind {
  const text = `${methodName} ${description ?? ''}`.toLowerCase();

  if (/рассроч|installment|частями/.test(text)) return 'installment';

  if (/сбп|система быстрых|sbp|qr[\s-]?код|нспк/.test(text)) return 'sbp';

  if (/карт|card|банковск|visa|master|мир/.test(text)) return 'card';

  return 'other';
}

/** По умолчанию — первая «карта», иначе первый способ из списка. */
export function pickDefaultPaymentMethod(methods: PaymentMethodOption[]): PaymentMethodOption | null {
  if (!methods.length) return null;
  const card = methods.find((m) => getPaymentMethodKind(m.methodName, m.description) === 'card');
  return card ?? methods[0] ?? null;
}
