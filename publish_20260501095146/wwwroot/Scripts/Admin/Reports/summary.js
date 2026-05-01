$(function () {
    UTILS.activateNavigationLink('reportLink');
    UTILS.activateMenuNavigationLink('menu-summary-report');
    $('[data-toggle="tooltip"]').tooltip();
    summaryHandler.loadCourseCards();
});


var greenColor = getComputedStyle(document.body).getPropertyValue('--dash-complete').trim();
var yellowColor = getComputedStyle(document.body).getPropertyValue('--dash-progress').trim();
var redColor = getComputedStyle(document.body).getPropertyValue('--dash-incomplete').trim();
var notstartedColor = getComputedStyle(document.body).getPropertyValue('--dash-notstarted').trim();

//$(document).ready(function () {
//    var departmentsData = [
//        { name: "HR", completed: 60, assigned: 100 },
//        { name: "IT", completed: 120, assigned: 150 },
//        { name: "Finance", completed: 90, assigned: 120 },
//        { name: "Marketing", completed: 40, assigned: 100 },
//        { name: "Sales", completed: 110, assigned: 140 }
//    ];

//    $("#toggleDetails").click(function (e) {
//        e.preventDefault();

//        $("#locationDetails").slideToggle(function () {
//            $("#locationDetails").toggleClass("visible");
//            var isExpanded = $("#locationDetails").hasClass("visible");
//            $("#toggleDetails").html(isExpanded ? "View less" : "View More");
//        });
//    });

//    // Initialize the Location Chart
//    var primaryColor = getComputedStyle(document.body).getPropertyValue('--color-primary').trim();
//    var ctx = document.getElementById("locationChart").getContext("2d");

//    var myChart = new Chart(ctx, {
//        type: "bar",
//        data: {
//            labels: [
//                "London", "Manchester", "Birmingham", "Glasgow", "Liverpool",
//                "Leeds", "Sheffield", "Edinburgh", "Bristol", "Cardiff",
//                "Nottingham", "Leicester", "Newcastle", "Southampton", "Aberdeen",
//                "Coventry", "Stoke-on-Trent", "Reading", "Hull", "Derby"
//            ],
//            datasets: [{
//                label: "Completions",
//                data: [150, 120, 100, 80, 60, 90, 110, 85, 95, 70, 75, 88, 92, 78, 65, 55, 45, 40, 50, 35],
//                backgroundColor: "#28a745" // All bars in green
//            }]
//        },
//        options: {
//            responsive: true,
//            maintainAspectRatio: false,
//            scales: {
//                x: {
//                    beginAtZero: true,
//                    barPercentage: 0.5,
//                    categoryPercentage: 0.6
//                }
//            }
//        }
//    });

//    $("#locationChart").on("click", function (event) {
//        var elements = myChart.getElementsAtEventForMode(event, "nearest", { intersect: true }, true);

//        if (elements.length > 0) {
//            var index = elements[0].index;
//            var cityName = myChart.data.labels[index];

//            let departmentHtml = "";
//            departmentsData.forEach(dept => {
//                let completionPercentage = Math.floor((dept.completed / dept.assigned) * 100);
//                let color = completionPercentage > 75 ? "#28a745" : completionPercentage > 50 ? "#ffc107" : "#dc3545"; // Green, Yellow, Red

//                departmentHtml += `
//        <div class="department-item">
//            <div class="dept-name">${dept.name}</div>
//            <div style="width:200px" data-toggle="tooltip" title="Completions - ${completionPercentage}%">
//                <div style="width: ${completionPercentage}%; background-color: ${color};" class="completion-bar"></div>
//            </div>
//            <div class="dept-stats">${dept.completed} / ${dept.assigned}</div>
//        </div>`;
//            });



//            $("#modalTitle").text(`Training Progress for ${cityName}`);
//            $("#modalBody").html(departmentHtml);
//            $("#myModal").modal("show");
//        }
//    });

//});

