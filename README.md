# Online School System

Монорепозиторий онлайн-школы: **ASP.NET Core Web API** (SQL Server, Entity Framework) и **React + TypeScript + Vite**.

## Структура

| Каталог | Описание |
|--------|----------|
| `backend/OnlineSchoolAPI/OnlineSchoolAPI/` | REST API (.NET 9) |
| `frontend/` | Веб-клиент |
| `database/` | Выгрузка схемы SQL и подсказки по миграциям |

Корневое решение Visual Studio: `OnlineSchoolSystem.sln`.

## Требования

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)
- SQL Server или [LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)

## База данных

Строка подключения: `ConnectionStrings:OnlineSchoolConnection` в `backend/OnlineSchoolAPI/OnlineSchoolAPI/appsettings.json` (по умолчанию LocalDB).

Применить миграции из корня репозитория:

```bash
dotnet tool restore
dotnet ef database update --project backend/OnlineSchoolAPI/OnlineSchoolAPI/OnlineSchoolAPI.csproj --startup-project backend/OnlineSchoolAPI/OnlineSchoolAPI/OnlineSchoolAPI.csproj
```

Подробнее: [`database/README.txt`](database/README.txt).

## Backend

```bash
cd backend/OnlineSchoolAPI/OnlineSchoolAPI
dotnet run
```

Порт смотрите в `Properties/launchSettings.json` (часто `https://localhost:7xxx` и HTTP для прокси). Swagger: `/swagger`.

## Frontend

```bash
cd frontend
npm install
copy env.example .env
npm run dev
```

Детали по фронтенду: [`frontend/README.md`](frontend/README.md).

## Лицензия

Укажите лицензию при необходимости.
