import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getCabinetLesson, type StudentCabinetLessonDetail } from '../../api/studentCabinet';
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

function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '-';
  const t = new Date(iso);
  if (Number.isNaN(t.getTime())) return dash(iso);
  return t.toLocaleString('ru-RU');
}

function isDirectVideoUrl(url: string): boolean {
  return /\.(mp4|webm|ogg)(\?.*)?$/i.test(url);
}

function assignmentCtaLabel(typeName: string | null | undefined): string {
  const t = (typeName ?? '').toLowerCase();
  if (t.includes('тест') || t.includes('test')) return 'Перейти к тесту';
  if (t.includes('контроль')) return 'Перейти к контрольной';
  return 'Перейти к заданию';
}

const LessonPage = () => {
  const { enrollmentId, lessonId } = useParams<{ enrollmentId: string; lessonId: string }>();
  const eid = Number(enrollmentId);
  const lid = Number(lessonId);
  const { user } = useAuth();
  const navigate = useNavigate();
  const studentId = user?.studentId;
  const [data, setData] = useState<StudentCabinetLessonDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (studentId == null || !Number.isFinite(eid) || !Number.isFinite(lid)) {
      setLoading(false);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const d = await getCabinetLesson(studentId, eid, lid);
        if (!cancelled) setData(d);
      } catch {
        if (!cancelled) setError('Урок не найден или нет доступа.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [studentId, eid, lid]);

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

  if (!Number.isFinite(eid) || !Number.isFinite(lid)) {
    navigate('/learn', { replace: true });
    return null;
  }

  const videoUrl = data?.videoUrl?.trim() || '';

  return (
    <div>
      <Link className="cabinet-back" to={`/learn/courses/${eid}`}>
        ← К курсу
      </Link>
      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}
      {!loading && data && (
        <>
          <h1 className="cabinet-page-title">{dash(data.title)}</h1>
          <p className="cabinet-page-lead">
            Модуль: {dash(data.moduleTitle)} · Тип урока: {dash(data.lessonTypeName ?? (data.lessonTypeId != null ? String(data.lessonTypeId) : null))}
          </p>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Видео
            </h2>
            {videoUrl ? (
              <>
                {isDirectVideoUrl(videoUrl) ? (
                  <div className="cabinet-video-wrap">
                    <video controls src={videoUrl} />
                  </div>
                ) : (
                  <p>
                    <a href={videoUrl} target="_blank" rel="noopener noreferrer">
                      Открыть ссылку на видео
                    </a>
                  </p>
                )}
              </>
            ) : (
              <p>-</p>
            )}
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Описание
            </h2>
            {data.content?.trim() ? (
              <div
                className="cabinet-lesson-html"
                dangerouslySetInnerHTML={{ __html: data.content }}
              />
            ) : (
              <p>-</p>
            )}
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Материалы
            </h2>
            {data.materials.length === 0 && <p>-</p>}
            <ul>
              {data.materials.map((m) => (
                <li key={m.materialId}>
                  <a href={m.fileUrl} target="_blank" rel="noopener noreferrer">
                    {dash(m.fileName)}
                  </a>
                  {m.fileType != null ? ` (${m.fileType})` : ''}
                  {m.fileSizeKb != null ? ` · ${m.fileSizeKb} КБ` : ''}
                </li>
              ))}
            </ul>
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Доступ и прогресс
            </h2>
            <dl className="cabinet-dl">
              <dt>Плановая дата доступа</dt>
              <dd>{data.access ? formatDate(data.access.plannedAccessDate) : '-'}</dd>
              <dt>Плановое время</dt>
              <dd>{dash(data.access?.plannedAccessTime)}</dd>
              <dt>Фактическое открытие</dt>
              <dd>{data.access?.actualOpenDatetime ? formatDateTime(data.access.actualOpenDatetime) : '-'}</dd>
              <dt>Доступен</dt>
              <dd>{data.access?.isAvailable == null ? '-' : data.access.isAvailable ? 'да' : 'нет'}</dd>
              <dt>Урок пройден</dt>
              <dd>{data.progress?.isCompleted == null ? '-' : data.progress.isCompleted ? 'да' : 'нет'}</dd>
              <dt>Завершён</dt>
              <dd>{data.progress?.completedAt ? formatDateTime(data.progress.completedAt) : '-'}</dd>
              <dt>Время просмотра, сек</dt>
              <dd>{dash(data.progress?.watchTimeSeconds)}</dd>
              <dt>Последний заход</dt>
              <dd>{data.progress?.lastAccessed ? formatDateTime(data.progress.lastAccessed) : '-'}</dd>
            </dl>
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Домашние задания
            </h2>
            {data.assignments.length === 0 && <p>-</p>}
            {data.assignments.map((a) => (
              <div key={a.assignmentId} style={{ marginBottom: '16px', paddingBottom: '16px', borderBottom: '1px solid #e2e8f0' }}>
                <strong>{dash(a.title)}</strong>
                <div style={{ fontSize: '0.88rem', color: '#64748b', marginTop: '6px' }}>
                  Тип: {dash(a.assignmentTypeName ?? String(a.assignmentTypeId))} · Макс. балл: {a.maxScore}
                </div>
                {a.description && <p style={{ marginTop: '8px' }}>{a.description}</p>}
                <dl className="cabinet-dl" style={{ marginTop: '10px' }}>
                  <dt>Дней после урока (из БД)</dt>
                  <dd>{dash(a.dueDaysAfterLesson)}</dd>
                  <dt>Расчётная дата сдачи</dt>
                  <dd>{a.calculatedDueDate ? formatDate(a.calculatedDueDate) : '-'}</dd>
                </dl>
                <Link
                  className="cabinet-assignment-cta"
                  to={`/learn/courses/${eid}/lessons/${lid}/assignments/${a.assignmentId}`}
                >
                  {assignmentCtaLabel(a.assignmentTypeName)}
                </Link>
              </div>
            ))}
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Отправки работ (из БД)
            </h2>
            {data.submissions.length === 0 && <p>-</p>}
            <ul>
              {data.submissions.map((s) => (
                <li key={s.submissionId}>
                  {formatDateTime(s.submittedAt)} — балл: {dash(s.score)} — статус: {dash(s.submissionStatusName)}
                  {s.teacherComment ? ` — комментарий: ${s.teacherComment}` : ''}
                </li>
              ))}
            </ul>
          </div>
        </>
      )}
    </div>
  );
};

export default LessonPage;
