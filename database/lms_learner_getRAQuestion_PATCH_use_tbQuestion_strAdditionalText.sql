-- Patch: make learner RA question fetch use tbQuestion.strAdditionalText
-- Note: This script assumes the existing proc signature shown in learnerDbEntity.edmx
--       Name: lms_learner_getRAQuestion(@learner bigint, @course bigint)

ALTER PROCEDURE [dbo].[lms_learner_getRAQuestion]
    @learner bigint,
    @course bigint
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @intRiskAssessmentResultID bigint;
    SET @intRiskAssessmentResultID = (
        SELECT TOP 1 intRiskAssessmentResultID
        FROM tbContactCourse
        WHERE intContactID = @learner AND intCourseID = @course
    );

    IF (@intRiskAssessmentResultID = 0 OR @intRiskAssessmentResultID IS NULL)
    BEGIN
        INSERT INTO tbRiskAssessmentResult
        (
            intContactID,
            intCourseID,
            intIssueCount,
            strUserNotes,
            strAdminNotes,
            datSignOff,
            datDue,
            datBegun,
            datCompleted,
            blnSummaryDisplay,
            blnCancelled,
            datCancelled,
            intCurrentQuestionID
        )
        VALUES
        (
            @learner,
            @course,
            0,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            NULL,
            1,
            0,
            NULL,
            0
        );

        SET @intRiskAssessmentResultID = SCOPE_IDENTITY();

        UPDATE tbContactCourse
        SET intRiskAssessmentResultID = @intRiskAssessmentResultID
        WHERE intContactID = @learner AND intCourseID = @course;
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM tbAnswer a
        WHERE a.intRiskAssessmentResultID = @intRiskAssessmentResultID
    )
    BEGIN
        INSERT INTO tbAnswer
        (
            intRiskAssessmentResultID,
            intQuestionID,
            strQuestion,
            intNumeric,
            intContactID,
            blnYes,
            blnNo,
            strText
        )
        SELECT
            @intRiskAssessmentResultID,
            q.intQuestionID,
            q.strQuestion,
            0,
            @learner,
            0,
            0,
            ''
        FROM tbQuestion q
        WHERE q.intCourseID = @course;
    END

    SELECT
        q.intQuestionID,
        q.strQuestion,
        q.strGroupText,
        q.intOrder,
        qo.intQuestionOptionID,
        qo.strQuestionOption,
        qo.strValue,
        qo.intOrder AS optOrder,
        qo.blnIssue AS optIssue,
        a.intanswerid,
        a.blnYes,
        a.blnNo,
        ISNULL(q.strAdditionalText, '') AS strText,
        a.blnIssue
    FROM tbQuestion q
    LEFT JOIN tbQuestionOption qo ON q.intQuestionID = qo.intQuestionID
    LEFT JOIN tbAnswer a
        ON a.intRiskAssessmentResultID = @intRiskAssessmentResultID
       AND a.intQuestionID = q.intQuestionID
    WHERE q.intCourseID = @course
      AND qo.intQuestionOptionID IS NOT NULL
    ORDER BY q.intOrder, qo.intOrder;
END
GO
