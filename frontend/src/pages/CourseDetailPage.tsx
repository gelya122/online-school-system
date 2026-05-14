import { useEffect, useMemo, useState } from 'react';
import { useParams, Link, useLocation } from 'react-router-dom';
import type { Course } from '../types';
import { getCourse } from '../api/courses';
import { getSubjects, getExams } from '../api/filters';
import type { Subject, Exam } from '../types';
import { useCart } from '../contexts/CartContext';
import { TrialLeadForm } from '../components/TrialLeadForm';
import './CourseDetailPage.css';

function splitList(text: string | undefined): string[] {
  if (!text?.trim()) return [];
  return text
    .split(/\r?\n|;/)
    .map((s) => s.trim().replace(/^[-•*]\s*/, ''))
    .filter(Boolean);
}

const CourseDetailPage = () => {
  const { id } = useParams();
  const location = useLocation();
  const { addToCart, items } = useCart();
  const cartHasCourse = useMemo(
    () => items.some((x) => x.courseId === Number(id)),
    [items, id],
  );

  const [course, setCourse] = useState<Course | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [exams, setExams] = useState<Exam[]>([]);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const [subjectsData, examsData] = await Promise.all([getSubjects(), getExams()]);
        if (!cancelled) {
          setSubjects(subjectsData.filter((s) => s.isActive !== false));
          setExams(examsData.filter((e) => e.isActive !== false));
        }
      } catch {
        if (!cancelled) {
          setSubjects([]);
          setExams([]);
        }
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

  const badgeLabel = useMemo(() => {
    if (!course) return 'Курс';
    if (course.examId) {
      const e = exams.find((x) => x.id === course.examId);
      if (e?.name) return e.name;
    }
    if (course.subjectId) {
      const s = subjects.find((x) => x.id === course.subjectId);
      if (s?.name) return s.name;
    }
    return 'Курс';
  }, [course, exams, subjects]);

  const whatYouLearn = useMemo(() => splitList(course?.whatYouGet), [course?.whatYouGet]);

  const totalLessons = useMemo(() => {
    if (!course?.modules?.length) return 0;
    return course.modules.reduce((acc, m) => acc + (m.lessonCount ?? 0), 0);
  }, [course?.modules]);

  const leadInitialSubjectIds = useMemo(
    () => (course?.subjectId ? [course.subjectId] : undefined),
    [course?.subjectId],
  );

  const metaPills = useMemo(() => {
    if (!course) return [];
    const pills: string[] = [];
    if (course.totalHours != null && course.totalHours > 0) {
      pills.push(`${course.totalHours} ч. теории и практики`);
    }
    if (totalLessons > 0) {
      pills.push(`${totalLessons} ${pluralLessons(totalLessons)}`);
    }
    const rc = course.reviewCount ?? 0;
    const ra = course.reviewAverage;
    if (rc > 0 && ra != null && !Number.isNaN(ra)) {
      pills.push(`${ra} ★ (${rc} ${pluralReviews(rc)})`);
    }
    return pills;
  }, [course, totalLessons]);

  const showAboutBlock = Boolean(
    course?.fullDescription?.trim() &&
      course.fullDescription.trim() !== (course.description ?? '').trim(),
  );

  if (loading) {
    return (
      <div className="course-detail-page">
        <p className="course-detail-state">Загрузка курса…</p>
      </div>
    );
  }

  if (error || !course) {
    return (
      <div className="course-detail-page">
        <p className="course-detail-state course-detail-state--error">{error ?? 'Курс не найден'}</p>
      </div>
    );
  }

  const heroBlurb =
    course.description?.trim() ||
    course.fullDescription?.trim() ||
    'Описание появится после публикации курса.';

  const priceLabel =
    course.price !== undefined ? `${Number(course.price).toLocaleString('ru-RU')} ₽` : '—';

  return (
    <div className="course-detail-page">
      <div className="course-detail-hero">
        <div className="course-detail-hero-inner">
          <div className="course-detail-block course-detail-hero-card">
            <div className="course-detail-hero-text">
              <span className="badge">{badgeLabel}</span>
              <h1>{course.title}</h1>
              <p className="course-detail-hero-lead">{heroBlurb}</p>
              {metaPills.length > 0 && (
                <div className="course-detail-meta-row">
                  {metaPills.map((t) => (
                    <span key={t} className="pill">
                      {t}
                    </span>
                  ))}
                </div>
              )}
              <div className="course-detail-price-row">
                <div className="price-main">{priceLabel}</div>
                <button
                  type="button"
                  className="btn btn-primary"
                  disabled={cartHasCourse}
                  onClick={() => {
                    if (!cartHasCourse) addToCart(course);
                  }}
                >
                  {cartHasCourse ? 'В корзине' : 'В корзину'}
                </button>
                {cartHasCourse && (
                  <Link to="/cart" className="course-detail-cart-link">
                    Перейти в корзину
                  </Link>
                )}
              </div>
            </div>
          </div>

          <TrialLeadForm
            key={course.id}
            phoneInputId={`course-lead-phone-${course.id}`}
            courseId={course.id}
            initialSubjectIds={leadInitialSubjectIds}
          />
        </div>
      </div>

      <div className="course-detail-body">
        <div className="course-columns">
          <div className="course-info-main">
            {showAboutBlock && (
              <section className="course-detail-block course-detail-section">
                <h2>О курсе</h2>
                <div className="course-detail-prose">{course.fullDescription}</div>
              </section>
            )}

            {whatYouLearn.length > 0 && (
              <section className="course-detail-block course-detail-section">
                <h2>Чему вы научитесь</h2>
                <ul className="check-list">
                  {whatYouLearn.map((line) => (
                    <li key={line}>{line}</li>
                  ))}
                </ul>
              </section>
            )}

            {course.modules && course.modules.length > 0 && (
              <section className="course-detail-block course-detail-section">
                <h2>Программа обучения</h2>
                <div className="modules-list">
                  {course.modules.map((m, idx) => (
                    <div key={m.id} className={`module-item${idx === 0 ? ' active' : ''}`}>
                      <div className="module-header">
                        <span>Модуль {idx + 1}</span>
                        <strong>{m.title}</strong>
                        {m.lessonCount > 0 && (
                          <span className="module-lesson-count">{m.lessonCount} уроков</span>
                        )}
                      </div>
                      {m.description?.trim() ? <p>{m.description}</p> : null}
                    </div>
                  ))}
                </div>
              </section>
            )}

            {!showAboutBlock && whatYouLearn.length === 0 && (!course.modules || course.modules.length === 0) && (
              <section className="course-detail-block course-detail-section">
                <p className="course-detail-empty-note">
                  Подробная программа и описание будут добавлены администратором школы.
                </p>
              </section>
            )}
          </div>

          <aside className="course-info-side">
            <div className="course-detail-block help-card">
              <h3>Нужна помощь?</h3>
              <p>Наставник ответит на ваши вопросы и поможет подобрать курс.</p>
              <Link to={`${location.pathname}${location.search}#contacts`} className="btn btn-outline btn-full">
                Контакты школы
              </Link>
            </div>
          </aside>
        </div>
      </div>
    </div>
  );
};

function pluralLessons(n: number): string {
  const m10 = n % 10;
  const m100 = n % 100;
  if (m10 === 1 && m100 !== 11) return 'урок';
  if (m10 >= 2 && m10 <= 4 && (m100 < 10 || m100 >= 20)) return 'урока';
  return 'уроков';
}

function pluralReviews(n: number): string {
  const m10 = n % 10;
  const m100 = n % 100;
  if (m10 === 1 && m100 !== 11) return 'отзыв';
  if (m10 >= 2 && m10 <= 4 && (m100 < 10 || m100 >= 20)) return 'отзыва';
  return 'отзывов';
}

export default CourseDetailPage;
