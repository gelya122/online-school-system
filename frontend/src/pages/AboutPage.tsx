import { useEffect, useMemo, useState } from 'react';
import { useLocation } from 'react-router-dom';
import type { Employee, FaqItem, Review, SchoolSetting } from '../types';
import { getEmployees } from '../api/employees';
import { getFaqItems } from '../api/faq';
import { getReviews } from '../api/reviews';
import { getSchoolSettings } from '../api/schoolSettings';
import './AboutPage.css';

const AboutPage = () => {
  const location = useLocation();
  const [schoolSettings, setSchoolSettings] = useState<SchoolSetting | null>(null);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [faqItems, setFaqItems] = useState<FaqItem[]>([]);
  const [reviews, setReviews] = useState<Review[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeFaq, setActiveFaq] = useState<number | null>(null); // Все закрыты по умолчанию

// Форматирование телефона по маске 8 (...) ...-..-..
  const formatPhoneNumber = (phone: string): string => {
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 11) {
      return `8 (${digits.slice(1, 4)}) ${digits.slice(4, 7)}-${digits.slice(7, 9)}-${digits.slice(9, 11)}`;
    }
    if (digits.length === 10) {
      return `8 (${digits.slice(0, 3)}) ${digits.slice(3, 6)}-${digits.slice(6, 8)}-${digits.slice(8, 10)}`;
    }
    return phone;
  };

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const [s, e, f, r] = await Promise.all([
          getSchoolSettings(),
          getEmployees(),
          getFaqItems(),
          getReviews(),
        ]);

        if (cancelled) return;

        setSchoolSettings(s);

        setEmployees((e ?? []).filter((x) => x.isActive === undefined || x.isActive));

        const sortedFaq = (f ?? [])
          .slice()
          .sort((a, b) => (a.order ?? 0) - (b.order ?? 0));
        setFaqItems(sortedFaq);
        setActiveFaq(null);

        const visibleReviews = (r ?? []).filter((x) => x.isPublished === undefined || x.isPublished);
        visibleReviews.sort((a, b) => {
          const ta = a.createdAt ? new Date(a.createdAt).getTime() : 0;
          const tb = b.createdAt ? new Date(b.createdAt).getTime() : 0;
          return tb - ta;
        });
        setReviews(visibleReviews);
      } catch (err: unknown) {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Не удалось загрузить данные');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  // Чтобы при переходах из футера/ссылок происходил корректный скролл
  useEffect(() => {
    if (loading) return;

    if (location.hash) {
      const id = location.hash.replace('#', '');
      const el = document.getElementById(id);
      if (el) el.scrollIntoView({ behavior: 'auto', block: 'start' });
      else window.scrollTo(0, 0);
    } else {
      window.scrollTo(0, 0);
    }
  }, [location.hash, location.pathname, loading]);

  const aboutText = schoolSettings?.aboutSchoolText?.trim() || '';

  const aboutParagraphs = useMemo(() => {
    if (!aboutText) return [];
    return aboutText.split('\n').map((p) => p.trim()).filter(Boolean);
  }, [aboutText]);

  return (
    <div className="about-page">
      <div className="about-hero">
        <div className="about-hero-inner" id="about-article">
          <h1>О школе EduSchool</h1>
          {aboutParagraphs.length > 0 ? (
            aboutParagraphs.map((p, idx) => <p key={idx}>{p}</p>)
          ) : (
            <p>Текст о школе будет отображаться здесь.</p>
          )}
        </div>
      </div>

      <div className="about-body">
        {/* Преподаватели */}
        <section className="about-section" id="about-teachers">
          <h2>Преподаватели</h2>
          {employees.length === 0 ? (
            <p>Преподаватели пока не добавлены.</p>
          ) : (
            <div className="teacher-grid">
              {employees.slice(0, 12).map((t) => {
                const fio = [t.firstName, t.lastName, t.patronymic].filter(Boolean).join(' ');
                return (
                  <div key={t.id} className="teacher-card">
                    {t.avatarUrl ? (
                      <img src={t.avatarUrl} alt={fio} className="teacher-avatar" />
                    ) : (
                      <div className="teacher-avatar teacher-avatar--placeholder" />
                    )}

                    <div className="teacher-info">
                      <div className="teacher-fio">{fio}</div>
                      <div className="teacher-meta">
                        {typeof t.workExperience === 'number' ? (
                          <span>Стаж: {t.workExperience} лет</span>
                        ) : (
                          <span>Стаж: —</span>
                        )}
                      </div>
                      <div className="teacher-desc">
                        {t.phone ? `Контакты: ${t.phone}` : 'Описание будет отображаться здесь.'}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </section>

        {/* FAQ */}
        <section className="about-section about-section--faq">
          <h2>FAQ</h2>
          {loading ? (
            <p>Загрузка FAQ...</p>
          ) : error ? (
            <p>{error}</p>
          ) : faqItems.length === 0 ? (
            <p>FAQ пока не добавлен</p>
          ) : (
            <ul className="about-faq-list">
              {faqItems.map((item, index) => (
                <li key={item.id ?? `faq-${index}`} className="about-faq-item">
                  <button
                    type="button"
                    className={`about-faq-question ${activeFaq === index ? 'open' : ''}`}
                    onClick={() => setActiveFaq(activeFaq === index ? null : index)}
                  >
                    <span className="about-faq-question-text">{item.question}</span>
                    <span className="about-faq-toggle">{activeFaq === index ? '−' : '+'}</span>
                  </button>
                  {activeFaq === index && <div className="about-faq-answer">{item.answer}</div>}
                </li>
              ))}
            </ul>
          )}
        </section>

        {/* Отзывы */}
        <section className="about-section about-section--reviews" id="about-reviews">
          <h2>Отзывы учеников</h2>
          {loading ? (
            <p>Загрузка отзывов...</p>
          ) : reviews.length === 0 ? (
            <p>Отзывов пока нет</p>
          ) : (
            <div className="review-grid">
              {reviews.slice(0, 6).map((rev) => {
                const starsCount = Math.max(0, Math.min(5, Math.round(rev.rating ?? 0)));
                const stars = '★'.repeat(starsCount) + '☆'.repeat(5 - starsCount);
                return (
                  <div key={rev.id} className="review-card">
                    <div className="review-stars">{stars}</div>
                    <div className="review-comment">"{rev.comment ?? '—'}"</div>
                    <div className="review-author">
                      <strong>Студент</strong>
                      </div>
                  </div>
                );
              })}
            </div>
          )}
        </section>

        {/* Документы */}
        <section className="about-section about-section--documents">
          <h2>Документы</h2>
          <div className="documents-list">
            <a
              className="document-link"
              href={schoolSettings?.privacyPolicyUrl ?? '#'}
              target="_blank"
              rel="noreferrer"
            >
              Политика конфиденциальности
            </a>
            <a
              className="document-link"
              href={schoolSettings?.termsOfUseUrl ?? '#'}
              target="_blank"
              rel="noreferrer"
            >
              Договор оферты
            </a>
          </div>
        </section>
      </div>
    </div>
  );
};

export default AboutPage;

