/*
  Миграция: структура тестов с разными типами вопросов (SQL Server).
  Идемпотентно: проверки OBJECT_ID / sys.indexes, транзакции, TRY/CATCH.
  Существующие таблицы assignment / assignment_variant / submission не удаляются.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* ---------- 1. Таблица вопросов теста ---------- */
    IF OBJECT_ID(N'[dbo].[test_question]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[test_question] (
            [question_id]           INT            NOT NULL IDENTITY(1, 1),
            [assignment_id]         INT            NOT NULL,
            [question_text]         NVARCHAR(MAX)  NOT NULL,
            [question_type]         VARCHAR(30)    NOT NULL,
            [max_points]            DECIMAL(10, 2) NOT NULL CONSTRAINT [DF_test_question_max_points] DEFAULT (1),
            [question_order]        INT            NOT NULL CONSTRAINT [DF_test_question_question_order] DEFAULT (1),
            [allow_partial_credit]  BIT            NOT NULL CONSTRAINT [DF_test_question_allow_partial] DEFAULT (0),
            [numeric_tolerance]     DECIMAL(18, 6) NULL,
            [case_insensitive_text] BIT            NOT NULL CONSTRAINT [DF_test_question_case_insensitive] DEFAULT (1),
            [explanation]           NVARCHAR(MAX) NULL,
            [created_at]            DATETIME2(0)   NOT NULL CONSTRAINT [DF_test_question_created_at] DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT [PK_test_question] PRIMARY KEY CLUSTERED ([question_id]),
            CONSTRAINT [FK_test_question_assignment] FOREIGN KEY ([assignment_id])
                REFERENCES [dbo].[assignment] ([assignment_id]),
            CONSTRAINT [CK_test_question_type] CHECK (
                [question_type] IN (N'single_choice', N'multiple_choice', N'text', N'number')
            ),
            CONSTRAINT [CK_test_question_max_points_positive] CHECK ([max_points] > 0)
        );
    END;

    /* ---------- 2. Варианты ответов (single / multiple choice) ---------- */
    IF OBJECT_ID(N'[dbo].[test_answer_variant]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[test_answer_variant] (
            [answer_variant_id] INT            NOT NULL IDENTITY(1, 1),
            [question_id]       INT            NOT NULL,
            [variant_text]      NVARCHAR(500)  NOT NULL,
            [is_correct]        BIT            NOT NULL CONSTRAINT [DF_test_answer_variant_is_correct] DEFAULT (0),
            [variant_order]     INT            NULL,
            [variant_points]    DECIMAL(10, 2) NULL,
            CONSTRAINT [PK_test_answer_variant] PRIMARY KEY CLUSTERED ([answer_variant_id]),
            CONSTRAINT [FK_test_answer_variant_question] FOREIGN KEY ([question_id])
                REFERENCES [dbo].[test_question] ([question_id]) ON DELETE CASCADE
        );
    END;

    /* ---------- 3. Эталонные ответы (текст / число), несколько строк — синонимы или допустимые варианты ---------- */
    IF OBJECT_ID(N'[dbo].[test_text_answer]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[test_text_answer] (
            [text_answer_id] INT             NOT NULL IDENTITY(1, 1),
            [question_id]    INT             NOT NULL,
            [answer_text]    NVARCHAR(MAX)   NULL,
            [answer_number]  DECIMAL(18, 6)  NULL,
            [answer_order]   INT             NULL,
            CONSTRAINT [PK_test_text_answer] PRIMARY KEY CLUSTERED ([text_answer_id]),
            CONSTRAINT [FK_test_text_answer_question] FOREIGN KEY ([question_id])
                REFERENCES [dbo].[test_question] ([question_id]) ON DELETE CASCADE,
            CONSTRAINT [CK_test_text_answer_has_value] CHECK (
                [answer_text] IS NOT NULL OR [answer_number] IS NOT NULL
            )
        );
    END;

    /* ---------- 4. Ответы учеников по вопросам ---------- */
    IF OBJECT_ID(N'[dbo].[test_student_answer]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[test_student_answer] (
            [student_answer_id]          INT             NOT NULL IDENTITY(1, 1),
            [submission_id]              INT             NOT NULL,
            [question_id]                INT             NOT NULL,
            [single_selected_variant_id] INT             NULL,
            [selected_variants_json]     NVARCHAR(MAX) NULL,
            [response_text]              NVARCHAR(MAX) NULL,
            [response_number]            DECIMAL(18, 6) NULL,
            [points_awarded]             DECIMAL(10, 2) NULL,
            [auto_grade_state]           VARCHAR(30)   NULL,
            [is_fully_auto_graded]       BIT            NOT NULL CONSTRAINT [DF_test_student_answer_auto] DEFAULT (0),
            [teacher_comment]            NVARCHAR(MAX) NULL,
            [answered_at]                DATETIME2(0)   NOT NULL CONSTRAINT [DF_test_student_answer_answered_at] DEFAULT (SYSUTCDATETIME()),
            CONSTRAINT [PK_test_student_answer] PRIMARY KEY CLUSTERED ([student_answer_id]),
            CONSTRAINT [FK_test_student_answer_submission] FOREIGN KEY ([submission_id])
                REFERENCES [dbo].[submission] ([submission_id]) ON DELETE CASCADE,
            CONSTRAINT [FK_test_student_answer_question] FOREIGN KEY ([question_id])
                REFERENCES [dbo].[test_question] ([question_id]),
            CONSTRAINT [FK_test_student_answer_single_variant] FOREIGN KEY ([single_selected_variant_id])
                REFERENCES [dbo].[test_answer_variant] ([answer_variant_id]),
            CONSTRAINT [CK_test_student_answer_auto_grade_state] CHECK (
                [auto_grade_state] IS NULL
                OR [auto_grade_state] IN (
                    N'not_evaluated', N'correct', N'incorrect', N'partial', N'manual_required'
                )
            )
        );
    END;

    /* ---------- Индексы ---------- */
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_test_question_assignment_order'
          AND i.object_id = OBJECT_ID(N'[dbo].[test_question]', N'U')
    )
        CREATE NONCLUSTERED INDEX [IX_test_question_assignment_order]
            ON [dbo].[test_question] ([assignment_id], [question_order]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_test_answer_variant_question_id'
          AND i.object_id = OBJECT_ID(N'[dbo].[test_answer_variant]', N'U')
    )
        CREATE NONCLUSTERED INDEX [IX_test_answer_variant_question_id]
            ON [dbo].[test_answer_variant] ([question_id]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_test_text_answer_question_id'
          AND i.object_id = OBJECT_ID(N'[dbo].[test_text_answer]', N'U')
    )
        CREATE NONCLUSTERED INDEX [IX_test_text_answer_question_id]
            ON [dbo].[test_text_answer] ([question_id]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_test_student_answer_submission_id'
          AND i.object_id = OBJECT_ID(N'[dbo].[test_student_answer]', N'U')
    )
        CREATE NONCLUSTERED INDEX [IX_test_student_answer_submission_id]
            ON [dbo].[test_student_answer] ([submission_id]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'IX_test_student_answer_question_id'
          AND i.object_id = OBJECT_ID(N'[dbo].[test_student_answer]', N'U')
    )
        CREATE NONCLUSTERED INDEX [IX_test_student_answer_question_id]
            ON [dbo].[test_student_answer] ([question_id]);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes i
        WHERE i.name = N'UQ_test_student_answer_submission_question'
          AND i.object_id = OBJECT_ID(N'[dbo].[test_student_answer]', N'U')
    )
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_test_student_answer_submission_question]
            ON [dbo].[test_student_answer] ([submission_id], [question_id]);

    /* ---------- 3. Перенос: assignment + assignment_variant → один вопрос на задание (legacy) ---------- */
    INSERT INTO [dbo].[test_question] (
        [assignment_id],
        [question_text],
        [question_type],
        [max_points],
        [question_order],
        [allow_partial_credit],
        [numeric_tolerance],
        [case_insensitive_text],
        [explanation]
    )
    SELECT
        a.[assignment_id],
        CASE
            WHEN NULLIF(LTRIM(RTRIM(CAST(a.[description] AS NVARCHAR(MAX)))), N'') IS NOT NULL
                THEN CONCAT(a.[title], NCHAR(13) + NCHAR(10), CAST(a.[description] AS NVARCHAR(MAX)))
            ELSE a.[title]
        END,
        CASE
            WHEN (
                SELECT COUNT_BIG(*)
                FROM [dbo].[assignment_variant] avc
                WHERE avc.[assignment_id] = a.[assignment_id] AND avc.[is_correct] = 1
            ) > 1 THEN N'multiple_choice'
            ELSE N'single_choice'
        END,
        CAST(a.[max_score] AS DECIMAL(10, 2)),
        1,
        CASE
            WHEN (
                SELECT COUNT_BIG(*)
                FROM [dbo].[assignment_variant] avc
                WHERE avc.[assignment_id] = a.[assignment_id] AND avc.[is_correct] = 1
            ) > 1 THEN 1
            ELSE 0
        END,
        NULL,
        1,
        NULL
    FROM [dbo].[assignment] AS a
    WHERE EXISTS (
        SELECT 1
        FROM [dbo].[assignment_variant] av
        WHERE av.[assignment_id] = a.[assignment_id]
    )
      AND NOT EXISTS (
        SELECT 1
        FROM [dbo].[test_question] tq
        WHERE tq.[assignment_id] = a.[assignment_id]
    );

    INSERT INTO [dbo].[test_answer_variant] (
        [question_id],
        [variant_text],
        [is_correct],
        [variant_order],
        [variant_points]
    )
    SELECT
        tq.[question_id],
        av.[variant_text],
        av.[is_correct],
        av.[variant_order],
        NULL
    FROM [dbo].[assignment_variant] AS av
    INNER JOIN [dbo].[test_question] AS tq
        ON tq.[assignment_id] = av.[assignment_id]
       AND tq.[question_order] = 1
       AND tq.[question_type] IN (N'single_choice', N'multiple_choice')
    WHERE NOT EXISTS (
        SELECT 1
        FROM [dbo].[test_answer_variant] x
        WHERE x.[question_id] = tq.[question_id]
          AND x.[variant_text] = av.[variant_text]
          AND ISNULL(x.[variant_order], -999999) = ISNULL(av.[variant_order], -999999)
    );

    /* Задания без вариантов, но с эталонным текстовым ответом в assignment.correct_answer */
    INSERT INTO [dbo].[test_question] (
        [assignment_id],
        [question_text],
        [question_type],
        [max_points],
        [question_order],
        [allow_partial_credit],
        [numeric_tolerance],
        [case_insensitive_text],
        [explanation]
    )
    SELECT
        a.[assignment_id],
        CASE
            WHEN NULLIF(LTRIM(RTRIM(CAST(a.[description] AS NVARCHAR(MAX)))), N'') IS NOT NULL
                THEN CONCAT(a.[title], NCHAR(13) + NCHAR(10), CAST(a.[description] AS NVARCHAR(MAX)))
            ELSE a.[title]
        END,
        N'text',
        CAST(a.[max_score] AS DECIMAL(10, 2)),
        1,
        0,
        NULL,
        1,
        NULL
    FROM [dbo].[assignment] AS a
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[assignment_variant] av WHERE av.[assignment_id] = a.[assignment_id]
    )
      AND NULLIF(LTRIM(RTRIM(a.[correct_answer])), N'') IS NOT NULL
      AND NOT EXISTS (
        SELECT 1 FROM [dbo].[test_question] tq WHERE tq.[assignment_id] = a.[assignment_id]
      );

    INSERT INTO [dbo].[test_text_answer] ([question_id], [answer_text], [answer_number], [answer_order])
    SELECT tq.[question_id], a.[correct_answer], NULL, 1
    FROM [dbo].[assignment] AS a
    INNER JOIN [dbo].[test_question] AS tq
        ON tq.[assignment_id] = a.[assignment_id]
       AND tq.[question_order] = 1
       AND tq.[question_type] = N'text'
    WHERE NOT EXISTS (
        SELECT 1 FROM [dbo].[assignment_variant] av WHERE av.[assignment_id] = a.[assignment_id]
    )
      AND NULLIF(LTRIM(RTRIM(a.[correct_answer])), N'') IS NOT NULL
      AND NOT EXISTS (
        SELECT 1 FROM [dbo].[test_text_answer] tta WHERE tta.[question_id] = tq.[question_id]
      );

    /* submission_review → test_student_answer (сопоставление варианта по тексту и порядку) */
    INSERT INTO [dbo].[test_student_answer] (
        [submission_id],
        [question_id],
        [single_selected_variant_id],
        [selected_variants_json],
        [response_text],
        [response_number],
        [points_awarded],
        [auto_grade_state],
        [is_fully_auto_graded],
        [teacher_comment],
        [answered_at]
    )
    SELECT
        sr.[submission_id],
        tq.[question_id],
        tav.[answer_variant_id],
        NULL,
        NULL,
        NULL,
        CAST(ISNULL(sr.[points_awarded], 0) AS DECIMAL(10, 2)),
        CASE
            WHEN sr.[is_correct] = 1 THEN N'correct'
            WHEN sr.[is_correct] = 0 THEN N'incorrect'
            ELSE N'not_evaluated'
        END,
        CASE WHEN sr.[is_correct] IS NOT NULL THEN 1 ELSE 0 END,
        sr.[teacher_comment],
        ISNULL(sr.[created_at], SYSUTCDATETIME())
    FROM [dbo].[submission_review] AS sr
    INNER JOIN [dbo].[submission] AS sub ON sub.[submission_id] = sr.[submission_id]
    INNER JOIN [dbo].[test_question] AS tq
        ON tq.[assignment_id] = sub.[assignment_id]
       AND tq.[question_order] = COALESCE(NULLIF(sr.[question_number], 0), 1)
       AND tq.[question_type] IN (N'single_choice', N'multiple_choice')
    INNER JOIN [dbo].[assignment_variant] AS av
        ON av.[variant_id] = sr.[student_variant_id]
    INNER JOIN [dbo].[test_answer_variant] AS tav
        ON tav.[question_id] = tq.[question_id]
       AND tav.[variant_text] = av.[variant_text]
       AND ISNULL(tav.[variant_order], -999999) = ISNULL(av.[variant_order], -999999)
    WHERE sr.[student_variant_id] IS NOT NULL
      AND NOT EXISTS (
        SELECT 1
        FROM [dbo].[test_student_answer] tsa
        WHERE tsa.[submission_id] = sr.[submission_id]
          AND tsa.[question_id] = tq.[question_id]
      );

    /* Текстовые ответы из submission.student_answer_text для текстовых вопросов */
    INSERT INTO [dbo].[test_student_answer] (
        [submission_id],
        [question_id],
        [single_selected_variant_id],
        [selected_variants_json],
        [response_text],
        [response_number],
        [points_awarded],
        [auto_grade_state],
        [is_fully_auto_graded],
        [teacher_comment],
        [answered_at]
    )
    SELECT
        sub.[submission_id],
        tq.[question_id],
        NULL,
        NULL,
        sub.[student_answer_text],
        NULL,
        CAST(sub.[score] AS DECIMAL(10, 2)),
        N'manual_required',
        0,
        sub.[teacher_comment],
        ISNULL(sub.[submitted_at], SYSUTCDATETIME())
    FROM [dbo].[submission] AS sub
    INNER JOIN [dbo].[test_question] AS tq
        ON tq.[assignment_id] = sub.[assignment_id]
       AND tq.[question_order] = 1
       AND tq.[question_type] = N'text'
    WHERE NULLIF(LTRIM(RTRIM(sub.[student_answer_text])), N'') IS NOT NULL
      AND NOT EXISTS (
        SELECT 1
        FROM [dbo].[test_student_answer] tsa
        WHERE tsa.[submission_id] = sub.[submission_id]
          AND tsa.[question_id] = tq.[question_id]
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* Представления — отдельные пакеты (CREATE VIEW должен быть первым в пакете) */
BEGIN TRY
    IF OBJECT_ID(N'[dbo].[vw_assignment_test_totals]', N'V') IS NOT NULL
        DROP VIEW [dbo].[vw_assignment_test_totals];
END TRY BEGIN CATCH END CATCH;
GO

CREATE VIEW [dbo].[vw_assignment_test_totals]
AS
SELECT
    a.[assignment_id],
    a.[title] AS [assignment_title],
    COUNT(q.[question_id]) AS [question_count],
    SUM(q.[max_points]) AS [total_max_points],
    a.[max_score] AS [assignment_max_score_legacy]
FROM [dbo].[assignment] AS a
LEFT JOIN [dbo].[test_question] AS q ON q.[assignment_id] = a.[assignment_id]
GROUP BY a.[assignment_id], a.[title], a.[max_score];
GO

BEGIN TRY
    IF OBJECT_ID(N'[dbo].[vw_test_question_flat]', N'V') IS NOT NULL
        DROP VIEW [dbo].[vw_test_question_flat];
END TRY BEGIN CATCH END CATCH;
GO

CREATE VIEW [dbo].[vw_test_question_flat]
AS
SELECT
    q.[question_id],
    q.[assignment_id],
    a.[title] AS [assignment_title],
    q.[question_order],
    q.[question_type],
    q.[max_points],
    q.[allow_partial_credit],
    q.[numeric_tolerance],
    q.[case_insensitive_text],
    v.[answer_variant_id],
    v.[variant_text],
    v.[is_correct],
    v.[variant_order],
    v.[variant_points]
FROM [dbo].[test_question] AS q
INNER JOIN [dbo].[assignment] AS a ON a.[assignment_id] = q.[assignment_id]
LEFT JOIN [dbo].[test_answer_variant] AS v ON v.[question_id] = q.[question_id];
GO

BEGIN TRY
    IF OBJECT_ID(N'[dbo].[vw_submission_question_grades]', N'V') IS NOT NULL
        DROP VIEW [dbo].[vw_submission_question_grades];
END TRY BEGIN CATCH END CATCH;
GO

CREATE VIEW [dbo].[vw_submission_question_grades]
AS
SELECT
    tsa.[student_answer_id],
    tsa.[submission_id],
    sub.[assignment_id],
    tsa.[question_id],
    q.[question_order],
    q.[question_type],
    q.[max_points] AS [question_max_points],
    tsa.[points_awarded],
    tsa.[auto_grade_state],
    tsa.[is_fully_auto_graded],
    tsa.[single_selected_variant_id],
    tsa.[selected_variants_json],
    tsa.[response_text],
    tsa.[response_number],
    sub.[score] AS [submission_total_score_legacy]
FROM [dbo].[test_student_answer] AS tsa
INNER JOIN [dbo].[submission] AS sub ON sub.[submission_id] = tsa.[submission_id]
INNER JOIN [dbo].[test_question] AS q ON q.[question_id] = tsa.[question_id];
GO

/* ---------- 4. Пример: тест из 3 вопросов (идемпотентно по заголовку задания) ---------- */
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @demo_title NVARCHAR(200) = N'Демо: смешанный тест (миграция 2026-04-23)';
    DECLARE @lesson_id INT;
    DECLARE @type_id INT;
    DECLARE @demo_assignment_id INT;

    SELECT TOP (1) @lesson_id = [lesson_id] FROM [dbo].[lesson] ORDER BY [lesson_id];
    SELECT TOP (1) @type_id = [type_id] FROM [dbo].[assignment_type] ORDER BY [type_id];

    IF @lesson_id IS NOT NULL
       AND @type_id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM [dbo].[assignment] WHERE [title] = @demo_title)
    BEGIN
        INSERT INTO [dbo].[assignment] (
            [lesson_id],
            [title],
            [description],
            [assignment_type_id],
            [max_score],
            [due_days_after_lesson],
            [correct_answer]
        )
        VALUES (
            @lesson_id,
            @demo_title,
            N'Пример после миграции: single choice (5 б.), multiple choice (10 б.), текст (5 б.).',
            @type_id,
            20,
            7,
            NULL
        );

        SET @demo_assignment_id = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO [dbo].[test_question] (
            [assignment_id], [question_text], [question_type], [max_points], [question_order],
            [allow_partial_credit], [numeric_tolerance], [case_insensitive_text]
        )
        VALUES
        (
            @demo_assignment_id,
            N'Выберите один верный вариант: чему равно 2 + 2?',
            N'single_choice',
            5.00,
            1,
            0,
            NULL,
            1
        ),
        (
            @demo_assignment_id,
            N'Выберите все верные утверждения о SQL Server.',
            N'multiple_choice',
            10.00,
            2,
            1,
            NULL,
            1
        ),
        (
            @demo_assignment_id,
            N'В одном слове напишите язык разметки веб-страниц (латиницей, нижний регистр).',
            N'text',
            5.00,
            3,
            0,
            NULL,
            1
        );

        DECLARE @q1 INT = (
            SELECT [question_id] FROM [dbo].[test_question]
            WHERE [assignment_id] = @demo_assignment_id AND [question_order] = 1
        );
        DECLARE @q2 INT = (
            SELECT [question_id] FROM [dbo].[test_question]
            WHERE [assignment_id] = @demo_assignment_id AND [question_order] = 2
        );
        DECLARE @q3 INT = (
            SELECT [question_id] FROM [dbo].[test_question]
            WHERE [assignment_id] = @demo_assignment_id AND [question_order] = 3
        );

        INSERT INTO [dbo].[test_answer_variant] ([question_id], [variant_text], [is_correct], [variant_order], [variant_points])
        VALUES
        (@q1, N'3', 0, 1, NULL),
        (@q1, N'4', 1, 2, NULL),
        (@q1, N'5', 0, 3, NULL),
        (@q1, N'22', 0, 4, NULL);

        INSERT INTO [dbo].[test_answer_variant] ([question_id], [variant_text], [is_correct], [variant_order], [variant_points])
        VALUES
        (@q2, N'Индексы ускоряют выборку данных', 1, 1, 3.33),
        (@q2, N'CHECK-ограничения задают допустимые значения столбца', 1, 2, 3.33),
        (@q2, N'PRIMARY KEY допускает несколько NULL в одной строке', 0, 3, NULL),
        (@q2, N'FOREIGN KEY связывает строки двух таблиц', 1, 4, 3.34);

        INSERT INTO [dbo].[test_text_answer] ([question_id], [answer_text], [answer_number], [answer_order])
        VALUES (@q3, N'html', NULL, 1), (@q3, N'hypertext markup language', NULL, 2);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
