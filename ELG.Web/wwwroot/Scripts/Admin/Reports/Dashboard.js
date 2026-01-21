$(function () {
    UTILS.activateNavigationLink('dashboardLink');
    UTILS.activateMenuNavigationLink('menu-dashboard');
    dashboardHeaderDataHandler.init();
    dashboardYearlyDataHandler.init();
    dashboardCourseProgressHandler.init();
    dashboardCourseCompletionDataHandler.init();
    dashboardUserQuotaHandler.init();

    //dashboardWeeklyDataHandler.init();
    //dashboardLicenceHandler.init();
    //dashboardNotoficationDataHandler.init();
    $('[data-toggle="tooltip"]').tooltip();
})

var greenColor = getComputedStyle(document.body).getPropertyValue('--dash-complete').trim();
var yellowColor = getComputedStyle(document.body).getPropertyValue('--dash-progress').trim();
var redColor = getComputedStyle(document.body).getPropertyValue('--dash-incomplete').trim();
var notstartedColor = getComputedStyle(document.body).getPropertyValue('--dash-notstarted').trim();
var lineColor = getComputedStyle(document.body).getPropertyValue('--dash-line-graph').trim();
function hexToRgba(hex, alpha) {
    var r = parseInt(hex.slice(1, 3), 16),
        g = parseInt(hex.slice(3, 5), 16),
        b = parseInt(hex.slice(5, 7), 16);

    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
function number_format(number, decimals, dec_point, thousands_sep) {
    // *     example: number_format(1234.56, 2, ',', ' ');
    // *     return: '1 234,56'
    number = (number + '').replace(',', '').replace(' ', '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
        sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep,
        dec = (typeof dec_point === 'undefined') ? '.' : dec_point,
        s = '',
        toFixedFix = function (n, prec) {
            var k = Math.pow(10, prec);
            return '' + Math.round(n * k) / k;
        };
    // Fix for IE parseFloat(0.55).toFixed(0) = 0;
    s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.');
    if (s[0].length > 3) {
        s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    }
    if ((s[1] || '').length < prec) {
        s[1] = s[1] || '';
        s[1] += new Array(prec - s[1].length + 1).join('0');
    }
    return s.join(dec);
}

//header info
var dashboardHeaderDataHandler = (function () {
    var $loader = '<div class="spinner-border" style="width:1rem; height:1rem;" role="status"> <span class="sr-only"> Loading...</span></div>';
    var $renewalDueData = $("#renewalDueData");
    var $totalModulesData = $("#totalModulesData");
    var $totalDocumentsData = $("#totalDocumentsData");
    var $totalRAIssueData = $("#totalRAIssueData");
    var $totalAcciInciData = $("#totalAcciInciData");
    var $totalCPDHours = $("#cpdScoreData");
    var $totalLocs = $("#totalLocationsData");
    var $usageData = $("#usageData");

    var $registeredUserData = $("#registeredUserData");
    var $assignedModulesData = $("#assignedModulesData");

    function init() {
        // set loader before showing info
        $('.dash-data').html('');
        $('.dash-data').html($loader);

        //fetch data
        var url = hdnBaseUrl + "Home/LoadDashboardHeaderInfo";
        UTILS.makeAjaxCall(url, {}, function (res) {
            // populate info
            populateInfo(res);
        }, function (err) { });
    }

    function populateInfo(res) {
        $renewalDueData.html('');
        $renewalDueData.html(res.header.RenewalDate);

        $totalModulesData.html('');
        $totalModulesData.html(res.header.ModuleCount + "/" + res.header.MaxCourseCount);

        $totalDocumentsData.html('');
        $totalDocumentsData.html(res.header.DocumentCount);

        $totalRAIssueData.html('');
        $totalRAIssueData.html(res.header.HSCount);

        $totalAcciInciData.html('');
        $totalAcciInciData.html(res.header.AcciInciCount);

        $totalCPDHours.html('');
        $totalCPDHours.html(res.header.CPDScore);

        $totalLocs.html('');
        $totalLocs.html(res.header.TotalLocations + "/" + res.header.MaxLocationCount);

        $usageData.html('');
        $usageData.html(res.header.TotalUsers + "/" + res.header.MaxUsers);

        $registeredUserData.html(res.header.TotalUsers);
        $assignedModulesData.html(res.header.ModuleCount);
    }


    return {
        init: init
    }
})()

//Yearly info
//var dashboardYearlyDataHandler = (function () {
//    var $loader = '<div class="spinner-border" style="width:1rem; height:1rem;" role="status"> <span class="sr-only"> Loading...</span></div>';
//    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
//    var lastMonthsData = [];
//    var lastMonths = []

//    var dataCreated = false;

//    function init() {
//        // set loader before showing info
//        //$('.dash-data').html('');
//        //$('.dash-data').html($loader);

//        //fetch data
//        var url = hdnBaseUrl + "Home/LoadDashboardYearlyCompletion";
//        UTILS.makeAjaxCall(url, {}, function (res) {
//            // populate info
//            var yearlydata = []
//            if (res.info != undefined && res.info != null && res.info.length > 0) {
//                for (i = 0; i < res.info.length; i++) {
//                    yearlydata.push(res.info[i].CompletionCount);
//                }
//                dataCreated = true;
//            }
//            //wait while data is ready
//            var isReady = chekIfReady(dataCreated)

//            if (isReady) {
//                getYearlyDisplayData(yearlydata);
//            }

//        }, function (err) { });
//    }

//    function chekIfReady(dataCreated) {
//        if (!dataCreated) {
//            setTimeout(chekIfReady(dataCreated), 100);
//        } else {
//            return dataCreated;
//        }

//    }

//    function getYearlyDisplayData(yearlydata) {
//        var d = new Date();
//        var n = d.getMonth();
//        for (var i = 0; i < 12; i++) {
//            if ((n + 1 + i) < 12) {
//                lastMonths.push(months[n + 1 + i]);
//                lastMonthsData.push(yearlydata[n + 1 + i]);
//            } else {
//                lastMonths.push(months[(n + 1) - (12- i)]);
//                lastMonthsData.push(yearlydata[(n + 1) - (12 - i)]);
//            }
//        }
//        populateInfo()
//    }

//    function populateInfo() {

//    // Area Chart Example
//    var ctx = document.getElementById("dash_yearly_data_chart");
//    var myLineChart = new Chart(ctx, {
//        type: 'line',
//        data: {
//            labels: lastMonths,
//            datasets: [{
//                label: "Completion",
//                data: lastMonthsData,
//                lineTension: 0.3,
//                backgroundColor: lineColor,//"rgba(78, 115, 223, 0.05)",
//                borderColor: lineColor,//"rgba(78, 115, 223, 1)",
//                pointRadius: 3,
//                //pointBackgroundColor: "rgba(78, 115, 223, 1)",
//                //pointBorderColor: "rgba(78, 115, 223, 1)",
//                //pointHoverRadius: 3,
//                //pointHoverBackgroundColor: "rgba(78, 115, 223, 1)",
//                //pointHoverBorderColor: "rgba(78, 115, 223, 1)",
//                //pointHitRadius: 10,
//                //pointBorderWidth: 2,
//            }],
//        },
//        options: {
//            maintainAspectRatio: false,
//            layout: {
//                padding: {
//                    left: 10,
//                    right: 25,
//                    top: 25,
//                    bottom: 0
//                }
//            },
//            scales: {
//                xAxes: [{
//                    time: {
//                        unit: 'date'
//                    },
//                    gridLines: {
//                        display: false,
//                        drawBorder: false
//                    },
//                    ticks: {
//                        maxTicksLimit: 7
//                    }
//                }],
//                yAxes: [{
//                    ticks: {
//                        maxTicksLimit: 5,
//                        padding: 10,
//                        // Include a dollar sign in the ticks
//                        callback: function (value, index, values) {
//                            return number_format(value);
//                        }
//                    },
//                    gridLines: {
//                        color: "rgb(234, 236, 244)",
//                        zeroLineColor: "rgb(234, 236, 244)",
//                        drawBorder: false,
//                        borderDash: [2],
//                        zeroLineBorderDash: [2]
//                    }
//                }],
//            },
//            legend: {
//                display: false
//            },
//            tooltips: {
//                backgroundColor: "rgb(255,255,255)",
//                bodyFontColor: "#858796",
//                titleMarginBottom: 10,
//                titleFontColor: '#6e707e',
//                titleFontSize: 14,
//                borderColor: '#dddfeb',
//                borderWidth: 1,
//                xPadding: 15,
//                yPadding: 15,
//                displayColors: false,
//                intersect: false,
//                mode: 'index',
//                caretPadding: 10,
//                callbacks: {
//                    label: function (tooltipItem, chart) {
//                        var datasetLabel = chart.datasets[tooltipItem.datasetIndex].label || '';
//                        return datasetLabel + ': ' + number_format(tooltipItem.yLabel);
//                    }
//                }
//            }
//        }
//    });
//    }


//    return {
//        init: init
//    }
//})()
var dashboardYearlyDataHandler = (function () {
    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    var labels = [], completionData = [], assignmentData = [], userData = [];

    function init() {
        var url = hdnBaseUrl + "Home/LoadDashboardYearlyCompletion";
        UTILS.makeAjaxCall(url, {}, function (res) {
            if (res.info && res.info.length > 0) {
                processYearlyData(res.info);
                populateInfo();
            }
        }, function (err) {
            console.error("Error loading dashboard data", err);
        });
    }

    function processYearlyData(data) {
        labels = [];
        completionData = [];
        assignmentData = [];
        userData = [];

        var start = new Date();
        start.setMonth(start.getMonth() - 8);
        start.setDate(1);

        for (var i = 0; i < 12; i++) {
            var ref = new Date(start.getFullYear(), start.getMonth() + i, 1);
            var year = ref.getFullYear();
            var month = ref.getMonth() + 1;
            var label = months[month - 1] + " " + year;

            labels.push(label);

            var match = data.find(x => x.Month === month && x.Year === year);
            completionData.push(match ? match.CompletionCount : 0);
            assignmentData.push(match ? match.AssignmentCount : 0);
            userData.push(match ? match.UserCount : 0);
        }
    }

    function populateInfo() {
        const ctx = document.getElementById("dash_yearly_data_chart");

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: "Completions",
                        data: completionData,
                        borderColor: greenColor,
                        backgroundColor: hexToRgba(greenColor, 0.05),
                        pointRadius: 3,
                        pointBackgroundColor: greenColor,
                        pointBorderColor: greenColor,
                        fill: false
                    },
                    {
                        label: "Assignments",
                        data: assignmentData,
                        borderColor: lineColor,
                        backgroundColor: hexToRgba(lineColor, 0.05),
                        pointRadius: 3,
                        pointBackgroundColor: lineColor,
                        pointBorderColor: lineColor,
                        fill: false
                    },
                    {
                        label: "Users Created",
                        data: userData,
                        borderColor: redColor,
                        backgroundColor: hexToRgba(redColor, 0.05),
                        pointRadius: 3,
                        pointBackgroundColor: redColor,
                        pointBorderColor: redColor,
                        fill: false
                    }
                ]
            },
            options: {
                maintainAspectRatio: false,
                layout: {
                    padding: {
                        top: 40,
                        left: 10,
                        right: 25,
                        bottom: 0
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false,
                            drawBorder: false
                        },
                        ticks: {
                            maxTicksLimit: 12
                        }
                    },
                    y: {
                        beginAtZero: true,
                        suggestedMax: 10,
                        ticks: {
                            maxTicksLimit: 6,
                            padding: 15,
                            callback: function (value) {
                                return number_format(value);
                            }
                        },
                        grid: {
                            color: "rgb(234, 236, 244)",
                            drawBorder: false,
                            borderDash: [2],
                            zeroLineBorderDash: [2]
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: true
                    },
                    tooltip: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyColor: "#858796",
                        titleColor: '#6e707e',
                        titleFont: { size: 14 },
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        padding: 15,
                        displayColors: true,
                        intersect: false,
                        mode: 'index',
                        caretPadding: 10,
                        callbacks: {
                            label: function (context) {
                                const label = context.dataset.label || '';
                                return `${label}: ${number_format(context.parsed.y)}`;
                            }
                        }
                    }
                }
            }
        });
    }

    return {
        init: init
    };
})();

