-- =============================================                          
-- Author:  Vaibhav Mishra                          
-- Create date: Decemeber 21, 2019                          
-- Description: To get learners' risk assessment report with in a organisation                          
-- exec lms_admin_getLearnerRAReport 1,80815,'',0,0,4334,0,0,0,526                     
--  =============================================                          
CREATE PROCEDURE [dbo].[lms_admin_getLearnerRAReport]                          
                          
@adminRole int,                    
@adminUserId bigint,                     
@learnerName nvarchar(250) = '',                          
@location bigint = 0,                          
@department bigint = 0,                          
@course bigint = 0,                          
@signedOff int = 0,                         
@raStatus int = 0,                          
@issues int = 0,                          
@company bigint,                          
@fromDate datetime = null,                          
@toDate datetime = null,                          
@sortCol varchar(100) = null,                          
@sortDir varchar(10) = null                          
                          
AS                          
                          
BEGIN                          
                          
IF @learnerName is null                          
 SET @learnerName = ''                          
                          
 --check for Admin with full Rights                    
 DECLARE @roleJoinQuery varchar(500) = '';                    
 IF @adminRole = 1                    
 SET @roleJoinQuery = ''                    
                    
 IF @adminRole = 2                    
 SET @roleJoinQuery = '  AND c.intLocationId in ( select intLocationId from tbDepartmentAdministrator where intContactID = ' + CONVERT(varchar, @adminUserId)  + ')    
 AND c.intDepartmentId in ( select intDepartmentId from tbDepartmentAdministrator where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '                    
                      
 IF @adminRole = 3                    
 SET @roleJoinQuery = '  AND c.intLocationId in ( select intLocationId from tbLocationAdministrator where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '                    
                   
 IF @adminRole = 9                
 SET @roleJoinQuery = '  AND c.intLocationId in ( select intLocationId from tbDepartmentSupervisior where intContactID = ' + CONVERT(varchar, @adminUserId)  + ')    
 AND c.intDepartmentId in ( select intDepartmentId from tbDepartmentSupervisior where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '                
                  
 IF @adminRole = 8                
 SET @roleJoinQuery = '  AND c.intLocationId in ( select intLocationId from tbLocationSupervisior where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '              
                     
DECLARE @locQuery varchar(100) = ''                          
IF @location > 0                          
 SET @locQuery = ' AND c.intLocationId = ' + CONVERT(varchar, @location)                          
                          
DECLARE @depQuery varchar(100) = ''                          
IF @department > 0                          
 SET @depQuery = ' AND c.intDepartmentId = ' + CONVERT(varchar, @department)                          
                          
DECLARE @issueQuery varchar(100) = ''                          
IF @issues = 1                          
 SET @issueQuery = ' AND ra.intIssueCount > 0 '                          
ELSE IF  @issues = 2                          
 SET @issueQuery = ' AND ra.intIssueCount = 0 '                          
                          
DECLARE @signedOffQuery varchar(100) = ''                          
IF @signedOff = 1                          
 SET @signedOffQuery = ' AND ra.datSignoff is not null '                          
ELSE IF  @signedOff = 2                          
 SET @signedOffQuery = ' AND ra.datSignoff is null '                 
               
DECLARE @raStatusQuery varchar(100) = ''                          
IF @raStatus = 1                          
 SET @raStatusQuery = ' AND ra.datCompleted is not null '                          
ELSE IF  @raStatus = 2                          
 SET @raStatusQuery = ' AND ra.datCompleted is null '               
                          
DECLARE @frmDateQuery varchar(100) = ''                          
IF @fromDate > 0                   
 SET @frmDateQuery = ' AND ra.datCompleted >= ''' + CONVERT(varchar, @fromDate) + ''''                          
                          
DECLARE @toDateQuery varchar(100) = ''                          
IF @toDate > 0                          
 SET @toDateQuery = ' AND ra.datCompleted <= ''' + CONVERT(varchar, @toDate) + ''''                          
                          
                           
DECLARE @courseQuery varchar(100) = ''                          
IF @course > 0                          
 SET @courseQuery = ' AND ra.intCourseID = ' +  CONVERT(varchar, @course)                          
                           
DECLARE @orgQuery varchar(100) = ' AND c.intOrganisationID = '+ CONVERT(varchar, @company)                          
                          
DECLARE @sortQuery varchar(100) = ''                          
IF @sortCol is not null                          
 SET @sortQuery = ' order by ' + @sortCol + ' ' + @sortDir                          
ELSE                          
 SET @sortQuery = '  order by c.intContactID desc  '                          
                          
DECLARE @query varchar(MAX)                          
set @query = 'SELECT   ra.intRiskAssessmentResultID,                        
 c.intContactID, c.intLocationID, c.intDepartmentID, co.intCourseId, c.strFirstName, c.strSurname, c.strEmail, c.strEmployeeNumber,                           
 l.strLocation,                          
 d.strDepartment,                          
 co.strCourse,                          
 ra.datCompleted, ra.intIssueCount, ra.datSignoff, ra.dateAssignedOn as dateAssignedOn, CASE WHEN ra.datSignoff is null THEN ''No'' ELSE ''Yes'' END as SignedOff ,   
 CASE WHEN ra.datCompleted is null THEN ''Not complete'' ELSE ''Completed'' END as RAStatus   
                         
                           
 FROM tbRiskAssessmentResult ra                           
                     
 LEFT JOIN tbContact c on ra.intcontactid = c.intContactId                          
 LEFT JOIN tbCourse co on ra.intCourseId = co.intCourseId                                           
 LEFT JOIN tbLocation l on c.intLocationID = l.intLocationID                          
 LEFT JOIN tbDepartment d on c.intDepartmentID = d.intDepartmentID                          
                           
 WHERE      
 co.blnCourseRA = 1 AND  
 c.blnCancelled = 0  AND ra.intRiskAssessmentResultID is not null                        
 AND (c.strFirstName + '' '' + c.strSurname like ''%'+@learnerName+'%''                         
 OR c.strEmail like ''%'+@learnerName+'%''                          
 OR c.strEmployeeNumber like ''%'+@learnerName+'%'')' + @frmDateQuery + @toDateQuery +   @courseQuery + @orgQuery + @roleJoinQuery +  @locQuery + @depQuery + @issueQuery + @signedOffQuery + @raStatusQuery + @sortQuery                       
 exec (@query)                          
                       
END