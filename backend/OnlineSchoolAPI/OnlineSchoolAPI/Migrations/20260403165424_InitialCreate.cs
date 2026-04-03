using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSchoolAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "application_status",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__applicat__3683B5319D36604F", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "assignment_type",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__assignme__2C0005983E0FA961", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "discount_type",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__discount__5B1B3754F7C0A2B0", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_status",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__enrollme__3683B5315C08870B", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "exam",
                columns: table => new
                {
                    exam_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    exam_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__exam__9C8C7BE9", x => x.exam_id);
                });

            migrationBuilder.CreateTable(
                name: "faq_category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    category_order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__faq_cate__D54EE9B4F8DDF668", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "lesson_type",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lesson_t__2C0005985353B43C", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "order_status",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__order_st__3683B53106712211", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "payment_status",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payment___3683B53112C26BD6", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "promo_code",
                columns: table => new
                {
                    promo_code_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: true),
                    discount_value = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    max_uses = table.Column<int>(type: "int", nullable: true),
                    current_uses = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__promo_co__C52CD3126ED9598B", x => x.promo_code_id);
                });

            migrationBuilder.CreateTable(
                name: "school_setting",
                columns: table => new
                {
                    setting_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    school_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    logo_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    contact_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    contact_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    about_school_text = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: true),
                    privacy_policy_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    terms_of_use_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__school_s__256E1E321D93193D", x => x.setting_id);
                });

            migrationBuilder.CreateTable(
                name: "subject",
                columns: table => new
                {
                    subject_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subject_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__subject__5004F660", x => x.subject_id);
                });

            migrationBuilder.CreateTable(
                name: "submission_status",
                columns: table => new
                {
                    status_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    status_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__submissi__3683B5310C9A541F", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_rol__760965CCF7A32D04", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "faq_item",
                columns: table => new
                {
                    faq_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    item_order = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__faq_item__66734BAF381EBE8D", x => x.faq_id);
                    table.ForeignKey(
                        name: "FK__faq_item__catego__756D6ECB",
                        column: x => x.category_id,
                        principalTable: "faq_category",
                        principalColumn: "category_id");
                });

            migrationBuilder.CreateTable(
                name: "course_category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    subject_id = table.Column<int>(type: "int", nullable: true),
                    exam_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_c__D54EE9B4D070B0E9", x => x.category_id);
                    table.ForeignKey(
                        name: "FK__course_ca__exam",
                        column: x => x.exam_id,
                        principalTable: "exam",
                        principalColumn: "exam_id");
                    table.ForeignKey(
                        name: "FK__course_ca__subject",
                        column: x => x.subject_id,
                        principalTable: "subject",
                        principalColumn: "subject_id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__B9BE370FE725EF09", x => x.user_id);
                    table.ForeignKey(
                        name: "FK__users__role_id__66603565",
                        column: x => x.role_id,
                        principalTable: "user_role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "course",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    short_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    cover_img_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    discount_price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    total_hours = table.Column<int>(type: "int", nullable: true),
                    what_you_get = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course__8F1EF7AED3B7CE62", x => x.course_id);
                    table.ForeignKey(
                        name: "FK__course__category__75A278F5",
                        column: x => x.category_id,
                        principalTable: "course_category",
                        principalColumn: "category_id");
                });

            migrationBuilder.CreateTable(
                name: "employee",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    patronymic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    work_experience = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__employee__C52E0BA85D738F50", x => x.employee_id);
                    table.ForeignKey(
                        name: "FK__employee__user_i__70DDC3D8",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    notification_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    related_entity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__E059842F14F888C5", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK__notificat__user___02C769E9",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "student",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    class_number = table.Column<int>(type: "int", nullable: false),
                    parent_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    parent_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student__2A33069A81FCD669", x => x.student_id);
                    table.ForeignKey(
                        name: "FK__student__user_id__6B24EA82",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_instance",
                columns: table => new
                {
                    instance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    instance_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    total_weeks = table.Column<int>(type: "int", nullable: true),
                    lessons_per_week = table.Column<int>(type: "int", nullable: true),
                    schedule_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    max_students = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_i__7DBD82E77478442E", x => x.instance_id);
                    table.ForeignKey(
                        name: "FK__course_in__cours__114A936A",
                        column: x => x.course_id,
                        principalTable: "course",
                        principalColumn: "course_id");
                });

            migrationBuilder.CreateTable(
                name: "course_module",
                columns: table => new
                {
                    module_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    module_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_m__1A2D065313F63517", x => x.module_id);
                    table.ForeignKey(
                        name: "FK__course_mo__cours__797309D9",
                        column: x => x.course_id,
                        principalTable: "course",
                        principalColumn: "course_id");
                });

            migrationBuilder.CreateTable(
                name: "trial_application",
                columns: table => new
                {
                    application_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    class_number = table.Column<int>(type: "int", nullable: false),
                    selected_subjects = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    application_status_id = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    assigned_manager_id = table.Column<int>(type: "int", nullable: true),
                    manager_comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    contacted_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__trial_ap__3BCBDCF28406F1D6", x => x.application_id);
                    table.ForeignKey(
                        name: "FK__trial_app__appli__671F4F74",
                        column: x => x.application_status_id,
                        principalTable: "application_status",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "FK__trial_app__assig__681373AD",
                        column: x => x.assigned_manager_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                });

            migrationBuilder.CreateTable(
                name: "app_order",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    order_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: true, defaultValue: 0m),
                    final_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    order_status_id = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    paid_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__app_orde__46596229B06C6B8B", x => x.order_id);
                    table.ForeignKey(
                        name: "FK__app_order__order__4C6B5938",
                        column: x => x.order_status_id,
                        principalTable: "order_status",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "FK__app_order__stude__4B7734FF",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "review",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_published = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__review__60883D901B037C57", x => x.review_id);
                    table.ForeignKey(
                        name: "FK__review__course_i__6EC0713C",
                        column: x => x.course_id,
                        principalTable: "course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK__review__student___6DCC4D03",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "course_instance_coordinator",
                columns: table => new
                {
                    coordinator_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    instance_id = table.Column<int>(type: "int", nullable: false),
                    employee_id = table.Column<int>(type: "int", nullable: false),
                    is_lead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    assigned_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_i__0622227BBF41D529", x => x.coordinator_id);
                    table.ForeignKey(
                        name: "FK__course_in__emplo__17F790F9",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK__course_in__insta__17036CC0",
                        column: x => x.instance_id,
                        principalTable: "course_instance",
                        principalColumn: "instance_id");
                });

            migrationBuilder.CreateTable(
                name: "enrollment",
                columns: table => new
                {
                    enrollment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    instance_id = table.Column<int>(type: "int", nullable: false),
                    assigned_teacher_id = table.Column<int>(type: "int", nullable: true),
                    enrolled_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    enrollment_status_id = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    completed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    final_score = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__enrollme__6D24AA7AD787A167", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "FK__enrollmen__assig__25518C17",
                        column: x => x.assigned_teacher_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK__enrollmen__enrol__2645B050",
                        column: x => x.enrollment_status_id,
                        principalTable: "enrollment_status",
                        principalColumn: "status_id");
                    table.ForeignKey(
                        name: "FK__enrollmen__insta__245D67DE",
                        column: x => x.instance_id,
                        principalTable: "course_instance",
                        principalColumn: "instance_id");
                    table.ForeignKey(
                        name: "FK__enrollmen__stude__236943A5",
                        column: x => x.student_id,
                        principalTable: "student",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "lesson",
                columns: table => new
                {
                    lesson_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    module_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    lesson_type_id = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    video_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    lesson_order = table.Column<int>(type: "int", nullable: false),
                    is_free_preview = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lesson__6421F7BED700B577", x => x.lesson_id);
                    table.ForeignKey(
                        name: "FK__lesson__lesson_t__7F2BE32F",
                        column: x => x.lesson_type_id,
                        principalTable: "lesson_type",
                        principalColumn: "type_id");
                    table.ForeignKey(
                        name: "FK__lesson__module_i__7E37BEF6",
                        column: x => x.module_id,
                        principalTable: "course_module",
                        principalColumn: "module_id");
                });

            migrationBuilder.CreateTable(
                name: "installment_plan",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    installment_count = table.Column<int>(type: "int", nullable: false),
                    monthly_payment = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    next_payment_date = table.Column<DateOnly>(type: "date", nullable: true),
                    plan_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__installm__BE9F8F1D65579815", x => x.plan_id);
                    table.ForeignKey(
                        name: "FK__installme__order__5D95E53A",
                        column: x => x.order_id,
                        principalTable: "app_order",
                        principalColumn: "order_id");
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    order_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    instance_id = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__order_it__3764B6BC0E7FE974", x => x.order_item_id);
                    table.ForeignKey(
                        name: "FK__order_ite__cours__5224328E",
                        column: x => x.course_id,
                        principalTable: "course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK__order_ite__insta__531856C7",
                        column: x => x.instance_id,
                        principalTable: "course_instance",
                        principalColumn: "instance_id");
                    table.ForeignKey(
                        name: "FK__order_ite__order__51300E55",
                        column: x => x.order_id,
                        principalTable: "app_order",
                        principalColumn: "order_id");
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    external_payment_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    payment_status_id = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    card_last_four = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payment__ED1FC9EA1DBF32F8", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK__payment__order_i__57DD0BE4",
                        column: x => x.order_id,
                        principalTable: "app_order",
                        principalColumn: "order_id");
                    table.ForeignKey(
                        name: "FK__payment__payment__58D1301D",
                        column: x => x.payment_status_id,
                        principalTable: "payment_status",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                columns: table => new
                {
                    assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assignment_type_id = table.Column<int>(type: "int", nullable: false),
                    max_score = table.Column<int>(type: "int", nullable: false),
                    due_days_after_lesson = table.Column<int>(type: "int", nullable: true),
                    correct_answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__assignme__DA891814914741D1", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK__assignmen__assig__08B54D69",
                        column: x => x.assignment_type_id,
                        principalTable: "assignment_type",
                        principalColumn: "type_id");
                    table.ForeignKey(
                        name: "FK__assignmen__lesso__07C12930",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "lesson_id");
                });

            migrationBuilder.CreateTable(
                name: "course_schedule_plan",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    instance_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    release_day_offset = table.Column<int>(type: "int", nullable: false),
                    release_time = table.Column<TimeOnly>(type: "time", nullable: true, defaultValue: new TimeOnly(0, 0, 0)),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_s__BE9F8F1DD08EDE69", x => x.plan_id);
                    table.ForeignKey(
                        name: "FK__course_sc__insta__1DB06A4F",
                        column: x => x.instance_id,
                        principalTable: "course_instance",
                        principalColumn: "instance_id");
                    table.ForeignKey(
                        name: "FK__course_sc__lesso__1EA48E88",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "lesson_id");
                });

            migrationBuilder.CreateTable(
                name: "lesson_material",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    file_size_kb = table.Column<int>(type: "int", nullable: true),
                    download_count = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    uploaded_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lesson_m__6BFE1D28F3A17522", x => x.material_id);
                    table.ForeignKey(
                        name: "FK__lesson_ma__lesso__03F0984C",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "lesson_id");
                });

            migrationBuilder.CreateTable(
                name: "installment_payment",
                columns: table => new
                {
                    installment_payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    plan_id = table.Column<int>(type: "int", nullable: false),
                    installment_number = table.Column<int>(type: "int", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "pending"),
                    paid_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__installm__29799A54E5B2195E", x => x.installment_payment_id);
                    table.ForeignKey(
                        name: "FK__installme__plan___625A9A57",
                        column: x => x.plan_id,
                        principalTable: "installment_plan",
                        principalColumn: "plan_id");
                });

            migrationBuilder.CreateTable(
                name: "assignment_variant",
                columns: table => new
                {
                    variant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assignment_id = table.Column<int>(type: "int", nullable: false),
                    variant_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    variant_order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__assignme__EACC68B7FAD84BD6", x => x.variant_id);
                    table.ForeignKey(
                        name: "FK__assignmen__assig__0C85DE4D",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "assignment_id");
                });

            migrationBuilder.CreateTable(
                name: "student_lesson_access",
                columns: table => new
                {
                    access_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    plan_id = table.Column<int>(type: "int", nullable: true),
                    planned_access_date = table.Column<DateOnly>(type: "date", nullable: false),
                    planned_access_time = table.Column<TimeOnly>(type: "time", nullable: true, defaultValue: new TimeOnly(0, 0, 0)),
                    actual_open_datetime = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_available = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    opened_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___10FA1E20A2B27802", x => x.access_id);
                    table.ForeignKey(
                        name: "FK__student_l__enrol__2CF2ADDF",
                        column: x => x.enrollment_id,
                        principalTable: "enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK__student_l__lesso__2DE6D218",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "lesson_id");
                    table.ForeignKey(
                        name: "FK__student_l__opene__2FCF1A8A",
                        column: x => x.opened_by_employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK__student_l__plan___2EDAF651",
                        column: x => x.plan_id,
                        principalTable: "course_schedule_plan",
                        principalColumn: "plan_id");
                });

            migrationBuilder.CreateTable(
                name: "student_progress",
                columns: table => new
                {
                    progress_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    access_id = table.Column<int>(type: "int", nullable: false),
                    is_completed = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    completed_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    watch_time_seconds = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    last_accessed = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___49B3D8C13285DD7D", x => x.progress_id);
                    table.ForeignKey(
                        name: "FK__student_p__acces__3864608B",
                        column: x => x.access_id,
                        principalTable: "student_lesson_access",
                        principalColumn: "access_id");
                    table.ForeignKey(
                        name: "FK__student_p__enrol__367C1819",
                        column: x => x.enrollment_id,
                        principalTable: "enrollment",
                        principalColumn: "enrollment_id");
                    table.ForeignKey(
                        name: "FK__student_p__lesso__37703C52",
                        column: x => x.lesson_id,
                        principalTable: "lesson",
                        principalColumn: "lesson_id");
                });

            migrationBuilder.CreateTable(
                name: "submission",
                columns: table => new
                {
                    submission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    progress_id = table.Column<int>(type: "int", nullable: false),
                    assignment_id = table.Column<int>(type: "int", nullable: false),
                    student_answer_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attached_file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    attached_file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    submission_status_id = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    score = table.Column<int>(type: "int", nullable: true),
                    teacher_comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    graded_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    graded_by_employee_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__submissi__9B53559500EEB980", x => x.submission_id);
                    table.ForeignKey(
                        name: "FK__submissio__assig__3F115E1A",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "assignment_id");
                    table.ForeignKey(
                        name: "FK__submissio__grade__40058253",
                        column: x => x.graded_by_employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id");
                    table.ForeignKey(
                        name: "FK__submissio__progr__3E1D39E1",
                        column: x => x.progress_id,
                        principalTable: "student_progress",
                        principalColumn: "progress_id");
                    table.ForeignKey(
                        name: "FK__submissio__submi__40F9A68C",
                        column: x => x.submission_status_id,
                        principalTable: "submission_status",
                        principalColumn: "status_id");
                });

            migrationBuilder.CreateTable(
                name: "submission_review",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    submission_id = table.Column<int>(type: "int", nullable: false),
                    question_number = table.Column<int>(type: "int", nullable: true),
                    student_variant_id = table.Column<int>(type: "int", nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: true),
                    teacher_comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    points_awarded = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__submissi__60883D90B01CED93", x => x.review_id);
                    table.ForeignKey(
                        name: "FK__submissio__submi__44CA3770",
                        column: x => x.submission_id,
                        principalTable: "submission",
                        principalColumn: "submission_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_order_order_status_id",
                table: "app_order",
                column: "order_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_order_student_id",
                table: "app_order",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ__app_orde__730E34DFC896B282",
                table: "app_order",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__applicat__501B37533F14CCB4",
                table: "application_status",
                column: "status_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assignment_assignment_type_id",
                table: "assignment",
                column: "assignment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_lesson_id",
                table: "assignment",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "UQ__assignme__543C4FD91EEA53C8",
                table: "assignment_type",
                column: "type_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assignment_variant_assignment_id",
                table: "assignment_variant",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_category_id",
                table: "course",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_category_exam_id",
                table: "course_category",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_category_subject_id",
                table: "course_category",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_course_id",
                table: "course_instance",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_instance_coordinator_employee_id",
                table: "course_instance_coordinator",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "UQ__course_i__E1EF625C28150AF2",
                table: "course_instance_coordinator",
                columns: new[] { "instance_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_module_course_id",
                table: "course_module",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_schedule_plan_lesson_id",
                table: "course_schedule_plan",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "UQ__course_s__8BFF9D9DF4CC949C",
                table: "course_schedule_plan",
                columns: new[] { "instance_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__employee__B9BE370ED2AF6F2C",
                table: "employee",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_assigned_teacher_id",
                table: "enrollment",
                column: "assigned_teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_enrollment_status_id",
                table: "enrollment",
                column: "enrollment_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_instance_id",
                table: "enrollment",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollment_student_id",
                table: "enrollment",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ__enrollme__501B3753A9CF44C7",
                table: "enrollment_status",
                column: "status_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__exam__D916B1FC",
                table: "exam",
                column: "exam_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_faq_item_category_id",
                table: "faq_item",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_installment_payment_plan_id",
                table: "installment_payment",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_installment_plan_order_id",
                table: "installment_plan",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_lesson_type_id",
                table: "lesson",
                column: "lesson_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_module_id",
                table: "lesson",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_material_lesson_id",
                table: "lesson_material",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "UQ__lesson_t__543C4FD9863219B2",
                table: "lesson_type",
                column: "type_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_user_id",
                table: "notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_course_id",
                table: "order_item",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_instance_id",
                table: "order_item",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_order_id",
                table: "order_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "UQ__order_st__501B3753E3436EE4",
                table: "order_status",
                column: "status_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_id",
                table: "payment",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_payment_status_id",
                table: "payment",
                column: "payment_status_id");

            migrationBuilder.CreateIndex(
                name: "UQ__payment___501B3753B8EE7B1D",
                table: "payment_status",
                column: "status_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__promo_co__357D4CF9FA2698A5",
                table: "promo_code",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_course_id",
                table: "review",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_student_id",
                table: "review",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ__student__B9BE370E83B5DF70",
                table: "student",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_access_lesson_id",
                table: "student_lesson_access",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_access_opened_by_employee_id",
                table: "student_lesson_access",
                column: "opened_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_lesson_access_plan_id",
                table: "student_lesson_access",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "UQ__student___9B66B500EF6E9354",
                table: "student_lesson_access",
                columns: new[] { "enrollment_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_progress_access_id",
                table: "student_progress",
                column: "access_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_progress_lesson_id",
                table: "student_progress",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "UQ__student___9B66B5004ABE33F9",
                table: "student_progress",
                columns: new[] { "enrollment_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__subject__5004F679",
                table: "subject",
                column: "subject_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_submission_assignment_id",
                table: "submission",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_graded_by_employee_id",
                table: "submission",
                column: "graded_by_employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_progress_id",
                table: "submission",
                column: "progress_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_submission_status_id",
                table: "submission",
                column: "submission_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_review_submission_id",
                table: "submission_review",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "UQ__submissi__501B37533F4A7911",
                table: "submission_status",
                column: "status_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_application_status_id",
                table: "trial_application",
                column: "application_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_trial_application_assigned_manager_id",
                table: "trial_application",
                column: "assigned_manager_id");

            migrationBuilder.CreateIndex(
                name: "UQ__user_rol__783254B15157E60E",
                table: "user_role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UQ__users__AB6E61649B7A09A4",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignment_variant");

            migrationBuilder.DropTable(
                name: "course_instance_coordinator");

            migrationBuilder.DropTable(
                name: "discount_type");

            migrationBuilder.DropTable(
                name: "faq_item");

            migrationBuilder.DropTable(
                name: "installment_payment");

            migrationBuilder.DropTable(
                name: "lesson_material");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "promo_code");

            migrationBuilder.DropTable(
                name: "review");

            migrationBuilder.DropTable(
                name: "school_setting");

            migrationBuilder.DropTable(
                name: "submission_review");

            migrationBuilder.DropTable(
                name: "trial_application");

            migrationBuilder.DropTable(
                name: "faq_category");

            migrationBuilder.DropTable(
                name: "installment_plan");

            migrationBuilder.DropTable(
                name: "payment_status");

            migrationBuilder.DropTable(
                name: "submission");

            migrationBuilder.DropTable(
                name: "application_status");

            migrationBuilder.DropTable(
                name: "app_order");

            migrationBuilder.DropTable(
                name: "assignment");

            migrationBuilder.DropTable(
                name: "student_progress");

            migrationBuilder.DropTable(
                name: "submission_status");

            migrationBuilder.DropTable(
                name: "order_status");

            migrationBuilder.DropTable(
                name: "assignment_type");

            migrationBuilder.DropTable(
                name: "student_lesson_access");

            migrationBuilder.DropTable(
                name: "enrollment");

            migrationBuilder.DropTable(
                name: "course_schedule_plan");

            migrationBuilder.DropTable(
                name: "employee");

            migrationBuilder.DropTable(
                name: "enrollment_status");

            migrationBuilder.DropTable(
                name: "student");

            migrationBuilder.DropTable(
                name: "course_instance");

            migrationBuilder.DropTable(
                name: "lesson");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "lesson_type");

            migrationBuilder.DropTable(
                name: "course_module");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "course");

            migrationBuilder.DropTable(
                name: "course_category");

            migrationBuilder.DropTable(
                name: "exam");

            migrationBuilder.DropTable(
                name: "subject");
        }
    }
}
