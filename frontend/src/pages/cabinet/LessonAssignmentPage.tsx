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

function formatTime(iso: string | null | undefined): string {
  if (!iso) return '-';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '-';
  return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
}

function normalizeAnswer(v: string): string {
  return v.trim().toLowerCase().replace(/\s+/g, ' ');
}

function getCorrectAnswerText(q: StudentCabinetQuestion): string | null {
  const extended = q as StudentCabinetQuestion & {
    correctAnswer?: string | null;
    correctAnswerText?: string | null;
    rightAnswer?: string | null;
  };
  const value = extended.correctAnswer ?? extended.correctAnswerText ?? extended.rightAnswer ?? null;
  if (value == null) return null;
  const trimmed = String(value).trim();
  return trimmed ? trimmed : null;
}

/** Как на API: несколько допустимых формулировок в `correct_answer` через `|`. */
function splitCorrectVariants(raw: string | null | undefined): string[] {
  if (raw == null || raw === '') return [];
  return String(raw)
    .split('|')
    .map((s) => s.trim())
    .filter((s) => s.length > 0);
}

/**
 * null — нет авто-сравнения (развёрнутый ответ / нет эталона);
 * true/false — для short_answer по вариантам из correct_answer.
 */
function autoGradeVerdict(q: StudentCabinetQuestion, studentAnswer: string): boolean | null {
  const slug = (q.questionType ?? '').trim().toLowerCase();
  if (slug === 'detailed_answer') return null;
  const variants = splitCorrectVariants(getCorrectAnswerText(q));
  if (variants.length === 0) return null;
  const norm = normalizeAnswer(studentAnswer);
  return variants.some((v) => normalizeAnswer(v) === norm);
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
  const [finishedAt, setFinishedAt] = useState<string | null>(null);

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
        setFinishedAt(null);
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
  const currentQuestion = questions[questionIndex] ?? null;
  const hasPrevQuestion = questionIndex > 0;
  const hasNextQuestion = questionIndex < questions.length - 1;
  const effectiveMaxScore = maxScore ?? assignment?.maxScore ?? 0;
  const effectiveTotalScore = totalScore ?? 0;
  const resultPercent = effectiveMaxScore > 0 ? Math.round((effectiveTotalScore / effectiveMaxScore) * 100) : 0;

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
        setFinishedAt(new Date().toISOString());
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
                  {sending ? 'Отправка...' : 'Отправить'}
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
            <>
              <div className="cabinet-panel">
                <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                  Итог теста
                </h2>
                <dl className="cabinet-dl">
                  <dt>Набранно баллов</dt>
                  <dd>{effectiveTotalScore}</dd>
                  <dt>Максимальное кол-во баллов</dt>
                  <dd>{effectiveMaxScore}</dd>
                  <dt>Выполнено, %</dt>
                  <dd>{resultPercent}%</dd>
                  <dt>Дата</dt>
                  <dd>{formatDate(finishedAt)}</dd>
                  <dt>Время</dt>
                  <dd>{formatTime(finishedAt)}</dd>
                </dl>
              </div>
              <div className="cabinet-panel">
                <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                  Просмотр
                </h2>
                {questions.map((q, idx) => {
                  const studentAnswer = q.studentAnswer?.trim() ?? '';
                  const correctRaw = getCorrectAnswerText(q);
                  const variants = splitCorrectVariants(correctRaw);
                  const verdict = autoGradeVerdict(q, studentAnswer);
                  const isNeutral = verdict === null;
                  const isCorrect = verdict === true;
                  const bg = isNeutral ? '#f8fafc' : isCorrect ? '#f0fdf4' : '#fef2f2';
                  const correctLabel =
                    variants.length > 1 ? variants.join(' · ') : correctRaw != null && correctRaw !== '' ? correctRaw : '—';
                  return (
                    <div
                      key={q.questionId}
                      style={{
                        marginBottom: '16px',
                        padding: '12px',
                        borderBottom: '1px solid #e2e8f0',
                        borderRadius: 10,
                        backgroundColor: bg,
                      }}
                    >
                      <p style={{ margin: '0 0 8px' }}>
                        Задание {idx + 1}. {q.questionText}
                      </p>
                      <p style={{ margin: '0 0 6px' }}>
                        Ваш ответ:{' '}
                        <span
                          style={{
                            color: isNeutral ? '#334155' : isCorrect ? '#15803d' : '#b91c1c',
                            fontWeight: 600,
                          }}
                        >
                          {studentAnswer || '-'}
                        </span>
                      </p>
                      <p style={{ margin: 0 }}>
                        {isNeutral ? (
                          <>Итог по вопросу выставляет преподаватель (авто-сравнение с эталоном не применяется).</>
                        ) : (
                          <>
                            Правильный ответ: <strong>{correctLabel}</strong>
                          </>
                        )}
                      </p>
                    </div>
                  );
                })}
              </div>
            </>
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
