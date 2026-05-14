using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchoolAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminDesktopDatabaseImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "created_by_employee_id",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "login",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_changed_at",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "users",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "converted_at",
                table: "trial_application",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "trial_application",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_contact_at",
                table: "trial_application",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "trial_application",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "student_id",
                table: "trial_application",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "utm_campaign",
                table: "trial_application",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "utm_medium",
                table: "trial_application",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "utm_source",
                table: "trial_application",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "student",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "applies_to_course_id",
                table: "promo_code",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "applies_to_instance_id",
                table: "promo_code",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_employee_id",
                table: "promo_code",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "promo_code",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "max_discount_amount",
                table: "promo_code",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "min_order_amount",
                table: "promo_code",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "promo_code",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_method_id",
                table: "payment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "campaign_id",
                table: "notification",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "read_at",
                table: "notification",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "lesson",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_id",
                table: "installment_payment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_status_id",
                table: "installment_payment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "employee",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "course_module",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                table: "course_instance",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by_employee_id",
                table: "course_instance",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "course_instance",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "enrollment_end_date",
                table: "course_instance",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "enrollment_start_date",
                table: "course_instance",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "course_instance",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "draft");

            migrationBuilder.AddColumn<string>(
                name: "timezone",
                table: "course_instance",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "course_instance",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "course",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_method_id",
                table: "app_order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "promo_code_id",
                table: "app_order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    audit_log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<int>(type: "int", nullable: true),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.audit_log_id);
                    table.ForeignKey(
                        name: "FK_audit_log_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_audit_log_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "course_instance_teacher",
                columns: table => new
                {
                    instance_teacher_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    instance_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    is_main_teacher = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_instance_teacher", x => x.instance_teacher_id);
                    table.ForeignKey(
                        name: "FK_course_instance_teacher_course_instance",
                        column: x => x.instance_id,
                        principalTable: "course_instance",
                        principalColumn: "instance_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_instance_teacher_employee",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "file_storage",
                columns: table => new
                {
                    file_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    original_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    stored_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    uploaded_by_user_id = table.Column<int>(type: "int", nullable: true),
                    related_entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    related_entity_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_storage", x => x.file_id);
                    table.ForeignKey(
                        name: "FK_file_storage_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "homework",
                columns: table => new
                {
                    homework_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    max_score = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    due_days_after_lesson = table.Column<int>(type: "int", nullable: true),
                    is_required = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    homework_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework", x => x.homework_id);
                    table.ForeignKey(
                        name: "FK_homework_lesson",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "lesson_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mailing_campaign",
                columns: table => new
                {
                    campaign_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "internal"),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "draft"),
                    target_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mailing_campaign", x => x.campaign_id);
                    table.ForeignKey(
                        name: "FK_mailing_campaign_employee_created_by",
                        column: x => x.created_by_employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "promo_code_usage",
                columns: table => new
                {
                    usage_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    promo_code_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_code_usage", x => x.usage_id);
                    table.ForeignKey(
                        name: "FK_promo_code_usage_app_order",
                        column: x => x.order_id,
                        principalTable: "app_order",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "FK_promo_code_usage_promo_code",
                        column: x => x.promo_code_id,
                        principalTable: "promo_code",
                        principalColumn: "promo_code_id");
                    table.ForeignKey(
                        name: "FK_promo_code_usage_student",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "site_banner",
                columns: table => new
                {
                    banner_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    subtitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    button_text = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    button_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    banner_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_banner", x => x.banner_id);
                });

            migrationBuilder.CreateTable(
                name: "site_setting",
                columns: table => new
                {
                    setting_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    site_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    main_page_title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    main_page_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact_phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    contact_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    vk_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    telegram_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    youtube_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    seo_title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    seo_description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_maintenance_mode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    updated_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_site_setting", x => x.setting_id);
                    table.ForeignKey(
                        name: "FK_site_setting_employee_updated_by",
                        column: x => x.updated_by_employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "student_note",
                columns: table => new
                {
                    note_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    note_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    note_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_note", x => x.note_id);
                    table.ForeignKey(
                        name: "FK_student_note_employee",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_student_note_student",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trial_application_comment",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    application_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: true),
                    comment_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trial_application_comment", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_trial_application_comment_employee",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_trial_application_comment_trial_application",
                        column: x => x.application_id,
                        principalTable: "trial_application",
                        principalColumn: "application_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trial_application_subject",
                columns: table => new
                {
                    application_subject_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    application_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trial_application_subject", x => x.application_subject_id);
                    table.ForeignKey(
                        name: "FK_trial_application_subject_subject",
                        column: x => x.subject_id,
                        principalTable: "subject",
                        principalColumn: "subject_id");
                    table.ForeignKey(
                        name: "FK_trial_application_subject_trial_application",
                        column: x => x.application_id,
                        principalTable: "trial_application",
                        principalColumn: "application_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_submission",
                columns: table => new
                {
                    submission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    homework_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    enrollment_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "draft"),
                    total_score = table.Column<int>(type: "int", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    checked_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    checked_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    teacher_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_submission", x => x.submission_id);
                    table.ForeignKey(
                        name: "FK_homework_submission_employee_checked_by",
                        column: x => x.checked_by_employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_homework_submission_enrollment",
                        column: x => x.enrollment_id,
                        principalTable: "enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK_homework_submission_homework",
                        column: x => x.homework_id,
                        principalTable: "homework",
                        principalColumn: "homework_id");
                    table.ForeignKey(
                        name: "FK_homework_submission_student",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "homework_task",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    homework_id = table.Column<int>(type: "int", nullable: false),
                    task_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    task_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    max_score = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    task_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    auto_check_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_task", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_homework_task_homework",
                        column: x => x.homework_id,
                        principalTable: "homework",
                        principalColumn: "homework_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mailing_recipient",
                columns: table => new
                {
                    recipient_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    sent_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    read_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mailing_recipient", x => x.recipient_id);
                    table.ForeignKey(
                        name: "FK_mailing_recipient_mailing_campaign",
                        column: x => x.campaign_id,
                        principalTable: "mailing_campaign",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mailing_recipient_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "homework_task_answer",
                columns: table => new
                {
                    answer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    answer_order = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_task_answer", x => x.answer_id);
                    table.ForeignKey(
                        name: "FK_homework_task_answer_homework_task",
                        column: x => x.task_id,
                        principalTable: "homework_task",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_task_submission",
                columns: table => new
                {
                    task_submission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    submission_id = table.Column<int>(type: "int", nullable: false),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    answer_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attached_file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    attached_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: true),
                    score = table.Column<int>(type: "int", nullable: true),
                    auto_check_result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    teacher_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    checked_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    checked_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_task_submission", x => x.task_submission_id);
                    table.ForeignKey(
                        name: "FK_homework_task_submission_employee_checked_by",
                        column: x => x.checked_by_employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK_homework_task_submission_homework_submission",
                        column: x => x.submission_id,
                        principalTable: "homework_submission",
                        principalColumn: "submission_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homework_task_submission_homework_task",
                        column: x => x.task_id,
                        principalTable: "homework_task",
                        principalColumn: "task_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_created_by_employee_id",
                table: "users",
                column: "created_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_is_active",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "UQ_users_login_not_null",
                table: "users",
                column: "login",
                unique: true,
                filter: "[login] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_student_id",
                table: "trial_application",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_last_name_first_name",
                table: "student",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_applies_to_course_id",
                table: "promo_code",
                column: "applies_to_course_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_applies_to_instance_id",
                table: "promo_code",
                column: "applies_to_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_created_by_employee_id",
                table: "promo_code",
                column: "created_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_payment_method_id",
                table: "payment",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_campaign_id",
                table: "notification",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_created_at",
                table: "notification",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notification_is_read",
                table: "notification",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "IX_installment_payment_payment_id",
                table: "installment_payment",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_installment_payment_payment_status_id",
                table: "installment_payment",
                column: "payment_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_last_name_first_name",
                table: "employee",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_created_by_employee_id",
                table: "course_instance",
                column: "created_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_order_payment_method_id",
                table: "app_order",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_order_promo_code_id",
                table: "app_order",
                column: "promo_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_employee_id",
                table: "audit_log",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_user_id",
                table: "audit_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_teacher_employee_id",
                table: "course_instance_teacher",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "UQ_course_instance_teacher_instance_employee",
                table: "course_instance_teacher",
                columns: new[] { "instance_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_file_storage_uploaded_by_user_id",
                table: "file_storage",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_lesson_id",
                table: "homework",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submission_checked_by_employee_id",
                table: "homework_submission",
                column: "checked_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submission_enrollment_id",
                table: "homework_submission",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submission_homework_id",
                table: "homework_submission",
                column: "homework_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submission_status",
                table: "homework_submission",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submission_student_id",
                table: "homework_submission",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ_homework_submission_unique",
                table: "homework_submission",
                columns: new[] { "homework_id", "student_id", "enrollment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_task_homework_id",
                table: "homework_task",
                column: "homework_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_task_answer_task_id",
                table: "homework_task_answer",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_task_submission_checked_by_employee_id",
                table: "homework_task_submission",
                column: "checked_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_task_submission_submission_id",
                table: "homework_task_submission",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_task_submission_task_id",
                table: "homework_task_submission",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_mailing_campaign_created_by_employee_id",
                table: "mailing_campaign",
                column: "created_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_mailing_recipient_user_id",
                table: "mailing_recipient",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_mailing_recipient_campaign_user",
                table: "mailing_recipient",
                columns: new[] { "campaign_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_usage_order_id",
                table: "promo_code_usage",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_promo_code_usage_student_id",
                table: "promo_code_usage",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ_promo_code_usage_promo_order",
                table: "promo_code_usage",
                columns: new[] { "promo_code_id", "order_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_site_setting_updated_by_employee_id",
                table: "site_setting",
                column: "updated_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_note_employee_id",
                table: "student_note",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_note_student_id",
                table: "student_note",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_comment_application_id",
                table: "trial_application_comment",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_comment_employee_id",
                table: "trial_application_comment",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_subject_subject_id",
                table: "trial_application_subject",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "UQ_trial_application_subject_app_subject",
                table: "trial_application_subject",
                columns: new[] { "application_id", "subject_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_app_order_payment_method_payment_method_id",
                table: "app_order",
                column: "payment_method_id",
                principalTable: "payment_method",
                principalColumn: "method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_app_order_promo_code_promo_code_id",
                table: "app_order",
                column: "promo_code_id",
                principalTable: "promo_code",
                principalColumn: "promo_code_id");

            migrationBuilder.AddForeignKey(
                name: "FK_course_instance_created_by_employee",
                table: "course_instance",
                column: "created_by_employee_id",
                principalTable: "employee",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_installment_payment_payment_payment_id",
                table: "installment_payment",
                column: "payment_id",
                principalTable: "payment",
                principalColumn: "payment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_installment_payment_payment_status_payment_status_id",
                table: "installment_payment",
                column: "payment_status_id",
                principalTable: "payment_status",
                principalColumn: "status_id");

            migrationBuilder.AddForeignKey(
                name: "FK_notification_mailing_campaign_campaign_id",
                table: "notification",
                column: "campaign_id",
                principalTable: "mailing_campaign",
                principalColumn: "campaign_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_payment_method_payment_method_id",
                table: "payment",
                column: "payment_method_id",
                principalTable: "payment_method",
                principalColumn: "method_id");

            migrationBuilder.AddForeignKey(
                name: "FK_promo_code_course_applies_to_course_id",
                table: "promo_code",
                column: "applies_to_course_id",
                principalTable: "course",
                principalColumn: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_promo_code_course_instance_applies_to_instance_id",
                table: "promo_code",
                column: "applies_to_instance_id",
                principalTable: "course_instance",
                principalColumn: "instance_id");

            migrationBuilder.AddForeignKey(
                name: "FK_promo_code_employee_created_by_employee_id",
                table: "promo_code",
                column: "created_by_employee_id",
                principalTable: "employee",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_trial_application_student_student_id",
                table: "trial_application",
                column: "student_id",
                principalTable: "student",
                principalColumn: "student_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_created_by_employee",
                table: "users",
                column: "created_by_employee_id",
                principalTable: "employee",
                principalColumn: "employee_id");

            // Backfill: new payment_method_id columns mirror existing method_id (legacy).
            migrationBuilder.Sql("""
                UPDATE ao
                SET payment_method_id = ao.method_id
                FROM app_order ao
                WHERE ao.payment_method_id IS NULL AND ao.method_id IS NOT NULL;

                UPDATE p
                SET payment_method_id = p.method_id
                FROM payment p
                WHERE p.payment_method_id IS NULL AND p.method_id IS NOT NULL;
                """);

            // Additional indexes requested for admin/desktop use (safe even with existing data).
            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_course_is_active",
                table: "course",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_start_date",
                table: "course_instance",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_status",
                table: "course_instance",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_is_active",
                table: "course_instance",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_lesson_order",
                table: "lesson",
                column: "lesson_order");

            // Enrollment: prevent double-enrollment into same instance.
            migrationBuilder.CreateIndex(
                name: "UQ_enrollment_student_instance",
                table: "enrollment",
                columns: new[] { "student_id", "instance_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_created_at",
                table: "trial_application",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_phone",
                table: "trial_application",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_email",
                table: "trial_application",
                column: "email");

            // CHECK constraints: use WITH NOCHECK to avoid failing on legacy inconsistent rows.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_student_class_number_1_11')
                    ALTER TABLE [student] WITH NOCHECK
                    ADD CONSTRAINT [CK_student_class_number_1_11] CHECK ([class_number] >= 1 AND [class_number] <= 11);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_course_price_non_negative')
                    ALTER TABLE [course] WITH NOCHECK
                    ADD CONSTRAINT [CK_course_price_non_negative] CHECK ([price] >= 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_course_discount_price_non_negative')
                    ALTER TABLE [course] WITH NOCHECK
                    ADD CONSTRAINT [CK_course_discount_price_non_negative] CHECK ([discount_price] IS NULL OR [discount_price] >= 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_promo_code_discount_value_positive')
                    ALTER TABLE [promo_code] WITH NOCHECK
                    ADD CONSTRAINT [CK_promo_code_discount_value_positive] CHECK ([discount_value] > 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_promo_code_current_uses_non_negative')
                    ALTER TABLE [promo_code] WITH NOCHECK
                    ADD CONSTRAINT [CK_promo_code_current_uses_non_negative] CHECK ([current_uses] IS NULL OR [current_uses] >= 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_promo_code_max_uses_positive')
                    ALTER TABLE [promo_code] WITH NOCHECK
                    ADD CONSTRAINT [CK_promo_code_max_uses_positive] CHECK ([max_uses] IS NULL OR [max_uses] > 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_homework_max_score_non_negative')
                    ALTER TABLE [homework] WITH NOCHECK
                    ADD CONSTRAINT [CK_homework_max_score_non_negative] CHECK ([max_score] >= 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_homework_task_max_score_non_negative')
                    ALTER TABLE [homework_task] WITH NOCHECK
                    ADD CONSTRAINT [CK_homework_task_max_score_non_negative] CHECK ([max_score] >= 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_payment_amount_positive')
                    ALTER TABLE [payment] WITH NOCHECK
                    ADD CONSTRAINT [CK_payment_amount_positive] CHECK ([amount] > 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_app_order_total_amount_non_negative')
                    ALTER TABLE [app_order] WITH NOCHECK
                    ADD CONSTRAINT [CK_app_order_total_amount_non_negative] CHECK ([total_amount] >= 0);

                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_app_order_final_amount_non_negative')
                    ALTER TABLE [app_order] WITH NOCHECK
                    ADD CONSTRAINT [CK_app_order_final_amount_non_negative] CHECK ([final_amount] >= 0);
                """);

            // Seeds: keep compatibility with existing IDs by inserting only missing well-known identity keys.
            // IMPORTANT: API uses StudentRoleId=7.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM user_role WHERE role_id = 7)
                BEGIN
                    SET IDENTITY_INSERT user_role ON;
                    IF NOT EXISTS (SELECT 1 FROM user_role WHERE role_id = 1)
                        INSERT INTO user_role(role_id, role_name, description) VALUES (1, N'admin', N'Администратор');
                    IF NOT EXISTS (SELECT 1 FROM user_role WHERE role_id = 2)
                        INSERT INTO user_role(role_id, role_name, description) VALUES (2, N'manager', N'Менеджер');
                    IF NOT EXISTS (SELECT 1 FROM user_role WHERE role_id = 3)
                        INSERT INTO user_role(role_id, role_name, description) VALUES (3, N'teacher', N'Преподаватель');
                    IF NOT EXISTS (SELECT 1 FROM user_role WHERE role_id = 7)
                        INSERT INTO user_role(role_id, role_name, description) VALUES (7, N'student', N'Ученик');
                    SET IDENTITY_INSERT user_role OFF;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_course_is_active",
                table: "course");

            migrationBuilder.DropIndex(
                name: "IX_course_instance_start_date",
                table: "course_instance");

            migrationBuilder.DropIndex(
                name: "IX_course_instance_status",
                table: "course_instance");

            migrationBuilder.DropIndex(
                name: "IX_course_instance_is_active",
                table: "course_instance");

            migrationBuilder.DropIndex(
                name: "IX_lesson_lesson_order",
                table: "lesson");

            migrationBuilder.DropIndex(
                name: "UQ_enrollment_student_instance",
                table: "enrollment");

            migrationBuilder.DropIndex(
                name: "IX_trial_application_created_at",
                table: "trial_application");

            migrationBuilder.DropIndex(
                name: "IX_trial_application_phone",
                table: "trial_application");

            migrationBuilder.DropIndex(
                name: "IX_trial_application_email",
                table: "trial_application");

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_student_class_number_1_11')
                    ALTER TABLE [student] DROP CONSTRAINT [CK_student_class_number_1_11];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_course_price_non_negative')
                    ALTER TABLE [course] DROP CONSTRAINT [CK_course_price_non_negative];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_course_discount_price_non_negative')
                    ALTER TABLE [course] DROP CONSTRAINT [CK_course_discount_price_non_negative];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_promo_code_discount_value_positive')
                    ALTER TABLE [promo_code] DROP CONSTRAINT [CK_promo_code_discount_value_positive];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_promo_code_current_uses_non_negative')
                    ALTER TABLE [promo_code] DROP CONSTRAINT [CK_promo_code_current_uses_non_negative];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_promo_code_max_uses_positive')
                    ALTER TABLE [promo_code] DROP CONSTRAINT [CK_promo_code_max_uses_positive];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_homework_max_score_non_negative')
                    ALTER TABLE [homework] DROP CONSTRAINT [CK_homework_max_score_non_negative];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_homework_task_max_score_non_negative')
                    ALTER TABLE [homework_task] DROP CONSTRAINT [CK_homework_task_max_score_non_negative];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_payment_amount_positive')
                    ALTER TABLE [payment] DROP CONSTRAINT [CK_payment_amount_positive];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_app_order_total_amount_non_negative')
                    ALTER TABLE [app_order] DROP CONSTRAINT [CK_app_order_total_amount_non_negative];
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_app_order_final_amount_non_negative')
                    ALTER TABLE [app_order] DROP CONSTRAINT [CK_app_order_final_amount_non_negative];
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_app_order_payment_method_payment_method_id",
                table: "app_order");

            migrationBuilder.DropForeignKey(
                name: "FK_app_order_promo_code_promo_code_id",
                table: "app_order");

            migrationBuilder.DropForeignKey(
                name: "FK_course_instance_created_by_employee",
                table: "course_instance");

            migrationBuilder.DropForeignKey(
                name: "FK_installment_payment_payment_payment_id",
                table: "installment_payment");

            migrationBuilder.DropForeignKey(
                name: "FK_installment_payment_payment_status_payment_status_id",
                table: "installment_payment");

            migrationBuilder.DropForeignKey(
                name: "FK_notification_mailing_campaign_campaign_id",
                table: "notification");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_payment_method_payment_method_id",
                table: "payment");

            migrationBuilder.DropForeignKey(
                name: "FK_promo_code_course_applies_to_course_id",
                table: "promo_code");

            migrationBuilder.DropForeignKey(
                name: "FK_promo_code_course_instance_applies_to_instance_id",
                table: "promo_code");

            migrationBuilder.DropForeignKey(
                name: "FK_promo_code_employee_created_by_employee_id",
                table: "promo_code");

            migrationBuilder.DropForeignKey(
                name: "FK_trial_application_student_student_id",
                table: "trial_application");

            migrationBuilder.DropForeignKey(
                name: "FK_users_created_by_employee",
                table: "users");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "course_instance_teacher");

            migrationBuilder.DropTable(
                name: "file_storage");

            migrationBuilder.DropTable(
                name: "homework_task_answer");

            migrationBuilder.DropTable(
                name: "homework_task_submission");

            migrationBuilder.DropTable(
                name: "mailing_recipient");

            migrationBuilder.DropTable(
                name: "promo_code_usage");

            migrationBuilder.DropTable(
                name: "site_banner");

            migrationBuilder.DropTable(
                name: "site_setting");

            migrationBuilder.DropTable(
                name: "student_note");

            migrationBuilder.DropTable(
                name: "trial_application_comment");

            migrationBuilder.DropTable(
                name: "trial_application_subject");

            migrationBuilder.DropTable(
                name: "homework_submission");

            migrationBuilder.DropTable(
                name: "homework_task");

            migrationBuilder.DropTable(
                name: "mailing_campaign");

            migrationBuilder.DropTable(
                name: "homework");

            migrationBuilder.DropIndex(
                name: "IX_users_created_by_employee_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_is_active",
                table: "users");

            migrationBuilder.DropIndex(
                name: "UQ_users_login_not_null",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_trial_application_student_id",
                table: "trial_application");

            migrationBuilder.DropIndex(
                name: "IX_student_last_name_first_name",
                table: "student");

            migrationBuilder.DropIndex(
                name: "IX_promo_code_applies_to_course_id",
                table: "promo_code");

            migrationBuilder.DropIndex(
                name: "IX_promo_code_applies_to_instance_id",
                table: "promo_code");

            migrationBuilder.DropIndex(
                name: "IX_promo_code_created_by_employee_id",
                table: "promo_code");

            migrationBuilder.DropIndex(
                name: "IX_payment_payment_method_id",
                table: "payment");

            migrationBuilder.DropIndex(
                name: "IX_notification_campaign_id",
                table: "notification");

            migrationBuilder.DropIndex(
                name: "IX_notification_created_at",
                table: "notification");

            migrationBuilder.DropIndex(
                name: "IX_notification_is_read",
                table: "notification");

            migrationBuilder.DropIndex(
                name: "IX_installment_payment_payment_id",
                table: "installment_payment");

            migrationBuilder.DropIndex(
                name: "IX_installment_payment_payment_status_id",
                table: "installment_payment");

            migrationBuilder.DropIndex(
                name: "IX_employee_last_name_first_name",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "IX_course_instance_created_by_employee_id",
                table: "course_instance");

            migrationBuilder.DropIndex(
                name: "IX_app_order_payment_method_id",
                table: "app_order");

            migrationBuilder.DropIndex(
                name: "IX_app_order_promo_code_id",
                table: "app_order");

            migrationBuilder.DropColumn(
                name: "created_by_employee_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "locked_until",
                table: "users");

            migrationBuilder.DropColumn(
                name: "login",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_changed_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "converted_at",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "next_contact_at",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "source",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "student_id",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "utm_campaign",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "utm_medium",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "utm_source",
                table: "trial_application");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "student");

            migrationBuilder.DropColumn(
                name: "applies_to_course_id",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "applies_to_instance_id",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "created_by_employee_id",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "max_discount_amount",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "min_order_amount",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "promo_code");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                table: "payment");

            migrationBuilder.DropColumn(
                name: "campaign_id",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "read_at",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "lesson");

            migrationBuilder.DropColumn(
                name: "payment_id",
                table: "installment_payment");

            migrationBuilder.DropColumn(
                name: "payment_status_id",
                table: "installment_payment");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "course_module");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "created_by_employee_id",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "enrollment_end_date",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "enrollment_start_date",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "status",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "timezone",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "course_instance");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "course");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                table: "app_order");

            migrationBuilder.DropColumn(
                name: "promo_code_id",
                table: "app_order");
        }
    }
}
