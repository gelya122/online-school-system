using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OnlineSchoolAPI.Models;

namespace OnlineSchoolAPI;

public partial class OnlineSchoolDbContext : DbContext
{
    public OnlineSchoolDbContext()
    {
    }

    public OnlineSchoolDbContext(DbContextOptions<OnlineSchoolDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppOrder> AppOrders { get; set; }

    public virtual DbSet<ApplicationStatus> ApplicationStatuses { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentType> AssignmentTypes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseInstance> CourseInstances { get; set; }

    public virtual DbSet<CourseInstanceStatus> CourseInstanceStatuses { get; set; }

    public virtual DbSet<CourseInstanceStaff> CourseInstanceStaff { get; set; }

    public virtual DbSet<CourseModule> CourseModules { get; set; }

    public virtual DbSet<CourseSchedulePlan> CourseSchedulePlans { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<EnrollmentStatus> EnrollmentStatuses { get; set; }

    public virtual DbSet<FaqCategory> FaqCategories { get; set; }

    public virtual DbSet<FaqItem> FaqItems { get; set; }

    public virtual DbSet<InstallmentPayment> InstallmentPayments { get; set; }

    public virtual DbSet<InstallmentPlan> InstallmentPlans { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonMaterial> LessonMaterials { get; set; }

    public virtual DbSet<LessonType> LessonTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<DiscountType> DiscountTypes { get; set; }

    public virtual DbSet<PromoCode> PromoCodes { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<SchoolSetting> SchoolSettings { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentProgress> StudentProgresses { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<SubmissionStatus> SubmissionStatuses { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<TrialApplication> TrialApplications { get; set; }

    public virtual DbSet<StudentNote> StudentNotes { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<TestQuestion> TestQuestions { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<TestStudentAnswer> TestStudentAnswers { get; set; }

    public virtual DbSet<MailingCampaign> MailingCampaigns { get; set; }

    public virtual DbSet<MailingRecipient> MailingRecipients { get; set; }

    public virtual DbSet<SiteSetting> SiteSettings { get; set; }

    public virtual DbSet<SiteBanner> SiteBanners { get; set; }

    public virtual DbSet<FileStorage> FileStorages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__app_orde__46596229B06C6B8B");

            entity.ToTable("app_order");

            entity.HasIndex(e => e.OrderNumber, "UQ__app_orde__730E34DFC896B282").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.FinalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("final_amount");
            entity.Property(e => e.PromoCodeId).HasColumnName("promo_code_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .HasColumnName("order_number");
            entity.Property(e => e.OrderStatusId)
                .HasDefaultValue(1)
                .HasColumnName("order_status_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.MethodId).HasColumnName("method_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.AppOrders)
                .HasForeignKey(d => d.OrderStatusId)
                .HasConstraintName("FK__app_order__order__4C6B5938");

            entity.HasOne(d => d.Student).WithMany(p => p.AppOrders)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__app_order__stude__4B7734FF");

            entity.HasOne(d => d.Method).WithMany(p => p.AppOrders)
                .HasForeignKey(d => d.MethodId)
                .HasConstraintName("FK_app_order_payment_method");

            entity.HasOne(d => d.PromoCode).WithMany()
                .HasForeignKey(d => d.PromoCodeId)
                .HasConstraintName("FK_app_order_promo_code_promo_code_id");
        });

        modelBuilder.Entity<ApplicationStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__applicat__3683B5319D36604F");

            entity.ToTable("application_status");

            entity.HasIndex(e => e.StatusName, "UQ__applicat__501B37533F14CCB4").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK_assignment");

            entity.ToTable("assignment");

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDaysAfterLesson).HasColumnName("due_days_after_lesson");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.MaxScore).HasColumnName("max_score");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_assignment_lesson");
        });

        modelBuilder.Entity<AssignmentType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__assignme__2C0005983E0FA961");

            entity.ToTable("assignment_type");

            entity.HasIndex(e => e.TypeName, "UQ__assignme__543C4FD91EEA53C8").IsUnique();

            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity.HasKey(e => e.QuestionTypeId).HasName("PK_question_type");
            entity.ToTable("question_type");
            entity.Property(e => e.QuestionTypeId).HasColumnName("question_type_id");
            entity.Property(e => e.Title).HasMaxLength(100).HasColumnName("title");
            entity.Property(e => e.Description).HasMaxLength(300).HasColumnName("description");
        });

        modelBuilder.Entity<TestQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK_test_question");
            entity.ToTable("test_question");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionTypeId).HasColumnName("question_type_id");
            entity.Property(e => e.MaxPoints).HasColumnType("decimal(10, 2)").HasColumnName("max_points");
            entity.Property(e => e.QuestionOrder).HasColumnName("question_order");
            entity.Property(e => e.Explanation).HasColumnName("explanation");
            entity.Property(e => e.CorrectAnswer).HasColumnName("correct_answer");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime2(0)")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Assignment).WithMany(p => p.TestQuestions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_test_question_assignment");

            entity.HasOne(d => d.QuestionType).WithMany(p => p.TestQuestions)
                .HasForeignKey(d => d.QuestionTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_test_question_question_type");
        });

