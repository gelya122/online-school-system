import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getCabinetCourses, getCabinetHomework, type StudentCabinetEnrollmentSummary, type StudentCabinetHomeworkRow } from '../../api/studentCabinet';
import { getSubmissionStatuses, type SubmissionStatusRecord } from '../../api/submissionStatuses';
import './cabinetPages.css';
import './HomeworkCabinetPage.css';

function dash(v: unknown): string {
  if (v === null || v === undefined) return '-';
  if (typeof v === 'string' && v.trim() === '') return '-';
  return String(v);
}

function formatDateRu(iso: string | null | undefined): string {
  if (!iso) return '-';
  const d = iso.slice(0, 10);
  if (d.length !== 10) return dash(iso);
  const [y, m, day] = d.split('-').map(Number);
  if (!y || !m || !day) return dash(iso);
  const months = [
    'января',
    'февраля',
    'марта',
    'апреля',
    'мая',
    'июня',
    'июля',
    'августа',
    'сентября',
    'октября',
    'ноября',
    'декабря',
  ];
  return `${day} ${months[m - 1] ?? ''}`;
}

function formatDeadlineShort(iso: string | null | undefined): string {
  if (!iso) return '-';
  const d = iso.slice(0, 10);
  if (d.length !== 10) return dash(iso);
  const [y, m, day] = d.split('-');
  return `${day}.${m}.${y}`;
}

const FILTER_ALL = '__all__';

