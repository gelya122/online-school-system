import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getCabinetEnrollment, type StudentCabinetEnrollmentDetail } from '../../api/studentCabinet';
import './cabinetPages.css';

function dash(v: unknown): string {
  if (v === null || v === undefined) return '-';
  if (typeof v === 'string' && v.trim() === '') return '-';
  return String(v);
}

function formatDate(iso: string | null | undefined): string {
  if (!iso) return '-';
  const d = iso.slice(0, 10);
  if (d.length !== 10) return dash(iso);
  const [y, m, day] = d.split('-');
  return `${day}.${m}.${y}`;
}

const CourseWorkspacePage = () => {
  const { enrollmentId } = useParams<{ enrollmentId: string }>();
  const eid = Number(enrollmentId);
  const { user } = useAuth();
  const navigate = useNavigate();
  const studentId = user?.studentId;
  const [data, setData] = useState<StudentCabinetEnrollmentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (studentId == null || !Number.isFinite(eid)) {
      setLoading(false);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const d = await getCabinetEnrollment(studentId, eid);
        if (!cancelled) setData(d);
      } catch {
        if (!cancelled) setError('Курс не найден или нет доступа.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [studentId, eid]);

  if (studentId == null) {
    return (
      <div>
        <Link className="cabinet-back" to="/learn">
          ← Мои курсы
        </Link>
        <div className="cabinet-alert">Профиль ученика не привязан к аккаунту.</div>
      </div>
    );
  }

  if (!Number.isFinite(eid)) {
    navigate('/learn', { replace: true });
    return null;
  }

  return (
    <div>
      <Link className="cabinet-back" to="/learn">
        ← Мои курсы
      </Link>
      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}
      {!loading && data && (
        <>
          <h1 className="cabinet-page-title">{dash(data.course.title)}</h1>
          <p className="cabinet-page-lead">Поток: {dash(data.instance.instanceName)}</p>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '14px' }}>
              Данные курса и потока
            </h2>
            <dl className="cabinet-dl">
              <dt>Краткое описание</dt>
              <dd>{dash(data.course.shortDescription)}</dd>
              <dt>Полное описание</dt>
              <dd>{dash(data.course.description)}</dd>
              <dt>Часов (по каталогу)</dt>
              <dd>{dash(data.course.totalHours)}</dd>
              <dt>Что получите</dt>
              <dd>{dash(data.course.whatYouGet)}</dd>
              <dt>Дата начала потока</dt>
              <dd>{formatDate(data.instance.startDate)}</dd>
              <dt>Дата окончания потока</dt>
              <dd>{formatDate(data.instance.endDate)}</dd>
              <dt>Недель</dt>
              <dd>{dash(data.instance.totalWeeks)}</dd>
              <dt>Уроков в неделю</dt>
              <dd>{dash(data.instance.lessonsPerWeek)}</dd>
              <dt>Расписание (текст)</dt>
              <dd>{dash(data.instance.scheduleDescription)}</dd>
              <dt>Дата записи</dt>
              <dd>{data.enrolledAt ? formatDate(data.enrolledAt) : '-'}</dd>
              <dt>Статус записи</dt>
              <dd>{dash(data.enrollmentStatusName ?? (data.enrollmentStatusId != null ? String(data.enrollmentStatusId) : null))}</dd>
            </dl>
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '14px' }}>
              Модули и уроки
            </h2>
            {data.modules.length === 0 && <p>В базе нет модулей для этого курса.</p>}
            {data.modules.map((mod) => (
              <details key={mod.moduleId} className="cabinet-module">
                <summary>
                  {dash(mod.title)} <span style={{ fontWeight: 400, color: '#64748b' }}>(порядок {mod.moduleOrder})</span>
                </summary>
                {mod.description && (
                  <p style={{ margin: '8px 16px', fontSize: '0.88rem', color: '#64748b' }}>{dash(mod.description)}</p>
                )}
                <ul className="cabinet-lesson-list">
                  {mod.lessons.map((les) => (
                    <li key={les.lessonId}>
                      <Link className="cabinet-lesson-link" to={`/learn/courses/${eid}/lessons/${les.lessonId}`}>
                        {dash(les.title)}
                        {les.durationMinutes != null ? ` · ${les.durationMinutes} мин` : ''}
                      </Link>
                    </li>
                  ))}
                </ul>
              </details>
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default CourseWorkspacePage;