        modelBuilder.Entity<TestStudentAnswer>(entity =>
        {
            entity.HasKey(e => e.StudentAnswerId).HasName("PK_test_student_answer");
            entity.ToTable("test_student_answer");
            entity.HasIndex(e => new { e.SubmissionId, e.QuestionId }, "UQ_test_student_answer_submission_question").IsUnique();
            entity.Property(e => e.StudentAnswerId).HasColumnName("student_answer_id");
            entity.Property(e => e.SubmissionId).HasColumnName("submission_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.ResponseText).HasColumnName("response_text");
            entity.Property(e => e.PointsAwarded).HasColumnType("decimal(10, 2)").HasColumnName("points_awarded");
            entity.Property(e => e.IsFullyAutoGraded).HasColumnName("is_fully_auto_graded");
            entity.Property(e => e.TeacherComment).HasColumnName("teacher_comment");
            entity.Property(e => e.AnsweredAt)
                .HasColumnType("datetime2(0)")
                .HasColumnName("answered_at");
            entity.Property(e => e.ReviewedByEmployeeId).HasColumnName("reviewed_by_employee_id");
            entity.Property(e => e.ReviewedAt)
                .HasColumnType("datetime2(0)")
                .HasColumnName("reviewed_at");

            entity.HasOne(d => d.Submission).WithMany(p => p.TestStudentAnswers)
                .HasForeignKey(d => d.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_test_student_answer_submission");

            entity.HasOne(d => d.Question).WithMany(p => p.TestStudentAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_test_student_answer_question");

            entity.HasOne(d => d.ReviewedByEmployee).WithMany(p => p.TestStudentAnswersReviewed)
                .HasForeignKey(d => d.ReviewedByEmployeeId)
                .HasConstraintName("FK_test_student_answer_reviewed_by_employee");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__course__8F1EF7AED3B7CE62");

            entity.ToTable("course");

            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CoverImgUrl)
                .HasMaxLength(500)
                .HasColumnName("cover_img_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_price");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ShortDescription)
                .HasMaxLength(500)
                .HasColumnName("short_description");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.TotalHours).HasColumnName("total_hours");
            entity.Property(e => e.WhatYouGet).HasColumnName("what_you_get");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course__category__75A278F5");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__course_c__D54EE9B4D070B0E9");

            entity.ToTable("course_category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");

            entity.HasOne(d => d.Exam).WithMany(p => p.CourseCategories)
                .HasForeignKey(d => d.ExamId)
                .HasConstraintName("FK__course_ca__exam");

            entity.HasOne(d => d.Subject).WithMany(p => p.CourseCategories)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK__course_ca__subject");
        });

        modelBuilder.Entity<CourseInstanceStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId);

            entity.ToTable("course_instance_status");

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
        });

        modelBuilder.Entity<CourseInstance>(entity =>
        {
            entity.HasKey(e => e.InstanceId).HasName("PK__course_i__7DBD82E77478442E");

            entity.ToTable("course_instance");

            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedByEmployeeId).HasColumnName("created_by_employee_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.EnrollmentEndDate).HasColumnName("enrollment_end_date");
            entity.Property(e => e.EnrollmentStartDate).HasColumnName("enrollment_start_date");
            entity.Property(e => e.InstanceName)
                .HasMaxLength(200)
                .HasColumnName("instance_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LessonsPerWeek).HasColumnName("lessons_per_week");
            entity.Property(e => e.MaxStudents).HasColumnName("max_students");
            entity.Property(e => e.ScheduleDescription)
                .HasMaxLength(500)
                .HasColumnName("schedule_description");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Timezone)
                .HasMaxLength(100)
                .HasColumnName("timezone");
            entity.Property(e => e.TotalWeeks).HasColumnName("total_weeks");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.ScheduleRulesJson)
                .HasMaxLength(int.MaxValue)
                .HasColumnName("schedule_rules_json");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseInstances)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_in__cours__114A936A");

            entity.HasOne(d => d.InstanceStatus).WithMany(p => p.CourseInstances)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_course_instance_status");

            entity.HasOne(d => d.CreatedByEmployee).WithMany()
                .HasForeignKey(d => d.CreatedByEmployeeId)
                .HasConstraintName("FK_course_instance_created_by_employee");
        });

        modelBuilder.Entity<CourseInstanceStaff>(entity =>
        {
            entity.HasKey(e => e.StaffAssignmentId);

            entity.ToTable("course_instance_staff");

            entity.HasIndex(e => new { e.InstanceId, e.EmployeeId, e.RoleId }, "UX_course_instance_staff_instance_employee_role_active")
                .IsUnique()
                .HasFilter("[deleted_at] IS NULL");

            entity.Property(e => e.StaffAssignmentId).HasColumnName("staff_assignment_id");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("assigned_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");

            entity.HasOne(d => d.Instance).WithMany(p => p.CourseInstanceStaff)
                .HasForeignKey(d => d.InstanceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_course_instance_staff_instance");

            entity.HasOne(d => d.Employee).WithMany(p => p.CourseInstanceStaffAssignments)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_course_instance_staff_employee");

            entity.HasOne(d => d.Role).WithMany(p => p.CourseInstanceStaffs)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_course_instance_staff_role");
        });

        modelBuilder.Entity<CourseModule>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__course_m__1A2D065313F63517");

            entity.ToTable("course_module");

            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.ModuleOrder).HasColumnName("module_order");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseModules)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_mo__cours__797309D9");
        });

        modelBuilder.Entity<CourseSchedulePlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__course_s__BE9F8F1DD08EDE69");

            entity.ToTable("course_schedule_plan");

            entity.HasIndex(e => new { e.InstanceId, e.LessonId }, "UQ__course_s__8BFF9D9DF4CC949C").IsUnique();

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.ReleaseDayOffset).HasColumnName("release_day_offset");
            entity.Property(e => e.ReleaseTime)
                .HasDefaultValue(new TimeOnly(0, 0, 0))
                .HasColumnName("release_time");
            entity.Property(e => e.ScheduledAt)
                .HasColumnType("datetime2(0)")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.LessonOrder).HasColumnName("lesson_order");
            entity.Property(e => e.IsPublished)
                .HasDefaultValue(true)
                .HasColumnName("is_published");

            entity.HasOne(d => d.Instance).WithMany(p => p.CourseSchedulePlans)
                .HasForeignKey(d => d.InstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_sc__insta__1DB06A4F");

            entity.HasOne(d => d.Lesson).WithMany(p => p.CourseSchedulePlans)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_sc__lesso__1EA48E88");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__employee__C52E0BA85D738F50");

            entity.ToTable("employee");

            entity.HasIndex(e => e.UserId, "UQ__employee__B9BE370ED2AF6F2C").IsUnique();
            entity.HasIndex(e => new { e.LastName, e.FirstName }, "IX_employee_last_name_first_name");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(100)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WorkExperience).HasColumnName("work_experience");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .HasConstraintName("FK__employee__user_i__70DDC3D8");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.ExamId).HasName("PK__exam__9C8C7BE9");

            entity.ToTable("exam");

            entity.HasIndex(e => e.ExamName, "UQ__exam__D916B1FC").IsUnique();

            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ExamName)
                .HasMaxLength(100)
                .HasColumnName("exam_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__enrollme__6D24AA7AD787A167");

            entity.ToTable("enrollment");

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.AssignedTeacherId).HasColumnName("assigned_mentor_id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.EnrolledAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("enrolled_at");
            entity.Property(e => e.EnrollmentStatusId)
                .HasDefaultValue(1)
                .HasColumnName("enrollment_status_id");
            entity.Property(e => e.FinalScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("final_score");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.AssignedTeacher).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.AssignedTeacherId)
                .HasConstraintName("FK_enrollment_assigned_mentor");

            entity.HasOne(d => d.EnrollmentStatus).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.EnrollmentStatusId)
                .HasConstraintName("FK__enrollmen__enrol__2645B050");

            entity.HasOne(d => d.Instance).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.InstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrollmen__insta__245D67DE");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrollmen__stude__236943A5");
        });

        modelBuilder.Entity<EnrollmentStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__enrollme__3683B5315C08870B");

            entity.ToTable("enrollment_status");

            entity.HasIndex(e => e.StatusName, "UQ__enrollme__501B3753A9CF44C7").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<FaqCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__faq_cate__D54EE9B4F8DDF668");

            entity.ToTable("faq_category");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
            entity.Property(e => e.CategoryOrder).HasColumnName("category_order");
        });

        modelBuilder.Entity<FaqItem>(entity =>
        {
            entity.HasKey(e => e.FaqId).HasName("PK__faq_item__66734BAF381EBE8D");

            entity.ToTable("faq_item");

            entity.Property(e => e.FaqId).HasColumnName("faq_id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ItemOrder).HasColumnName("item_order");
            entity.Property(e => e.Question)
                .HasMaxLength(500)
                .HasColumnName("question");

            entity.HasOne(d => d.Category).WithMany(p => p.FaqItems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__faq_item__catego__756D6ECB");
        });

        modelBuilder.Entity<InstallmentPayment>(entity =>
        {
            entity.HasKey(e => e.InstallmentPaymentId).HasName("PK__installm__29799A54E5B2195E");

            entity.ToTable("installment_payment");

            entity.Property(e => e.InstallmentPaymentId).HasColumnName("installment_payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.InstallmentNumber).HasColumnName("installment_number");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("pending")
                .HasColumnName("payment_status");
            entity.Property(e => e.PaymentStatusId).HasColumnName("payment_status_id");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.InstallmentPayments)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__installme__plan___625A9A57");

            entity.HasOne(d => d.Payment).WithMany()
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK_installment_payment_payment_payment_id");

            entity.HasOne(d => d.PaymentStatusNavigation).WithMany()
                .HasForeignKey(d => d.PaymentStatusId)
                .HasConstraintName("FK_installment_payment_payment_status_payment_status_id");
        });

        modelBuilder.Entity<InstallmentPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__installm__BE9F8F1D65579815");

            entity.ToTable("installment_plan");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InstallmentCount).HasColumnName("installment_count");
            entity.Property(e => e.MonthlyPayment)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monthly_payment");
            entity.Property(e => e.NextPaymentDate).HasColumnName("next_payment_date");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PlanStatus)
                .HasMaxLength(50)
                .HasDefaultValue("active")
                .HasColumnName("plan_status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Order).WithMany(p => p.InstallmentPlans)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__installme__order__5D95E53A");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__lesson__6421F7BED700B577");

            entity.ToTable("lesson");

            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.LessonOrder).HasColumnName("lesson_order");
            entity.Property(e => e.LessonTypeId).HasColumnName("lesson_type_id");
            entity.Property(e => e.ModuleId).HasColumnName("module_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(500)
                .HasColumnName("video_url");

            entity.HasOne(d => d.LessonType).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.LessonTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lesson__lesson_t__7F2BE32F");

            entity.HasOne(d => d.Module).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lesson__module_i__7E37BEF6");
        });

        modelBuilder.Entity<LessonMaterial>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__lesson_m__6BFE1D28F3A17522");

            entity.ToTable("lesson_material");

            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.DownloadCount)
                .HasDefaultValue(0)
                .HasColumnName("download_count");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSizeKb).HasColumnName("file_size_kb");
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .HasColumnName("file_type");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .HasColumnName("file_url");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonMaterials)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__lesson_ma__lesso__03F0984C");
        });

        modelBuilder.Entity<LessonType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__lesson_t__2C0005985353B43C");

            entity.ToTable("lesson_type");

            entity.HasIndex(e => e.TypeName, "UQ__lesson_t__543C4FD9863219B2").IsUnique();

            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__notifica__E059842F14F888C5");

            entity.ToTable("notification");

            entity.HasIndex(e => e.UserId, "IX_notification_user_id");
            entity.HasIndex(e => e.IsRead, "IX_notification_is_read");
            entity.HasIndex(e => e.CreatedAt, "IX_notification_created_at");

            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.NotificationType)
                .HasMaxLength(50)
                .HasColumnName("notification_type");
            entity.Property(e => e.ReadAt)
                .HasColumnType("datetime")
                .HasColumnName("read_at");
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(50)
                .HasColumnName("related_entity_type");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__notificat__user___02C769E9");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CampaignId)
                .HasConstraintName("FK_notification_mailing_campaign_campaign_id");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__order_it__3764B6BC0E7FE974");

            entity.ToTable("order_item");

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.HasOne(d => d.Course).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__order_ite__cours__5224328E");

            entity.HasOne(d => d.Instance).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.InstanceId)
                .HasConstraintName("FK__order_ite__insta__531856C7");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__order_ite__order__51300E55");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__order_st__3683B53106712211");

            entity.ToTable("order_status");

            entity.HasIndex(e => e.StatusName, "UQ__order_st__501B3753E3436EE4").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__payment__ED1FC9EA1DBF32F8");

            entity.ToTable("payment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CardLastFour)
                .HasMaxLength(4)
                .HasColumnName("card_last_four");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExternalPaymentId)
                .HasMaxLength(100)
                .HasColumnName("external_payment_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.MethodId).HasColumnName("method_id");
            entity.Property(e => e.PaymentStatusId)
                .HasDefaultValue(1)
                .HasColumnName("payment_status_id");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment__order_i__57DD0BE4");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentStatusId)
                .HasConstraintName("FK__payment__payment__58D1301D");

            entity.HasOne(d => d.Method).WithMany(p => p.Payments)
                .HasForeignKey(d => d.MethodId)
                .HasConstraintName("FK_payment_payment_method");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.MethodId);

            entity.ToTable("payment_method");

            entity.Property(e => e.MethodId).HasColumnName("method_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MethodName)
                .HasMaxLength(50)
                .HasColumnName("method_name");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__payment___3683B53112C26BD6");

            entity.ToTable("payment_status");

            entity.HasIndex(e => e.StatusName, "UQ__payment___501B3753B8EE7B1D").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<DiscountType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__discount__5B1B3754F7C0A2B0");

            entity.ToTable("discount_type");

            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasKey(e => e.PromoCodeId).HasName("PK__promo_co__C52CD3126ED9598B");

            entity.ToTable("promo_code");

            entity.HasIndex(e => e.Code, "UQ__promo_co__357D4CF9FA2698A5").IsUnique();

            entity.Property(e => e.AppliesToCourseId).HasColumnName("applies_to_course_id");
            entity.Property(e => e.AppliesToInstanceId).HasColumnName("applies_to_instance_id");
            entity.Property(e => e.CreatedByEmployeeId).HasColumnName("created_by_employee_id");
            entity.Property(e => e.PromoCodeId).HasColumnName("promo_code_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentUses)
                .HasDefaultValue(0)
                .HasColumnName("current_uses");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.TypeId)
                .HasColumnName("type_id");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxDiscountAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("max_discount_amount");
            entity.Property(e => e.MaxUses).HasColumnName("max_uses");
            entity.Property(e => e.MinOrderAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("min_order_amount");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");

            entity.HasOne(d => d.DiscountType).WithMany(p => p.PromoCodes)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_promo_code_discount_type");

            entity.HasOne(d => d.AppliesToCourse).WithMany()
                .HasForeignKey(d => d.AppliesToCourseId)
                .HasConstraintName("FK_promo_code_course_applies_to_course_id");

            entity.HasOne(d => d.AppliesToInstance).WithMany()
                .HasForeignKey(d => d.AppliesToInstanceId)
                .HasConstraintName("FK_promo_code_course_instance_applies_to_instance_id");

            entity.HasOne(d => d.CreatedByEmployee).WithMany()
                .HasForeignKey(d => d.CreatedByEmployeeId)
                .HasConstraintName("FK_promo_code_employee_created_by_employee_id");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__review__60883D901B037C57");

            entity.ToTable("review", t =>
                t.HasCheckConstraint("CK_review_rating", "[rating] IS NULL OR ([rating] >= 1 AND [rating] <= 5)"));

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPublished)
                .HasDefaultValue(false)
                .HasColumnName("is_published");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__review__course_i__6EC0713C");

            entity.HasOne(d => d.Student).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__review__student___6DCC4D03");
        });

        modelBuilder.Entity<SchoolSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__school_s__256E1E321D93193D");

            entity.ToTable("school_setting");

            entity.Property(e => e.SettingId).HasColumnName("setting_id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(20)
                .HasColumnName("contact_phone");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.AboutSchoolText)
                .HasMaxLength(int.MaxValue)
                .HasColumnName("about_school_text");
            entity.Property(e => e.PrivacyPolicyUrl)
                .HasMaxLength(500)
                .HasColumnName("privacy_policy_url");
            entity.Property(e => e.SchoolName)
                .HasMaxLength(200)
                .HasColumnName("school_name");
            entity.Property(e => e.TermsOfUseUrl)
                .HasMaxLength(500)
                .HasColumnName("terms_of_use_url");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__student__2A33069A81FCD669");

            entity.ToTable("student");

            entity.HasIndex(e => e.UserId, "UQ__student__B9BE370E83B5DF70").IsUnique();
            entity.HasIndex(e => new { e.LastName, e.FirstName }, "IX_student_last_name_first_name");

            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.ClassNumber).HasColumnName("class_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ParentEmail)
                .HasMaxLength(255)
                .HasColumnName("parent_email");
            entity.Property(e => e.ParentPhone)
                .HasMaxLength(20)
                .HasColumnName("parent_phone");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("FK__student__user_id__6B24EA82");
        });

        modelBuilder.Entity<StudentProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__student___49B3D8C13285DD7D");

            entity.ToTable("student_progress");

            entity.HasIndex(e => new { e.EnrollmentId, e.LessonId }, "UQ__student___9B66B5004ABE33F9").IsUnique();

            entity.Property(e => e.ProgressId).HasColumnName("progress_id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.LastAccessed)
                .HasColumnType("datetime")
                .HasColumnName("last_accessed");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.WatchTimeSeconds)
                .HasDefaultValue(0)
                .HasColumnName("watch_time_seconds");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.StudentProgresses)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_p__enrol__367C1819");

            entity.HasOne(d => d.Lesson).WithMany(p => p.StudentProgresses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_p__lesso__37703C52");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__submissi__9B53559500EEB980");

            entity.ToTable("submission");

            entity.Property(e => e.SubmissionId).HasColumnName("submission_id");
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GradedAt)
                .HasColumnType("datetime")
                .HasColumnName("graded_at");
            entity.Property(e => e.GradedByEmployeeId).HasColumnName("graded_by_employee_id");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.SubmissionStatusId)
                .HasDefaultValue(1)
                .HasColumnName("submission_status_id");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("submitted_at");
            entity.Property(e => e.TeacherComment).HasColumnName("teacher_comment");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__submissio__assig__3F115E1A");

            entity.HasOne(d => d.GradedByEmployee).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.GradedByEmployeeId)
                .HasConstraintName("FK__submissio__grade__40058253");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_submission_enrollment");

            entity.HasOne(d => d.SubmissionStatus).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.SubmissionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .IsRequired()
                .HasConstraintName("FK__submissio__submi__40F9A68C");
        });

        modelBuilder.Entity<SubmissionStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__submissi__3683B5310C9A541F");

            entity.ToTable("submission_status");

            entity.HasIndex(e => e.StatusName, "UQ__submissi__501B37533F4A7911").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__subject__5004F660");

            entity.ToTable("subject");

            entity.HasIndex(e => e.SubjectName, "UQ__subject__5004F679").IsUnique();

            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.SubjectName)
                .HasMaxLength(100)
                .HasColumnName("subject_name");
        });

        modelBuilder.Entity<TrialApplication>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__trial_ap__3BCBDCF28406F1D6");

            entity.ToTable("trial_application");

            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.ApplicationStatusId)
                .HasDefaultValue(1)
                .HasColumnName("application_status_id");
            entity.Property(e => e.AssignedManagerId).HasColumnName("assigned_manager_id");
            entity.Property(e => e.ClassNumber).HasColumnName("class_number");
            entity.Property(e => e.ContactedAt)
                .HasColumnType("datetime")
                .HasColumnName("contacted_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ManagerComment)
                .HasMaxLength(1000)
                .HasColumnName("manager_comment");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.SelectedSubjects)
                .HasMaxLength(500)
                .HasColumnName("selected_subjects");

            entity.HasOne(d => d.ApplicationStatus).WithMany(p => p.TrialApplications)
                .HasForeignKey(d => d.ApplicationStatusId)
                .HasConstraintName("FK__trial_app__appli__671F4F74");

            entity.HasOne(d => d.AssignedManager).WithMany(p => p.TrialApplications)
                .HasForeignKey(d => d.AssignedManagerId)
                .HasConstraintName("FK__trial_app__assig__681373AD");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370FE725EF09");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E61649B7A09A4").IsUnique();
            entity.HasIndex(e => e.Login, "UQ_users_login_not_null")
                .IsUnique()
                .HasFilter("[login] IS NOT NULL");
            entity.HasIndex(e => e.RoleId, "IX_users_role_id");
            entity.HasIndex(e => e.IsActive, "IX_users_is_active");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedByEmployeeId).HasColumnName("created_by_employee_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FailedLoginAttempts)
                .HasDefaultValue(0)
                .HasColumnName("failed_login_attempts");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsEmailConfirmed)
                .HasDefaultValue(false)
                .HasColumnName("is_email_confirmed");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("datetime")
                .HasColumnName("last_login_at");
            entity.Property(e => e.LockedUntil)
                .HasColumnType("datetime")
                .HasColumnName("locked_until");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.PasswordChangedAt)
                .HasColumnType("datetime")
                .HasColumnName("password_changed_at");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_id__66603565");

            entity.HasOne(d => d.CreatedByEmployee).WithMany(p => p.CreatedUsers)
                .HasForeignKey(d => d.CreatedByEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_users_created_by_employee");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__user_rol__760965CCF7A32D04");

            entity.ToTable("user_role");

            entity.HasIndex(e => e.RoleName, "UQ__user_rol__783254B15157E60E").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK_audit_log");
            entity.ToTable("audit_log");
            entity.Property(e => e.AuditLogId).HasColumnName("audit_log_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Action).HasMaxLength(100).HasColumnName("action");
            entity.Property(e => e.EntityType).HasMaxLength(100).HasColumnName("entity_type");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.OldValues).HasColumnName("old_values");
            entity.Property(e => e.NewValues).HasColumnName("new_values");
            entity.Property(e => e.IpAddress).HasMaxLength(50).HasColumnName("ip_address");
            entity.Property(e => e.UserAgent).HasMaxLength(500).HasColumnName("user_agent");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Employee).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_audit_log_employee_employee_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_audit_log_users_user_id");
        });

        modelBuilder.Entity<StudentNote>(entity =>
        {
            entity.HasKey(e => e.NoteId).HasName("PK_student_note");
            entity.ToTable("student_note");
            entity.Property(e => e.NoteId).HasColumnName("note_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.NoteType).HasMaxLength(50).HasColumnName("note_type");
            entity.Property(e => e.NoteText).HasColumnName("note_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentNotes)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_student_note_student");

            entity.HasOne(d => d.Employee).WithMany(p => p.StudentNotes)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_student_note_employee");
        });

        modelBuilder.Entity<MailingCampaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK_mailing_campaign");
            entity.ToTable("mailing_campaign");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Channel).HasMaxLength(50).HasDefaultValue("internal").HasColumnName("channel");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("draft").HasColumnName("status");
            entity.Property(e => e.TargetType).HasMaxLength(50).HasColumnName("target_type");
            entity.Property(e => e.CreatedByEmployeeId).HasColumnName("created_by_employee_id");
            entity.Property(e => e.ScheduledAt).HasColumnType("datetime").HasColumnName("scheduled_at");
            entity.Property(e => e.SentAt).HasColumnType("datetime").HasColumnName("sent_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByEmployee).WithMany(p => p.MailingCampaignsCreated)
                .HasForeignKey(d => d.CreatedByEmployeeId)
                .HasConstraintName("FK_mailing_campaign_employee_created_by");
        });

        modelBuilder.Entity<MailingRecipient>(entity =>
        {
            entity.HasKey(e => e.RecipientId).HasName("PK_mailing_recipient");
            entity.ToTable("mailing_recipient");
            entity.HasIndex(e => new { e.CampaignId, e.UserId }, "UQ_mailing_recipient_campaign_user").IsUnique();
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("pending").HasColumnName("status");
            entity.Property(e => e.SentAt).HasColumnType("datetime").HasColumnName("sent_at");
            entity.Property(e => e.ReadAt).HasColumnType("datetime").HasColumnName("read_at");
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000).HasColumnName("error_message");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Campaign).WithMany(p => p.MailingRecipients)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_mailing_recipient_mailing_campaign");

            entity.HasOne(d => d.User).WithMany(p => p.MailingRecipients)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_mailing_recipient_users");
        });

        modelBuilder.Entity<SiteSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK_site_setting");
            entity.ToTable("site_setting");
            entity.Property(e => e.SettingId).HasColumnName("setting_id");
            entity.Property(e => e.SiteName).HasMaxLength(200).HasColumnName("site_name");
            entity.Property(e => e.MainPageTitle).HasMaxLength(300).HasColumnName("main_page_title");
            entity.Property(e => e.MainPageDescription).HasColumnName("main_page_description");
            entity.Property(e => e.ContactPhone).HasMaxLength(50).HasColumnName("contact_phone");
            entity.Property(e => e.ContactEmail).HasMaxLength(255).HasColumnName("contact_email");
            entity.Property(e => e.VkUrl).HasMaxLength(500).HasColumnName("vk_url");
            entity.Property(e => e.TelegramUrl).HasMaxLength(500).HasColumnName("telegram_url");
            entity.Property(e => e.YoutubeUrl).HasMaxLength(500).HasColumnName("youtube_url");
            entity.Property(e => e.SeoTitle).HasMaxLength(300).HasColumnName("seo_title");
            entity.Property(e => e.SeoDescription).HasMaxLength(1000).HasColumnName("seo_description");
            entity.Property(e => e.IsMaintenanceMode).HasDefaultValue(false).HasColumnName("is_maintenance_mode");
            entity.Property(e => e.UpdatedByEmployeeId).HasColumnName("updated_by_employee_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.UpdatedByEmployee).WithMany(p => p.SiteSettingsUpdated)
                .HasForeignKey(d => d.UpdatedByEmployeeId)
                .HasConstraintName("FK_site_setting_employee_updated_by");
        });

        modelBuilder.Entity<SiteBanner>(entity =>
        {
            entity.HasKey(e => e.BannerId).HasName("PK_site_banner");
            entity.ToTable("site_banner");
            entity.Property(e => e.BannerId).HasColumnName("banner_id");
            entity.Property(e => e.Title).HasMaxLength(200).HasColumnName("title");
            entity.Property(e => e.Subtitle).HasMaxLength(500).HasColumnName("subtitle");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
            entity.Property(e => e.ButtonText).HasMaxLength(100).HasColumnName("button_text");
            entity.Property(e => e.ButtonUrl).HasMaxLength(500).HasColumnName("button_url");
            entity.Property(e => e.BannerOrder).HasDefaultValue(0).HasColumnName("banner_order");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
        });

        modelBuilder.Entity<FileStorage>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK_file_storage");
            entity.ToTable("file_storage");
            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.OriginalFileName).HasMaxLength(255).HasColumnName("original_file_name");
            entity.Property(e => e.StoredFileName).HasMaxLength(255).HasColumnName("stored_file_name");
            entity.Property(e => e.FileUrl).HasMaxLength(500).HasColumnName("file_url");
            entity.Property(e => e.FileType).HasMaxLength(100).HasColumnName("file_type");
            entity.Property(e => e.MimeType).HasMaxLength(100).HasColumnName("mime_type");
            entity.Property(e => e.FileSizeBytes).HasColumnName("file_size_bytes");
            entity.Property(e => e.UploadedByUserId).HasColumnName("uploaded_by_user_id");
            entity.Property(e => e.RelatedEntityType).HasMaxLength(100).HasColumnName("related_entity_type");
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.UploadedByUser).WithMany(p => p.FileStorages)
                .HasForeignKey(d => d.UploadedByUserId)
                .HasConstraintName("FK_file_storage_users_uploaded_by_user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
