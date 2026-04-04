using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchoolAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncSchemaPaymentMethod20260404 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "app_order");

            migrationBuilder.AlterColumn<string>(
                name: "selected_subjects",
                table: "trial_application",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                table: "trial_application",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "class_number",
                table: "trial_application",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "terms_of_use_url",
                table: "school_setting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "privacy_policy_url",
                table: "school_setting",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "method_id",
                table: "payment",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "discount_type",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "method_id",
                table: "app_order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_method",
                columns: table => new
                {
                    method_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    method_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_method", x => x.method_id);
                });

            migrationBuilder.Sql("""
                INSERT INTO payment_method (method_name, description, is_active) VALUES
                (N'Банковская карта', NULL, CAST(1 AS bit)),
                (N'Онлайн‑кошелёк', NULL, CAST(1 AS bit)),
                (N'Безналичный расчёт', NULL, CAST(1 AS bit));
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_review_rating",
                table: "review",
                sql: "[rating] IS NULL OR ([rating] >= 1 AND [rating] <= 5)");

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_type_id",
                table: "promo_code",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_method_id",
                table: "payment",
                column: "method_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_order_method_id",
                table: "app_order",
                column: "method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_app_order_payment_method",
                table: "app_order",
                column: "method_id",
                principalTable: "payment_method",
                principalColumn: "method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_payment_method",
                table: "payment",
                column: "method_id",
                principalTable: "payment_method",
                principalColumn: "method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_promo_code_discount_type",
                table: "promo_code",
                column: "type_id",
                principalTable: "discount_type",
                principalColumn: "type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_app_order_payment_method",
                table: "app_order");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_payment_method",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_promo_code_discount_type",
                table: "promo_code");

            migrationBuilder.Sql("""
                DELETE FROM payment_method WHERE method_name IN (
                    N'Банковская карта', N'Онлайн‑кошелёк', N'Безналичный расчёт');
                """);

            migrationBuilder.DropTable(
                name: "payment_method");

            migrationBuilder.DropCheckConstraint(
                name: "CK_review_rating",
                table: "review");

            migrationBuilder.DropIndex(
                name: "IX_promo_code_type_id",
                table: "promo_code");

            migrationBuilder.DropIndex(
                name: "IX_payment_method_id",
                table: "payment");

            migrationBuilder.DropIndex(
                name: "IX_app_order_method_id",
                table: "app_order");

            migrationBuilder.DropColumn(
                name: "method_id",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "method_id",
                table: "app_order");

            migrationBuilder.AlterColumn<string>(
                name: "selected_subjects",
                table: "trial_application",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                table: "trial_application",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "class_number",
                table: "trial_application",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "terms_of_use_url",
                table: "school_setting",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "privacy_policy_url",
                table: "school_setting",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "payment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "discount_type",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "app_order",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
