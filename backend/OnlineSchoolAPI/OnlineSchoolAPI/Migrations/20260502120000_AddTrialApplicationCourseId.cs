using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchoolAPI.Migrations;

/// <inheritdoc />
/// Схема <c>trial_application</c> без <c>course_id</c> (см. script6.sql). Миграция оставлена в цепочке как no-op,
/// чтобы <c>dotnet ef database update</c> не добавлял столбец обратно на БД, уже приведённых к script6.
public partial class AddTrialApplicationCourseId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
