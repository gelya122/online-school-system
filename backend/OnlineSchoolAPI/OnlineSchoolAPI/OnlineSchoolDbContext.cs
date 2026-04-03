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

    public virtual DbSet<AssignmentVariant> AssignmentVariants { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseInstance> CourseInstances { get; set; }

    public virtual DbSet<CourseInstanceCoordinator> CourseInstanceCoordinators { get; set; }

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

    public virtual DbSet<DiscountType> DiscountTypes { get; set; }

    public virtual DbSet<PromoCode> PromoCodes { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<SchoolSetting> SchoolSettings { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentLessonAccess> StudentLessonAccesses { get; set; }

    public virtual DbSet<StudentProgress> StudentProgresses { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<SubmissionReview> SubmissionReviews { get; set; }

    public virtual DbSet<SubmissionStatus> SubmissionStatuses { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<TrialApplication> TrialApplications { get; set; }

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
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .HasColumnName("order_number");
            entity.Property(e => e.OrderStatusId)
                .HasDefaultValue(1)
                .HasColumnName("order_status_id");
            entity.Property(e => e.PaidAt)
                .HasColumnType("datetime")
                .HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
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
            entity.HasKey(e => e.AssignmentId).HasName("PK__assignme__DA891814914741D1");

            entity.ToTable("assignment");

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.AssignmentTypeId).HasColumnName("assignment_type_id");
            entity.Property(e => e.CorrectAnswer).HasColumnName("correct_answer");
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

            entity.HasOne(d => d.AssignmentType).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.AssignmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__assignmen__assig__08B54D69");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__assignmen__lesso__07C12930");
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

        modelBuilder.Entity<AssignmentVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__assignme__EACC68B7FAD84BD6");

            entity.ToTable("assignment_variant");

            entity.Property(e => e.VariantId).HasColumnName("variant_id");
            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");
            entity.Property(e => e.VariantOrder).HasColumnName("variant_order");
            entity.Property(e => e.VariantText)
                .HasMaxLength(500)
                .HasColumnName("variant_text");

            entity.HasOne(d => d.Assignment).WithMany(p => p.AssignmentVariants)
                .HasForeignKey(d => d.AssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__assignmen__assig__0C85DE4D");
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

        modelBuilder.Entity<CourseInstance>(entity =>
        {
            entity.HasKey(e => e.InstanceId).HasName("PK__course_i__7DBD82E77478442E");

            entity.ToTable("course_instance");

            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
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
            entity.Property(e => e.TotalWeeks).HasColumnName("total_weeks");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseInstances)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_in__cours__114A936A");
        });

        modelBuilder.Entity<CourseInstanceCoordinator>(entity =>
        {
            entity.HasKey(e => e.CoordinatorId).HasName("PK__course_i__0622227BBF41D529");

            entity.ToTable("course_instance_coordinator");

            entity.HasIndex(e => new { e.InstanceId, e.EmployeeId }, "UQ__course_i__E1EF625C28150AF2").IsUnique();

            entity.Property(e => e.CoordinatorId).HasColumnName("coordinator_id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("assigned_at");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.InstanceId).HasColumnName("instance_id");
            entity.Property(e => e.IsLead)
                .HasDefaultValue(false)
                .HasColumnName("is_lead");

            entity.HasOne(d => d.Employee).WithMany(p => p.CourseInstanceCoordinators)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_in__emplo__17F790F9");

            entity.HasOne(d => d.Instance).WithMany(p => p.CourseInstanceCoordinators)
                .HasForeignKey(d => d.InstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_in__insta__17036CC0");
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

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
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
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
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
            entity.Property(e => e.AssignedTeacherId).HasColumnName("assigned_teacher_id");
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
                .HasConstraintName("FK__enrollmen__assig__25518C17");

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
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("pending")
                .HasColumnName("payment_status");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.InstallmentPayments)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__installme__plan___625A9A57");
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
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.IsFreePreview)
                .HasDefaultValue(false)
                .HasColumnName("is_free_preview");
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
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
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
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasKey(e => e.PromoCodeId).HasName("PK__promo_co__C52CD3126ED9598B");

            entity.ToTable("promo_code");

            entity.HasIndex(e => e.Code, "UQ__promo_co__357D4CF9FA2698A5").IsUnique();

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
            entity.Property(e => e.TypeId)
                .HasColumnName("type_id");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxUses).HasColumnName("max_uses");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidUntil).HasColumnName("valid_until");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__review__60883D901B037C57");

            entity.ToTable("review");

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
            entity.Property(e => e.PrivacyPolicyUrl).HasColumnName("privacy_policy_url");
            entity.Property(e => e.SchoolName)
                .HasMaxLength(200)
                .HasColumnName("school_name");
            entity.Property(e => e.TermsOfUseUrl).HasColumnName("terms_of_use_url");
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

        modelBuilder.Entity<StudentLessonAccess>(entity =>
        {
            entity.HasKey(e => e.AccessId).HasName("PK__student___10FA1E20A2B27802");

            entity.ToTable("student_lesson_access");

            entity.HasIndex(e => new { e.EnrollmentId, e.LessonId }, "UQ__student___9B66B500EF6E9354").IsUnique();

            entity.Property(e => e.AccessId).HasColumnName("access_id");
            entity.Property(e => e.ActualOpenDatetime)
                .HasColumnType("datetime")
                .HasColumnName("actual_open_datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IsAvailable)
                .HasDefaultValue(false)
                .HasColumnName("is_available");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.OpenedByEmployeeId).HasColumnName("opened_by_employee_id");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.PlannedAccessDate).HasColumnName("planned_access_date");
            entity.Property(e => e.PlannedAccessTime)
                .HasDefaultValue(new TimeOnly(0, 0, 0))
                .HasColumnName("planned_access_time");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.StudentLessonAccesses)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_l__enrol__2CF2ADDF");

            entity.HasOne(d => d.Lesson).WithMany(p => p.StudentLessonAccesses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_l__lesso__2DE6D218");

            entity.HasOne(d => d.OpenedByEmployee).WithMany(p => p.StudentLessonAccesses)
                .HasForeignKey(d => d.OpenedByEmployeeId)
                .HasConstraintName("FK__student_l__opene__2FCF1A8A");

            entity.HasOne(d => d.Plan).WithMany(p => p.StudentLessonAccesses)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK__student_l__plan___2EDAF651");
        });

        modelBuilder.Entity<StudentProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__student___49B3D8C13285DD7D");

            entity.ToTable("student_progress");

            entity.HasIndex(e => new { e.EnrollmentId, e.LessonId }, "UQ__student___9B66B5004ABE33F9").IsUnique();

            entity.Property(e => e.ProgressId).HasColumnName("progress_id");
            entity.Property(e => e.AccessId).HasColumnName("access_id");
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

            entity.HasOne(d => d.Access).WithMany(p => p.StudentProgresses)
                .HasForeignKey(d => d.AccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_p__acces__3864608B");

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
            entity.Property(e => e.AttachedFileName)
                .HasMaxLength(255)
                .HasColumnName("attached_file_name");
            entity.Property(e => e.AttachedFileUrl)
                .HasMaxLength(500)
                .HasColumnName("attached_file_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GradedAt)
                .HasColumnType("datetime")
                .HasColumnName("graded_at");
            entity.Property(e => e.GradedByEmployeeId).HasColumnName("graded_by_employee_id");
            entity.Property(e => e.ProgressId).HasColumnName("progress_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.StudentAnswerText).HasColumnName("student_answer_text");
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

            entity.HasOne(d => d.Progress).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.ProgressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__submissio__progr__3E1D39E1");

            entity.HasOne(d => d.SubmissionStatus).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.SubmissionStatusId)
                .HasConstraintName("FK__submissio__submi__40F9A68C");
        });

        modelBuilder.Entity<SubmissionReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__submissi__60883D90B01CED93");

            entity.ToTable("submission_review");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.PointsAwarded).HasColumnName("points_awarded");
            entity.Property(e => e.QuestionNumber).HasColumnName("question_number");
            entity.Property(e => e.StudentVariantId).HasColumnName("student_variant_id");
            entity.Property(e => e.SubmissionId).HasColumnName("submission_id");
            entity.Property(e => e.TeacherComment)
                .HasMaxLength(1000)
                .HasColumnName("teacher_comment");

            entity.HasOne(d => d.Submission).WithMany(p => p.SubmissionReviews)
                .HasForeignKey(d => d.SubmissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__submissio__submi__44CA3770");
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
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
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

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsEmailConfirmed)
                .HasDefaultValue(false)
                .HasColumnName("is_email_confirmed");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_id__66603565");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
