-- =============================================                      
-- Author:  Vaibhav Mishra                      
-- Create date: Decemeber 21, 2019                      
-- Description: To get learners' progress report with in a organisation         
-- Update: 14 October 2022, Include course access status active/suspend        
-- Update: 03 May 2023, Include user status active/inactive/recent active (accessed or created with in 500 days)        
-- exec lms_admin_getLearnerProgressReport 1,32801,'',2,830,0,null,0,399,null,null,null,null,0              
--  =============================================                      
CREATE PROCEDURE [dbo].[lms_admin_getLearnerProgressReport]                      
                      
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
@toDate datetime = null ,                      
@accessStatus int = 1                      
                      
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
                      
DECLARE @statusQuery varchar(100) = ''                      
IF @status is not null  AND  @status <> '0'           
 BEGIN           
  IF @status = 'not-passed'          
   SET @statusQuery = ' AND pd.strStatus <> ''passed'''            
  ELSE          
   SET @statusQuery = ' AND pd.strStatus = ''' + CONVERT(varchar, @status) +''''            
 END        
       
              
DECLARE @learnerStatusQuery varchar(1000) = ''              
IF @learnerStatus = 1              
 SET @learnerStatusQuery = ' AND c.blnCancelled = 0 '              
ELSE IF @learnerStatus = 2              
 SET @learnerStatusQuery = ' AND c.blnCancelled = 1 '              
ELSE IF @learnerStatus = 3              
 SET @learnerStatusQuery = ' AND c.blnCancelled = 0       
 AND ((datediff(DAY,c.datLastLogin, GETDATE()) < 500) OR ((c.datLastLogin is null) AND (datediff(DAY,c.datCreated, GETDATE()) < 500)))'        
                      
             
DECLARE @courseQuery varchar(100) = ''                      
IF @course > 0                      
 SET @courseQuery = ' AND pd.intCourseid = ' + CONVERT(varchar, @course)                         
                       
DECLARE @orgQuery varchar(100) = ' AND c.intOrganisationID = '+ CONVERT(varchar, @company)            
        
        
DECLARE @accessStatusQuery varchar(100) = ' AND cc.blnCancelled = '+ CONVERT(varchar, @accessStatus)          
                      
DECLARE @sortQuery varchar(100) = ''                      
IF @sortCol is not null                      
 SET @sortQuery = ' order by ' + @sortCol + ' ' + @sortDir                      
ELSE                      
 SET @sortQuery = '  order by c.intcontactid desc  '                      
                      
DECLARE @frmDateQuery varchar(100) = ''                      
IF @fromDate > 0                     
 SET @frmDateQuery = ' AND pd.dateCompletedOn >= ''' + CONVERT(varchar, @fromDate) + ''''                      
                      
DECLARE @toDateQuery varchar(100) = ''                      
IF @toDate > 0                      
 SET @toDateQuery = ' AND pd.dateCompletedOn <= ''' + CONVERT(varchar, @toDate) + ''''                      
                      
DECLARE @query varchar(MAX)                      
set @query = 'SELECT                      
 c.intContactID, c.intLocationID, c.intDepartmentID, co.intCourseId, c.strFirstName, c.strSurname, c.strEmail, c.strEmployeeNumber,                       
 l.strLocation,                      
 d.strDepartment,                      
 co.strCourse,                   
 pd.intRecordID ,pd.strStatus, pd.intScore,pd.dateAssignedOn, pd.dateCompletedOn , pd.dateLastStarted                     
                      
                      
 FROM tbCourseProgressDetails pd                       
                      
 LEFT JOIN tbContact c on pd.intcontactid = c.intContactId                      
 LEFT JOIN tbCourse co on pd.intCourseId = co.intCourseId                      
 LEFT JOIN tbContactCourse cc on pd.intCourseId = cc.intCourseId  and pd.intcontactid = cc.intContactId                   
 LEFT JOIN tbLocation l on c.intLocationID = l.intLocationID                      
 LEFT JOIN tbDepartment d on c.intDepartmentID = d.intDepartmentID                      
                       
 WHERE                
 co.blnCourseRA <> 1                   
 AND (c.strFirstName + '' '' + c.strSurname like ''%'+@learnerName+'%''                       
 OR c.strEmail like ''%'+@learnerName+'%''                      
 OR c.strEmployeeNumber like ''%'+@learnerName+'%'')' + @learnerStatusQuery + @frmDateQuery + @toDateQuery + @courseQuery + @orgQuery + @roleJoinQuery +  @locQuery + @depQuery + @statusQuery + @accessStatusQuery + @sortQuery                              
 exec (@query)         
    
END