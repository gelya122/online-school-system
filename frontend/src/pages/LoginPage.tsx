import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { isValidEmailFormat } from '../utils/emailValidation';
import './LoginPage.css';

const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!isValidEmailFormat(email)) {
      setError('Введите корректный адрес электронной почты.');
      return;
    }
    setLoading(true);

    try {
      await login({ email: email.trim(), password });
      navigate('/profile');
    } catch (err: unknown) {
      const ax = err as { response?: { data?: unknown } };
      const d = ax.response?.data;
      let msg = 'Ошибка входа. Проверьте данные.';
      if (typeof d === 'string') msg = d;
      else if (err instanceof Error) msg = err.message;
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-container">
        <h1>Вход в систему</h1>
        <form onSubmit={handleSubmit} className="login-form" noValidate>
          {error && <div className="error-message">{error}</div>}
          
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="text"
              inputMode="email"
              autoComplete="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="your@email.com"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Пароль</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="••••••••"
            />
          </div>

          <button type="submit" className="btn btn-primary" disabled={loading}>
            {loading ? 'Вход...' : 'Войти'}
          </button>
        </form>

        <p className="register-link">
          Нет аккаунта? <Link to="/register">Зарегистрироваться</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginPage;

