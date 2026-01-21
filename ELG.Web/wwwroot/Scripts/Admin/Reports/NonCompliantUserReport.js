
var manageFreeLicenseHandler = (function () {

    var $nc_modal = $('#learnerListForNonCompliantModal');
    var course = 0;
    var department = 0;
    var location = 0;
    var $txtRptFrom = $('#txtAdvCompRptFrom');
    var $txtRptTo = $('#txtAdvCompRptTo');

    //populate location list with admin rights
    var nonCompliantTable = $('#learnerListFor_NonCompliant').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "searching": false,
        "ordering": false,
        "ajax": {
            "url": hdnBaseUrl + "/Report/LoadAdvCompliantUserData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.Location = location;
                data.Department = department;
                data.Course = course;
                data.FromDate = $txtRptFrom.val();
                data.ToDate = $txtRptTo.val();
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "createdRow": function (row, data, dataIndex) {
            if (data["CourseStatus"] == 'passed' || data["CourseStatus"] =='completed') {
                $(row).addClass('table-success');
            }
        },
        "columns": [
            { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
            { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
            { "data": "Location", "name": "l.strLocation", "autoWidth": true },
            { "data": "Department", "name": "l.strDepartment", "autoWidth": true },
            { "data": "CourseStatus", "name": "pd.strStatus", "autoWidth": true },
            { "data": "CompletionDate", "name": "pd.dateCompletedOn", "autoWidth": true }
        ],
        columnDefs: [
            {
                // render action buttons in the last column
                targets: [0], render: function (a, b, data, d) {
                    return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
                }
            }, {
                targets: [4], render: function (a, b, data, d) {
                    if (data["CourseStatus"] == 'incomplete')
                        return "<span>In-progress</span>";
                    else
                        return "<span>" + data["CourseStatus"] + "</span>";
                }
            }]
    });

    function showNonCompliantUserList(cid, lid, did) {
        course = cid;
        location = lid;
        department = did;
        if (did > 0 && lid > 0) {
            $nc_modal.modal('show');
            nonCompliantTable.draw();
        }
    }
    //$('body').tooltip({ selector: '[data-toggle="tooltip"]' });
    return {
        showNonCompliantUserList: showNonCompliantUserList
    }
})();