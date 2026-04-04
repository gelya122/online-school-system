Схема БД (online_school_db)
============================

1) Источник правды — миграции Entity Framework в проекте API:
   backend/OnlineSchoolAPI/OnlineSchoolAPI/Migrations/

2) Применить схему к SQL Server (из корня репозитория):
   dotnet ef database update --project backend/OnlineSchoolAPI/OnlineSchoolAPI/OnlineSchoolAPI.csproj --startup-project backend/OnlineSchoolAPI/OnlineSchoolAPI/OnlineSchoolAPI.csproj

   Строка подключения (ключ OnlineSchoolConnection):
   - appsettings.json — запасной вариант (по умолчанию LocalDB, для шаблона репозитория);
   - appsettings.Development.json — ваш реальный SQL Server при ASPNETCORE_ENVIRONMENT=Development
     (в Visual Studio / dotnet run в режиме Debug это обычно уже так);
   - либо переменная окружения ConnectionStrings__OnlineSchoolConnection (перекрывает файлы).

3) Идемпотентный SQL из миграций (ручное развёртывание / обзор DDL):
   В репозитории поддерживается актуальная копия: database/schema_from_migrations.sql
   Пересобрать её из корня репозитория:
   dotnet ef migrations script --idempotent -o database/schema_from_migrations.sql --context OnlineSchoolDbContext
   (запускать из каталога backend/OnlineSchoolAPI/OnlineSchoolAPI, где лежит OnlineSchoolAPI.csproj).
   Файл database/schema.sql при необходимости замените этим же содержимым или удалите, если используете только копию выше.

4) После изменения модели создавайте новую миграцию:
   dotnet ef migrations add <Имя> --project ... --startup-project ...
