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

function isDirectVideoUrl(url: string): boolean {
  return /\.(mp4|webm|ogg)(\?.*)?$/i.test(url);
}

function assignmentCtaLabel(): string {
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
            {dash(data.lessonTypeName ?? (data.lessonTypeId != null ? String(data.lessonTypeId) : null))}
          </p>

          <div className="cabinet-panel">
            {videoUrl ? (
              <>
                {isDirectVideoUrl(videoUrl) ? (
                  <div className="cabinet-video-wrap">
                    <video controls src={videoUrl} />
                  </div>
                ) : (
                  <p>
                    <a href={videoUrl} target="_blank" rel="noopener noreferrer">
                      Открыть ссылку
                    </a>
                  </p>
                )}
              </>
            ) : (
              <div className="cabinet-video-placeholder">Видео пока не добавлено</div>
            )}
          </div>

          {data.content?.trim() && (
            <div className="cabinet-panel">
              <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                Описание
              </h2>
              <div
                className="cabinet-lesson-html"
                dangerouslySetInnerHTML={{ __html: data.content }}
              />
            </div>
          )}

          {data.materials.length > 0 && (
            <div className="cabinet-panel">
              <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
                Материалы к уроку
              </h2>
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
          )}

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Домашние задания
            </h2>
            {data.assignments.length === 0 && <p>-</p>}
            {data.assignments.map((a) => (
              <div key={a.assignmentId} style={{ marginBottom: '16px', paddingBottom: '16px', borderBottom: '1px solid #e2e8f0' }}>
                <strong>{dash(a.title)}</strong>
                <div style={{ fontSize: '0.88rem', color: '#64748b', marginTop: '6px' }}>
                  Макс. балл: {a.maxScore}
                </div>
                {a.description && <p style={{ marginTop: '8px' }}>{a.description}</p>}
                <dl className="cabinet-dl" style={{ marginTop: '10px' }}>
                  <dt>Дедлайн</dt>
                  <dd>{a.calculatedDueDate ? formatDate(a.calculatedDueDate) : '-'}</dd>
                </dl>
                <Link
                  className="cabinet-assignment-cta"
                  to={`/learn/courses/${eid}/lessons/${lid}/assignments/${a.assignmentId}`}
                >
                  {assignmentCtaLabel()}
                </Link>
              </div>
            ))}
          </div>

          <div className="cabinet-panel">
            <h2 className="cabinet-page-title" style={{ fontSize: '1.1rem', marginBottom: '12px' }}>
              Твои ответы
            </h2>
            {data.submissions.length === 0 && <p>-</p>}
            <ul style={{ listStyle: 'none', paddingLeft: 0 }}>
              {data.submissions.map((s) => (
                <li key={s.submissionId}>
                  {formatDate(s.submittedAt)} | балл: {dash(s.score)} | статус: {dash(s.submissionStatusName)}
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
