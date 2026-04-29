import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import {
  getCabinetLesson,
  submitCabinetAssignment,
  type StudentCabinetAssignment,
  type StudentCabinetSubmission,
} from '../../api/studentCabinet';
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
  const [submissions, setSubmissions] = useState<StudentCabinetSubmission[]>([]);
  const [answerText, setAnswerText] = useState('');
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [sending, setSending] = useState(false);

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
        setSubmissions((d.submissions ?? []).filter((s) => s.assignmentId === aid));
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

  const handleSend = async () => {
    if (studentId == null || !assignment) return;
    const trimmed = answerText.trim();
    if (!trimmed) {
      setSubmitError('Введите ответ перед отправкой.');
      return;
    }
    setSending(true);
    setSubmitError(null);
    try {
      const created = await submitCabinetAssignment(studentId, eid, lid, assignment.assignmentId, trimmed);
      setSubmissions((prev) => [created, ...prev]);
      setAnswerText('');
    } catch (e: unknown) {
      const ax = e as { response?: { data?: unknown } };
      const d = ax.response?.data;
      if (typeof d === 'string' && d.trim()) {
        setSubmitError(d);
      } else {
        setSubmitError('Не удалось отправить решение.');
      }
    } finally {
      setSending(false);
    }
  };

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
            <label style={{ display: 'block', marginBottom: '8px', color: '#334155' }}>Ваш ответ</label>
            <textarea
              value={answerText}
              onChange={(e) => setAnswerText(e.target.value)}
              rows={6}
              placeholder="Введите решение, комментарий или ответы на вопросы."
              style={{
                width: '100%',
                border: '1px solid #cbd5e1',
                borderRadius: 10,
                padding: 12,
                fontSize: '1rem',
                fontFamily: 'inherit',
                boxSizing: 'border-box',
              }}
            />
            {submitError && <p className="cabinet-error" style={{ marginTop: 10 }}>{submitError}</p>}
            <button
              type="button"
              className="cabinet-assignment-cta"
              onClick={handleSend}
              disabled={sending}
              style={{ marginTop: 12 }}
            >
              {sending ? 'Отправка...' : 'Отправить решение'}
            </button>
            <p style={{ color: '#64748b', margin: '10px 0 0' }}>
              Сейчас поддержан текстовый ответ. Файлы и отдельный режим тестирования можно добавить следующим шагом.
            </p>
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Мои отправки
            </h2>
            {submissions.length === 0 ? (
              <p style={{ margin: 0 }}>Пока нет отправленных решений.</p>
            ) : (
              <ul>
                {submissions.map((s) => (
                  <li key={s.submissionId}>
                    {s.submittedAt ? formatDate(s.submittedAt) : '-'} · статус: {dash(s.submissionStatusName)} · балл:{' '}
                    {dash(s.score)}
                  </li>
                ))}
              </ul>
            )}
          </div>
        </>
      )}
    </div>
  );
};

export default LessonAssignmentPage;