//Course completion info
var dashboardCourseCompletionDataHandler = (function () {
    var $loader = '<div class="spinner-border" style="width:1rem; height:1rem;" role="status"> <span class="sr-only"> </span></div>';
    function init() {
        // set loader before showing info
        //$('.dash-data').html('');
        //$('.dash-data').html($loader);

        //fetch data
        var url = hdnBaseUrl + "Home/LoadCourseCompletionData";
        UTILS.makeAjaxCall(url, {}, function (res) {
            // populate info
            populateInfo(res.info);

        }, function (err) { });
    }
    
    function populateInfo(courseData) {
        var completed = courseData.Completed;
        var total = courseData.TotalAssignment;
        var incomplete = Math.max(total - completed, 0);

        const data = {
            labels: ['Complete', 'Incomplete'],
            datasets: [{
                label: 'Completion Percentage',
                data: [completed, incomplete],
                backgroundColor: [greenColor, notstartedColor],
                hoverBackgroundColor: [greenColor, notstartedColor]
            }],
        };

        const dc_options = {
            maintainAspectRatio: false,
            cutout: '80%', 
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: "#000000",
                    bodyColor: "#ffffff",
                    borderColor: '#dddfeb',
                    borderWidth: 1,
                    padding: 15,
                    displayColors: false,
                    caretPadding: 10,
                }
            }
        };

        const centerTextPlugin = {
            id: 'centerText',
            beforeDraw(chart) {
                const { width, height, ctx } = chart;
                const text = total.toString();
                const fontSize = (height / 100).toFixed(2);

                ctx.save();
                ctx.font = `${fontSize}em sans-serif`;
                ctx.fillStyle = '#06262D';
                ctx.textBaseline = 'middle';

                const textX = Math.round((width - ctx.measureText(text).width) / 2);
                const textY = height / 2;

                ctx.fillText(text, textX, textY);
                ctx.restore();
            }
        };

        const canvas = document.getElementById("dash_course_completion_chart");
        if (!canvas) {
            console.error("Canvas element with id 'dash_course_completion_chart' not found.");
            return;
        }
        const ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error("Could not get 2D context for 'dash_course_completion_chart'.");
            return;
        }
        // Destroy previous chart if needed
        if (window.courseCompletionChart) {
            window.courseCompletionChart.destroy();
        }
        // Create chart with plugin
        window.courseCompletionChart = new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: dc_options,
            plugins: [centerTextPlugin]
        });
    }

    //function populateInfo(courseData) {
    //    var ctx = document.getElementById("dash_course_completion_chart");

    //    // Destroy existing chart if it exists
    //    if (window.myPieChart) {
    //        window.myPieChart.destroy();
    //    }

    //    // Plugin to draw total in center
    //    Chart.plugins.register({
    //        beforeDraw: function (chart) {
    //            if (chart.config.type === 'doughnut') {
    //                var width = chart.chart.width,
    //                    height = chart.chart.height,
    //                    ctx = chart.chart.ctx;

    //                ctx.restore();
    //                var fontSize = (height / 114).toFixed(2);
    //                ctx.font = fontSize + "em sans-serif";
    //                ctx.textBaseline = "middle";

    //                var text = courseData.TotalAssignment.toString(),
    //                    textX = Math.round((width - ctx.measureText(text).width) / 2),
    //                    textY = height / 2;

    //                ctx.fillStyle = "#4e73df"; // Customize if needed
    //                ctx.fillText(text, textX, textY);
    //                ctx.save();
    //            }
    //        }
    //    });

    //    var completed = courseData.Completed;
    //    var total = courseData.TotalAssignment;
    //    var incomplete = Math.max(total - completed, 0);

    //    // Create the chart
    //    window.myPieChart = new Chart(ctx, {
    //        type: 'doughnut',
    //        data: {
    //            labels: ["Completed", "Incomplete"],
    //            datasets: [{
    //                data: [completed, incomplete],
    //                backgroundColor: [greenColor, notstartedColor],
    //                hoverBackgroundColor: [greenColor, notstartedColor],
    //                hoverBorderColor: "rgba(234, 236, 244, 1)",
    //            }],
    //        },
    //        options: {
    //            maintainAspectRatio: false,
    //            cutoutPercentage: 80,
    //            tooltips: {
    //                backgroundColor: "#000000",
    //                bodyFontColor: "#858796",
    //                borderColor: '#dddfeb',
    //                borderWidth: 1,
    //                xPadding: 15,
    //                yPadding: 15,
    //                displayColors: true,
    //                caretPadding: 10,
    //                callbacks: {
    //                    label: function (tooltipItem, data) {
    //                        var label = data.labels[tooltipItem.index] || '';
    //                        var value = data.datasets[0].data[tooltipItem.index];
    //                        return `${label}: ${number_format(value)}`;
    //                    }
    //                }
    //            },
    //            legend: {
    //                display: true,
    //                position: 'bottom'
    //            }
    //        }
    //    });
    //}

    return {
        init: init
    }
})()

