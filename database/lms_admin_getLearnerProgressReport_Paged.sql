-- =============================================
-- Author: Optimized paging version
-- Description: Paged learner progress report for large datasets using OFFSET/FETCH.
-- Notes: Keeps existing lms_admin_getLearnerProgressReport unchanged for backward compatibility.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[lms_admin_getLearnerProgressReport_Paged]
    @adminRole int,
    @adminUserId bigint,
    @learnerName nvarchar(250) = '',
    @learnerStatus int = null,
    @location bigint = 0,
    @department bigint = 0,
    @status varchar(20) = null,
    @course bigint = 0,
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
    SET @skip     = CASE WHEN @skip    < 0  THEN 0  ELSE @skip    END;
    SET @pageSize = CASE WHEN @pageSize < 1 THEN 25 ELSE @pageSize END;

    -- Whitelist sort column to prevent injection; only table-qualified column names allowed
    DECLARE @sortExpr nvarchar(128) =
        CASE @sortCol
            WHEN 'c.strFirstName'    THEN N'c.strFirstName'
            WHEN 'c.strEmail'        THEN N'c.strEmail'
            WHEN 'l.strLocation'     THEN N'l.strLocation'
            WHEN 'd.strDepartment'   THEN N'd.strDepartment'
            WHEN 'co.strCourse'      THEN N'co.strCourse'
            WHEN 'pd.dateAssignedOn' THEN N'pd.dateAssignedOn'
            WHEN 'pd.dateLastStarted'THEN N'pd.dateLastStarted'
            WHEN 'pd.strStatus'      THEN N'pd.strStatus'
            WHEN 'pd.intScore'       THEN N'pd.intScore'
            WHEN 'pd.dateCompletedOn'THEN N'pd.dateCompletedOn'
            ELSE N'c.intContactID'
        END;

    DECLARE @sortDirection nvarchar(4) =
        CASE WHEN LOWER(ISNULL(@sortDir, 'desc')) = 'asc' THEN N'ASC' ELSE N'DESC' END;

    -- Only the ORDER BY clause uses string concatenation; everything else is parameterised
    DECLARE @orderBy nvarchar(300) = @sortExpr + N' ' + @sortDirection;

    DECLARE @searchPattern nvarchar(260) = N'%' + @learnerName + N'%';

    -- Use OFFSET/FETCH (SQL Server 2012+) instead of ROW_NUMBER CTE to avoid
    -- "non-boolean type near RowNum" parser error inside sp_executesql
    DECLARE @sql nvarchar(max) = N'
    SELECT
        c.intContactID,
        c.intLocationID,
        c.intDepartmentID,
        co.intCourseId,
        c.strFirstName,
        c.strSurname,
        c.strEmail,
        c.strEmployeeNumber,
        l.strLocation,
        d.strDepartment,
        co.strCourse,
        pd.intRecordID,
        pd.strStatus,
        pd.intScore,
        pd.dateAssignedOn,
        pd.dateCompletedOn,
        pd.dateLastStarted,
        COUNT(1) OVER() AS TotalRecords
    FROM tbCourseProgressDetails pd
    INNER JOIN tbContact c  ON pd.intcontactid = c.intContactId
    INNER JOIN tbCourse co  ON pd.intCourseId  = co.intCourseId
    INNER JOIN tbContactCourse cc ON pd.intCourseId = cc.intCourseId AND pd.intcontactid = cc.intContactId
    LEFT  JOIN tbLocation l ON c.intLocationID = l.intLocationID
    LEFT  JOIN tbDepartment d ON c.intDepartmentID = d.intDepartmentID
    WHERE
        co.blnCourseRA <> 1
        AND c.intOrganisationID = @company
        AND cc.blnCancelled = @accessStatus
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
        AND (@location = 0    OR c.intLocationId  = @location)
        AND (@department = 0  OR c.intDepartmentId = @department)
        AND (@course = 0      OR pd.intCourseId    = @course)
        AND (@fromDate IS NULL OR pd.dateCompletedOn >= @fromDate)
        AND (@toDate   IS NULL OR pd.dateCompletedOn <  DATEADD(DAY, 1, @toDate))
        AND (
            @status IS NULL
            OR @status = ''0''
            OR (@status = ''not-passed'' AND pd.strStatus <> ''passed'')
            OR (@status <> ''not-passed'' AND pd.strStatus = @status)
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
        N'@adminRole     int,
          @adminUserId   bigint,
          @learnerStatus int,
          @location      bigint,
          @department    bigint,
          @course        bigint,
          @company       bigint,
          @accessStatus  int,
          @fromDate      datetime,
          @toDate        datetime,
          @status        varchar(20),
          @searchPattern nvarchar(260),
          @skip          int,
          @pageSize      int',
        @adminRole     = @adminRole,
        @adminUserId   = @adminUserId,
        @learnerStatus = @learnerStatus,
        @location      = @location,
        @department    = @department,
        @course        = @course,
        @company       = @company,
        @accessStatus  = @accessStatus,
        @fromDate      = @fromDate,
        @toDate        = @toDate,
        @status = @status,
        @searchPattern = @searchPattern,
        @skip = @skip,
        @pageSize = @pageSize;
END
GO
