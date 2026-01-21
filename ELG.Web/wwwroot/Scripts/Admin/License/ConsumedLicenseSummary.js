
var manageConsumedLicenseHandler = (function () {

    var $cl_modal = $('#learnerWithConsumedLicenseModal');
    var $learner = $('#txt_lwcl_Learner')
    var $ddlLoc = $('#ddl_lwcl_Location')
    var $ddlDep = $('#ddl_lwcl_Department')
    var $searchBtn = $('#search_lwcl_Learner')
    var $clearsearchBtn = $('#clearSearch_lwcl_Learner')
    var course = 0;

    //populate location list with admin rights
    var consumedLicenseTable = $('#learnerListFor_LWCL').DataTable({
        "processing": true,
        "language": {
            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
            "emptyTable": "No record(s) found."
        },
        "serverSide": true,
        "filter": false,
        "searching": false,
        "ajax": {
            "url": hdnBaseUrl + "/CourseManagement/LoadConsumedLicenseLearnerData",
            "type": "POST",
            "datatype": "json",
            "data": function (data) {
                data.SearchText = $learner.val();
                data.Location = $ddlLoc.val();
                data.Department = $ddlDep.val();
                data.Course = course;
            },
            "error": function (xhr, error, code) {
                console.log(xhr);
                console.log(code);
                alert("Oops! Something went wrong please try again later.");
            }
        },
        "columns": [
            { "data": "FirstName", "name": "c.strFirstName", "autoWidth": true },
            { "data": "EmailId", "name": "c.strEmail", "autoWidth": true },
            { "data": "Location", "name": "l.strLocation", "autoWidth": true },
            { "data": "Department", "name": "d.strDepartment", "autoWidth": true }
        ],
        columnDefs: [{
            targets: [0], render(a, b, data, d) {
                return '<span>' + data["FirstName"] + ' ' + data["LastName"] + '</span>'
            }
        }]
    });

    function showLearnerWithConsumedLicenses($btn) {
        course = $btn.id.split('-').pop();
        $cl_modal.modal('show');
        consumedLicenseTable.draw();
    }

    $searchBtn.click(function() {
        consumedLicenseTable.draw();
    })

    $clearsearchBtn.click(function () {
        $learner.val('');
        $ddlLoc.val('0');
        $ddlDep.val('0');
        consumedLicenseTable.draw();
    })

    return {
        showLearnerWithConsumedLicenses: showLearnerWithConsumedLicenses
    }
})();