//Course completion info
var dashboardUserQuotaHandler = (function () {
    var $loader = '<div class="spinner-border" style="width:1rem; height:1rem;" role="status"> <span class="sr-only"> </span></div>';
    function init() {
        //fetch data
        var url = hdnBaseUrl + "Home/LoadDashboardUserQuotaPerLocation";
        UTILS.makeAjaxCall(url, {}, function (res) {
            // populate info
            renderUserQuotaChart(res.info, res.info[0].TotalQuota);

        }, function (err) { });
    }
    
    function renderUserQuotaChart(data, totalQuota) {
        const usedTotal = data.reduce((sum, item) => sum + item.UsedUsers, 0);
        const remaining = Math.max(totalQuota - usedTotal, 0);

        // Add synthetic row for remaining quota
        const chartData = [...data.map(item => ({
            label: item.LocationName,
            value: item.UsedUsers
        }))];

        chartData.push({
            label: "Remaining Quota",
            value: remaining
        });

        const labels = chartData.map(item => item.label);
        const values = chartData.map(item => item.value);
        const colors = chartData.map(item =>
            item.label === "Remaining Quota" ? notstartedColor : greenColor
        );

        const ctx = document.getElementById("dash_user_quota_chart").getContext("2d");

        if (window.userQuotaChart) {
            window.userQuotaChart.destroy();
        }

        window.userQuotaChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Users',
                    data: values,
                    backgroundColor: colors
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        max: totalQuota,
                        ticks: {
                            precision: 0
                        }
                    },
                    y: {
                        ticks: {
                            autoSkip: false
                        }
                    }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (tooltipItem) {
                                return `${tooltipItem.label}: ${tooltipItem.raw}`;
                            }
                        }
                    }
                }
            }
        });
    }

    //function populateInfo(courseData) {
    //    var ctx = document.getElementById("dash_course_completion_chart");

    //    // Destroy existing chart if it exists
    //    if (window.myPieChart) {
    //        window.myPieChart.destroy();
    //    }

    //    // Plugin to draw total in center
    //    Chart.plugins.register({
    //        beforeDraw: function (chart) {
    //            if (chart.config.type === 'doughnut') {
    //                var width = chart.chart.width,
    //                    height = chart.chart.height,
    //                    ctx = chart.chart.ctx;

    //                ctx.restore();
    //                var fontSize = (height / 114).toFixed(2);
    //                ctx.font = fontSize + "em sans-serif";
    //                ctx.textBaseline = "middle";

    //                var text = courseData.TotalAssignment.toString(),
    //                    textX = Math.round((width - ctx.measureText(text).width) / 2),
    //                    textY = height / 2;

    //                ctx.fillStyle = "#4e73df"; // Customize if needed
    //                ctx.fillText(text, textX, textY);
    //                ctx.save();
    //            }
    //        }
    //    });

    //    var completed = courseData.Completed;
    //    var total = courseData.TotalAssignment;
    //    var incomplete = Math.max(total - completed, 0);

    //    // Create the chart
    //    window.myPieChart = new Chart(ctx, {
    //        type: 'doughnut',
    //        data: {
    //            labels: ["Completed", "Incomplete"],
    //            datasets: [{
    //                data: [completed, incomplete],
    //                backgroundColor: [greenColor, notstartedColor],
    //                hoverBackgroundColor: [greenColor, notstartedColor],
    //                hoverBorderColor: "rgba(234, 236, 244, 1)",
    //            }],
    //        },
    //        options: {
    //            maintainAspectRatio: false,
    //            cutoutPercentage: 80,
    //            tooltips: {
    //                backgroundColor: "#000000",
    //                bodyFontColor: "#858796",
    //                borderColor: '#dddfeb',
    //                borderWidth: 1,
    //                xPadding: 15,
    //                yPadding: 15,
    //                displayColors: true,
    //                caretPadding: 10,
    //                callbacks: {
    //                    label: function (tooltipItem, data) {
    //                        var label = data.labels[tooltipItem.index] || '';
    //                        var value = data.datasets[0].data[tooltipItem.index];
    //                        return `${label}: ${number_format(value)}`;
    //                    }
    //                }
    //            },
    //            legend: {
    //                display: true,
    //                position: 'bottom'
    //            }
    //        }
    //    });
    //}

    return {
        init: init
    }
})()
//Weekly info
//var dashboardWeeklyDataHandler = (function () {
//    var $loader = '<div class="spinner-border" style="width:1rem; height:1rem;" role="status"> <span class="sr-only"> Loading...</span></div>';
//    function init() {
//        // set loader before showing info
//        //$('.dash-data').html('');
//        //$('.dash-data').html($loader);

