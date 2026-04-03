IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [application_status] (
        [status_id] int NOT NULL IDENTITY,
        [status_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__applicat__3683B5319D36604F] PRIMARY KEY ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [assignment_type] (
        [type_id] int NOT NULL IDENTITY,
        [type_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__assignme__2C0005983E0FA961] PRIMARY KEY ([type_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [discount_type] (
        [type_id] int NOT NULL IDENTITY,
        [type_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        [is_active] bit NULL,
        CONSTRAINT [PK__discount__5B1B3754F7C0A2B0] PRIMARY KEY ([type_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [enrollment_status] (
        [status_id] int NOT NULL IDENTITY,
        [status_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__enrollme__3683B5315C08870B] PRIMARY KEY ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [exam] (
        [exam_id] int NOT NULL IDENTITY,
        [exam_name] nvarchar(100) NOT NULL,
        [description] nvarchar(500) NULL,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__exam__9C8C7BE9] PRIMARY KEY ([exam_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [faq_category] (
        [category_id] int NOT NULL IDENTITY,
        [category_name] nvarchar(100) NOT NULL,
        [category_order] int NULL,
        CONSTRAINT [PK__faq_cate__D54EE9B4F8DDF668] PRIMARY KEY ([category_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [lesson_type] (
        [type_id] int NOT NULL IDENTITY,
        [type_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__lesson_t__2C0005985353B43C] PRIMARY KEY ([type_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [order_status] (
        [status_id] int NOT NULL IDENTITY,
        [status_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__order_st__3683B53106712211] PRIMARY KEY ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [payment_status] (
        [status_id] int NOT NULL IDENTITY,
        [status_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__payment___3683B53112C26BD6] PRIMARY KEY ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [promo_code] (
        [promo_code_id] int NOT NULL IDENTITY,
        [code] nvarchar(50) NOT NULL,
        [type_id] int NULL,
        [discount_value] decimal(10,2) NOT NULL,
        [valid_from] date NOT NULL,
        [valid_until] date NULL,
        [max_uses] int NULL,
        [current_uses] int NULL DEFAULT 0,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__promo_co__C52CD3126ED9598B] PRIMARY KEY ([promo_code_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [school_setting] (
        [setting_id] int NOT NULL IDENTITY,
        [school_name] nvarchar(200) NOT NULL,
        [logo_url] nvarchar(500) NULL,
        [contact_phone] nvarchar(20) NULL,
        [contact_email] nvarchar(255) NULL,
        [address] nvarchar(500) NULL,
        [about_school_text] nvarchar(max) NULL,
        [privacy_policy_url] nvarchar(max) NULL,
        [terms_of_use_url] nvarchar(max) NULL,
        [updated_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__school_s__256E1E321D93193D] PRIMARY KEY ([setting_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [subject] (
        [subject_id] int NOT NULL IDENTITY,
        [subject_name] nvarchar(100) NOT NULL,
        [description] nvarchar(500) NULL,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__subject__5004F660] PRIMARY KEY ([subject_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [submission_status] (
        [status_id] int NOT NULL IDENTITY,
        [status_name] nvarchar(50) NOT NULL,
        [description] nvarchar(200) NULL,
        CONSTRAINT [PK__submissi__3683B5310C9A541F] PRIMARY KEY ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [user_role] (
        [role_id] int NOT NULL IDENTITY,
        [role_name] nvarchar(50) NOT NULL,
        [description] nvarchar(255) NULL,
        CONSTRAINT [PK__user_rol__760965CCF7A32D04] PRIMARY KEY ([role_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [faq_item] (
        [faq_id] int NOT NULL IDENTITY,
        [category_id] int NULL,
        [question] nvarchar(500) NOT NULL,
        [answer] nvarchar(max) NOT NULL,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [item_order] int NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__faq_item__66734BAF381EBE8D] PRIMARY KEY ([faq_id]),
        CONSTRAINT [FK__faq_item__catego__756D6ECB] FOREIGN KEY ([category_id]) REFERENCES [faq_category] ([category_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [course_category] (
        [category_id] int NOT NULL IDENTITY,
        [category_name] nvarchar(100) NOT NULL,
        [description] nvarchar(500) NULL,
        [subject_id] int NULL,
        [exam_id] int NULL,
        CONSTRAINT [PK__course_c__D54EE9B4D070B0E9] PRIMARY KEY ([category_id]),
        CONSTRAINT [FK__course_ca__exam] FOREIGN KEY ([exam_id]) REFERENCES [exam] ([exam_id]),
        CONSTRAINT [FK__course_ca__subject] FOREIGN KEY ([subject_id]) REFERENCES [subject] ([subject_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [users] (
        [user_id] int NOT NULL IDENTITY,
        [email] nvarchar(255) NOT NULL,
        [password_hash] nvarchar(255) NOT NULL,
        [role_id] int NOT NULL,
        [is_email_confirmed] bit NULL DEFAULT CAST(0 AS bit),
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__users__B9BE370FE725EF09] PRIMARY KEY ([user_id]),
        CONSTRAINT [FK__users__role_id__66603565] FOREIGN KEY ([role_id]) REFERENCES [user_role] ([role_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [course] (
        [course_id] int NOT NULL IDENTITY,
        [title] nvarchar(200) NOT NULL,
        [description] nvarchar(max) NULL,
        [short_description] nvarchar(500) NULL,
        [category_id] int NOT NULL,
        [cover_img_url] nvarchar(500) NULL,
        [price] decimal(10,2) NOT NULL,
        [discount_price] decimal(10,2) NULL,
        [total_hours] int NULL,
        [what_you_get] nvarchar(max) NULL,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__course__8F1EF7AED3B7CE62] PRIMARY KEY ([course_id]),
        CONSTRAINT [FK__course__category__75A278F5] FOREIGN KEY ([category_id]) REFERENCES [course_category] ([category_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [employee] (
        [employee_id] int NOT NULL IDENTITY,
        [user_id] int NOT NULL,
        [first_name] nvarchar(100) NOT NULL,
        [last_name] nvarchar(100) NOT NULL,
        [patronymic] nvarchar(100) NULL,
        [phone] nvarchar(20) NULL,
        [date_of_birth] date NULL,
        [avatar_url] nvarchar(500) NULL,
        [work_experience] int NULL,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__employee__C52E0BA85D738F50] PRIMARY KEY ([employee_id]),
        CONSTRAINT [FK__employee__user_i__70DDC3D8] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [notification] (
        [notification_id] int NOT NULL IDENTITY,
        [user_id] int NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [message] nvarchar(1000) NOT NULL,
        [notification_type] nvarchar(50) NULL,
        [is_read] bit NULL DEFAULT CAST(0 AS bit),
        [related_entity_type] nvarchar(50) NULL,
        [related_entity_id] int NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__notifica__E059842F14F888C5] PRIMARY KEY ([notification_id]),
        CONSTRAINT [FK__notificat__user___02C769E9] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [student] (
        [student_id] int NOT NULL IDENTITY,
        [user_id] int NOT NULL,
        [first_name] nvarchar(100) NOT NULL,
        [last_name] nvarchar(100) NOT NULL,
        [phone] nvarchar(20) NULL,
        [date_of_birth] date NULL,
        [avatar_url] nvarchar(500) NULL,
        [class_number] int NOT NULL,
        [parent_phone] nvarchar(20) NULL,
        [parent_email] nvarchar(255) NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__student__2A33069A81FCD669] PRIMARY KEY ([student_id]),
        CONSTRAINT [FK__student__user_id__6B24EA82] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [course_instance] (
        [instance_id] int NOT NULL IDENTITY,
        [course_id] int NOT NULL,
        [instance_name] nvarchar(200) NOT NULL,
        [start_date] date NOT NULL,
        [end_date] date NULL,
        [total_weeks] int NULL,
        [lessons_per_week] int NULL,
        [schedule_description] nvarchar(500) NULL,
        [max_students] int NULL,
        [is_active] bit NULL DEFAULT CAST(1 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__course_i__7DBD82E77478442E] PRIMARY KEY ([instance_id]),
        CONSTRAINT [FK__course_in__cours__114A936A] FOREIGN KEY ([course_id]) REFERENCES [course] ([course_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [course_module] (
        [module_id] int NOT NULL IDENTITY,
        [course_id] int NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [description] nvarchar(1000) NULL,
        [module_order] int NOT NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__course_m__1A2D065313F63517] PRIMARY KEY ([module_id]),
        CONSTRAINT [FK__course_mo__cours__797309D9] FOREIGN KEY ([course_id]) REFERENCES [course] ([course_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [trial_application] (
        [application_id] int NOT NULL IDENTITY,
        [first_name] nvarchar(100) NOT NULL,
        [last_name] nvarchar(100) NOT NULL,
        [phone] nvarchar(20) NOT NULL,
        [email] nvarchar(255) NULL,
        [class_number] int NOT NULL,
        [selected_subjects] nvarchar(500) NOT NULL,
        [application_status_id] int NULL DEFAULT 1,
        [assigned_manager_id] int NULL,
        [manager_comment] nvarchar(1000) NULL,
        [contacted_at] datetime NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__trial_ap__3BCBDCF28406F1D6] PRIMARY KEY ([application_id]),
        CONSTRAINT [FK__trial_app__appli__671F4F74] FOREIGN KEY ([application_status_id]) REFERENCES [application_status] ([status_id]),
        CONSTRAINT [FK__trial_app__assig__681373AD] FOREIGN KEY ([assigned_manager_id]) REFERENCES [employee] ([employee_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [app_order] (
        [order_id] int NOT NULL IDENTITY,
        [student_id] int NOT NULL,
        [order_number] nvarchar(50) NOT NULL,
        [total_amount] decimal(10,2) NOT NULL,
        [discount_amount] decimal(10,2) NULL DEFAULT 0.0,
        [final_amount] decimal(10,2) NOT NULL,
        [order_status_id] int NULL DEFAULT 1,
        [payment_method] nvarchar(50) NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        [paid_at] datetime NULL,
        CONSTRAINT [PK__app_orde__46596229B06C6B8B] PRIMARY KEY ([order_id]),
        CONSTRAINT [FK__app_order__order__4C6B5938] FOREIGN KEY ([order_status_id]) REFERENCES [order_status] ([status_id]),
        CONSTRAINT [FK__app_order__stude__4B7734FF] FOREIGN KEY ([student_id]) REFERENCES [student] ([student_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [review] (
        [review_id] int NOT NULL IDENTITY,
        [student_id] int NOT NULL,
        [course_id] int NOT NULL,
        [rating] int NULL,
        [comment] nvarchar(max) NULL,
        [is_published] bit NULL DEFAULT CAST(0 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__review__60883D901B037C57] PRIMARY KEY ([review_id]),
        CONSTRAINT [FK__review__course_i__6EC0713C] FOREIGN KEY ([course_id]) REFERENCES [course] ([course_id]),
        CONSTRAINT [FK__review__student___6DCC4D03] FOREIGN KEY ([student_id]) REFERENCES [student] ([student_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [course_instance_coordinator] (
        [coordinator_id] int NOT NULL IDENTITY,
        [instance_id] int NOT NULL,
        [employee_id] int NOT NULL,
        [is_lead] bit NULL DEFAULT CAST(0 AS bit),
        [assigned_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__course_i__0622227BBF41D529] PRIMARY KEY ([coordinator_id]),
        CONSTRAINT [FK__course_in__emplo__17F790F9] FOREIGN KEY ([employee_id]) REFERENCES [employee] ([employee_id]),
        CONSTRAINT [FK__course_in__insta__17036CC0] FOREIGN KEY ([instance_id]) REFERENCES [course_instance] ([instance_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [enrollment] (
        [enrollment_id] int NOT NULL IDENTITY,
        [student_id] int NOT NULL,
        [instance_id] int NOT NULL,
        [assigned_teacher_id] int NULL,
        [enrolled_at] datetime NULL DEFAULT ((getdate())),
        [enrollment_status_id] int NULL DEFAULT 1,
        [completed_at] datetime NULL,
        [final_score] decimal(5,2) NULL,
        CONSTRAINT [PK__enrollme__6D24AA7AD787A167] PRIMARY KEY ([enrollment_id]),
        CONSTRAINT [FK__enrollmen__assig__25518C17] FOREIGN KEY ([assigned_teacher_id]) REFERENCES [employee] ([employee_id]),
        CONSTRAINT [FK__enrollmen__enrol__2645B050] FOREIGN KEY ([enrollment_status_id]) REFERENCES [enrollment_status] ([status_id]),
        CONSTRAINT [FK__enrollmen__insta__245D67DE] FOREIGN KEY ([instance_id]) REFERENCES [course_instance] ([instance_id]),
        CONSTRAINT [FK__enrollmen__stude__236943A5] FOREIGN KEY ([student_id]) REFERENCES [student] ([student_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [lesson] (
        [lesson_id] int NOT NULL IDENTITY,
        [module_id] int NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [lesson_type_id] int NOT NULL,
        [content] nvarchar(max) NULL,
        [video_url] nvarchar(500) NULL,
        [duration_minutes] int NULL,
        [lesson_order] int NOT NULL,
        [is_free_preview] bit NULL DEFAULT CAST(0 AS bit),
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__lesson__6421F7BED700B577] PRIMARY KEY ([lesson_id]),
        CONSTRAINT [FK__lesson__lesson_t__7F2BE32F] FOREIGN KEY ([lesson_type_id]) REFERENCES [lesson_type] ([type_id]),
        CONSTRAINT [FK__lesson__module_i__7E37BEF6] FOREIGN KEY ([module_id]) REFERENCES [course_module] ([module_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [installment_plan] (
        [plan_id] int NOT NULL IDENTITY,
        [order_id] int NOT NULL,
        [total_amount] decimal(10,2) NOT NULL,
        [installment_count] int NOT NULL,
        [monthly_payment] decimal(10,2) NOT NULL,
        [next_payment_date] date NULL,
        [plan_status] nvarchar(50) NULL DEFAULT N'active',
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__installm__BE9F8F1D65579815] PRIMARY KEY ([plan_id]),
        CONSTRAINT [FK__installme__order__5D95E53A] FOREIGN KEY ([order_id]) REFERENCES [app_order] ([order_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [order_item] (
        [order_item_id] int NOT NULL IDENTITY,
        [order_id] int NOT NULL,
        [course_id] int NOT NULL,
        [instance_id] int NULL,
        [price] decimal(10,2) NOT NULL,
        [quantity] int NULL DEFAULT 1,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__order_it__3764B6BC0E7FE974] PRIMARY KEY ([order_item_id]),
        CONSTRAINT [FK__order_ite__cours__5224328E] FOREIGN KEY ([course_id]) REFERENCES [course] ([course_id]),
        CONSTRAINT [FK__order_ite__insta__531856C7] FOREIGN KEY ([instance_id]) REFERENCES [course_instance] ([instance_id]),
        CONSTRAINT [FK__order_ite__order__51300E55] FOREIGN KEY ([order_id]) REFERENCES [app_order] ([order_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [payment] (
        [payment_id] int NOT NULL IDENTITY,
        [order_id] int NOT NULL,
        [external_payment_id] nvarchar(100) NULL,
        [amount] decimal(10,2) NOT NULL,
        [payment_status_id] int NULL DEFAULT 1,
        [payment_method] nvarchar(50) NULL,
        [card_last_four] nvarchar(4) NULL,
        [paid_at] datetime NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__payment__ED1FC9EA1DBF32F8] PRIMARY KEY ([payment_id]),
        CONSTRAINT [FK__payment__order_i__57DD0BE4] FOREIGN KEY ([order_id]) REFERENCES [app_order] ([order_id]),
        CONSTRAINT [FK__payment__payment__58D1301D] FOREIGN KEY ([payment_status_id]) REFERENCES [payment_status] ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [assignment] (
        [assignment_id] int NOT NULL IDENTITY,
        [lesson_id] int NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [description] nvarchar(max) NULL,
        [assignment_type_id] int NOT NULL,
        [max_score] int NOT NULL,
        [due_days_after_lesson] int NULL,
        [correct_answer] nvarchar(max) NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__assignme__DA891814914741D1] PRIMARY KEY ([assignment_id]),
        CONSTRAINT [FK__assignmen__assig__08B54D69] FOREIGN KEY ([assignment_type_id]) REFERENCES [assignment_type] ([type_id]),
        CONSTRAINT [FK__assignmen__lesso__07C12930] FOREIGN KEY ([lesson_id]) REFERENCES [lesson] ([lesson_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [course_schedule_plan] (
        [plan_id] int NOT NULL IDENTITY,
        [instance_id] int NOT NULL,
        [lesson_id] int NOT NULL,
        [release_day_offset] int NOT NULL,
        [release_time] time NULL DEFAULT '00:00:00',
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__course_s__BE9F8F1DD08EDE69] PRIMARY KEY ([plan_id]),
        CONSTRAINT [FK__course_sc__insta__1DB06A4F] FOREIGN KEY ([instance_id]) REFERENCES [course_instance] ([instance_id]),
        CONSTRAINT [FK__course_sc__lesso__1EA48E88] FOREIGN KEY ([lesson_id]) REFERENCES [lesson] ([lesson_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [lesson_material] (
        [material_id] int NOT NULL IDENTITY,
        [lesson_id] int NOT NULL,
        [file_name] nvarchar(255) NOT NULL,
        [file_url] nvarchar(500) NOT NULL,
        [file_type] nvarchar(50) NULL,
        [file_size_kb] int NULL,
        [download_count] int NULL DEFAULT 0,
        [uploaded_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__lesson_m__6BFE1D28F3A17522] PRIMARY KEY ([material_id]),
        CONSTRAINT [FK__lesson_ma__lesso__03F0984C] FOREIGN KEY ([lesson_id]) REFERENCES [lesson] ([lesson_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [installment_payment] (
        [installment_payment_id] int NOT NULL IDENTITY,
        [plan_id] int NOT NULL,
        [installment_number] int NOT NULL,
        [due_date] date NOT NULL,
        [amount] decimal(10,2) NOT NULL,
        [payment_status] nvarchar(50) NULL DEFAULT N'pending',
        [paid_at] datetime NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__installm__29799A54E5B2195E] PRIMARY KEY ([installment_payment_id]),
        CONSTRAINT [FK__installme__plan___625A9A57] FOREIGN KEY ([plan_id]) REFERENCES [installment_plan] ([plan_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [assignment_variant] (
        [variant_id] int NOT NULL IDENTITY,
        [assignment_id] int NOT NULL,
        [variant_text] nvarchar(500) NOT NULL,
        [is_correct] bit NULL DEFAULT CAST(0 AS bit),
        [variant_order] int NULL,
        CONSTRAINT [PK__assignme__EACC68B7FAD84BD6] PRIMARY KEY ([variant_id]),
        CONSTRAINT [FK__assignmen__assig__0C85DE4D] FOREIGN KEY ([assignment_id]) REFERENCES [assignment] ([assignment_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [student_lesson_access] (
        [access_id] int NOT NULL IDENTITY,
        [enrollment_id] int NOT NULL,
        [lesson_id] int NOT NULL,
        [plan_id] int NULL,
        [planned_access_date] date NOT NULL,
        [planned_access_time] time NULL DEFAULT '00:00:00',
        [actual_open_datetime] datetime NULL,
        [is_available] bit NULL DEFAULT CAST(0 AS bit),
        [opened_by_employee_id] int NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__student___10FA1E20A2B27802] PRIMARY KEY ([access_id]),
        CONSTRAINT [FK__student_l__enrol__2CF2ADDF] FOREIGN KEY ([enrollment_id]) REFERENCES [enrollment] ([enrollment_id]),
        CONSTRAINT [FK__student_l__lesso__2DE6D218] FOREIGN KEY ([lesson_id]) REFERENCES [lesson] ([lesson_id]),
        CONSTRAINT [FK__student_l__opene__2FCF1A8A] FOREIGN KEY ([opened_by_employee_id]) REFERENCES [employee] ([employee_id]),
        CONSTRAINT [FK__student_l__plan___2EDAF651] FOREIGN KEY ([plan_id]) REFERENCES [course_schedule_plan] ([plan_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [student_progress] (
        [progress_id] int NOT NULL IDENTITY,
        [enrollment_id] int NOT NULL,
        [lesson_id] int NOT NULL,
        [access_id] int NOT NULL,
        [is_completed] bit NULL DEFAULT CAST(0 AS bit),
        [completed_at] datetime NULL,
        [watch_time_seconds] int NULL DEFAULT 0,
        [last_accessed] datetime NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__student___49B3D8C13285DD7D] PRIMARY KEY ([progress_id]),
        CONSTRAINT [FK__student_p__acces__3864608B] FOREIGN KEY ([access_id]) REFERENCES [student_lesson_access] ([access_id]),
        CONSTRAINT [FK__student_p__enrol__367C1819] FOREIGN KEY ([enrollment_id]) REFERENCES [enrollment] ([enrollment_id]),
        CONSTRAINT [FK__student_p__lesso__37703C52] FOREIGN KEY ([lesson_id]) REFERENCES [lesson] ([lesson_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [submission] (
        [submission_id] int NOT NULL IDENTITY,
        [progress_id] int NOT NULL,
        [assignment_id] int NOT NULL,
        [student_answer_text] nvarchar(max) NULL,
        [attached_file_url] nvarchar(500) NULL,
        [attached_file_name] nvarchar(255) NULL,
        [submitted_at] datetime NULL DEFAULT ((getdate())),
        [submission_status_id] int NULL DEFAULT 1,
        [score] int NULL,
        [teacher_comment] nvarchar(max) NULL,
        [graded_at] datetime NULL,
        [graded_by_employee_id] int NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__submissi__9B53559500EEB980] PRIMARY KEY ([submission_id]),
        CONSTRAINT [FK__submissio__assig__3F115E1A] FOREIGN KEY ([assignment_id]) REFERENCES [assignment] ([assignment_id]),
        CONSTRAINT [FK__submissio__grade__40058253] FOREIGN KEY ([graded_by_employee_id]) REFERENCES [employee] ([employee_id]),
        CONSTRAINT [FK__submissio__progr__3E1D39E1] FOREIGN KEY ([progress_id]) REFERENCES [student_progress] ([progress_id]),
        CONSTRAINT [FK__submissio__submi__40F9A68C] FOREIGN KEY ([submission_status_id]) REFERENCES [submission_status] ([status_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE TABLE [submission_review] (
        [review_id] int NOT NULL IDENTITY,
        [submission_id] int NOT NULL,
        [question_number] int NULL,
        [student_variant_id] int NULL,
        [is_correct] bit NULL,
        [teacher_comment] nvarchar(1000) NULL,
        [points_awarded] int NULL,
        [created_at] datetime NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__submissi__60883D90B01CED93] PRIMARY KEY ([review_id]),
        CONSTRAINT [FK__submissio__submi__44CA3770] FOREIGN KEY ([submission_id]) REFERENCES [submission] ([submission_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_app_order_order_status_id] ON [app_order] ([order_status_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_app_order_student_id] ON [app_order] ([student_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__app_orde__730E34DFC896B282] ON [app_order] ([order_number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__applicat__501B37533F14CCB4] ON [application_status] ([status_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_assignment_assignment_type_id] ON [assignment] ([assignment_type_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_assignment_lesson_id] ON [assignment] ([lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__assignme__543C4FD91EEA53C8] ON [assignment_type] ([type_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_assignment_variant_assignment_id] ON [assignment_variant] ([assignment_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_category_id] ON [course] ([category_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_category_exam_id] ON [course_category] ([exam_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_category_subject_id] ON [course_category] ([subject_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_instance_course_id] ON [course_instance] ([course_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_instance_coordinator_employee_id] ON [course_instance_coordinator] ([employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__course_i__E1EF625C28150AF2] ON [course_instance_coordinator] ([instance_id], [employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_module_course_id] ON [course_module] ([course_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_course_schedule_plan_lesson_id] ON [course_schedule_plan] ([lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__course_s__8BFF9D9DF4CC949C] ON [course_schedule_plan] ([instance_id], [lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__employee__B9BE370ED2AF6F2C] ON [employee] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_enrollment_assigned_teacher_id] ON [enrollment] ([assigned_teacher_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_enrollment_enrollment_status_id] ON [enrollment] ([enrollment_status_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_enrollment_instance_id] ON [enrollment] ([instance_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_enrollment_student_id] ON [enrollment] ([student_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__enrollme__501B3753A9CF44C7] ON [enrollment_status] ([status_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__exam__D916B1FC] ON [exam] ([exam_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_faq_item_category_id] ON [faq_item] ([category_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_installment_payment_plan_id] ON [installment_payment] ([plan_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_installment_plan_order_id] ON [installment_plan] ([order_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_lesson_lesson_type_id] ON [lesson] ([lesson_type_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_lesson_module_id] ON [lesson] ([module_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_lesson_material_lesson_id] ON [lesson_material] ([lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__lesson_t__543C4FD9863219B2] ON [lesson_type] ([type_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_notification_user_id] ON [notification] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_order_item_course_id] ON [order_item] ([course_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_order_item_instance_id] ON [order_item] ([instance_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_order_item_order_id] ON [order_item] ([order_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__order_st__501B3753E3436EE4] ON [order_status] ([status_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_payment_order_id] ON [payment] ([order_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_payment_payment_status_id] ON [payment] ([payment_status_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__payment___501B3753B8EE7B1D] ON [payment_status] ([status_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__promo_co__357D4CF9FA2698A5] ON [promo_code] ([code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_review_course_id] ON [review] ([course_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_review_student_id] ON [review] ([student_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__student__B9BE370E83B5DF70] ON [student] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_student_lesson_access_lesson_id] ON [student_lesson_access] ([lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_student_lesson_access_opened_by_employee_id] ON [student_lesson_access] ([opened_by_employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_student_lesson_access_plan_id] ON [student_lesson_access] ([plan_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__student___9B66B500EF6E9354] ON [student_lesson_access] ([enrollment_id], [lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_student_progress_access_id] ON [student_progress] ([access_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_student_progress_lesson_id] ON [student_progress] ([lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__student___9B66B5004ABE33F9] ON [student_progress] ([enrollment_id], [lesson_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__subject__5004F679] ON [subject] ([subject_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_submission_assignment_id] ON [submission] ([assignment_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_submission_graded_by_employee_id] ON [submission] ([graded_by_employee_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_submission_progress_id] ON [submission] ([progress_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_submission_submission_status_id] ON [submission] ([submission_status_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_submission_review_submission_id] ON [submission_review] ([submission_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__submissi__501B37533F4A7911] ON [submission_status] ([status_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_trial_application_application_status_id] ON [trial_application] ([application_status_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_trial_application_assigned_manager_id] ON [trial_application] ([assigned_manager_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__user_rol__783254B15157E60E] ON [user_role] ([role_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_users_role_id] ON [users] ([role_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__users__AB6E61649B7A09A4] ON [users] ([email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403165424_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403165424_InitialCreate', N'9.0.0');
END;

COMMIT;
GO

