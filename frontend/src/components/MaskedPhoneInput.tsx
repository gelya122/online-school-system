import { formatRuMobileMask, extractRuMobileDigits } from '../utils/phoneMask';

type MaskedPhoneInputProps = {
  id: string;
  valueDigits: string;
  onDigitsChange: (digits: string) => void;
  disabled?: boolean;
  className?: string;
  placeholder?: string;
  'aria-invalid'?: boolean | 'true' | 'false';
};

/**
 * Телефон РФ: отображение +7 (…) …; накопление 10 цифр после кода страны.
 * Нативный required не используем — проверяйте длину в обработчике формы.
 */
export function MaskedPhoneInput({
  id,
  valueDigits,
  onDigitsChange,
  disabled,
  className,
  placeholder = '+7 (000) 000-00-00',
  'aria-invalid': ariaInvalid,
}: MaskedPhoneInputProps) {
  return (
    <input
      id={id}
      type="tel"
      inputMode="numeric"
      autoComplete="tel"
      disabled={disabled}
      className={className}
      title="Формат: +7 (000) 000-00-00"
      placeholder={placeholder}
      value={formatRuMobileMask(valueDigits)}
      onChange={(e) => onDigitsChange(extractRuMobileDigits(e.target.value))}
      aria-invalid={ariaInvalid}
    />
  );
}