//        //fetch data
//        var url = hdnBaseUrl + "Home/LoadDashboardWeeklyData";
//        UTILS.makeAjaxCall(url, {}, function (res) {
//            // populate info
//            populateInfo(res.info);

//        }, function (err) { });
//    }
    
//    function populateInfo(weeklydata) {
//        // Pie Chart Example
//        var ctx = document.getElementById("dash_weekly_data_chart");
//        var myPieChart = new Chart(ctx, {
//            type: 'doughnut',
//            data: {
//                labels: ["Completed", "In-progress", "Not started"],
//                datasets: [{
//                    data: [weeklydata.WeeklyCompleted, weeklydata.WeeklyInProgress, weeklydata.WeeklyNotStarted],
//                    backgroundColor: [greenColor, yellowColor, notstartedColor],
//                    hoverBackgroundColor: [greenColor, yellowColor, notstartedColor],
//                    hoverBorderColor: "rgba(234, 236, 244, 1)",
//                }],
//            },
//            options: {
//                maintainAspectRatio: false,
//                tooltips: {
//                    backgroundColor: "#000000",
//                    bodyFontColor: "#858796",
//                    borderColor: '#dddfeb',
//                    borderWidth: 1,
//                    xPadding: 15,
//                    yPadding: 15,
//                    displayColors: false,
//                    caretPadding: 10,
//                },
//                legend: {
//                    display: false
//                },
//                cutoutPercentage: 80,
//            },
//        });
//    }


