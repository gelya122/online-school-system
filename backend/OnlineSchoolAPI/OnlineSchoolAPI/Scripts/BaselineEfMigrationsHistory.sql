-- Use this ONLY if your database already contains the schema tables,
-- but EF Core migrations have never been applied (no __EFMigrationsHistory).
-- It "stamps" existing migrations as applied, without executing them.

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory
    (
        MigrationId    nvarchar(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
        ProductVersion nvarchar(32)  NOT NULL
    );
END;
GO

-- EF Core version used by this project migrations
DECLARE @pv nvarchar(32) = N'9.0.0';

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260403165424_InitialCreate')
    INSERT INTO dbo.__EFMigrationsHistory(MigrationId, ProductVersion) VALUES (N'20260403165424_InitialCreate', @pv);

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260404181627_SyncSchemaPaymentMethod20260404')
    INSERT INTO dbo.__EFMigrationsHistory(MigrationId, ProductVersion) VALUES (N'20260404181627_SyncSchemaPaymentMethod20260404', @pv);

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260502120000_AddTrialApplicationCourseId')
    INSERT INTO dbo.__EFMigrationsHistory(MigrationId, ProductVersion) VALUES (N'20260502120000_AddTrialApplicationCourseId', @pv);

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260505180239_AddAdminDesktopDatabaseImprovements')
    INSERT INTO dbo.__EFMigrationsHistory(MigrationId, ProductVersion) VALUES (N'20260505180239_AddAdminDesktopDatabaseImprovements', @pv);

-- Если база уже развёрнута из script6.sql, отметьте все миграции выше и не выполняйте «update» на старой схеме,
-- либо выполните только: dotnet ef database update (оставшиеся миграции должны быть no-op или отсутствовать).
