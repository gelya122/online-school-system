/** URL статики API (аватары и т.п.): в dev через прокси Vite достаточно относительного пути. */
export function publicApiFileUrl(path: string | null | undefined): string | undefined {
  if (!path) return undefined;
  if (/^https?:\/\//i.test(path)) return path;
  const base = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '');
  if (base) return `${base}${path.startsWith('/') ? path : `/${path}`}`;
  return path;
}
