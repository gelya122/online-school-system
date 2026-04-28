import axios from 'axios';
import { useEffect, useRef, useState, type ChangeEvent, type FormEvent } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getStudent, uploadStudentAvatar, type StudentRecord } from '../api/students';
import { MaskedPhoneInput } from '../components/MaskedPhoneInput';
import { extractRuMobileDigits, formatRuMobileMask, isRuPhoneComplete } from '../utils/phoneMask';
import { publicApiFileUrl } from '../utils/publicUrl';
import './ProfilePage.css';

const MAX_AVATAR_BYTES = 512 * 1024;

type ProfileDetail = {
  email: string;
  firstName: string;
  lastName: string;
  phoneDigits: string;
  parentPhoneDigits: string;
  dateOfBirth: string;
  classNumber: number;
  parentEmail: string;
};

function emptyDetailFromUser(email: string, firstName?: string, lastName?: string): ProfileDetail {
  return {
    email,
    firstName: firstName ?? '',
    lastName: lastName ?? '',
    phoneDigits: '',
    parentPhoneDigits: '',
    dateOfBirth: '',
    classNumber: 9,
    parentEmail: '',
  };
}

function formatRuDate(iso: string): string {
  if (!iso || iso.length < 10) return '—';
  const [y, m, d] = iso.slice(0, 10).split('-');
  if (!y || !m || !d) return '—';
  return `${d}.${m}.${y}`;
}

function detailFromStudent(s: StudentRecord, accountEmail: string): ProfileDetail {
  const dob = s.dateOfBirth?.slice(0, 10) ?? '';
  return {
    email: accountEmail,
    firstName: s.firstName ?? '',
    lastName: s.lastName ?? '',
    phoneDigits: extractRuMobileDigits(s.phone ?? ''),
    parentPhoneDigits: extractRuMobileDigits(s.parentPhone ?? ''),
    dateOfBirth: dob,
    classNumber: s.classNumber >= 1 && s.classNumber <= 11 ? s.classNumber : 9,
    parentEmail: s.parentEmail?.trim() ?? '',
  };
}

