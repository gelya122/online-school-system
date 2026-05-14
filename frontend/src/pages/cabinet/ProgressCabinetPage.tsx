import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import {
  getCabinetProgressDashboard,
  type StudentCabinetProgressDashboard,
  type StudentCabinetProgressWeekChartPoint,
} from '../../api/studentCabinet';
import './cabinetPages.css';
import './ProgressCabinetPage.css';

function dash(v: unknown): string {
  if (v === null || v === undefined) return '—';
  if (typeof v === 'string' && v.trim() === '') return '—';
  return String(v);
}

function formatStudyTime(totalSeconds: number): string {
  if (!totalSeconds || totalSeconds < 0) return '0 ч';
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  if (h <= 0) return `${m} мин`;
  return m > 0 ? `${h} ч ${m} мин` : `${h} ч`;
}

function formatFinalScore(v: number | null | undefined): string {
  if (v == null || Number.isNaN(Number(v))) return '—';
  return Number.isInteger(Number(v)) ? String(v) : Number(v).toFixed(1);
}

function DualWeekLineChart({
  title,
  points,
}: {
  title: string;
  points: StudentCabinetProgressWeekChartPoint[];
}) {
  const w = 640;
  const h = 220;
  const padL = 46;
  const padR = 20;
  const padT = 18;
  const padB = 40;
  const chartW = w - padL - padR;
  const chartH = h - padT - padB;
  const n = points.length;
  const denom = Math.max(n - 1, 1);

  const xAt = (i: number) => padL + (chartW * i) / denom;
  const yAt = (pct: number) => padT + chartH - (chartH * Math.min(100, Math.max(0, pct))) / 100;

  const pathStudent =
    n === 0
      ? ''
      : points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${xAt(i)} ${yAt(p.studentPercent)}`).join(' ');
  const pathGroup =
    n === 0
      ? ''
      : points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${xAt(i)} ${yAt(p.groupAveragePercent)}`).join(' ');

  const gridYs = [0, 25, 50, 75, 100];

  return (
    <div className="progress-chart-block">
      <h3 className="progress-chart-title">{title}</h3>
      <div className="progress-chart-svg-wrap">
        <svg viewBox={`0 0 ${w} ${h}`} width="100%" height={h} aria-hidden>
          {gridYs.map((gy) => (
            <line
              key={gy}
              x1={padL}
              x2={w - padR}
              y1={yAt(gy)}
              y2={yAt(gy)}
              stroke="#e5e7eb"
              strokeWidth={1}
            />
          ))}
          <text x={padL - 6} y={yAt(100)} fill="#94a3b8" fontSize={11} textAnchor="end">
            100%
          </text>
          <text x={padL - 6} y={yAt(0)} fill="#94a3b8" fontSize={11} textAnchor="end">
            0%
          </text>
          {pathGroup && (
            <path d={pathGroup} fill="none" stroke="#94a3b8" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
          )}
          {pathStudent && (
            <path d={pathStudent} fill="none" stroke="#2563eb" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
          )}
          {points.map((p, i) => (
            <g key={p.weekNumber}>
              <circle cx={xAt(i)} cy={yAt(p.studentPercent)} r={4} fill="#2563eb" />
              <circle cx={xAt(i)} cy={yAt(p.groupAveragePercent)} r={4} fill="#94a3b8" />
              <text x={xAt(i)} y={h - 12} fill="#64748b" fontSize={11} textAnchor="middle">
                нед. {p.weekNumber}
              </text>
            </g>
          ))}
        </svg>
      </div>
      <div className="progress-chart-legend">
        <span>
          <span className="progress-legend-dot" style={{ background: '#2563eb' }} />
          Вы
        </span>
        <span>
          <span className="progress-legend-dot" style={{ background: '#94a3b8' }} />
          Среднее по потоку
        </span>
      </div>
    </div>
  );
}

