/** Только цифры. */
export function digitsOnly(s: string): string {
  return s.replace(/\D/g, '');
}

/** Маска номера карты: группы по 4. */
export function formatCardNumberInput(raw: string): string {
  const d = digitsOnly(raw).slice(0, 19);
  const parts: string[] = [];
  for (let i = 0; i < d.length; i += 4) {
    parts.push(d.slice(i, i + 4));
  }
  return parts.join(' ');
}

/** MM/YY */
export function formatCardExpiryInput(raw: string): string {
  const d = digitsOnly(raw).slice(0, 4);
  if (d.length <= 2) return d;
  return `${d.slice(0, 2)}/${d.slice(2)}`;
}

export function isCardNumberValid(formatted: string): boolean {
  const n = digitsOnly(formatted).length;
  return n >= 16 && n <= 19;
}

export function isCardExpiryValid(formatted: string): boolean {
  const d = digitsOnly(formatted);
  if (d.length !== 4) return false;
  const mm = Number(d.slice(0, 2));
  const yy = Number(d.slice(2, 4));
  if (mm < 1 || mm > 12) return false;
  if (yy < 0 || yy > 99) return false;
  return true;
}

export function isCardCvcValid(cvc: string): boolean {
  const n = digitsOnly(cvc).length;
  return n === 3 || n === 4;
}
