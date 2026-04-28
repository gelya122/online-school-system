import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getCabinetLesson, type StudentCabinetAssignment } from '../../api/studentCabinet';
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

function assignmentHeading(a: StudentCabinetAssignment): string {
  const t = (a.assignmentTypeName ?? '').toLowerCase();
  if (t.includes('тест') || t.includes('test')) return 'Тест';
  if (t.includes('контроль')) return 'Контрольная работа';
  return 'Задание';
}

const LessonAssignmentPage = () => {
  const { enrollmentId, lessonId, assignmentId } = useParams<{
    enrollmentId: string;
    lessonId: string;
    assignmentId: string;
  }>();
  const eid = Number(enrollmentId);
  const lid = Number(lessonId);
  const aid = Number(assignmentId);
  const { user } = useAuth();
  const navigate = useNavigate();
  const studentId = user?.studentId;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lessonTitle, setLessonTitle] = useState<string>('');
  const [assignment, setAssignment] = useState<StudentCabinetAssignment | null>(null);

  useEffect(() => {
    if (studentId == null || !Number.isFinite(eid) || !Number.isFinite(lid) || !Number.isFinite(aid)) {
      setLoading(false);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const d = await getCabinetLesson(studentId, eid, lid);
        if (cancelled) return;
        setLessonTitle(d.title ?? '');
        const found = d.assignments.find((x) => x.assignmentId === aid) ?? null;
        setAssignment(found);
        if (!found) setError('Такого задания нет в этом уроке или нет доступа.');
      } catch {
        if (!cancelled) setError('Урок не найден или нет доступа.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [studentId, eid, lid, aid]);

  const lessonPath = useMemo(() => `/learn/courses/${eid}/lessons/${lid}`, [eid, lid]);

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

  if (!Number.isFinite(eid) || !Number.isFinite(lid) || !Number.isFinite(aid)) {
    navigate('/learn', { replace: true });
    return null;
  }

  return (
    <div>
      <Link className="cabinet-back" to={lessonPath}>
        ← К уроку
      </Link>
      <Link className="cabinet-back" style={{ marginLeft: '16px' }} to={`/learn/courses/${eid}`}>
        К курсу
      </Link>
      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}
      {!loading && assignment && (
        <>
          <p className="cabinet-page-lead" style={{ marginBottom: '8px' }}>
            Урок: {dash(lessonTitle)}
          </p>
          <h1 className="cabinet-page-title">
            {assignmentHeading(assignment)}: {dash(assignment.title)}
          </h1>
          <p className="cabinet-page-lead">
            Тип: {dash(assignment.assignmentTypeName ?? String(assignment.assignmentTypeId))} · Максимум баллов:{' '}
            {assignment.maxScore}
          </p>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Условие
            </h2>
            {assignment.description?.trim() ? (
              <p style={{ whiteSpace: 'pre-wrap', margin: 0 }}>{assignment.description}</p>
            ) : (
              <p>Описание не указано.</p>
            )}
            <dl className="cabinet-dl" style={{ marginTop: '18px' }}>
              <dt>Дней после урока</dt>
              <dd>{dash(assignment.dueDaysAfterLesson)}</dd>
              <dt>Расчётная дата сдачи</dt>
              <dd>{assignment.calculatedDueDate ? formatDate(assignment.calculatedDueDate) : '-'}</dd>
            </dl>
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Сдача работы
            </h2>
            <p style={{ color: '#64748b', margin: 0 }}>
              Форма ответа (тест, файл, текст) будет подключена к этой странице отдельно. Пока задание можно
              просмотреть здесь, отправки по уроку отображаются на странице урока.
            </p>
          </div>
        </>
      )}
    </div>
  );
};

export default LessonAssignmentPage;