const ProfilePage = () => {
  const { user, saveProfile } = useAuth();
  const roleText = user?.roleLabel ?? user?.role;

  const [detail, setDetail] = useState<ProfileDetail | null>(null);
  const [draft, setDraft] = useState<ProfileDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [avatarDisplay, setAvatarDisplay] = useState<string | undefined>(user?.avatarUrl);
  const [avatarPendingDataUrl, setAvatarPendingDataUrl] = useState<string | null>(null);
  const avatarFileInputRef = useRef<HTMLInputElement>(null);

  const hasStudent = user?.studentId != null;

  useEffect(() => {
    setAvatarDisplay(user?.avatarUrl);
  }, [user?.avatarUrl]);

  useEffect(() => {
    if (!formError) return;
    document.querySelector('.profile-form-error')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }, [formError]);

  useEffect(() => {
    if (!user) {
      setDetail(null);
      setLoadingDetail(false);
      return;
    }
    let cancelled = false;

    if (!user.studentId) {
      setDetail(emptyDetailFromUser(user.email, user.firstName, user.lastName));
      setLoadError(null);
      setLoadingDetail(false);
      return () => {
        cancelled = true;
      };
    }

    setLoadingDetail(true);
    setLoadError(null);

    (async () => {
      try {
        const s = await getStudent(user.studentId!);
        if (cancelled) return;
        setDetail(detailFromStudent(s, user.email));
        const av = publicApiFileUrl(s.avatarUrl ?? undefined);
        if (av) setAvatarDisplay(av);
      } catch (err: unknown) {
        if (cancelled) return;
        const ax = err as { response?: { data?: unknown } };
        const d = ax.response?.data;
        let msg = 'Не удалось загрузить профиль.';
        if (typeof d === 'string') msg = d;
        else if (err instanceof Error) msg = err.message;
        setLoadError(msg);
        setDetail(emptyDetailFromUser(user.email, user.firstName, user.lastName));
      } finally {
        if (!cancelled) setLoadingDetail(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [user?.id, user?.studentId, user?.email]);

  const startEdit = () => {
    if (!detail) return;
    setFormError(null);
    setAvatarPendingDataUrl(null);
    if (avatarFileInputRef.current) avatarFileInputRef.current.value = '';
    setDraft({ ...detail });
    setEditing(true);
  };

  const cancelEdit = () => {
    setEditing(false);
    setDraft(null);
    setFormError(null);
    setAvatarPendingDataUrl(null);
    if (avatarFileInputRef.current) avatarFileInputRef.current.value = '';
  };

  const onAvatarFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) {
      setAvatarPendingDataUrl(null);
      return;
    }
    if (file.size > MAX_AVATAR_BYTES) {
      setFormError('Аватар не больше 512 КБ.');
      e.target.value = '';
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const r = reader.result;
      if (typeof r === 'string') {
        setAvatarPendingDataUrl(r);
        setFormError(null);
      }
    };
    reader.readAsDataURL(file);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!draft) return;
    setFormError(null);
    setSaving(true);
    try {
      if (user?.studentId) {
        const pickedFile = avatarFileInputRef.current?.files?.[0];
        if (pickedFile) {
          await uploadStudentAvatar(user.studentId, pickedFile);
        } else if (avatarPendingDataUrl) {
          const blob = await (await fetch(avatarPendingDataUrl)).blob();
          const ext = blob.type.includes('png')
            ? 'png'
            : blob.type.includes('webp')
              ? 'webp'
              : blob.type.includes('gif')
                ? 'gif'
                : 'jpg';
          await uploadStudentAvatar(
            user.studentId,
            new File([blob], `avatar.${ext}`, { type: blob.type || `image/${ext}` }),
          );
        }
      }
      await saveProfile({
        email: draft.email,
        firstName: draft.firstName,
        lastName: draft.lastName,
        phoneDigits: draft.phoneDigits,
        parentPhoneDigits: draft.parentPhoneDigits,
        dateOfBirth: draft.dateOfBirth,
        classNumber: draft.classNumber,
        parentEmail: draft.parentEmail,
      });
      setDetail({ ...draft });
      setEditing(false);
      setDraft(null);
      setAvatarPendingDataUrl(null);
      if (avatarFileInputRef.current) avatarFileInputRef.current.value = '';
    } catch (err: unknown) {
      let msg = 'Не удалось сохранить.';
      if (axios.isAxiosError(err)) {
        const d = err.response?.data;
        if (typeof d === 'string') msg = d;
        else if (d && typeof d === 'object' && 'title' in d) msg = String((d as { title?: string }).title);
        else if (err.message) msg = err.message;
      } else if (err instanceof Error) {
        msg = err.message;
      }
      setFormError(msg);
    } finally {
      setSaving(false);
    }
  };

  const headSrc = draft ?? detail;
  const showLast = headSrc?.lastName?.trim() || '—';
  const showFirst = headSrc?.firstName?.trim() || '—';
  const avatarHeaderSrc =
    editing && avatarPendingDataUrl ? avatarPendingDataUrl : avatarDisplay;

  return (
    <div className="profile-page-root">
      <div className="profile-shell">
        <div className="profile-identity">
          <div
            className={
              editing && hasStudent
                ? 'profile-avatar-wrap-center profile-avatar-wrap--editable'
                : 'profile-avatar-wrap-center'
            }
          >
            <div className="profile-avatar-ring">
              {avatarHeaderSrc ? (
                <img className="profile-avatar-xl" src={avatarHeaderSrc} alt="" />
              ) : (
                <div className="profile-avatar-xl profile-avatar-xl-fallback" aria-hidden>
                  {(detail?.firstName?.[0] ?? user?.firstName?.[0] ?? user?.email?.[0] ?? '?').toUpperCase()}
                </div>
              )}
              {editing && hasStudent && (
                <button
                  type="button"
                  className="profile-avatar-change-btn"
                  onClick={() => avatarFileInputRef.current?.click()}
                  aria-label="Изменить фото профиля"
                  title="Изменить фото"
                >
                  <svg className="profile-avatar-change-icon" viewBox="0 0 24 24" fill="none" aria-hidden>
                    <path
                      d="M23 19a2 2 0 01-2 2H3a2 2 0 01-2-2V8a2 2 0 012-2h4l2-3h6l2 3h4a2 2 0 012 2v11z"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    />
                    <circle cx="12" cy="13" r="4" stroke="currentColor" strokeWidth="2" />
                  </svg>
                </button>
              )}
            </div>
            {editing && hasStudent && (
              <>
                <input
                  ref={avatarFileInputRef}
                  type="file"
                  accept="image/jpeg,image/png,image/gif,image/webp"
                  className="profile-avatar-file-input"
                  onChange={onAvatarFileChange}
                  aria-label="Выбор файла аватара"
                />
                <p className="profile-avatar-hint">JPEG, PNG, GIF или WebP, до 512 КБ.</p>
              </>
            )}
          </div>

          <div className="profile-name-row">
            <p className="profile-name-lines">
              <span className="profile-name-surname">{showLast}</span>{' '}
              <span className="profile-name-given">{showFirst}</span>
            </p>
            {!loadingDetail && detail && !editing && (
              <button
                type="button"
                className="profile-pencil"
                onClick={startEdit}
                aria-label="Редактировать профиль"
                title="Редактировать"
              >
                <svg
                  className="profile-pencil-svg"
                  viewBox="0 0 24 24"
                  fill="none"
                  xmlns="http://www.w3.org/2000/svg"
                  aria-hidden
                >
                  <path
                    d="M12 20h9M16.5 3.5a2.12 2.12 0 013 3L7 19l-4 1 1-4L16.5 3.5z"
                    stroke="currentColor"
                    strokeWidth="2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </button>
            )}
          </div>
        </div>

        {loadError && <p className="profile-banner-error">{loadError}</p>}

        <div className="profile-data-card">
          {loadingDetail ? (
            <p className="profile-muted">Загрузка данных…</p>
          ) : !detail ? (
            <p className="profile-muted">Нет данных.</p>
          ) : editing && draft ? (
            <form onSubmit={handleSubmit} className="profile-edit-form">
              <h2 className="profile-card-title">{hasStudent ? 'Данные студента' : 'Данные профиля'}</h2>
              <div className="profile-form-grid">
                <label className="profile-field">
                  <span className="profile-field-label">Email</span>
                  <input
                    className="profile-field-input"
                    type="email"
                    autoComplete="email"
                    value={draft.email}
                    onChange={(e) => setDraft((d) => (d ? { ...d, email: e.target.value } : d))}
                    required
                  />
                </label>

                {hasStudent && (
                  <>
                    <label className="profile-field">
                      <span className="profile-field-label">Фамилия</span>
                      <input
                        className="profile-field-input"
                        type="text"
                        autoComplete="family-name"
                        value={draft.lastName}
                        onChange={(e) => setDraft((d) => (d ? { ...d, lastName: e.target.value } : d))}
                        required
                      />
                    </label>
                    <label className="profile-field">
                      <span className="profile-field-label">Имя</span>
                      <input
                        className="profile-field-input"
                        type="text"
                        autoComplete="given-name"
                        value={draft.firstName}
                        onChange={(e) => setDraft((d) => (d ? { ...d, firstName: e.target.value } : d))}
                        required
                      />
                    </label>
                    <label className="profile-field">
                      <span className="profile-field-label">Телефон</span>
                      <MaskedPhoneInput
                        id="profile-phone"
                        className="profile-field-input"
                        valueDigits={draft.phoneDigits}
                        onDigitsChange={(v) => setDraft((d) => (d ? { ...d, phoneDigits: v } : d))}
                      />
                    </label>
                    <label className="profile-field">
                      <span className="profile-field-label">Дата рождения</span>
                      <input
                        className="profile-field-input"
                        type="date"
                        value={draft.dateOfBirth}
                        onChange={(e) => setDraft((d) => (d ? { ...d, dateOfBirth: e.target.value } : d))}
                        required
                      />
                    </label>
                    <label className="profile-field">
                      <span className="profile-field-label">Класс</span>
                      <input
                        className="profile-field-input"
                        type="number"
                        min={1}
                        max={11}
                        step={1}
                        value={draft.classNumber}
                        onChange={(e) =>
                          setDraft((d) =>
                            d ? { ...d, classNumber: Number(e.target.value) || 1 } : d,
                          )
                        }
                        required
                      />
                    </label>
                    <label className="profile-field">
                      <span className="profile-field-label">Телефон родителя</span>
                      <MaskedPhoneInput
                        id="profile-parent-phone"
                        className="profile-field-input"
                        valueDigits={draft.parentPhoneDigits}
                        onDigitsChange={(v) => setDraft((d) => (d ? { ...d, parentPhoneDigits: v } : d))}
                      />
                    </label>
                    <label className="profile-field">
                      <span className="profile-field-label">Email родителя</span>
                      <input
                        className="profile-field-input"
                        type="email"
                        autoComplete="email"
                        value={draft.parentEmail}
                        onChange={(e) => setDraft((d) => (d ? { ...d, parentEmail: e.target.value } : d))}
                        required
                      />
                    </label>
                  </>
                )}

                {!hasStudent && (
                  <p className="profile-hint">
                    Профиль ученика не привязан: можно изменить только email. Имя и фамилия в системе не
                    редактируются.
                  </p>
                )}
              </div>

              {formError && <p className="profile-form-error">{formError}</p>}

              <div className="profile-save-row">
                <button type="submit" className="profile-btn-save" disabled={saving}>
                  {saving ? 'Сохранение…' : 'Сохранить'}
                </button>
                <button type="button" className="profile-btn-cancel" onClick={cancelEdit} disabled={saving}>
                  Отмена
                </button>
              </div>
            </form>
          ) : (
            <div className="profile-readonly">
              <h2 className="profile-card-title">{hasStudent ? 'Данные студента' : 'Данные профиля'}</h2>
              <div className="profile-field-row">
                <span className="profile-field-label">Email</span>
                <span className="profile-field-value">{detail.email}</span>
              </div>
              {!hasStudent && (
                <>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Фамилия</span>
                    <span className="profile-field-value">{detail.lastName?.trim() || '—'}</span>
                  </div>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Имя</span>
                    <span className="profile-field-value">{detail.firstName?.trim() || '—'}</span>
                  </div>
                </>
              )}
              {hasStudent && (
                <>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Телефон</span>
                    <span className="profile-field-value">
                      {isRuPhoneComplete(detail.phoneDigits)
                        ? formatRuMobileMask(detail.phoneDigits)
                        : '—'}
                    </span>
                  </div>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Дата рождения</span>
                    <span className="profile-field-value">
                      {detail.dateOfBirth ? formatRuDate(detail.dateOfBirth) : '—'}
                    </span>
                  </div>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Класс</span>
                    <span className="profile-field-value">{detail.classNumber}</span>
                  </div>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Телефон родителя</span>
                    <span className="profile-field-value">
                      {isRuPhoneComplete(detail.parentPhoneDigits)
                        ? formatRuMobileMask(detail.parentPhoneDigits)
                        : '—'}
                    </span>
                  </div>
                  <div className="profile-field-row">
                    <span className="profile-field-label">Email родителя</span>
                    <span className="profile-field-value">{detail.parentEmail || '—'}</span>
                  </div>
                </>
              )}
              {roleText && (
                <div className="profile-field-row profile-field-row-role">
                  <span className="profile-field-label">Роль</span>
                  <span className="profile-field-value">{roleText}</span>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
