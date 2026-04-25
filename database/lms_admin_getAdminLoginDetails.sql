              
              
CREATE PROCEDURE [dbo].[lms_admin_getAdminLoginDetails]                                      
              
@organisation bigint,  
@useremail nvarchar(250),                                      
@pwd nvarchar(MAX),                                      
@masterPwd bit = 0                                      
                                      
AS                                      
BEGIN                                      
                                      
IF @masterPwd = 1                                       
BEGIN                                      
 SELECT DISTINCT c.intcontactid, c.strFirstName, c.strSurname, c.strEmail, c.IsRestPassword, o.intOrganisationID, o.UL_sequentialId, o.strOrganisation, o.UL_strLogo, o.UL_strCertificate,             
 o.incidentAccidentEnabled,  o.courseAssignMode,              
  img.org_banner, img.org_certificate , c.profilePic, o.UL_blnLive  , o.trainingResetType , o.adminMenuSettings                                
                                      
 FROM tbContact c                                       
                                      
 LEFT JOIN tborganisation o on c.intOrganisationID = o.intOrganisationID                                      
 LEFT JOIN tbContactAdminlevel al on c.intContactID = al.intContactID                          
  LEFT JOIN tbOrganisationImages img on o.intOrganisationID = img.org_id                                  
                                      
 WHERE   
 c.intOrganisationID = @organisation  
 AND c.blnCancelled = 0                                       
 AND al.intAdminLevelID in (1,2,3,4,5,6,8,9)                                    
 AND ( c.strEmail = @useremail or c.strEmployeeNumber = @useremail)            
END                                      
ELSE                                      
 BEGIN                                      
  SELECT DISTINCT  c.intcontactid, c.strFirstName, c.strSurname, c.strEmail, c.IsRestPassword, o.intOrganisationID, o.UL_sequentialId, o.strOrganisation, o.UL_strLogo, o.UL_strCertificate,             
  o.incidentAccidentEnabled,  o.courseAssignMode,                      
  img.org_banner, img.org_certificate , c.profilePic, o.UL_blnLive , o.trainingResetType , o.adminMenuSettings                  
                                      
  FROM tbContact c                                       
                                      
  LEFT JOIN tborganisation o on c.intOrganisationID = o.intOrganisationID                                      
  LEFT JOIN tbContactAdminlevel al on c.intContactID = al.intContactID                        
  LEFT JOIN tbOrganisationImages img on o.intOrganisationID = img.org_id                      
                                      
  WHERE  
 c.intOrganisationID = @organisation  
 AND c.blnCancelled = 0                                       
  AND al.intAdminLevelID in (1,2,3,4,5,6,8,9)             
  and o.isSSOOrg = 0                                 
  AND ( c.strEmail = @useremail or c.strEmployeeNumber = @useremail)                                      
  AND c.strPassword = @pwd COLLATE SQL_Latin1_General_CP1_CS_AS                                      
 END                                      
                                       
END