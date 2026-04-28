import { useEffect, useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { getCabinetProgress, type StudentCabinetProgressRow } from '../../api/studentCabinet';
import './cabinetPages.css';

function dash(v: unknown): string {
  if (v === null || v === undefined) return '-';
  if (typeof v === 'string' && v.trim() === '') return '-';
  return String(v);
}

function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '-';
  const t = new Date(iso);
  if (Number.isNaN(t.getTime())) return dash(iso);
  return t.toLocaleString('ru-RU');
}

const ProgressCabinetPage = () => {
  const { user } = useAuth();
  const studentId = user?.studentId;
  const [rows, setRows] = useState<StudentCabinetProgressRow[]>([]);
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
        const data = await getCabinetProgress(studentId);
        if (!cancelled) setRows(data);
      } catch {
        if (!cancelled) setError('Не удалось загрузить прогресс.');
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
        <h1 className="cabinet-page-title">Прогресс</h1>
        <div className="cabinet-alert">Профиль ученика не привязан к аккаунту.</div>
      </div>
    );
  }

  return (
    <div>
      <h1 className="cabinet-page-title">Прогресс</h1>
      <p className="cabinet-page-lead">Данные из таблицы прогресса по вашим записям на курсы.</p>
      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}
      {!loading && !error && rows.length === 0 && <p>Нет записей прогресса.</p>}
      {!loading && rows.length > 0 && (
        <div className="cabinet-table-wrap">
          <table className="cabinet-table">
            <thead>
              <tr>
                <th>Курс</th>
                <th>Модуль</th>
                <th>Порядок модуля</th>
                <th>Урок</th>
                <th>Порядок урока</th>
                <th>Завершён</th>
                <th>Дата завершения</th>
                <th>Просмотр, сек</th>
                <th>Последний заход</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={`${r.enrollmentId}-${r.lessonId}`}>
                  <td>{dash(r.courseTitle)}</td>
                  <td>{dash(r.moduleTitle)}</td>
                  <td>{r.moduleOrder}</td>
                  <td>{dash(r.lessonTitle)}</td>
                  <td>{r.lessonOrder}</td>
                  <td>{r.isCompleted == null ? '-' : r.isCompleted ? 'да' : 'нет'}</td>
                  <td>{formatDateTime(r.completedAt)}</td>
                  <td>{dash(r.watchTimeSeconds)}</td>
                  <td>{formatDateTime(r.lastAccessed)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ProgressCabinetPage;
