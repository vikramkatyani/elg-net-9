var progressHistoryHandler = (function () {

    const historyModal = document.querySelector('#courseProgressHistoryModal');
    let course = 0;
    let historyTable = null;

    // Initialize DataTable only when the table element exists
    function initializeHistoryTable() {
        if (historyTable === null) {
            const tableElement = document.querySelector('#learnerCourseProgressHistoryTable');
            if (tableElement) {
                historyTable = new DataTable('#learnerCourseProgressHistoryTable', {
                    processing: true,
                    language: {
                        processing: '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i><span class="sr-only">Loading...</span> ',
                        emptyTable: "No record(s) found."
                    },
                    serverSide: true,
                    filter: false,
                    searching: false,
                    ordering: false,
                    ajax: {
                        url: hdnBaseUrl + "CourseHistoryList",
                        type: "POST",
                        datatype: "json",
                        data: function (data) {
                            data.Course = course;
                        },
                        error: function (xhr, error, code) {
                            alert("Oops! Something went wrong please try again later.");
                            window.location.reload();
                        }
                    },
                    columns: [
                        { data: "Course", name: "m.strcourse", autoWidth: true },
                        { data: "ProgressStatus", name: "pd.strstatus", autoWidth: true },
                        { data: "Score", name: "pd.intscore", autoWidth: true },
                        { data: "AssignedOn", name: "pd.dateassignedon", autoWidth: true },
                        { data: "CompletedOn", name: "pd.datecompletedon", autoWidth: true }
                    ],
                    columnDefs: [
                        {
                            // render action buttons in the last column
                            targets: [5], render: function (a, b, data, d) {
                                if (data["ProgressStatus"] === "PASSED") {
                                    return '<button type="button" id="cert-rec-' + data["RecordId"] + '" class="elg-btn elg-btn-primary mb-1" onclick = "myCoursesHandler.createCertificate(this)" > <i class="fa fa-print"></i> Print Certificate</button>';
                                } else {
                                    return '<button type="button" id="cert-rec-' + data["RecordId"] + '" class="elg-btn elg-btn-primary mb-1 disabled"> <i class="fa fa-print"></i> Print Certificate</button>';
                                }
                            }
                        }]
                });
            }
        }
    }

    function showCourseHistory(cid) {
        course = cid;
        if (cid > 0) {
            // Initialize table before showing modal
            initializeHistoryTable();
            new bootstrap.Modal(historyModal).show();
            if (historyTable) {
                historyTable.draw();
            }
        }
    }

    return {
        showCourseHistory: showCourseHistory
    }
})();
