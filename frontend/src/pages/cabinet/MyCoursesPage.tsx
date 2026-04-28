import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getCabinetCourses, type StudentCabinetEnrollmentSummary } from '../../api/studentCabinet';
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

const MyCoursesPage = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const studentId = user?.studentId;
  const [rows, setRows] = useState<StudentCabinetEnrollmentSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (studentId == null) {
      setLoading(false);
      setRows([]);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await getCabinetCourses(studentId);
        if (!cancelled) setRows(data);
      } catch {
        if (!cancelled) setError('Не удалось загрузить курсы.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [studentId]);

  if (studentId == null) {
    return (
      <div>
        <h1 className="cabinet-page-title">Мои курсы</h1>
        <p className="cabinet-page-lead">Здесь отображаются купленные курсы.</p>
        <div className="cabinet-alert">Профиль ученика не привязан к аккаунту. Обратитесь в поддержку школы.</div>
      </div>
    );
  }

  return (
    <div>
      <h1 className="cabinet-page-title">Мои курсы</h1>
      <p className="cabinet-page-lead">Курсы, на которые вы записаны после оплаты.</p>
      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}
      {!loading && !error && rows.length === 0 && <p>У вас пока нет записей на курсы.</p>}
      {!loading && rows.length > 0 && (
        <div className="cabinet-grid">
          {rows.map((r) => (
            <button
              key={r.enrollmentId}
              type="button"
              className="cabinet-card"
              onClick={() => navigate(`/learn/courses/${r.enrollmentId}`)}
            >
              {r.course.coverImgUrl ? (
                <img className="cabinet-card-cover" src={r.course.coverImgUrl} alt="" />
              ) : (
                <div className="cabinet-card-cover" aria-hidden />
              )}
              <div className="cabinet-card-body">
                <h2 className="cabinet-card-title">{dash(r.course.title)}</h2>
                <div className="cabinet-card-meta">
                  <div>Поток: {dash(r.instance.instanceName)}</div>
                  <div>Старт: {formatDate(r.instance.startDate)}</div>
                  <div>Статус: {dash(r.enrollmentStatusName ?? (r.enrollmentStatusId != null ? String(r.enrollmentStatusId) : null))}</div>
                </div>
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default MyCoursesPage;
