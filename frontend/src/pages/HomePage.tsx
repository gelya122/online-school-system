import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import type { FaqItem, Review, Subject } from '../types';
import { getFaqItems } from '../api/faq';
import { getReviews } from '../api/reviews';
import { getSubjects } from '../api/filters';
import { createTrialApplication } from '../api/trialApplications';
import { isValidEmailFormat } from '../utils/emailValidation';
import { MaskedPhoneInput } from '../components/MaskedPhoneInput';
import { isRuPhoneComplete, ruPhoneToStoredString } from '../utils/phoneMask';
import './HomePage.css';

const HomePage = () => {
  const [faqItems, setFaqItems] = useState<FaqItem[]>([]);
  const [activeFaq, setActiveFaq] = useState<number | null>(null); // Изменено: все закрыты по умолчанию
  const [loadingFaq, setLoadingFaq] = useState(true);
  const [faqError, setFaqError] = useState<string | null>(null);

  const [reviews, setReviews] = useState<Review[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(true);
  const [reviewsError, setReviewsError] = useState<string | null>(null);

  // Форма заявки на главной
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

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const items = await getFaqItems();
        items.sort((a, b) => (a.order ?? 0) - (b.order ?? 0));

        if (cancelled) return;
        setFaqItems(items);
        setActiveFaq(null); // Все вопросы закрыты по умолчанию
      } catch (error: unknown) {
        if (cancelled) return;
        const message = error instanceof Error ? error.message : 'Неизвестная ошибка';
        setFaqError(message);
        setFaqItems([]);
        setActiveFaq(null);
      } finally {
        if (!cancelled) setLoadingFaq(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

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

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const data = await getReviews();
        const visible = data.filter((r) => r.isPublished === undefined || r.isPublished);
        visible.sort((a, b) => {
          const ta = a.createdAt ? new Date(a.createdAt).getTime() : 0;
          const tb = b.createdAt ? new Date(b.createdAt).getTime() : 0;
          return tb - ta;
        });

        if (cancelled) return;
        setReviews(visible);
      } catch (e: unknown) {
        if (cancelled) return;
        const message = e instanceof Error ? e.message : 'Не удалось загрузить отзывы';
        setReviewsError(message);
        setReviews([]);
      } finally {
        if (!cancelled) setLoadingReviews(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

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
      });

      setLeadSuccess(true);
      setLeadLastName('');
      setLeadFirstName('');
      setLeadEmail('');
      setLeadPhoneDigits('');
      setLeadClassNumber('');
      setSelectedSubjectIds([]);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Не удалось отправить заявку';
      setLeadError(message);
    } finally {
      setLeadSubmitting(false);
    }
  };

  return (
    <div className="home-page">
      {/* Hero секция */}
      <section className="hero">
        <div className="hero-container">
          <div className="hero-content">
            <h1 className="hero-title">
              Подготовка к ОГЭ и ЕГЭ <span className="text-blue-600">с лучшими преподавателями</span>
            </h1>
            <p className="hero-subtitle">
              Начни подготовку к экзаменам сегодня и получи высокие баллы для поступления в вуз мечты
            </p>
            <div className="hero-actions">
              <Link to="/courses" className="btn btn-primary">Выбрать курс</Link>
            </div>
          </div>

          <div className="hero-card" id="leave-request">
            <h3 className="hero-card-title">Оставить заявку</h3>
            <form onSubmit={handleLeadSubmit}>
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
                    className="subject-dropdown-btn"
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
                                  prev.includes(s.id)
                                    ? prev.filter((id) => id !== s.id)
                                    : [...prev, s.id],
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
                <MaskedPhoneInput
                  id="lead-phone"
                  valueDigits={leadPhoneDigits}
                  onDigitsChange={setLeadPhoneDigits}
                />
                <p className="form-hint" style={{ marginTop: 6, fontSize: 13 }}>
                  Формат: +7 (000) 000-00-00
                </p>
              </div>
              <button type="submit" className="btn btn-primary btn-full" disabled={leadSubmitting}>
                Записаться на курс
              </button>
              {leadError && <p className="form-hint" style={{ color: '#ef4444' }}>{leadError}</p>}
              {leadSuccess && <p className="form-hint">Заявка отправлена!</p>}
              <p className="form-hint">
                Нажимая кнопку, вы соглашаетесь с политикой конфиденциальности
              </p>
            </form>
          </div>
        </div>
      </section>

      {/* Почему выбирают нас */}
      <section className="advantages">
        <div className="advantages-container">
          <h2 className="section-title">Почему выбирают нас?</h2>
          <p className="section-subtitle">
            Готовим к экзаменам с 2015 года. 95% наших учеников сдают ОГЭ и ЕГЭ на 80+ баллов
          </p>

          <div className="cards-grid">
            <div className="card">
              <div className="card-icon">🎯</div>
              <h3>Индивидуальный подход</h3>
              <p>Персональный план подготовки с учетом уровня знаний и целей ученика</p>
            </div>
            <div className="card">
              <div className="card-icon">👨‍🏫</div>
              <h3>Эксперты ЕГЭ и ОГЭ</h3>
              <p>Преподаватели с опытом проверки экзаменационных работ</p>
            </div>
            <div className="card">
              <div className="card-icon">📊</div>
              <h3>Актуальные материалы</h3>
              <p>Учебные пособия и задания по последним требованиям ФИПИ</p>
            </div>
            <div className="card">
              <div className="card-icon">💯</div>
              <h3>Результат гарантирован</h3>
              <p>Повышение успеваемости и высокие баллы на экзаменах</p>
            </div>
          </div>
        </div>
      </section>

      {/* FAQ секция */}
      <section className="faq">
        <div className="faq-container">
          <h2 className="section-title">Часто задаваемые вопросы</h2>
          <p className="section-subtitle">
            Ответы на самые популярные вопросы о подготовке к экзаменам
          </p>

          <div className="faq-grid">
            <div className="faq-contact">
              <h3>Остались вопросы?</h3>
              <p>Напишите нам в чат или позвоните, поможем подобрать программу подготовки</p>
              <a href="tel:+74951234567" className="faq-phone">+7 (495) 123-45-67</a>
            </div>

            <div className="faq-list-container">
              {loadingFaq ? (
                <p>Загрузка FAQ...</p>
              ) : faqError ? (
                <p>{faqError}</p>
              ) : faqItems.length === 0 ? (
                <p>FAQ пока не добавлен</p>
              ) : (
                <ul className="faq-list">
                  {faqItems.map((item, index) => (
                    <li key={item.id ?? `faq-${index}`} className="faq-item">
                      <button
                        className={`faq-question ${activeFaq === index ? 'open' : ''}`}
                        onClick={() => setActiveFaq(activeFaq === index ? null : index)}
                      >
                        {item.question}
                        <span className="faq-toggle">{activeFaq === index ? '−' : '+'}</span>
                      </button>
                      {activeFaq === index && <div className="faq-answer">{item.answer}</div>}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        </div>
      </section>

      {/* Отзывы */}
      <section className="testimonials">
        <div className="testimonials-container">
          <h2 className="testimonials-title">Отзывы учеников</h2>
          <p className="testimonials-subtitle">
            Более 5000 учеников уже выбрали нас для подготовки к экзаменам
          </p>

          {loadingReviews ? (
            <p>Загрузка отзывов...</p>
          ) : reviewsError ? (
            <p>{reviewsError}</p>
          ) : reviews.length === 0 ? (
            <p>Отзывов пока нет</p>
          ) : (
            <div className="testimonials-grid">
              {reviews.slice(0, 3).map((review) => {
                const ratingValue = typeof review.rating === 'number' ? review.rating : 0;
                const starsCount = Math.max(0, Math.min(5, Math.round(ratingValue)));
                const starsFilled = '★'.repeat(starsCount);
                const starsEmpty = starsCount < 5 ? '☆'.repeat(5 - starsCount) : '';

                return (
                  <div key={review.id ?? review.courseId} className="testimonial-card">
                    <p className="testimonial-text">"{review.comment ?? '—'}"</p>
                    <div className="stars">
                      {starsFilled}
                      {starsEmpty}
                    </div>
                    <div className="testimonial-author">
                      <strong>Студент</strong>
                      {/* Убрана строка "Курс #1" */}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </section>

      {/* CTA секция */}
      <section className="cta">
        <div className="cta-container">
          <h2>Начни подготовку к экзаменам сегодня</h2>
          <p className="cta-subtitle">Первый урок бесплатно</p>
          <p className="cta-text">
            Запишитесь на бесплатное пробное занятие и узнайте свой текущий уровень подготовки
          </p>
          <div className="cta-buttons">
            <Link to="/courses" className="btn btn-primary btn-large" role="button">
              Выбрать курс
            </Link>
            <a href="#leave-request" className="btn btn-outline btn-large" role="button">
              Записаться на пробный урок →
            </a>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;