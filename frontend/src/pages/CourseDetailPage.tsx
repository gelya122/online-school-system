import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import type { Course } from '../types';
import { getCourse } from '../api/courses';
import { isValidEmailFormat } from '../utils/emailValidation';
import { MaskedPhoneInput } from '../components/MaskedPhoneInput';
import { isRuPhoneComplete } from '../utils/phoneMask';
import './CourseDetailPage.css';

const CourseDetailPage = () => {
  const { id } = useParams();
  const [course, setCourse] = useState<Course | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [leadName, setLeadName] = useState('');
  const [leadEmail, setLeadEmail] = useState('');
  const [leadPhoneDigits, setLeadPhoneDigits] = useState('');
  const [leadFormError, setLeadFormError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const courseId = id ? Number(id) : NaN;
        if (!courseId || Number.isNaN(courseId)) {
          throw new Error('Некорректный id курса');
        }

        const data = await getCourse(courseId);
        if (cancelled) return;
        setCourse(data);
      } catch (e: unknown) {
        if (cancelled) return;
        const message = e instanceof Error ? e.message : 'Не удалось загрузить курс';
        setError(message);
        setCourse(null);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  if (loading) {
    return <div className="course-detail-page">Загрузка курса...</div>;
  }

  if (error || !course) {
    return <div className="course-detail-page">{error ?? 'Курс не найден'}</div>;
  }

  return (
    <div className="course-detail-page">
      <div className="course-detail-hero">
        <div className="course-detail-hero-inner">
          <div className="course-detail-hero-text">
            <span className="badge">Разработка</span>
            <h1>{course.title}</h1>
            <p>
              {course.description ?? 'Описание курса будет отображаться здесь.'}
            </p>
            <div className="course-detail-meta-row">
              <span className="pill">12 месяцев</span>
              <span className="pill">120 уроков</span>
              <span className="pill">4.9 ★ (1500+ отзывов)</span>
            </div>
            <div className="course-detail-price-row">
              <div className="price-main">{course.price !== undefined ? `${course.price} ₽` : '—'}</div>
              <Link to="/cart" className="btn btn-primary">
                В корзину
              </Link>
            </div>
          </div>

          <div className="course-detail-side-card">
            <h3>Оставить заявку на курс</h3>
            <form
              className="course-detail-form"
              noValidate
              onSubmit={(e) => {
                e.preventDefault();
                setLeadFormError(null);
                if (!leadName.trim()) {
                  setLeadFormError('Введите имя');
                  return;
                }
                if (!isValidEmailFormat(leadEmail)) {
                  setLeadFormError('Введите корректный адрес электронной почты');
                  return;
                }
                if (!isRuPhoneComplete(leadPhoneDigits)) {
                  setLeadFormError('Введите полный номер телефона (+7 (000) 000-00-00)');
                  return;
                }
                setLeadFormError(null);
              }}
            >
              <input
                type="text"
                placeholder="Ваше имя"
                value={leadName}
                onChange={(e) => setLeadName(e.target.value)}
              />
              <input
                type="text"
                inputMode="email"
                autoComplete="email"
                placeholder="Email"
                value={leadEmail}
                onChange={(e) => setLeadEmail(e.target.value)}
              />
              <MaskedPhoneInput
                id="course-lead-phone"
                valueDigits={leadPhoneDigits}
                onDigitsChange={setLeadPhoneDigits}
              />
              <p className="course-detail-phone-hint">Формат: +7 (000) 000-00-00</p>
              {leadFormError && <p className="course-detail-form-error">{leadFormError}</p>}
              <button type="submit" className="btn btn-primary btn-full">
                Отправить заявку
              </button>
            </form>
            <p className="course-detail-form-hint">
              Нажимая кнопку, вы соглашаетесь с обработкой персональных данных.
            </p>
          </div>
        </div>
      </div>

      <div className="course-detail-body">
        <div className="course-columns">
          <section className="course-info-main">
            <h2>Чему вы научитесь</h2>
            <ul className="check-list">
              <li>Верстать адаптивные сайты на HTML и CSS.</li>
              <li>Программировать на JavaScript и работать с React.</li>
              <li>Взаимодействовать с API и отправлять запросы на сервер.</li>
              <li>Работать с Git и публиковать проекты.</li>
            </ul>

            <h2>Программа обучения</h2>
            <div className="modules-list">
              <div className="module-item active">
                <div className="module-header">
                  <span>Модуль 1</span>
                  <strong>Основы веб‑разработки</strong>
                </div>
                <p>Знакомство с технологиями, настройка окружения, первые страницы и стили.</p>
              </div>
              <div className="module-item">
                <div className="module-header">
                  <span>Модуль 2</span>
                  <strong>JavaScript для начинающих</strong>
                </div>
              </div>
              <div className="module-item">
                <div className="module-header">
                  <span>Модуль 3</span>
                  <strong>React и современные фреймворки</strong>
                </div>
              </div>
            </div>
          </section>

          <aside className="course-info-side">
            <div className="help-card">
              <h3>Нужна помощь?</h3>
              <p>Наставник ответит на ваши вопросы и поможет подобрать курс.</p>
              <button type="button" className="btn btn-outline btn-full">
                Написать наставнику
              </button>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
};

export default CourseDetailPage;

