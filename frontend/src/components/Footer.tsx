import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import './Footer.css';
import type { SchoolSetting } from '../types';
import { getSchoolSettings } from '../api/schoolSettings';

const Footer = () => {
  const [settings, setSettings] = useState<SchoolSetting | null>(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        const data = await getSchoolSettings();
        if (cancelled) return;
        setSettings(data);
      } catch {
        if (cancelled) return;
        setSettings(null);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const contactAddress = settings?.address ?? '—';
  const contactPhone = settings?.contactPhone ?? '';
  const contactEmail = settings?.contactEmail ?? '';

  return (
    <footer className="footer">
      <div className="footer-container">
        <div className="footer-grid">
          <div className="footer-col logo-col">
            <h3 className="footer-logo">EduSchool</h3>
            <p className="footer-description">
              Онлайн-школа подготовки к ОГЭ и ЕГЭ. Помогаем ученикам сдать экзамены на высокие баллы и поступить в вуз мечты.
            </p>
          </div>

          <div className="footer-col">
            <h4>Обучение</h4>
            <ul>
              <li>
                <Link to="/courses">Все курсы</Link>
              </li>
            </ul>
          </div>

          <div className="footer-col">
            <h4>О школе</h4>
            <ul>
              <li>
                <Link to="/about#about-article">О нас</Link>
              </li>
              <li>
                <Link to="/about#about-teachers">Преподаватели</Link>
              </li>
              <li>
                <Link to="/about#about-reviews">Отзывы</Link>
              </li>
              <li>
                <Link to="/about#about-documents">Документы</Link>
              </li>
            </ul>
          </div>

          <div className="footer-col">
            <h4>Контакты</h4>
            <ul className="contact-info">
              <li>{contactAddress}</li>
              {contactPhone && (
                <li>
                  <a href={`tel:${contactPhone}`}>{contactPhone}</a>
                </li>
              )}
              {contactEmail && (
                <li>
                  <a href={`mailto:${contactEmail}`}>{contactEmail}</a>
                </li>
              )}
            </ul>
          </div>
        </div>

        <div className="footer-bottom">
          <p>© 2024 EduSchool. Все права защищены.</p>
          <div className="footer-links">
            <Link to="/about#about-documents">Политика конфиденциальности</Link>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;