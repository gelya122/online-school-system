import { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import {
  getCabinetAssignmentQuestions,
  getCabinetAssignmentResult,
  getCabinetLesson,
  type StudentCabinetAssignment,
  type StudentCabinetQuestion,
  submitCabinetQuestionAnswer,
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
  const [questions, setQuestions] = useState<StudentCabinetQuestion[]>([]);
  const [questionIndex, setQuestionIndex] = useState(0);
  const [answerText, setAnswerText] = useState('');
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [sending, setSending] = useState(false);
  const [isFinished, setIsFinished] = useState(false);
  const [totalScore, setTotalScore] = useState<number | null>(null);
  const [maxScore, setMaxScore] = useState<number | null>(null);

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
        if (!found) {
          setError('Такого задания нет в этом уроке или нет доступа.');
          return;
        }
        const qs = await getCabinetAssignmentQuestions(studentId, eid, lid, aid);
        if (cancelled) return;
        const sorted = [...qs].sort((a, b) => a.questionOrder - b.questionOrder || a.questionId - b.questionId);
        setQuestions(sorted);
        setQuestionIndex(0);
        setIsFinished(false);
        setTotalScore(null);
        setMaxScore(null);
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
  const assignmentText = (assignment?.description ?? '').trim();
  const currentQuestion = questions[questionIndex] ?? null;
  const hasPrevQuestion = questionIndex > 0;
  const hasNextQuestion = questionIndex < questions.length - 1;

  useEffect(() => {
    if (!currentQuestion) return;
    setAnswerText(currentQuestion.studentAnswer ?? '');
    setSubmitError(null);
  }, [currentQuestion]);

  const handleSend = async () => {
    if (studentId == null || !assignment || !currentQuestion) return;
    const trimmed = answerText.trim();
    if (!trimmed) {
      setSubmitError('Введите ответ перед отправкой.');
      return;
    }
    setSending(true);
    setSubmitError(null);
    try {
      const saved = await submitCabinetQuestionAnswer(
        studentId,
        eid,
        lid,
        assignment.assignmentId,
        currentQuestion.questionId,
        trimmed,
      );
      setQuestions((prev) =>
        prev.map((q) =>
          q.questionId === saved.questionId
            ? { ...q, studentAnswer: saved.studentAnswer, pointsAwarded: saved.pointsAwarded }
            : q,
        ),
      );
      if (hasNextQuestion) {
        setQuestionIndex((v) => v + 1);
      } else {
        const result = await getCabinetAssignmentResult(studentId, eid, lid, assignment.assignmentId);
        setTotalScore(result.totalScore);
        setMaxScore(result.maxScore);
        setIsFinished(true);
      }
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
            Тест: {dash(assignment.title)}
          </h1>
          {!isFinished && currentQuestion && (
            <p className="cabinet-page-lead">
              Вопрос {questionIndex + 1} из {questions.length} · Максимум баллов за вопрос: {currentQuestion.maxPoints}
            </p>
          )}

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Текст задания
            </h2>
            {assignmentText ? (
              <p style={{ whiteSpace: 'pre-wrap', margin: 0 }}>{assignmentText}</p>
            ) : (
              <p style={{ margin: 0, color: '#64748b' }}>
                В базе пока не заполнено описание задания (`description`).
              </p>
            )}
            <dl className="cabinet-dl" style={{ marginTop: '18px' }}>
              <dt>Дней после урока</dt>
              <dd>{dash(assignment.dueDaysAfterLesson)}</dd>
              <dt>Расчётная дата сдачи</dt>
              <dd>{assignment.calculatedDueDate ? formatDate(assignment.calculatedDueDate) : '-'}</dd>
            </dl>
          </div>

          {!isFinished && currentQuestion && (
            <div className="cabinet-panel">
              <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                Вопрос
              </h2>
              <p style={{ whiteSpace: 'pre-wrap', margin: '0 0 16px' }}>{currentQuestion.questionText}</p>
              <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                Ответ
              </h2>
              <label style={{ display: 'block', marginBottom: '8px', color: '#334155' }}>Ваш ответ</label>
              <input
                type="text"
                value={answerText}
                onChange={(e) => setAnswerText(e.target.value)}
                maxLength={100}
                placeholder="Введите ответ (до 100 символов)"
                style={{
                  width: '100%',
                  border: '1px solid #cbd5e1',
                  borderRadius: 10,
                  padding: 12,
                  fontSize: '1rem',
                  fontFamily: 'inherit',
                  boxSizing: 'border-box',
                  backgroundColor: '#ffffff',
                  color: '#111827',
                }}
              />
              <p style={{ marginTop: 8, color: '#64748b' }}>{answerText.length}/100</p>
              {submitError && <p className="cabinet-error" style={{ marginTop: 10 }}>{submitError}</p>}
              <div style={{ display: 'flex', gap: 12, marginTop: 12 }}>
                {hasPrevQuestion && (
                  <button
                    type="button"
                    className="cabinet-assignment-cta"
                    onClick={() => setQuestionIndex((v) => Math.max(0, v - 1))}
                    disabled={sending}
                  >
                    Назад
                  </button>
                )}
                <button
                  type="button"
                  className="cabinet-assignment-cta"
                  onClick={handleSend}
                  disabled={sending}
                >
                  {sending ? 'Отправка...' : hasNextQuestion ? 'Отправить и далее' : 'Отправить'}
                </button>
              </div>
              {currentQuestion.studentAnswer && (
                <p style={{ marginTop: 10, color: '#64748b' }}>
                  Ответ можно изменить: при повторной отправке он перезапишется.
                </p>
              )}
            </div>
          )}

          {isFinished && (
            <div className="cabinet-panel">
              <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                Итог теста
              </h2>
              <p style={{ margin: 0 }}>
                Набрано баллов: <strong>{totalScore ?? 0}</strong> из <strong>{maxScore ?? assignment.maxScore}</strong>.
              </p>
            </div>
          )}
          {!isFinished && !hasNextQuestion && currentQuestion && (
            <p style={{ color: '#64748b', marginTop: 10 }}>
              Это последний вопрос. После отправки покажем итоговые баллы.
            </p>
          )}
        </>
      )}
    </div>
  );
};

export default LessonAssignmentPage;
