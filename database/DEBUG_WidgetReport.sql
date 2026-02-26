-- DEBUG QUERIES FOR WIDGET REPORT DATA

-- 1. Check if tb_widget_responses has any data
SELECT COUNT(*) as TotalWidgetResponses FROM tb_widget_responses;

-- 2. Check sample widget responses
SELECT TOP 10 
    resp_id, 
    resp_user_id, 
    resp_que_guid, 
    resp_text,
    resp_mac_1,
    resp_mac_2,
    resp_mac_3
FROM tb_widget_responses;

-- 3. Check if contacts match organization 2 and are active
SELECT COUNT(*) as ActiveContactsInOrg2 
FROM tbContact 
WHERE intOrganisationID = 2 AND blnCancelled = 0;

-- 4. Check sample contacts in org 2
SELECT TOP 10 
    intContactID, 
    strFirstName, 
    strSurname, 
    strEmail, 
    intOrganisationID,
    blnCancelled
FROM tbContact 
WHERE intOrganisationID = 2 AND blnCancelled = 0;

-- 5. Check if widget courses exist
SELECT COUNT(*) as TotalWidgetCourses FROM tb_widget_course;

-- 6. Check sample widget courses
SELECT TOP 10 widget_course_id, widget_course_name FROM tb_widget_course;

-- 7. Check if widget questions exist
SELECT COUNT(*) as TotalWidgetQuestions FROM tb_widget_ques;

-- 8. Check sample widget questions
SELECT TOP 10 
    widget_que_guid, 
    widget_que_text, 
    widget_que_type,
    widget_course_id
FROM tb_widget_ques;

-- 9. JOIN TEST - See what matches when joining all tables
SELECT TOP 20
    c.intContactID,
    c.strFirstName,
    c.strSurname,
    c.strEmail,
    c.intOrganisationID,
    wc.widget_course_name,
    q.widget_que_text,
    q.widget_que_type as qtype,
    r.resp_text,
    r.resp_mac_1,
    r.resp_mac_2,
    r.resp_mac_3
FROM tb_widget_responses r
LEFT JOIN tbContact c ON r.resp_user_id = c.intContactID
LEFT JOIN tb_widget_ques q ON r.resp_que_guid = q.widget_que_guid
LEFT JOIN tb_widget_course wc ON q.widget_course_id = wc.widget_course_id
WHERE c.intOrganisationID = 2 AND c.blnCancelled = 0;

-- 10. Check if there are ANY widget responses with contacts from org 2
SELECT COUNT(*) as WidgetResponsesWithContactsInOrg2
FROM tb_widget_responses r
INNER JOIN tbContact c ON r.resp_user_id = c.intContactID
WHERE c.intOrganisationID = 2 AND c.blnCancelled = 0;
