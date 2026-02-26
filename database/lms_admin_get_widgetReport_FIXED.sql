ALTER PROCEDURE [dbo].[lms_admin_get_widgetReport]                      
                      
@adminRole int,                
@adminUserId bigint,                 
@learnerName nvarchar(250) = '',                 
@learnerStatus int = null,      
@location bigint = 0,                      
@department bigint = 0,                          
@course bigint = 0,                      
@company bigint,                      
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
 SET @roleJoinQuery = ' AND c.intLocationId in ( select intLocationId from tbDepartmentAdministrator where intContactID = ' + CONVERT(varchar, @adminUserId)  + ')        
 AND c.intDepartmentId in ( select intDepartmentId from tbDepartmentAdministrator where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '                
                  
 IF @adminRole = 3                
 SET @roleJoinQuery = ' AND c.intLocationId in ( select intLocationId from tbLocationAdministrator where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '               
                
 IF @adminRole = 9                
 SET @roleJoinQuery = ' AND c.intLocationId in ( select intLocationId from tbDepartmentSupervisior where intContactID = ' + CONVERT(varchar, @adminUserId)  + ')        
 AND c.intDepartmentId in ( select intDepartmentId from tbDepartmentSupervisior where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '                
                  
 IF @adminRole = 8                
 SET @roleJoinQuery = ' AND c.intLocationId in ( select intLocationId from tbLocationSupervisior where intContactID = ' + CONVERT(varchar, @adminUserId)  + ') '              
                  
DECLARE @locQuery varchar(100) = ''                      
IF @location > 0                      
 SET @locQuery = ' AND c.intLocationId = ' + CONVERT(varchar, @location)                      
                      
DECLARE @depQuery varchar(100) = ''                      
IF @department > 0                      
 SET @depQuery = ' AND c.intDepartmentId = ' + CONVERT(varchar, @department)                      
                      
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
 SET @courseQuery = ' AND wc.widget_course_id = ' + CONVERT(varchar, @course)                         
                       
DECLARE @orgQuery varchar(100) = ''
IF @company > 0             
 SET @orgQuery = ' AND c.intOrganisationID = '+ CONVERT(varchar, @company)
ELSE
 SET @orgQuery = ' AND c.intOrganisationID > 0'            
                     
                      
DECLARE @sortQuery varchar(100) = ''                      
IF @sortCol is not null            
 SET @sortQuery = ' order by ' + @sortCol + ' ' + @sortDir                      
ELSE                      
 SET @sortQuery = ' order by c.intcontactid desc '                                          
                      
DECLARE @query varchar(MAX)                      
set @query = '                              
  
SELECT   
c.intContactID, c.intLocationID, c.intDepartmentID,   
c.strFirstName, c.strSurname, c.strEmail, c.strEmployeeNumber,                       
l.strLocation,                      
d.strDepartment,            
wc.widget_course_id as intCourseId,             
wc.widget_course_name as strCourse,                   
r.resp_id as intRecordID,    
q.widget_que_text,   
q.widget_que_text_after,   
q.widget_que_type as qtype,  
r.resp_text,   
r.resp_text_after,   
r.resp_mac_1,   
r.resp_mac_2,   
r.resp_mac_3,   
r.resp_mac_feedback,   
r.resp_mac_feedback_text   
from tb_widget_responses r  
LEFT JOIN tbContact c on r.resp_user_id = c.intContactID  
LEFT JOIN tb_widget_ques q on r.resp_que_guid = q.widget_que_guid  
LEFT JOIN tb_widget_course wc on q.widget_course_id = wc.widget_course_id  
LEFT JOIN tbLocation l on c.intLocationID = l.intLocationID  
LEFT JOIN tbDepartment d on c.intDepartmentID = d.intDepartmentID  
WHERE c.intContactID is not null 
AND (len('''+@learnerName+''') = 0 OR (c.strFirstName + '' '' + c.strSurname like ''%'+@learnerName+'%'' OR c.strEmail like ''%'+@learnerName+'%'' OR c.strEmployeeNumber like ''%'+@learnerName+'%''))'
+ @learnerStatusQuery + @courseQuery + @orgQuery + @roleJoinQuery + @locQuery + @depQuery + @sortQuery 

exec (@query)  
END