//    return {
//        init: init
//    }
//})()

////licence usage
//var dashboardLicenceHandler = (function () {
//    var $loader = '<div class="text-center"><div class="spinner-border" style="width:3rem; height:3rem;" role="status"> <span class="sr-only"> Loading...</span></div><div>';
//    var $container = $("#licenceUsageContainer");
//    var $template = '<h4 class="small font-weight-bold">{{course_name}}<span class="float-right">org_licence_consumed%</span></h4><div class="progress mb-4"><div class="progress-bar {{usage_class}}" role="progressbar" style="width: org_licence_consumed%" aria-valuenow="org_licence_consumed" aria-valuemin="0" aria-valuemax="100"></div></div>';

//    function init() {
//        // set loader before showing info
//        $container.html('');
//        $container.html($loader);

//        //fetch data
//        var url = hdnBaseUrl + "Home/GetDashboardLicenseUsage";
//        UTILS.makeAjaxCall(url, {}, function (res) {
//            // populate info
//            populateInfo(res.info);
//        }, function (err) { });
//    }

//    function populateInfo(result) {
//        $container.html('');
//        if (result != undefined && result != null && result.length > 0) {
//            for (i = 0; i < result.length; i++) {
//                var courseRow = $template;
//                var rowclass="";
//                courseRow = courseRow.replace("{{course_name}}", result[i].CourseName)

