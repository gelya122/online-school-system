import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react';
import type { Subject } from '../types';
import { getSubjects } from '../api/filters';
import { createTrialApplication } from '../api/trialApplications';
import { isValidEmailFormat } from '../utils/emailValidation';
import { MaskedPhoneInput } from './MaskedPhoneInput';
import { isRuPhoneComplete, ruPhoneToStoredString } from '../utils/phoneMask';
import '../pages/HomePage.css';

export type TrialLeadFormProps = {
  phoneInputId: string;
  /** Если указан — уходит в заявку вместе с предметами. */
  courseId?: number;
  /** Id предметов, которые будут отмечены при первой загрузке списка (например предмет открытого курса). */
  initialSubjectIds?: number[];
  anchorId?: string;
};

export function TrialLeadForm({ phoneInputId, courseId, initialSubjectIds, anchorId }: TrialLeadFormProps) {
  const [leadLastName, setLeadLastName] = useState('');
  const [leadFirstName, setLeadFirstName] = useState('');
  const [leadEmail, setLeadEmail] = useState('');
  const [leadPhoneDigits, setLeadPhoneDigits] = useState('');
  const [leadClassNumber, setLeadClassNumber] = useState('');

  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [loadingLeadSubjects, setLoadingLeadSubjects] = useState(true);
  const [selectedSubjectIds, setSelectedSubjectIds] = useState<number[]>([]);
  const [subjectsOpen, setSubjectsOpen] = useState(false);
  const subjectsDropdownRef = useRef<HTMLDivElement | null>(null);

  const [leadSubmitting, setLeadSubmitting] = useState(false);
  const [leadError, setLeadError] = useState<string | null>(null);
  const [leadSuccess, setLeadSuccess] = useState(false);

  const seedTargetRef = useRef<string>('');

  const initialKey = useMemo(
    () => (initialSubjectIds ?? []).slice().sort((a, b) => a - b).join(','),
    [initialSubjectIds],
  );

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const data = await getSubjects();
        if (cancelled) return;
        setSubjects(data.filter((s) => s.isActive !== false));
      } catch {
        if (cancelled) return;
        setSubjects([]);
      } finally {
        if (!cancelled) setLoadingLeadSubjects(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    if (!subjects.length) return;
    const target = `${courseId ?? 'home'}:${initialKey}`;
    if (seedTargetRef.current === target) return;
    seedTargetRef.current = target;
    const valid = (initialSubjectIds ?? []).filter((id) => subjects.some((s) => s.id === id));
    setSelectedSubjectIds(valid);
  }, [subjects, courseId, initialKey, initialSubjectIds]);

  useEffect(() => {
    const onDocMouseDown = (ev: MouseEvent) => {
      if (!subjectsOpen) return;
      const el = subjectsDropdownRef.current;
      if (!el) return;
      const target = ev.target as Node | null;
      if (target && el.contains(target)) return;
      setSubjectsOpen(false);
    };

    document.addEventListener('mousedown', onDocMouseDown);
    return () => {
      document.removeEventListener('mousedown', onDocMouseDown);
    };
  }, [subjectsOpen]);

  const selectedSubjectsLabel = useMemo(() => {
    if (!selectedSubjectIds.length) return 'Выберите предмет(ы)';
    const names = subjects
      .filter((s) => selectedSubjectIds.includes(s.id))
      .map((s) => s.name)
      .filter(Boolean);
    if (!names.length) return `${selectedSubjectIds.length} выбран(о/ы)`;
    if (names.length === 1) return names[0];
    return `${names[0]} + ${names.length - 1}`;
  }, [selectedSubjectIds, subjects]);

  const handleLeadSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLeadError(null);
    setLeadSuccess(false);

    if (!leadLastName.trim()) {
      setLeadError('Введите фамилию');
      return;
    }
    if (!leadFirstName.trim()) {
      setLeadError('Введите имя');
      return;
    }
    if (!isRuPhoneComplete(leadPhoneDigits)) {
      setLeadError('Введите полный номер телефона (+7 (000) 000-00-00)');
      return;
    }

    const em = leadEmail.trim();
    if (em && !isValidEmailFormat(em)) {
      setLeadError('Введите корректный адрес электронной почты');
      return;
    }

    const classNumberNum = Number(leadClassNumber);
    if (!Number.isFinite(classNumberNum) || classNumberNum <= 0) {
      setLeadError('Введите номер класса');
      return;
    }

    if (selectedSubjectIds.length === 0) {
      setLeadError('Выберите предмет(ы)');
      return;
    }

    setLeadSubmitting(true);
    try {
      const selectedSubjectsText = subjects
        .filter((s) => selectedSubjectIds.includes(s.id))
        .map((s) => s.name)
        .filter(Boolean)
        .join(',');

      if (!selectedSubjectsText.trim()) {
        setLeadError('Выберите предмет(ы)');
        return;
      }

      await createTrialApplication({
        FirstName: leadFirstName.trim(),
        LastName: leadLastName.trim(),
        Email: em || undefined,
        Phone: ruPhoneToStoredString(leadPhoneDigits),
        ClassNumber: classNumberNum,
        SelectedSubjects: selectedSubjectsText,
        ...(courseId != null ? { CourseId: courseId } : {}),
      });

      setLeadSuccess(true);
      setLeadLastName('');
      setLeadFirstName('');
      setLeadEmail('');
      setLeadPhoneDigits('');
      setLeadClassNumber('');
      setSelectedSubjectIds([]);
    } catch (err: unknown) {
      const ax = err as { response?: { data?: unknown } };
      const d = ax.response?.data;
      let msg = 'Не удалось отправить заявку.';
      if (typeof d === 'string') msg = d;
      else if (err instanceof Error) msg = err.message;
      setLeadError(msg);
    } finally {
      setLeadSubmitting(false);
    }
  };

  return (
    <aside className="hero-card" id={anchorId}>
      <h2 className="hero-card-title">Оставить заявку</h2>
      <form className="home-lead-form" onSubmit={handleLeadSubmit}>
        <div className="form-group">
          <input
            type="text"
            placeholder="Фамилия"
            value={leadLastName}
            onChange={(e) => setLeadLastName(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <input
            type="text"
            placeholder="Имя"
            value={leadFirstName}
            onChange={(e) => setLeadFirstName(e.target.value)}
            required
          />
        </div>
        <div className="form-group">
          <input
            type="number"
            placeholder="Номер класса"
            value={leadClassNumber}
            onChange={(e) => setLeadClassNumber(e.target.value)}
            min={1}
            step={1}
            required
          />
        </div>
        <div className="form-group">
          <div className="subject-dropdown" ref={subjectsDropdownRef}>
            <button
              type="button"
              className={`subject-dropdown-btn${selectedSubjectIds.length === 0 ? ' subject-dropdown-btn--empty' : ''}`}
              disabled={loadingLeadSubjects || subjects.length === 0}
              onClick={() => {
                if (loadingLeadSubjects || subjects.length === 0) return;
                setSubjectsOpen((v) => !v);
              }}
            >
              {selectedSubjectsLabel}
            </button>

            {subjectsOpen && (
              <div className="subject-dropdown-menu">
                {subjects.map((s) => {
                  const checked = selectedSubjectIds.includes(s.id);
                  return (
                    <label key={s.id} className="subject-option">
                      <input
                        type="checkbox"
                        checked={checked}
                        disabled={loadingLeadSubjects}
                        onChange={() => {
                          setSelectedSubjectIds((prev) =>
                            prev.includes(s.id) ? prev.filter((id) => id !== s.id) : [...prev, s.id],
                          );
                        }}
                      />
                      <span>{s.name}</span>
                    </label>
                  );
                })}
              </div>
            )}
          </div>
        </div>
        <div className="form-group">
          <input
            type="text"
            inputMode="email"
            autoComplete="email"
            placeholder="Email (необязательно)"
            value={leadEmail}
            onChange={(e) => setLeadEmail(e.target.value)}
          />
        </div>
        <div className="form-group">
          <MaskedPhoneInput id={phoneInputId} valueDigits={leadPhoneDigits} onDigitsChange={setLeadPhoneDigits} />
          <p className="form-hint form-hint--inline">Формат: +7 (000) 000-00-00</p>
        </div>
        <button type="submit" className="home-btn home-btn--primary home-btn--block" disabled={leadSubmitting}>
          Записаться на курс
        </button>
        {leadError && <p className="form-hint form-hint--error">{leadError}</p>}
        {leadSuccess && <p className="form-hint form-hint--success">Заявка отправлена!</p>}
        <p className="form-hint">Нажимая кнопку, вы соглашаетесь с политикой конфиденциальности</p>
      </form>
    </aside>
  );
}
