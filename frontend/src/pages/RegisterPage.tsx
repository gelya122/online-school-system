import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { isValidEmailFormat } from '../utils/emailValidation';
import { MaskedPhoneInput } from '../components/MaskedPhoneInput';
import { isRuPhoneComplete, ruPhoneToStoredString } from '../utils/phoneMask';
import './LoginPage.css';

const MAX_AVATAR_BYTES = 512 * 1024;

const RegisterPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [classNumber, setClassNumber] = useState(9);
  const [phoneDigits, setPhoneDigits] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [parentPhoneDigits, setParentPhoneDigits] = useState('');
  const [parentEmail, setParentEmail] = useState('');
  const [avatarBase64, setAvatarBase64] = useState<string | null>(null);
  const [avatarName, setAvatarName] = useState('');
  const [emailCheck, setEmailCheck] = useState<'idle' | 'ok' | 'bad'>('idle');
  const [parentEmailCheck, setParentEmailCheck] = useState<'idle' | 'ok' | 'bad'>('idle');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const runStudentEmailCheck = (value: string) => {
    const t = value.trim();
    if (!t) {
      setEmailCheck('idle');
      return;
    }
    setEmailCheck(isValidEmailFormat(t) ? 'ok' : 'bad');
  };

  const runParentEmailCheck = (value: string) => {
    const t = value.trim();
    if (!t) {
      setParentEmailCheck('idle');
      return;
    }
    setParentEmailCheck(isValidEmailFormat(t) ? 'ok' : 'bad');
  };

  const onAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) {
      setAvatarBase64(null);
      setAvatarName('');
      return;
    }
    if (file.size > MAX_AVATAR_BYTES) {
      setError('Аватар не больше 512 КБ.');
      e.target.value = '';
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      const r = reader.result;
      if (typeof r === 'string') {
        setAvatarBase64(r);
        setAvatarName(file.name);
        setError('');
      }
    };
    reader.readAsDataURL(file);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const trimmed = email.trim();
      if (!isValidEmailFormat(trimmed)) {
        setEmailCheck('bad');
        setError('Введите корректный адрес электронной почты (логин).');
        setLoading(false);
        return;
      }
      setEmailCheck('ok');

      const parentTrimmed = parentEmail.trim();
      if (!isValidEmailFormat(parentTrimmed)) {
        setParentEmailCheck('bad');
        setError('Введите корректную электронную почту родителя.');
        setLoading(false);
        return;
      }
      setParentEmailCheck('ok');

      if (!isRuPhoneComplete(phoneDigits)) {
        setError('Введите полный номер телефона ученика (10 цифр после +7).');
        setLoading(false);
        return;
      }
      if (!isRuPhoneComplete(parentPhoneDigits)) {
        setError('Введите полный номер телефона родителя (10 цифр после +7).');
        setLoading(false);
        return;
      }

      await register({
        email: trimmed,
        password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        classNumber,
        phone: ruPhoneToStoredString(phoneDigits),
        dateOfBirth,
        parentPhone: ruPhoneToStoredString(parentPhoneDigits),
        parentEmail: parentTrimmed,
        avatarBase64,
      });
      navigate('/courses');
    } catch (err: unknown) {
      const ax = err as { response?: { data?: unknown } };
      const d = ax.response?.data;
      let msg = 'Ошибка регистрации.';
      if (typeof d === 'string') msg = d;
      else if (d && typeof d === 'object' && 'message' in d && typeof (d as { message?: string }).message === 'string')
        msg = (d as { message: string }).message;
      else if (err instanceof Error) msg = err.message;
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-container register-container-wide">
        <h1>Регистрация</h1>
        <form onSubmit={handleSubmit} className="login-form" noValidate>
          {error && <div className="error-message">{error}</div>}

          <div className="form-group">
            <label htmlFor="reg-first">Имя</label>
            <input
              id="reg-first"
              type="text"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              required
              placeholder="Имя"
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-last">Фамилия</label>
            <input
              id="reg-last"
              type="text"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              required
              placeholder="Фамилия"
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-email">Email (логин)</label>
            <input
              id="reg-email"
              type="text"
              inputMode="email"
              autoComplete="email"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                setEmailCheck('idle');
              }}
              onBlur={() => runStudentEmailCheck(email)}
              required
              placeholder="your@email.com"
            />
            {emailCheck === 'bad' && (
              <p className="field-error">Введите корректный адрес электронной почты.</p>
            )}
            {emailCheck === 'ok' && <p className="field-hint ok">Почта принята.</p>}
          </div>

          <div className="form-group">
            <label htmlFor="reg-password">Пароль</label>
            <input
              id="reg-password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="••••••••"
              minLength={6}
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-dob">Дата рождения</label>
            <input
              id="reg-dob"
              type="date"
              value={dateOfBirth}
              onChange={(e) => setDateOfBirth(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-phone">Телефон ученика</label>
            <MaskedPhoneInput id="reg-phone" valueDigits={phoneDigits} onDigitsChange={setPhoneDigits} />
            <p className="field-hint">Формат: +7 (000) 000-00-00 — набор с цифр после «+7 (».</p>
          </div>

          <div className="form-group">
            <label htmlFor="reg-class">Класс (номер)</label>
            <input
              id="reg-class"
              type="number"
              min={1}
              max={11}
              value={classNumber}
              onChange={(e) => setClassNumber(Number(e.target.value) || 1)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="reg-parent-phone">Телефон родителя</label>
            <MaskedPhoneInput
              id="reg-parent-phone"
              valueDigits={parentPhoneDigits}
              onDigitsChange={setParentPhoneDigits}
            />
            <p className="field-hint">Формат: +7 (000) 000-00-00</p>
          </div>

          <div className="form-group">
            <label htmlFor="reg-parent-email">Почта родителя</label>
            <input
              id="reg-parent-email"
              type="text"
              inputMode="email"
              autoComplete="email"
              value={parentEmail}
              onChange={(e) => {
                setParentEmail(e.target.value);
                setParentEmailCheck('idle');
              }}
              onBlur={() => runParentEmailCheck(parentEmail)}
              required
              placeholder="parent@email.com"
            />
            {parentEmailCheck === 'bad' && (
              <p className="field-error">Введите корректный адрес электронной почты.</p>
            )}
            {parentEmailCheck === 'ok' && <p className="field-hint ok">Почта принята.</p>}
          </div>

          <div className="form-group">
            <label htmlFor="reg-avatar">Аватар (необязательно, до 512 КБ)</label>
            <input id="reg-avatar" type="file" accept="image/jpeg,image/png,image/gif,image/webp" onChange={onAvatarChange} />
            {avatarName && <p className="field-hint ok">Выбран файл: {avatarName}</p>}
          </div>

          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Регистрация...' : 'Зарегистрироваться'}
          </button>
        </form>

        <p className="register-link">
          Уже есть аккаунт? <Link to="/login">Войти</Link>
        </p>
      </div>
    </div>
  );
};

export default RegisterPage;
