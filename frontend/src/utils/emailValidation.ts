/**
 * Клиентская проверка в духе серверного EmailValidator (структура + домен с точкой).
 * Окончательное решение — всё равно на сервере при POST.
 */
export function isValidEmailFormat(email: string): boolean {
  const trimmed = email.trim();
  if (!trimmed || trimmed.length > 254) return false;

  const at = trimmed.lastIndexOf('@');
  if (at <= 0 || at >= trimmed.length - 1) return false;

  const local = trimmed.slice(0, at);
  const domain = trimmed.slice(at + 1);
  if (!local || !domain || local.length > 64 || domain.length > 253) return false;
  if (!domain.includes('.')) return false;
  if (/\s/.test(trimmed)) return false;

  const parts = domain.split('.');
  const tld = parts[parts.length - 1];
  if (!tld || tld.length < 2) return false;
  if (domain.startsWith('.') || domain.endsWith('.')) return false;

  return true;
}
