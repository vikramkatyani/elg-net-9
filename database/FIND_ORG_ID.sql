-- Find Contact 1's Organization
SELECT 
    intContactID,
    strFirstName,
    strSurname,
    strEmail,
    intOrganisationID
FROM tbContact
WHERE intContactID = 1;

-- Try test without org filter - see all widget data
SELECT TOP 20
    c.intContactID,
    c.strFirstName,
    c.strSurname,
    c.intOrganisationID,
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
WHERE c.blnCancelled = 0;

-- Test procedure call with Contact 1's organization
-- Uncomment after finding Contact 1's org ID from first query
-- EXEC lms_admin_get_widgetReport 1, 2, '', 1, 0, 0, 0, [INSERT_CONTACT_1_ORG_ID]
