-- =============================================
-- Author: Equivalent paged sub-module report
-- Description: Paged learner sub-module progress report using
--              tbCourseSubmoduleProgressDetails + tb_course_subModules
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[lms_admin_getLearnerSubModuleProgressReport_Paged]
    @adminRole int,
    @adminUserId bigint,
    @learnerName nvarchar(250) = '',
    @learnerStatus int = null,
    @location bigint = 0,
    @department bigint = 0,
    @status varchar(20) = null,
    @course bigint = 0,
    @subModuleId bigint = 0,
    @company bigint,
    @sortCol varchar(100) = null,
    @sortDir varchar(10) = null,
    @fromDate datetime = null,
    @toDate datetime = null,
    @accessStatus int = 1,
    @skip int = 0,
    @pageSize int = 25
AS
BEGIN
    SET NOCOUNT ON;

    SET @learnerName = ISNULL(@learnerName, '');
    SET @skip = CASE WHEN @skip < 0 THEN 0 ELSE @skip END;
    SET @pageSize = CASE WHEN @pageSize < 1 THEN 25 ELSE @pageSize END;

    DECLARE @sortExpr nvarchar(128) =
        CASE @sortCol
            WHEN 'c.strFirstName' THEN N'c.strFirstName'
            WHEN 'c.strEmail' THEN N'c.strEmail'
            WHEN 'l.strLocation' THEN N'l.strLocation'
            WHEN 'd.strDepartment' THEN N'd.strDepartment'
            WHEN 'co.strCourse' THEN N'co.strCourse'
            WHEN 'sm.subModule_name' THEN N'sm.subModule_name'
            WHEN 'pd.dateAssignedOn' THEN N'pd.dateAssignedOn'
            WHEN 'smpd.strStatus' THEN N'smpd.strStatus'
            WHEN 'smpd.lastAccessedOn' THEN N'smpd.lastAccessedOn'
            ELSE N'c.intContactID'
        END;

    DECLARE @sortDirection nvarchar(4) =
        CASE WHEN LOWER(ISNULL(@sortDir, 'desc')) = 'asc' THEN N'ASC' ELSE N'DESC' END;

    DECLARE @orderBy nvarchar(300) = @sortExpr + N' ' + @sortDirection;
    DECLARE @searchPattern nvarchar(260) = N'%' + @learnerName + N'%';

    DECLARE @assignedCol sysname = NULL;
    IF COL_LENGTH('dbo.tbCourseProgressDetails', 'dateAssignedOn') IS NOT NULL
        SET @assignedCol = N'dateAssignedOn';
    ELSE IF COL_LENGTH('dbo.tbCourseProgressDetails', 'dateassignedon') IS NOT NULL
        SET @assignedCol = N'dateassignedon';

    IF @assignedCol IS NULL
    BEGIN
        RAISERROR('Assignment date column was not found in dbo.tbCourseProgressDetails.', 16, 1);
        RETURN;
    END

    DECLARE @sql nvarchar(max) = N'
    SELECT
        c.intContactID,
        c.intLocationID,
        c.intDepartmentID,
        co.intCourseId,
        sm.intSubModuleID,
        c.strFirstName,
        c.strSurname,
        c.strEmail,
        c.strEmployeeNumber,
        l.strLocation,
        d.strDepartment,
        co.strCourse,
        sm.subModule_name,
        smpd.intRecordId AS intRecordID,
        smpd.strStatus,
        pd.dateAssignedOn,
        smpd.lastAccessedOn,
        COUNT(1) OVER() AS TotalRecords
    FROM tbCourseSubmoduleProgressDetails smpd
    INNER JOIN tb_course_subModules sm ON smpd.intSubModuleId = sm.intSubModuleID
    INNER JOIN tbCourse co ON sm.intCourseID = co.intCourseId
    INNER JOIN tbContact c ON smpd.intContactId = c.intContactID
    LEFT JOIN tbContactCourse cc ON cc.intContactId = c.intContactId AND cc.intCourseId = co.intCourseId
    OUTER APPLY (
        SELECT TOP (1) pd0.' + QUOTENAME(@assignedCol) + N' AS dateAssignedOn
        FROM tbCourseProgressDetails pd0
        WHERE pd0.intContactId = c.intContactID
          AND pd0.intCourseId = co.intCourseId
        ORDER BY pd0.intRecordID DESC
    ) pd
    LEFT JOIN tbLocation l ON c.intLocationID = l.intLocationID
    LEFT JOIN tbDepartment d ON c.intDepartmentID = d.intDepartmentID
    WHERE
        co.blnCourseRA <> 1
        AND c.intOrganisationID = @company
        AND (cc.blnCancelled = @accessStatus OR cc.blnCancelled IS NULL)
        AND (
            @adminRole = 1
            OR (@adminRole = 2
                AND c.intLocationId IN (SELECT intLocationId FROM tbDepartmentAdministrator WHERE intContactID = @adminUserId)
                AND c.intDepartmentId IN (SELECT intDepartmentId FROM tbDepartmentAdministrator WHERE intContactID = @adminUserId))
            OR (@adminRole = 3
                AND c.intLocationId IN (SELECT intLocationId FROM tbLocationAdministrator WHERE intContactID = @adminUserId))
            OR (@adminRole = 9
                AND c.intLocationId IN (SELECT intLocationId FROM tbDepartmentSupervisior WHERE intContactID = @adminUserId)
                AND c.intDepartmentId IN (SELECT intDepartmentId FROM tbDepartmentSupervisior WHERE intContactID = @adminUserId))
            OR (@adminRole = 8
                AND c.intLocationId IN (SELECT intLocationId FROM tbLocationSupervisior WHERE intContactID = @adminUserId))
        )
        AND (@location = 0 OR c.intLocationID = @location)
        AND (@department = 0 OR c.intDepartmentID = @department)
        AND (@course = 0 OR co.intCourseId = @course)
        AND (@subModuleId = 0 OR sm.intSubModuleID = @subModuleId)
        AND (@fromDate IS NULL OR smpd.lastAccessedOn >= @fromDate)
        AND (@toDate IS NULL OR smpd.lastAccessedOn < DATEADD(DAY, 1, @toDate))
        AND (
            @status IS NULL
            OR @status = ''0''
            OR (@status = ''not-passed'' AND smpd.strStatus <> ''passed'')
            OR (@status <> ''not-passed'' AND smpd.strStatus = @status)
        )
        AND (
            @learnerStatus IS NULL
            OR (@learnerStatus = 1 AND c.blnCancelled = 0)
            OR (@learnerStatus = 2 AND c.blnCancelled = 1)
            OR (
                @learnerStatus = 3
                AND c.blnCancelled = 0
                AND (
                    DATEDIFF(DAY, c.datLastLogin, GETDATE()) < 500
                    OR (c.datLastLogin IS NULL AND DATEDIFF(DAY, c.datCreated, GETDATE()) < 500)
                )
            )
        )
        AND (
            c.strFirstName + '' '' + c.strSurname LIKE @searchPattern
            OR c.strEmail LIKE @searchPattern
            OR c.strEmployeeNumber LIKE @searchPattern
        )
    ORDER BY ' + @orderBy + N'
    OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY;';

    EXEC sp_executesql
        @sql,
        N'@adminRole int,
          @adminUserId bigint,
          @learnerStatus int,
          @location bigint,
          @department bigint,
          @course bigint,
          @subModuleId bigint,
          @company bigint,
          @accessStatus int,
          @fromDate datetime,
          @toDate datetime,
          @status varchar(20),
          @searchPattern nvarchar(260),
          @skip int,
          @pageSize int',
        @adminRole = @adminRole,
        @adminUserId = @adminUserId,
        @learnerStatus = @learnerStatus,
        @location = @location,
        @department = @department,
        @course = @course,
        @subModuleId = @subModuleId,
        @company = @company,
        @accessStatus = @accessStatus,
        @fromDate = @fromDate,
        @toDate = @toDate,
        @status = @status,
        @searchPattern = @searchPattern,
        @skip = @skip,
        @pageSize = @pageSize;
END
GO