function CohortComparisonChart({ row }: { row: StudentCabinetProgressDashboard }) {
  const studentHours = row.totalStudySeconds / 3600;
  const maxHours = Math.max(studentHours, row.cohort.groupAvgStudyHours, 0.25) * 1.15;
  const assignStudent = row.averageAssignmentPercent ?? 0;
  const assignGroup = row.cohort.groupAvgAssignmentPercent;

  const bars: { label: string; student: number; group: number }[] = [
    {
      label: 'Уроки',
      student: row.lessonProgressPercent,
      group: row.cohort.groupAvgLessonPercent,
    },
    {
      label: 'Задания',
      student: assignStudent,
      group: assignGroup,
    },
    {
      label: 'Время',
      student: (studentHours / maxHours) * 100,
      group: (row.cohort.groupAvgStudyHours / maxHours) * 100,
    },
  ];

  const w = 560;
  const h = 168;
  const baseY = h - 28;
  const barMaxH = h - 52;
  const groupW = w / 3;

  return (
    <div className="progress-chart-block">
      <h3 className="progress-chart-title">Сравнение с группой (поток)</h3>
      <div className="progress-chart-svg-wrap">
        <svg viewBox={`0 0 ${w} ${h}`} width="100%" height={h}>
          {bars.map((b, idx) => {
            const cx = idx * groupW + groupW / 2;
            const wBar = 28;
            const hS = (barMaxH * Math.min(100, b.student)) / 100;
            const hG = (barMaxH * Math.min(100, b.group)) / 100;
            return (
              <g key={b.label}>
                <rect x={cx - wBar - 6} y={baseY - hS} width={wBar} height={hS} rx={4} fill="#2563eb" opacity={0.9} />
                <rect x={cx + 6} y={baseY - hG} width={wBar} height={hG} rx={4} fill="#94a3b8" opacity={0.95} />
                <text x={cx} y={h - 8} fill="#475569" fontSize={12} textAnchor="middle">
                  {b.label}
                </text>
                {b.label === 'Время' ? (
                  <text x={cx} y={18} fill="#64748b" fontSize={10} textAnchor="middle">
                    {formatStudyTime(row.totalStudySeconds)} / {row.cohort.groupAvgStudyHours.toFixed(1)} ч
                  </text>
                ) : (
                  <text x={cx} y={18} fill="#64748b" fontSize={10} textAnchor="middle">
                    {b.student.toFixed(0)}% / {b.group.toFixed(0)}%
                  </text>
                )}
              </g>
            );
          })}
          <line x1={24} x2={w - 24} y1={baseY} y2={baseY} stroke="#e5e7eb" strokeWidth={1} />
        </svg>
      </div>
      <div className="progress-chart-legend">
        <span>
          <span className="progress-legend-dot" style={{ background: '#2563eb' }} />
          Вы
        </span>
        <span>
          <span className="progress-legend-dot" style={{ background: '#94a3b8' }} />
          Среднее по потоку
        </span>
      </div>
    </div>
  );
}

const ProgressCabinetPage = () => {
  const { user } = useAuth();
  const studentId = user?.studentId;
  const [courses, setCourses] = useState<StudentCabinetProgressDashboard[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (studentId == null) {
      setLoading(false);
      setCourses([]);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await getCabinetProgressDashboard(studentId);
        if (!cancelled) setCourses(data);
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

  const sortedCourses = useMemo(
    () => [...courses].sort((a, b) => a.courseTitle.localeCompare(b.courseTitle, 'ru')),
    [courses],
  );

  if (studentId == null) {
    return (
      <div>
        <h1 className="cabinet-page-title">Прогресс</h1>
        <div className="cabinet-alert">Профиль ученика не привязан к аккаунту.</div>
      </div>
    );
  }

  return (
    <div className="progress-page">
      <h1 className="cabinet-page-title">Прогресс</h1>
      <p className="cabinet-page-lead">
        Показатели по каждому курсу: завершение уроков, задания и сравнение со средним по вашему потоку.
      </p>
      {error && <div className="cabinet-error">{error}</div>}
      {loading && <p>Загрузка...</p>}
      {!loading && !error && sortedCourses.length === 0 && <p>У вас пока нет записей на курсы.</p>}
      {!loading &&
        sortedCourses.map((row) => (
          <article key={row.enrollmentId} className="progress-course-card">
            <h2 className="progress-course-title">{dash(row.courseTitle)}</h2>
            <p className="progress-course-meta">Поток: {dash(row.instanceName)}</p>

            <div className="progress-metrics-grid">
              <div className="progress-metric">
                <div className="progress-metric-label">Общий прогресс по урокам</div>
                <div className="progress-metric-value">{row.lessonProgressPercent}%</div>
              </div>
              <div className="progress-metric">
                <div className="progress-metric-label">Завершено уроков</div>
                <div className="progress-metric-value">
                  {row.completedLessons} / {row.totalLessons}
                </div>
              </div>
              <div className="progress-metric">
                <div className="progress-metric-label">Финальная оценка</div>
                <div className="progress-metric-value">{formatFinalScore(row.finalScore ?? undefined)}</div>
              </div>
              <div className="progress-metric">
                <div className="progress-metric-label">Общее время обучения</div>
                <div className="progress-metric-value">{formatStudyTime(row.totalStudySeconds)}</div>
              </div>
              <div className="progress-metric">
                <div className="progress-metric-label">Средний балл по заданиям</div>
                <div className="progress-metric-value">
                  {row.averageAssignmentPercent != null ? `${row.averageAssignmentPercent}%` : '—'}
                </div>
              </div>
              <div className="progress-metric">
                <div className="progress-metric-label">Сдано заданий</div>
                <div className="progress-metric-value">{row.submittedAssignmentsCount}</div>
              </div>
              <div className="progress-metric">
                <div className="progress-metric-label">Успешно сдано (&gt;50% баллов)</div>
                <div className="progress-metric-value">{row.successfulAssignmentsPercent}%</div>
              </div>
            </div>

            <p className="progress-cohort-note">Размер группы (ваш поток): {row.cohort.groupSize}</p>

            {row.weeklyLessonProgress.length > 0 && (
              <DualWeekLineChart title="Прогресс по неделям (накопленная доля завершённых уроков)" points={row.weeklyLessonProgress} />
            )}
            {row.weeklyPerformance.length > 0 && (
              <DualWeekLineChart title="Успеваемость по неделям (средний % баллов за сданные работы)" points={row.weeklyPerformance} />
            )}
            <CohortComparisonChart row={row} />
          </article>
        ))}
    </div>
  );
};

export default ProgressCabinetPage;
