import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import type { Course, Employee, Exam, FaqItem, Review } from '../types';
import { getCourses } from '../api/courses';
import { getEmployees } from '../api/employees';
import { getFaqItems } from '../api/faq';
import { getReviews } from '../api/reviews';
import { getExams } from '../api/filters';
import { TrialLeadForm } from '../components/TrialLeadForm';
import './HomePage.css';

const HomePage = () => {
  const [faqItems, setFaqItems] = useState<FaqItem[]>([]);
  const [activeFaq, setActiveFaq] = useState<number | null>(null);
  const [loadingFaq, setLoadingFaq] = useState(true);
  const [faqError, setFaqError] = useState<string | null>(null);

  const [reviews, setReviews] = useState<Review[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(true);
  const [reviewsError, setReviewsError] = useState<string | null>(null);

  const [homeCourses, setHomeCourses] = useState<Course[]>([]);
  const [exams, setExams] = useState<Exam[]>([]);
  const [loadingCatalog, setLoadingCatalog] = useState(true);

  const [teachers, setTeachers] = useState<Employee[]>([]);

  const examNameById = useMemo(() => {
    const m = new Map<number, string>();
    exams.forEach((e) => m.set(e.id, e.name));
    return m;
  }, [exams]);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const items = await getFaqItems();
        items.sort((a, b) => (a.order ?? 0) - (b.order ?? 0));

        if (cancelled) return;
        setFaqItems(items);
        setActiveFaq(null);
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

  /** Курсы, экзамены и преподаватели для блоков на главной */
  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const [coursesData, examsData, employeesData] = await Promise.all([
          getCourses(),
          getExams(),
          getEmployees(),
        ]);
        if (cancelled) return;
        setHomeCourses(coursesData.slice(0, 6));
        setExams((examsData ?? []).filter((e) => e.isActive !== false));
        const activeTeachers = (employeesData ?? []).filter((x) => x.isActive === undefined || x.isActive);
        setTeachers(activeTeachers.slice(0, 4));
      } catch {
        if (cancelled) return;
        setHomeCourses([]);
        setExams([]);
        setTeachers([]);
      } finally {
        if (!cancelled) setLoadingCatalog(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="home-page">
      {/* ——— Hero: УТП, CTA, форма заявки ——— */}
      <section className="home-hero">
        <div className="home-container home-hero-grid">
          <div className="home-hero-copy">
            <p className="home-kicker">Онлайн-школа · ОГЭ и ЕГЭ</p>
            <h1 className="home-hero-title">
              Готовьтесь к экзаменам спокойно и системно
            </h1>
            <p className="home-hero-lead">
              Живая работа с преподавателем, понятная программа и материалы под актуальные требования ФИПИ — в одном
              месте.
            </p>
            <div className="home-hero-actions">
              <Link to="/courses" className="home-btn home-btn--primary home-btn--hero-cta">
                Начать обучение
              </Link>
              <a href="#leave-request" className="home-btn home-btn--secondary">
                Оставить заявку
              </a>
            </div>
          </div>

          <TrialLeadForm anchorId="leave-request" phoneInputId="lead-phone" />
        </div>
      </section>

      {/* ——— Преимущества ——— */}
      <section className="home-section home-advantages">
        <div className="home-container">
          <header className="home-section-head">
            <h2 className="home-section-title">Почему EduSchool</h2>
            <p className="home-section-desc">
              Минимум суеты, максимум ясности: вы всегда понимаете, что учить дальше и зачем.
            </p>
          </header>
          <div className="home-feature-grid">
            {[
              { icon: '◆', title: 'Структура', text: 'Пошаговая программа от базы до экзаменационных задач.' },
              { icon: '◇', title: 'Преподаватели', text: 'Опыт наставников, которые умеют объяснять сложное просто.' },
              { icon: '○', title: 'Обратная связь', text: 'Разбор ошибок и рекомендации, куда двигаться дальше.' },
              { icon: '◎', title: 'Гибкий формат', text: 'Учитесь из дома в удобное время, без лишней дороги.' },
              { icon: '△', title: 'Актуальные задания', text: 'Ориентир на формат ФИПИ и реальные экзамены.' },
              { icon: '▽', title: 'Личный кабинет', text: 'Материалы, прогресс и домашние задания в одном месте.' },
            ].map((item) => (
              <article key={item.title} className="home-feature-card">
                <span className="home-feature-icon" aria-hidden>
                  {item.icon}
                </span>
                <h3 className="home-feature-title">{item.title}</h3>
                <p className="home-feature-text">{item.text}</p>
              </article>
            ))}
          </div>
        </div>
      </section>

      {/* ——— Курсы (превью каталога) ——— */}
      <section className="home-section home-courses-preview">
        <div className="home-container">
          <header className="home-section-head home-section-head--row">
            <div>
              <h2 className="home-section-title">Популярные курсы</h2>
              <p className="home-section-desc">Выберите направление и уровень — детали и оплата на странице курса.</p>
            </div>
            <Link to="/courses" className="home-link-arrow">
              Все курсы →
            </Link>
          </header>

          {loadingCatalog ? (
            <p className="home-muted">Загрузка курсов…</p>
          ) : homeCourses.length === 0 ? (
            <p className="home-muted">Курсы скоро появятся в каталоге.</p>
          ) : (
            <div className="home-course-grid">
              {homeCourses.map((course) => {
                const level = course.examId ? examNameById.get(course.examId) ?? 'Экзамен' : 'Курс';
                return (
                  <article key={course.id} className="home-course-card">
                    <div className="home-course-card-visual">
                      {course.imageUrl ? (
                        <img src={course.imageUrl} alt="" className="home-course-img" />
                      ) : (
                        <div className="home-course-placeholder" aria-hidden />
                      )}
                      <span className="home-course-badge">{level}</span>
                    </div>
                    <div className="home-course-body">
                      <h3 className="home-course-name">{course.title}</h3>
                      {course.description && <p className="home-course-desc">{course.description}</p>}
                      <div className="home-course-meta">
                        {course.price !== undefined && (
                          <span className="home-course-price">{course.price.toLocaleString('ru-RU')} ₽</span>
                        )}
                        <Link to={`/courses/${course.id}`} className="home-course-link">
                          Подробнее
                        </Link>
                      </div>
                    </div>
                  </article>
                );
              })}
            </div>
          )}
        </div>
      </section>

      {/* ——— О школе (кратко) ——— */}
      <section className="home-section home-about-teaser">
        <div className="home-container">
          <div className="home-about-inner">
            <h2 className="home-section-title">Миссия школы</h2>
            <p className="home-about-text">
              Мы помогаем школьникам уверенно сдать ОГЭ и ЕГЭ: сочетаем методичность, человеческую поддержку и
              современный онлайн-формат. Наша цель — чтобы экзамен не казался «чёрным ящиком», а был предсказуемым
              этапом на пути к цели.
            </p>
            <Link to="/about" className="home-btn home-btn--outline">
              Узнать больше о школе
            </Link>
          </div>
        </div>
      </section>

      {/* ——— Отзывы ——— */}
      <section className="home-section home-testimonials">
        <div className="home-container">
          <header className="home-section-head">
            <h2 className="home-section-title">Отзывы студентов</h2>
            <p className="home-section-desc">Честные впечатления тех, кто уже занимался с нами.</p>
          </header>

          {loadingReviews ? (
            <p className="home-muted">Загрузка отзывов…</p>
          ) : reviewsError ? (
            <p className="home-muted">{reviewsError}</p>
          ) : reviews.length === 0 ? (
            <p className="home-muted">Отзывов пока нет — скоро добавим первые истории успеха.</p>
          ) : (
            <div className="home-review-grid">
              {reviews.slice(0, 3).map((review) => {
                const ratingValue = typeof review.rating === 'number' ? review.rating : 0;
                const starsCount = Math.max(0, Math.min(5, Math.round(ratingValue)));
                const starsFilled = '★'.repeat(starsCount);
                const starsEmpty = starsCount < 5 ? '☆'.repeat(5 - starsCount) : '';
                return (
                  <article key={review.id ?? review.courseId} className="home-review-card">
                    <div className="home-review-head">
                      <div className="home-review-avatar" aria-hidden>
                        С
                      </div>
                      <div className="home-review-person">
                        <span className="home-review-name">Студент</span>
                        <span className="home-review-role">Выпускник курса</span>
                      </div>
                    </div>
                    <blockquote className="home-review-quote">«{review.comment ?? '—'}»</blockquote>
                    <div className="home-review-stars" aria-label={`Оценка ${starsCount} из 5`}>
                      {starsFilled}
                      {starsEmpty}
                    </div>
                  </article>
                );
              })}
            </div>
          )}
        </div>
      </section>

      {/* ——— Преподаватели ——— */}
      <section className="home-section home-teachers">
        <div className="home-container">
          <header className="home-section-head home-section-head--row">
            <div>
              <h2 className="home-section-title">Преподаватели</h2>
              <p className="home-section-desc">Команда, с которой приятно готовиться к серьёзным целям.</p>
            </div>
            <Link to="/about#about-teachers" className="home-link-arrow">
              Все наставники →
            </Link>
          </header>

          {teachers.length === 0 ? (
            <p className="home-muted">Список преподавателей появится здесь после загрузки из админки.</p>
          ) : (
            <div className="home-teacher-grid">
              {teachers.map((t) => {
                const fio = [t.firstName, t.lastName].filter(Boolean).join(' ');
                return (
                  <article key={t.id} className="home-teacher-card">
                    {t.avatarUrl ? (
                      <img src={t.avatarUrl} alt="" className="home-teacher-photo" />
                    ) : (
                      <div className="home-teacher-photo home-teacher-photo--ph" aria-hidden />
                    )}
                    <h3 className="home-teacher-name">{fio}</h3>
                    <p className="home-teacher-meta">
                      {typeof t.workExperience === 'number' ? `Стаж ${t.workExperience} лет` : 'Преподаватель'}
                    </p>
                    <p className="home-teacher-bio">
                      {t.phone ? `Связь: ${t.phone}` : 'Эксперт в подготовке к экзаменам и сопровождении учеников.'}
                    </p>
                  </article>
                );
              })}
            </div>
          )}
        </div>
      </section>

      {/* ——— FAQ ——— */}
      <section className="home-section home-faq">
        <div className="home-container">
          <header className="home-section-head">
            <h2 className="home-section-title">Вопросы и ответы</h2>
            <p className="home-section-desc">Коротко о формате, оплате и старте обучения.</p>
          </header>

          <div className="home-faq-layout">
            <div className="home-faq-aside">
              <h3 className="home-faq-aside-title">Нужна консультация?</h3>
              <p className="home-faq-aside-text">Поможем подобрать курс и ответим на любые вопросы.</p>
              <a href="tel:+74951234567" className="home-faq-phone">
                +7 (495) 123-45-67
              </a>
            </div>

            <div className="faq-list-container">
              {loadingFaq ? (
                <p className="home-muted">Загрузка вопросов…</p>
              ) : faqError ? (
                <p className="home-muted">{faqError}</p>
              ) : faqItems.length === 0 ? (
                <p className="home-muted">Раздел FAQ скоро наполнится.</p>
              ) : (
                <ul className="faq-list">
                  {faqItems.map((item, index) => (
                    <li key={item.id ?? `faq-${index}`} className="faq-item">
                      <button
                        type="button"
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

      {/* ——— Финальный CTA ——— */}
      <section className="home-section home-cta">
        <div className="home-container">
          <div className="home-cta-panel">
            <h2 className="home-cta-title">Первый шаг к уверенной сдаче</h2>
            <p className="home-cta-lead">Пробное занятие — чтобы оценить уровень и познакомиться с форматом.</p>
            <div className="home-cta-actions">
              <Link to="/courses" className="home-btn home-btn--on-dark">
                Смотреть курсы
              </Link>
              <a href="#leave-request" className="home-btn home-btn--ghost-on-dark">
                Записаться →
              </a>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;
