Схема БД (online_school_db)
============================

1) Источник правды — миграции Entity Framework в проекте API:
   backend/OnlineSchoolAPI/OnlineSchoolAPI/Migrations/

2) Применить схему к SQL Server (из корня репозитория):
   dotnet ef database update --project backend/OnlineSchoolAPI/OnlineSchoolAPI/OnlineSchoolAPI.csproj --startup-project backend/OnlineSchoolAPI/OnlineSchoolAPI/OnlineSchoolAPI.csproj

   Строка подключения задаётся в appsettings.json (ключ OnlineSchoolConnection)
   или через переменную окружения ConnectionStrings__OnlineSchoolConnection.

3) Файл schema.sql — идемпотентный SQL, сгенерированный из миграций
   (удобно для ручного развёртывания или обзора DDL):
   dotnet ef migrations script --idempotent -o <путь>/database/schema.sql
   (запускать из каталога с OnlineSchoolAPI.csproj, см. документацию dotnet-ef).

4) После изменения модели создавайте новую миграцию:
   dotnet ef migrations add <Имя> --project ... --startup-project ...
