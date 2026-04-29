CREATE PROCEDURE [dbo].[lms_learner_get_subModule_RAQuestion]  
 @learner bigint,  
 @intRiskAssessmentResultID bigint = null,  
 @subModuleId bigint  
AS  
BEGIN  
    DECLARE @raid bigint;  
    SET @raid = (select subModule_RAID FROM tb_course_subModules where intSubModuleID = @subModuleId);  
  
    -- If no mapping found, return empty result set (avoid INSERT of NULL intCourseID)  
    IF @raid IS NULL  
    BEGIN  
        -- return same shape but no rows  
         SELECT q.intQuestionID, q.strQuestion, q.strGroupText, q.intOrder,  
             qo.intQuestionOptionID, qo.strQuestionOption, qo.strValue, qo.intOrder as optOrder, qo.blnIssue as optIssue,  
             a.intanswerid, a.blnYes, a.blnNo, ISNULL(q.strAdditionalText, '') as strText, a.blnIssue, @intRiskAssessmentResultID as RARID  
        FROM tbQuestion q  
        LEFT JOIN tbQuestionOption qo on q.intQuestionID = qo.intQuestionID  
        LEFT JOIN tbAnswer a on 1 = 0  -- no answers  
        WHERE 1 = 0;  
        RETURN;  
    END  
  
    -- if risk assessment not created  
    IF (@intRiskAssessmentResultID = 0 OR @intRiskAssessmentResultID is null)  
    BEGIN  
        INSERT INTO tbRiskAssessmentResult (intContactID, intCourseID, intSubModuleId, intIssueCount, strUserNotes, strAdminNotes, datSignOff, datDue, datBegun, datCompleted, blnSummaryDisplay, blnCancelled, datCancelled, intCurrentQuestionID)  
        VALUES(@learner, @raid, @subModuleId, 0, null, null, null, null, null, null, 1, 0, null, 0)  
          
        SET @intRiskAssessmentResultID = SCOPE_IDENTITY()  
    END  
  
    IF NOT EXISTS (SELECT a.intRiskAssessmentResultID from tbAnswer a where a.intRiskAssessmentResultID = @intRiskAssessmentResultID)  
    BEGIN  
        INSERT INTO tbAnswer (intRiskAssessmentResultID, intQuestionID, strQuestion, intNumeric, intContactID, blnYes, blnNo, strText)  
        SELECT @intRiskAssessmentResultID, intQuestionID, strQuestion,0,@learner,0,0,'' FROM tbQuestion where intCourseID = @raid  
    END  
  
        SELECT q.intQuestionID, q.strQuestion, q.strGroupText, q.intOrder,  
            qo.intQuestionOptionID, qo.strQuestionOption, qo.strValue, qo.intOrder as optOrder, qo.blnIssue as optIssue,  
            a.intanswerid, a.blnYes, a.blnNo, ISNULL(q.strAdditionalText, '') as strText, a.blnIssue, @intRiskAssessmentResultID as RARID  
    FROM tbQuestion q  
    LEFT JOIN tbQuestionOption qo on q.intQuestionID = qo.intQuestionID  
    LEFT JOIN tbAnswer a on a.intRiskAssessmentResultID = @intRiskAssessmentResultID and a.intQuestionID = q.intQuestionID  
    where q.intCourseID = @raid and qo.intQuestionOptionID is not null  
    order by q.intOrder, qo.intOrder  
END  