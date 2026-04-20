-- =============================================
-- Author:      Equivalent paged RA report
-- Description: Paged learner risk assessment report using
--              safe parameterized dynamic SQL with DB-level pagination.
--              Replaces the old lms_admin_getLearnerRAReport which did
--              in-app paging on a full result set.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[lms_admin_getLearnerRAReport_Paged]
    @adminRole   int,
    @adminUserId bigint,
    @learnerName nvarchar(250) = '',
    @location    bigint        = 0,
    @department  bigint        = 0,
    @course      bigint        = 0,
    @signedOff   int           = 0,
    @raStatus    int           = 0,
    @issues      int           = 0,
    @company     bigint,
    @fromDate    datetime      = null,
    @toDate      datetime      = null,
    @sortCol     varchar(100)  = null,
    @sortDir     varchar(10)   = null,
    @skip        int           = 0,
    @pageSize    int           = 25
AS
BEGIN
    SET NOCOUNT ON;

    SET @learnerName = ISNULL(@learnerName, '');
    SET @skip     = CASE WHEN @skip    < 0 THEN 0  ELSE @skip     END;
    SET @pageSize = CASE WHEN @pageSize < 1 THEN 25 ELSE @pageSize END;

    -- Whitelist sort column to prevent injection
    DECLARE @sortExpr nvarchar(128) =
        CASE @sortCol
            WHEN 'c.strFirstName'    THEN N'c.strFirstName'
            WHEN 'c.strEmail'        THEN N'c.strEmail'
            WHEN 'l.strLocation'     THEN N'l.strLocation'
            WHEN 'd.strDepartment'   THEN N'd.strDepartment'
            WHEN 'co.strCourse'      THEN N'co.strCourse'
            WHEN 'ra.datCompleted'   THEN N'ra.datCompleted'
            WHEN 'ra.datSignoff'     THEN N'ra.datSignoff'
            WHEN 'ra.intIssueCount'  THEN N'ra.intIssueCount'
            WHEN 'ra.dateAssignedOn' THEN N'ra.dateAssignedOn'
            ELSE N'c.intContactID'
        END;

    DECLARE @sortDirection nvarchar(4) =
        CASE WHEN LOWER(ISNULL(@sortDir, 'desc')) = 'asc' THEN N'ASC' ELSE N'DESC' END;

    DECLARE @orderBy        nvarchar(300) = @sortExpr + N' ' + @sortDirection;
    DECLARE @searchPattern  nvarchar(260) = N'%' + @learnerName + N'%';

    DECLARE @sql nvarchar(max) = N'
    SELECT
        ra.intRiskAssessmentResultID,
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
        ra.datCompleted,
        ra.intIssueCount,
        ra.datSignoff,
        ra.dateAssignedOn,
        CASE WHEN ra.datSignoff    IS NULL THEN ''No''           ELSE ''Yes''       END AS SignedOff,
        CASE WHEN ra.datCompleted  IS NULL THEN ''Not complete'' ELSE ''Completed'' END AS RAStatus,
        COUNT(1) OVER() AS TotalRecords
    FROM tbRiskAssessmentResult ra
    LEFT JOIN tbContact    c  ON ra.intContactId  = c.intContactId
    LEFT JOIN tbCourse     co ON ra.intCourseId   = co.intCourseId
    LEFT JOIN tbLocation   l  ON c.intLocationID  = l.intLocationID
    LEFT JOIN tbDepartment d  ON c.intDepartmentID = d.intDepartmentID
    WHERE
        co.blnCourseRA = 1
        AND c.blnCancelled = 0
        AND ra.intRiskAssessmentResultID IS NOT NULL
        AND c.intOrganisationID = @company
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
        AND (@location   = 0 OR c.intLocationID  = @location)
        AND (@department = 0 OR c.intDepartmentID = @department)
        AND (@course     = 0 OR co.intCourseId    = @course)
        AND (@fromDate IS NULL OR ra.datCompleted >= @fromDate)
        AND (@toDate   IS NULL OR ra.datCompleted  < DATEADD(DAY, 1, @toDate))
        AND (
            @signedOff = 0
            OR (@signedOff = 1 AND ra.datSignoff IS NOT NULL)
            OR (@signedOff = 2 AND ra.datSignoff IS NULL)
        )
        AND (
            @raStatus = 0
            OR (@raStatus = 1 AND ra.datCompleted IS NOT NULL)
            OR (@raStatus = 2 AND ra.datCompleted IS NULL)
        )
        AND (
            @issues = 0
            OR (@issues = 1 AND ra.intIssueCount > 0)
            OR (@issues = 2 AND ra.intIssueCount = 0)
        )
        AND (
            c.strFirstName + '' '' + c.strSurname LIKE @searchPattern
            OR c.strEmail          LIKE @searchPattern
            OR c.strEmployeeNumber LIKE @searchPattern
        )
    ORDER BY ' + @orderBy + N'
    OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY;';

    EXEC sp_executesql
        @sql,
        N'@adminRole    int,
          @adminUserId  bigint,
          @location     bigint,
          @department   bigint,
          @course       bigint,
          @company      bigint,
          @fromDate     datetime,
          @toDate       datetime,
          @signedOff    int,
          @raStatus     int,
          @issues       int,
          @searchPattern nvarchar(260),
          @skip         int,
          @pageSize     int',
        @adminRole     = @adminRole,
        @adminUserId   = @adminUserId,
        @location      = @location,
        @department    = @department,
        @course        = @course,
        @company       = @company,
        @fromDate      = @fromDate,
        @toDate        = @toDate,
        @signedOff     = @signedOff,
        @raStatus      = @raStatus,
        @issues        = @issues,
        @searchPattern = @searchPattern,
        @skip          = @skip,
        @pageSize      = @pageSize;
END
GO
