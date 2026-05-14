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
          <h1 className="cabinet-page-title">{dash(data.instance.instanceName)}</h1>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '14px' }}>
              О курсе
            </h2>
            <dl className="cabinet-dl">
              <dt>Полное описание</dt>
              <dd>{dash(data.course.description)}</dd>
              <dt>Кол-во часов</dt>
              <dd>{dash(data.course.totalHours)}</dd>
              <dt>Расписание</dt>
              <dd>{dash(data.instance.scheduleDescription)}</dd>
            </dl>
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '14px' }}>
              Уроки
            </h2>
            {data.modules.length === 0 && <p>В базе нет модулей для этого курса.</p>}
            {data.modules.map((mod) => (
              <details key={mod.moduleId} className="cabinet-module">
                <summary>
                  <span>{dash(mod.title)}</span>
                  <span className="cabinet-module-chevron" aria-hidden>
                    {'>'}
                  </span>
                </summary>
                <ul className="cabinet-lesson-list">
                  {mod.lessons.map((les) => (
                    <li key={les.lessonId}>
                      <Link className="cabinet-lesson-link" to={`/learn/courses/${eid}/lessons/${les.lessonId}`}>
                        {dash(les.title)}
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