const HomeworkCabinetPage = () => {
  const { user } = useAuth();
  const studentId = user?.studentId;
  const [enrollments, setEnrollments] = useState<StudentCabinetEnrollmentSummary[]>([]);
  const [homeworkRows, setHomeworkRows] = useState<StudentCabinetHomeworkRow[]>([]);
  const [submissionStatuses, setSubmissionStatuses] = useState<SubmissionStatusRecord[]>([]);
  const [selectedEnrollmentId, setSelectedEnrollmentId] = useState<number | null>(null);
  const [statusFilter, setStatusFilter] = useState(FILTER_ALL);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (studentId == null) {
      setLoading(false);
      setEnrollments([]);
      setHomeworkRows([]);
      setSubmissionStatuses([]);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [coursesData, hwData, statusesData] = await Promise.all([
          getCabinetCourses(studentId),
          getCabinetHomework(studentId),
          getSubmissionStatuses(),
        ]);
        if (!cancelled) {
          setEnrollments(coursesData);
          setHomeworkRows(hwData);
          setSubmissionStatuses(statusesData);
        }
      } catch {
        if (!cancelled) setError('Не удалось загрузить данные.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [studentId]);

  useEffect(() => {
    setStatusFilter(FILTER_ALL);
  }, [selectedEnrollmentId]);

  const filteredByCourse = useMemo(() => {
    if (selectedEnrollmentId == null) return [];
    return homeworkRows.filter((r) => r.enrollmentId === selectedEnrollmentId);
  }, [homeworkRows, selectedEnrollmentId]);

  const displayRows = useMemo(() => {
    if (statusFilter === FILTER_ALL) return filteredByCourse;
    return filteredByCourse.filter((r) => (r.submissionStatusName?.trim() ?? '') === statusFilter);
  }, [filteredByCourse, statusFilter]);

  const selectedEnrollment = useMemo(
    () => enrollments.find((e) => e.enrollmentId === selectedEnrollmentId) ?? null,
    [enrollments, selectedEnrollmentId],
  );

  if (studentId == null) {
    return (
      <div>
        <h1 className="cabinet-page-title">ДЗ</h1>
        <div className="cabinet-alert">Профиль ученика не привязан к аккаунту.</div>
      </div>
    );
  }

  return (
    <div className={selectedEnrollmentId != null ? 'hw-page' : undefined}>
      <div className="hw-title-row">
        <h1 className="cabinet-page-title" style={{ marginBottom: 0 }}>
          Домашние работы
        </h1>
        {selectedEnrollmentId != null && (
          <span
            className="hw-help"
            title="Список строится из ваших записей на курс, уроков, заданий и дат из базы (плановый доступ к уроку, расчётный срок сдачи при наличии полей в БД). Статусы фильтра — из справочника submission_status."
          >
            ?
          </span>
        )}
      </div>

      {selectedEnrollmentId != null && selectedEnrollment ? (
        <>
          <button type="button" className="cabinet-back cabinet-back--button" onClick={() => setSelectedEnrollmentId(null)}>
            ← Выбор курса
          </button>
          <p className="cabinet-page-lead">
            <strong>{dash(selectedEnrollment.course.title)}</strong>
            {selectedEnrollment.instance.instanceName ? ` · ${selectedEnrollment.instance.instanceName}` : ''}
          </p>
        </>
      ) : (
        <p className="cabinet-page-lead">Выберите курс, чтобы посмотреть домашние работы.</p>
      )}

      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}

      {!loading && !error && selectedEnrollmentId == null && (
        <>
          {enrollments.length === 0 ? (
            <p>У вас нет записей на курсы.</p>
          ) : (
            <div className="cabinet-grid">
              {enrollments.map((e) => (
                <button
                  key={e.enrollmentId}
                  type="button"
                  className="cabinet-card"
                  onClick={() => setSelectedEnrollmentId(e.enrollmentId)}
                >
                  {e.course.coverImgUrl ? (
                    <img className="cabinet-card-cover" src={e.course.coverImgUrl} alt="" />
                  ) : (
                    <div className="cabinet-card-cover" aria-hidden />
                  )}
                  <div className="cabinet-card-body">
                    <h2 className="cabinet-card-title">{dash(e.course.title)}</h2>
                    <div className="cabinet-card-meta">
                      <div>Поток: {dash(e.instance.instanceName)}</div>
                    </div>
                  </div>
                </button>
              ))}
            </div>
          )}
        </>
      )}

      {!loading && !error && selectedEnrollmentId != null && selectedEnrollment && (
        <>
          {filteredByCourse.length === 0 ? (
            <p>Для этого курса в базе нет привязанных заданий.</p>
          ) : (
            <>
              <div className="hw-toolbar">
                <div className="hw-select-wrap">
                  <select
                    className="hw-select"
                    aria-label="Статус из базы"
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                  >
                    <option value={FILTER_ALL}>Все статусы</option>
                    {submissionStatuses.map((s) => (
                      <option key={s.statusId} value={s.statusName}>
                        {dash(s.statusName)}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="hw-cards-legend">
                <span>Тема</span>
                <span>Статус</span>
                <span>Дедлайн</span>
              </div>

              {displayRows.length === 0 ? (
                <div className="hw-empty-filters">Нет заданий с выбранным статусом.</div>
              ) : (
                <div className="hw-cards">
                  {displayRows.map((r) => {
                    const typeLabel = dash(r.assignmentTypeName ?? (r.assignmentTypeId != null ? `Тип ${r.assignmentTypeId}` : null));
                    const statusText = r.submissionStatusName?.trim() ? r.submissionStatusName : '—';
                    const lid =
                      typeof r.lessonId === 'number' && Number.isFinite(r.lessonId) && r.lessonId > 0
                        ? Math.trunc(r.lessonId)
                        : undefined;
                    const assignmentLink =
                      lid != null
                        ? `/learn/courses/${r.enrollmentId}/lessons/${lid}/assignments/${r.assignmentId}`
                        : null;
                    return (
                      <article key={`${r.enrollmentId}-${r.lessonId}-${r.assignmentId}`} className="hw-card">
                        <div className="hw-card-top">
                          <div className="hw-card-kicker">{dash(r.moduleTitle)}</div>
                          <h2 className="hw-card-title">
                            {dash(r.lessonTitle)} | {dash(r.assignmentTitle)}
                          </h2>
                        </div>
                        <div className="hw-card-band">
                          <div className="hw-type">
                            <span className="hw-type-icon" aria-hidden />
                            <span className="hw-type-label">{typeLabel}</span>
                          </div>
                          <div className={r.submissionStatusName?.trim() ? 'hw-status' : 'hw-status hw-status--muted'}>
                            {statusText}
                          </div>
                          <div className="hw-dates">
                            <span className="hw-dates-line">Доступно {formatDateRu(r.plannedLessonAccessDate)}</span>
                            <span className="hw-dates-line">Дедлайн: {formatDeadlineShort(r.calculatedDueDate)}</span>
                          </div>
                          {assignmentLink ? (
                            <Link className="hw-open-dz" to={assignmentLink}>
                              Открыть задание
                            </Link>
                          ) : (
                            <button type="button" className="hw-open-dz" disabled>
                              Открыть задание
                            </button>
                          )}
                        </div>
                      </article>
                    );
                  })}
                </div>
              )}
            </>
          )}
        </>
      )}
    </div>
  );
};

export default HomeworkCabinetPage;