//                if (result[i].UsagePercentage > 90)
//                    rowclass = "bg-danger";
//                else if (result[i].UsagePercentage < 50)
//                    rowclass = "bg-success";
//                else
//                    rowclass = "bg-warning";

//                courseRow = courseRow.replace("{{usage_class}}", rowclass);
//                courseRow = courseRow.replace(/org_licence_consumed/g, result[i].UsagePercentage)
//                $container.append(courseRow)
//            }
//        }
//        else {
//            $container.html('');
//            $container.html('<div class="alert alert-info">No courses found.</div>');
//        }
//    }
//    function redirectToLicenceUsage() {
//        window.location.href = hdnBaseUrl + "Licenses/ReviewLicenses";
//    }


//    return {
//        init: init,
//        redirectToLicenceUsage: redirectToLicenceUsage
//    }
//})()


//course progress
var dashboardCourseProgressHandler = (function () {
    var $loader = '<div class="text-center"><div class="spinner-border" style="width:3rem; height:3rem;" role="status"> <span class="sr-only"></span></div><div>';
    var $container = $("#licenceUsageContainer");
    var $template = '<h4 class="small font-weight-bold">{{course_name}}<span class="float-end">org_course_assigned%</span></h4><div class="progress mb-4"><div class="progress-bar" role="progressbar" style="width: org_course_assigned%;  {{usage_class}}" aria-valuenow="org_course_assigned" aria-valuemin="0" aria-valuemax="100"></div></div>';

    function init() {
        // set loader before showing info
        $container.html('');
        $container.html($loader);

        //fetch data
        var url = hdnBaseUrl + "Home/GetDashboardCourseAssignments";
        UTILS.makeAjaxCall(url, {}, function (res) {
            // populate info
            populateInfo(res.info);
        }, function (err) { });
    }

    function populateInfo(result) {
        $container.html('');
        if (result != undefined && result != null && result.length > 0) {
            for (i = 0; i < result.length; i++) {
                var courseRow = $template;
                var rowclass = "";
                courseRow = courseRow.replace("{{course_name}}", result[i].CourseName)

                if (result[i].AssignmentPercentage > 90)
                    rowclass = `background-color:${redColor};`
                else if (result[i].AssignmentPercentage < 30)
                    rowclass = `background-color:${greenColor};`
                else
                    rowclass = `background-color:${lineColor};`

                courseRow = courseRow.replace("{{usage_class}}", rowclass);
                courseRow = courseRow.replace(/org_course_assigned/g, result[i].AssignmentPercentage)
                $container.append(courseRow)
            }
        }
        else {
            $container.html('');
            $container.html('<div class="alert alert-info">No courses found.</div>');
        }
    }
    function redirectToLicenceUsage() {
        window.location.href = hdnBaseUrl + "Licenses/ReviewLicenses";
    }


    return {
        init: init,
        redirectToLicenceUsage: redirectToLicenceUsage
    }
})()

//notification info
var dashboardNotoficationDataHandler = (function () {
    var $notification = $('#notificationText');
    var $loader = '<div class="spinner-border" style="width:1rem; height:1rem;" role="status"> <span class="sr-only"> Loading...</span></div>';
    function init() {
        // set loader before showing info
        $notification.html('');
        $notification.html($loader);

        //fetch data
        var url = hdnBaseUrl + "Home/GetDashboardNotification";
        UTILS.makeAjaxCall(url, {}, function (res) {
            // populate info
            populateInfo(res);
        }, function (err) { });
    }

    function populateInfo(res) {
        $notification.html('');
        $notification.html(res.notification);
    }


    return {
        init: init
    }
})()