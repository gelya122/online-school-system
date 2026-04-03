import { useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import type { Course, Subject, Exam } from '../types';
import { getCourses } from '../api/courses';
import { getSubjects, getExams } from '../api/filters';
import { useCart } from '../contexts/CartContext';
import './CoursesPage.css';

const CoursesPage = () => {
  const { addToCart, items } = useCart();
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [cartAddedNotice, setCartAddedNotice] = useState<string | null>(null);
  const noticeTimerRef = useRef<ReturnType<typeof window.setTimeout> | null>(null);

  const [search, setSearch] = useState('');
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [exams, setExams] = useState<Exam[]>([]);
  const [selectedExamIds, setSelectedExamIds] = useState<number[]>([]);
  const [selectedSubjectIds, setSelectedSubjectIds] = useState<number[]>([]);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const [coursesData, subjectsData, examsData] = await Promise.all([
          getCourses(),
          getSubjects(),
          getExams(),
        ]);

        if (cancelled) return;
        setCourses(coursesData);
        setSubjects(subjectsData.filter((s) => s.isActive !== false));
        setExams(examsData.filter((e) => e.isActive !== false));
      } catch (e: unknown) {
        if (cancelled) return;
        const message = e instanceof Error ? e.message : 'Не удалось загрузить данные фильтров';
        setError(message);
        setCourses([]);
        setSubjects([]);
        setExams([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    return () => {
      if (noticeTimerRef.current) window.clearTimeout(noticeTimerRef.current);
    };
  }, []);

  const handleToggleExam = (examId: number) => {
    setSelectedExamIds((prev) =>
      prev.includes(examId) ? prev.filter((id) => id !== examId) : [...prev, examId],
    );
  };

  const handleToggleSubject = (subjectId: number) => {
    setSelectedSubjectIds((prev) =>
      prev.includes(subjectId) ? prev.filter((id) => id !== subjectId) : [...prev, subjectId],
    );
  };

  const filteredCourses = useMemo(() => {
    const searchValue = search.trim().toLowerCase();
    const examActive = selectedExamIds.length > 0;
    const subjectsActive = selectedSubjectIds.length > 0;

    return courses.filter((course) => {
      const text = `${course.title} ${course.description ?? ''}`.toLowerCase();

      if (searchValue && !text.includes(searchValue)) {
        return false;
      }

      if (examActive) {
        if (!course.examId || !selectedExamIds.includes(course.examId)) return false;
      }

      if (subjectsActive) {
        if (!course.subjectId || !selectedSubjectIds.includes(course.subjectId)) return false;
      }

      return true;
    });
  }, [courses, search, selectedExamIds, selectedSubjectIds]);

  const cartCourseIdSet = useMemo(() => new Set(items.map((x) => x.courseId)), [items]);

  return (
    <div className="courses-page">
      <h1 className="courses-title">Каталог курсов</h1>
      {error && <p className="courses-error">{error}</p>}
      {cartAddedNotice && <div className="cart-added-notice">{cartAddedNotice}</div>}

      <div className="courses-layout">
        <aside className="courses-filters">
          <div className="filters-card">
            <div className="filters-group">
              <label className="filters-label" htmlFor="course-search">
                Поиск курса
              </label>
              <input
                id="course-search"
                type="text"
                placeholder="Название курса"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>

            <div className="filters-group">
              <div className="filters-group-title">Экзамен</div>
              {exams.map((exam) => (
                <label key={exam.id} className="checkbox-row">
                  <input
                    type="checkbox"
                    checked={selectedExamIds.includes(exam.id)}
                    onChange={() => handleToggleExam(exam.id)}
                  />
                  <span>{exam.name}</span>
                </label>
              ))}
            </div>

            <div className="filters-group">
              <div className="filters-group-title">Предмет</div>
              {subjects.map((subj) => (
                <label key={subj.id} className="checkbox-row">
                  <input
                    type="checkbox"
                    checked={selectedSubjectIds.includes(subj.id)}
                    onChange={() => handleToggleSubject(subj.id)}
                  />
                  <span>{subj.name}</span>
                </label>
              ))}
            </div>

            <button
              type="button"
              className="filters-reset"
              onClick={() => {
                setSearch('');
                setSelectedExamIds([]);
                setSelectedSubjectIds([]);
              }}
            >
              Сбросить фильтры
            </button>
          </div>
        </aside>

        <main className="courses-content">
          {loading ? (
            <div className="courses-loading">Загрузка курсов...</div>
          ) : courses.length === 0 ? (
            <p>Курсы пока не добавлены</p>
          ) : filteredCourses.length === 0 ? (
            <p>По заданным параметрам курсы не найдены</p>
          ) : (
            <div className="courses-grid">
              {filteredCourses.map((course) => (
                <div key={course.id} className="course-card">
                  {course.imageUrl && (
                    <div className="course-image-wrapper">
                      <img src={course.imageUrl} alt={course.title} className="course-image" />
                    </div>
                  )}
                  <div className="course-card-body">
                    <h3 className="course-card-title">{course.title}</h3>
                    {course.description && (
                      <p className="course-card-description">{course.description}</p>
                    )}
                    <div className="course-card-footer">
                      {course.price !== undefined && (
                        <div className="course-price">{course.price} ₽</div>
                      )}
                      <div className="course-actions">
                        <button
                          type="button"
                          className="btn-outline"
                          disabled={cartCourseIdSet.has(course.id)}
                          onClick={() => {
                            if (cartCourseIdSet.has(course.id)) return;

                            addToCart(course);
                            setCartAddedNotice('Товар добавлен в корзину');

                            if (noticeTimerRef.current) {
                              window.clearTimeout(noticeTimerRef.current);
                            }
                            noticeTimerRef.current = window.setTimeout(() => {
                              setCartAddedNotice(null);
                            }, 2000);
                          }}
                        >
                          {cartCourseIdSet.has(course.id) ? 'В корзине' : 'В корзину'}
                        </button>
                        <Link to={`/courses/${course.id}`} className="btn-primary">
                          Подробнее
                        </Link>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default CoursesPage;