const summaryHandler = (function () {


    let $loader = '<div class="text-center"><div class="spinner-border" style="width:3rem; height:3rem;" role="status"> <span class="sr-only"> Loading...</span></div><div>';
    let $container = $("#div-card-container");
    let $noRecMessage = `<div class="alert alert-info">No record(s) found.</div>`;
    function loadCourseCards() {
        // set loader before showing info
        $container.html('');
        $container.html($loader);

        //fetch data
        var url = hdnBaseUrl + "Report/LoadLearningStatistics";
        UTILS.makeAjaxCall(url, {}, function (res) {
            // populate info

            if (res != null && res.stats != null && res.stats.length > 0) {
                $container.html('');
                res.stats.forEach(record => {
                    let card = getCourseCard(record)
                    $container.append(card);
                    summaryHandler.drawCompletionDoughnut(record, 'completionChart_' + record.CourseID);
                });
            }
            else {
                $container.html($noRecMessage);
            }
        }, function (err) { });
    }

    function fetchCourseLocationStats(course) {
        var url = hdnBaseUrl + "Report/LoadLearningStatisticsForLocations";

        return new Promise((resolve, reject) => {
            UTILS.makeAjaxCall(url, { course }, function (res) {
                if (res && res.stats && res.stats.length > 0) {
                    resolve(res.stats); // Resolve the promise with data
                } else {
                    reject("No records found.");
                }
            }, function (err) {
                reject("Error fetching data.");
            });
        });
    }

    function loadCourseLocationStats($locStatContainer, course) {
        $locStatContainer.html($loader);

        fetchCourseLocationStats(course)
            .then(stats => {
                $locStatContainer.html(`<canvas id="locationChart_${course}"></canvas>`);
                setTimeout(() => {
                    let canvasId = `locationChart_${course}`;
                    if (!$(`#${canvasId}`).length) {
                        $locStatContainer.append(`<canvas id="${canvasId}"></canvas>`);
                    }
                    drawLocationStatsChart(stats, canvasId);
                }, 500); 

                //drawLocationStatsChart(stats, `locationChart_${course}`);
            })
            .catch(errorMessage => {
                $locStatContainer.html(`<p class="error">${errorMessage}</p>`);
            });
    }


    //function loadCourseLocationStats($locStatContainer, course) {
    //    // Set loader before fetching data
    //    $locStatContainer.html($loader);

    //    // Define the API URL
    //    var url = hdnBaseUrl + "Report/LoadLearningStatisticsForLocations";

    //    // Make AJAX call to fetch data
    //    UTILS.makeAjaxCall(url, { course }, function (res) {
    //        if (res && res.stats && res.stats.length > 0) {
    //            // Clear previous content before rendering chart
    //            $locStatContainer.empty();

    //            // Ensure canvas doesn't exist multiple times (prevents duplicate charts)
    //            var canvasId = `locationChart_${course}`;
    //            if (!$(`#${canvasId}`).length) {
    //                $locStatContainer.append(`<canvas id="${canvasId}"></canvas>`);
    //            }

    //            const observer = new MutationObserver(() => {
    //                if (document.getElementById(canvasId)) {
    //                    observer.disconnect(); // Stop observing after element is found
    //                    drawLocationStatsChart(res.stats, canvasId);
    //                }
    //            });
    //            observer.observe(document.body, { childList: true, subtree: true });
    //            //// Draw the chart
    //            //drawLocationStatsChart(res.stats, canvasId);
    //        } else {
    //            // Display message when no records are found
    //            $locStatContainer.html($noRecMessage);
    //        }
    //    }, function (err) {
    //        console.error("Error fetching location stats:", err);
    //        $locStatContainer.html("<p class='error'>Failed to load data. Please try again.</p>");
    //    });
    //}


    function getCourseCard(course) {
        let courseCard = `
    <div class="card shadow-sm mt-4">
        <!-- Card Header -->
        <div class="card-header">
            <h5 class="mb-0">${course.Course}</h5>
        </div>

        <!-- Statistics Row -->
        <div class="card-body">
            <div class="stats-container">
                <!-- Doughnut Chart -->
                <div class="stat-item-first">
                    <div class="chart-container">
                        <canvas id="completionChart_${course.CourseID}"></canvas>
                    </div>
                </div>
                <div class="stat-item">
                    <span class="stat-title">Completed</span>
                    <span class="stat-value">${course.Completed}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-title">Assigned</span>
                    <span class="stat-value">${course.Assigned}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-title">In progress</span>
                    <span class="stat-value">${course.Inprogress}</span>
                </div>
                <div class="stat-item stat-item-last">
                    <span class="stat-title">Not started</span>
                    <span class="stat-value">${course.Notstarted}</span>
                </div>
            </div>
        </div>

        <!-- Footer Row with Links -->
        <div class="card-footer">
            <button type="button" id="toggleDetails_${course.CourseID}" class="btn btn-xs btn-primary toggleDetails" onclick="summaryHandler.toggleDetailView(this)">View more</button>
            <button type="button" id="btn_showreport_${course.CourseID}" class="btn btn-xs btn-primary" onclick="summaryHandler.redirectToReport(this)">Show records</button>
        </div>

        <!-- Hidden Location Chart Section -->
        <div id="locationDetails_${course.CourseID}" class="p-3 div-loc-chart" style="display: none;">
            
        </div>

    </div>`;

        return courseCard;
    }

    function redirectToReport(btn) {
        let course = btn.id.split('_').pop()
        window.location.href = `${hdnBaseUrl}Report/LearningProgress?course=${course}`;
    } 
    function drawCompletionDoughnut(res, chartId) {
        var ctx = document.getElementById(chartId).getContext("2d");
        if (!ctx) {
            console.error(`Canvas element with ID ${chartId} not found.`);
            return;
        }

        var completionPercent = res.Assigned > 0
            ? Math.floor((res.Completed / res.Assigned) * 100)
            : 0;

        // Chart data
        const data = {
            labels: ['Complete', 'Incomplete'],
            datasets: [{
                label: 'Completion Percentage',
                data: [res.Completed, (res.Assigned - res.Completed)],
                backgroundColor: [greenColor, notstartedColor],
                hoverBackgroundColor: [greenColor, notstartedColor]
            }],
        };

        // Chart configuration
        const dc_options = {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '80%',
            plugins: {
                legend: {
                    display: false,
                }
            },
            interaction: {
                intersect: false,
                mode: 'index',
            },
        };

        // Create the chart
        const chart = new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: dc_options,
            plugins: [{
                id: 'myDoughnutPlugin',
                beforeDraw: (chart) => {
                    let ctx = chart.ctx;
                    ctx.save();

                    let completionPercentLocal = Math.floor(
                        (chart.data.datasets[0].data[0] / (chart.data.datasets[0].data[0] + chart.data.datasets[0].data[1])) * 100
                    );

                    let fontSize = (chart.height / 62).toFixed(2);
                    ctx.font = fontSize + "em sans-serif";
                    ctx.fillStyle = '#06262D';
                    ctx.textBaseline = "middle";
                    let text = completionPercentLocal + '%';
                    let textX = Math.round((chart.width - ctx.measureText(text).width) / 2);
                    let textY = chart.height / 1.8;
                    ctx.fillText(text, textX, textY);

                    ctx.restore();
                }
            }]
        });
    }

    function drawLocationStatsChart(res, chartId) {
        var locations = res.map(item => item.Location); // Extract location names
        var completions = res.map(item => item.Completed); // Extract completion counts
        var assigned = res.map(item => item.Assigned); // Extract assigned counts
        var locationIds = res.map(item => item.LocationId);
        var ctx = document.getElementById(chartId).getContext("2d");

        if (!ctx) {
            console.error(`Canvas element with ID ${chartId} not found.`);
            return;
        }
        var myChart = new Chart(ctx, {
            type: "bar",
            data: {
                labels: locations, // Use dynamically populated locations
                datasets: [
                    {
                        label: "Completed",
                    data: completions, // Use dynamically populated completion counts
                        backgroundColor: greenColor, // Green for completed
                    maxBarThickness: 30
                    },
                    {
                        label: "Assigned",
                        data: assigned, // Use dynamically populated assigned counts
                        backgroundColor: '#CCCCCC', // Light gray for assigned
                        maxBarThickness: 30
                    }
                ]
            },
            options: {
                onClick: function (event, elements) {
                    if (elements.length > 0) {
                        var index = elements[0].index;
                        var locationId = locationIds[index]; // Get location ID
                        var courseId = chartId.split("_").pop(); // Extract Course ID
                        var locationName = myChart.data.labels[index];

                        openLocationDetailsModal(locationId, courseId, locationName);
                    }
                },
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        barPercentage: 0.2,
                        categoryPercentage: 0.3
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1, // Ensures only whole numbers appear
                            callback: function (value) {
                                return Number.isInteger(value) ? value : ''; // Removes decimal values
                            }
                        }
                    }

                },
                elements: {
                    bar: {
                        maxBarThickness: 30 // Limits the bar width to prevent it from looking huge
                    }
                }
            }
        });

        
    }

    function toggleDetailView(btn) {
        let course = btn.id.split('_').pop();
        let $locationDetails = $("#locationDetails_" + course);

        // Check if the AJAX call has already been made
        if (!$(btn).data("ajaxLoaded")) {
            loadCourseLocationStats($locationDetails, course);
            $(btn).data("ajaxLoaded", true); 
        }

        // Toggle visibility
        $locationDetails.slideToggle(function () {
            $locationDetails.toggleClass("visible");
            var isExpanded = $locationDetails.hasClass("visible");
            $(btn).html(isExpanded ? "View less" : "View More");
        });
    }

    
    function openLocationDetailsModal(location, course, locationName) {
        // Show loading indicator in the modal
        $("#departmentStatsModal #modalTitle").text(`Training Progress for ${locationName}`);
        $("#departmentStatsModal #modalBody").html($loader);
        $("#departmentStatsModal").modal("show");

        var url = hdnBaseUrl + "Report/LoadLearningStatisticsForDepartments"; // Replace with your endpoint
        UTILS.makeAjaxCall(url, { course, location }, function (res) {
            // populate info

            if (res != null && res.stats != null && res.stats.length > 0) {

                let departmentHtml = "";
                res.stats.forEach(dept => {
                    let completionPercentage = Math.floor((dept.Completed / dept.Assigned) * 100);
                    let color = completionPercentage > 75 ? greenColor : completionPercentage > 50 ? yellowColor : redColor; // Green, Yellow, Red

                    departmentHtml += `
                        <div class="department-item">
                            <div class="dept-name">${dept.Department}</div>
                            <div style="width:200px" data-toggle="tooltip" title="Completions - ${completionPercentage}%">
                                <div style="width: ${completionPercentage}%; background-color: ${color};" class="completion-bar"></div>
                            </div>
                            <div class="dept-stats">${dept.Completed} / ${dept.Assigned}</div>
                        </div>`;

                    $("#departmentStatsModal #modalBody").html('');
                    $("#departmentStatsModal #modalBody").html(departmentHtml); // Populate modal with response data
                });
            }
            else {
                $("#departmentStatsModal #modalBody").html($noRecMessage);
            }
        }, function (err) {
            $("#departmentStatsModal #modalBody").html("<p>Error fetching details.</p>");
        });
    }

    return {
        loadCourseCards: loadCourseCards,
        loadCourseLocationStats: loadCourseLocationStats,
        drawCompletionDoughnut: drawCompletionDoughnut,
        redirectToReport: redirectToReport,
        toggleDetailView: toggleDetailView
    };
})();

// Handle Generate Report button click for Excel export
$(document).on('click', '#generateReportBtn', function (e) {
    e.preventDefault();
    var $btn = $(this);
    var $originalText = $btn.data('original-text');
    var $loadingText = $btn.data('loading-text');
    
    // Store original text if not already stored
    if (!$originalText) {
        $originalText = $btn.html();
        $btn.data('original-text', $originalText);
    }
    
    // Change button text to loading state
    $btn.html($loadingText).prop('disabled', true);
    
    // Initiate download by navigating to the download endpoint
    var url = hdnBaseUrl + "Report/DownloadSummaryReport";
    window.location.href = url;
    
    // Reset button after a delay (to allow download to start)
    setTimeout(function () {
        $btn.html($originalText).prop('disabled', false);
    }, 2000);
});