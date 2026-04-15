/** Из строки ввода извлекает до 10 цифр национального номера (после +7 / 8). */
export function extractRuMobileDigits(fromDisplay: string): string {
  let d = fromDisplay.replace(/\D/g, '');
  if (d.startsWith('8')) d = '7' + d.slice(1);
  if (d.startsWith('7')) d = d.slice(1);
  return d.slice(0, 10);
}

/** Маска: +7 (XXX) XXX-XX-XX; при 0 цифр — «+7 (», ввод по сути начинается после скобки. */
export function formatRuMobileMask(digits: string): string {
  const d = digits.replace(/\D/g, '').slice(0, 10);
  if (d.length === 0) return '';

  let out = '+7 (' + d.slice(0, 3);
  if (d.length <= 3) return out;

  out += ') ' + d.slice(3, 6);
  if (d.length <= 6) return out;

  out += '-' + d.slice(6, 8);
  if (d.length <= 8) return out;

  out += '-' + d.slice(8, 10);
  return out;
}

export function isRuPhoneComplete(digits: string): boolean {
  return extractRuMobileDigits(digits).length === 10;
}

/** Строка для отправки на API / сохранения. */
export function ruPhoneToStoredString(digits: string): string {
  const d = extractRuMobileDigits(digits);
  return formatRuMobileMask(d);
}
