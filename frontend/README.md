# Frontend — Online School System

Клиентская часть онлайн-школы: **React 19**, **TypeScript**, **Vite**.

## Технологии

- **React 19** — UI
- **TypeScript** — типизация
- **Vite** — сборка и dev-сервер
- **React Router** — маршрутизация
- **Axios** — HTTP к бэкенду
- **Context API** — авторизация и корзина (`AuthContext`, `CartContext`)

## Структура `src/`

```
src/
├── api/                    # Клиенты API (axiosInstance + доменные модули)
│   ├── axiosInstance.ts
│   ├── auth.ts
│   ├── courses.ts
│   ├── faq.ts
│   ├── filters.ts
│   ├── paymentMethods.ts
│   ├── promoCodes.ts
│   ├── reviews.ts
│   ├── schoolSettings.ts
│   ├── trialApplications.ts
│   └── employees.ts
├── components/             # Layout, Header, Footer, PrivateRoute
├── contexts/               # AuthContext, CartContext
├── pages/
│   ├── HomePage.tsx
│   ├── CoursesPage.tsx
│   ├── CourseDetailPage.tsx
│   ├── AboutPage.tsx
│   ├── CartPage.tsx
│   ├── CheckoutPage.tsx
│   ├── LoginPage.tsx
│   └── ProfilePage.tsx
├── types/
├── router.tsx
├── App.tsx
└── main.tsx
```

## Установка и запуск

### Зависимости

```bash
npm install
```

### Переменные окружения

Скопируйте пример и при необходимости поправьте URL API:

```bash
copy env.example .env
```

В `.env` (не коммитится):

```env
VITE_API_BASE_URL=http://localhost:5189/api
```

Убедитесь, что API запущен (порт по умолчанию в прокси — **5189**).

### Dev-сервер

```bash
npm run dev
```

Vite поднимает сервер со встроенным **прокси** `/api` → `http://localhost:5189` (см. `vite.config.ts`), чтобы в разработке обходить CORS.

### Production-сборка

```bash
npm run build
```

Артефакты — в каталоге `dist/`.

```bash
npm run preview   # предпросмотр сборки
npm run lint      # ESLint
```

## API

Используйте общий экземпляр из `src/api/axiosInstance.ts` — токен подставляется interceptor’ом.

```typescript
import axiosInstance from './api/axiosInstance';

const { data } = await axiosInstance.get('/courses');
```

## Маршруты

Публичные: главная, каталог курсов, карточка курса, о сайте, корзина, оформление заказа, вход.  
Защищённые (`PrivateRoute`): профиль.

Новые страницы: компонент в `pages/`, запись в `router.tsx`, при необходимости ссылка в `Header.tsx`.

## Стили

Глобально — `index.css`, рядом с компонентами — отдельные `.css` файлы.

## Монорепозиторий

Бэкенд лежит в каталоге `backend/` на уровень выше `frontend/`. Схема БД и миграции — в корне репозитория (`database/`, проект `OnlineSchoolAPI`).
