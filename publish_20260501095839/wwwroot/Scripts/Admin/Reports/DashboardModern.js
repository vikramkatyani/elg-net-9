$(function () {
    UTILS.activateNavigationLink('dashboardLink');
    UTILS.activateMenuNavigationLink('menu-dashboard');

    dashboardModern.init();
    $('[data-toggle="tooltip"]').tooltip();
});

var dashboardModern = (function () {
    var charts = {
        trend: null,
        completion: null,
        quota: null
    };

    function init() {
        loadHeader();
        loadYearlyTrend();
        loadCompletionSnapshot();
        loadCourseAssignments();
        loadQuotaByLocation();
    }

    function makeAjax(url, success) {
        UTILS.makeAjaxCall(url, {}, success, function () {
            // keep screen stable on partial service failures
        });
    }

    function parseIntSafe(value) {
        var n = parseInt(value, 10);
        return isNaN(n) ? 0 : n;
    }

    function pct(used, max) {
        if (!max || max <= 0) return 0;
        return Math.round((used / max) * 100);
    }

    function setText(id, value) {
        $(id).text(value == null || value === '' ? '-' : value);
    }

    function loadHeader() {
        var url = hdnBaseUrl + "Home/LoadDashboardHeaderInfo";
        makeAjax(url, function (res) {
            if (!res || !res.header) return;

            var h = res.header;
            var users = parseIntSafe(h.TotalUsers);
            var usersMax = parseIntSafe(h.MaxUsers);
            var courses = parseIntSafe(h.ModuleCount);
            var coursesMax = parseIntSafe(h.MaxCourseCount);
            var locs = parseIntSafe(h.TotalLocations);
            var locsMax = parseIntSafe(h.MaxLocationCount);

            setText('#md-renewalDue', h.RenewalDate);
            setText('#md-userUtilization', pct(users, usersMax) + '%');
            setText('#md-courseUtilization', pct(courses, coursesMax) + '%');
            setText('#md-locationUtilization', pct(locs, locsMax) + '%');

            setText('#md-usersDetail', users + ' / ' + usersMax + ' users in use');
            setText('#md-courseDetail', courses + ' / ' + coursesMax + ' courses used');
            setText('#md-locationDetail', locs + ' / ' + locsMax + ' locations active');

            setText('#md-documents', parseIntSafe(h.DocumentCount));
            setText('#md-raIssues', parseIntSafe(h.HSCount));
            setText('#md-accInc', parseIntSafe(h.AcciInciCount));
            setText('#md-cpd', parseIntSafe(h.CPDScore));
        });
    }

    function loadYearlyTrend() {
        var url = hdnBaseUrl + "Home/LoadDashboardYearlyCompletion";
        makeAjax(url, function (res) {
            var info = (res && res.info) ? res.info : [];

            var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            var labels = [];
            var completionData = [];
            var assignmentData = [];
            var userData = [];

            var start = new Date();
            start.setMonth(start.getMonth() - 8);
            start.setDate(1);

            for (var i = 0; i < 12; i++) {
                var ref = new Date(start.getFullYear(), start.getMonth() + i, 1);
                var year = ref.getFullYear();
                var month = ref.getMonth() + 1;

                labels.push(months[month - 1] + ' ' + year);

                var match = info.find(function (x) { return x.Month === month && x.Year === year; });
                completionData.push(match ? parseIntSafe(match.CompletionCount) : 0);
                assignmentData.push(match ? parseIntSafe(match.AssignmentCount) : 0);
                userData.push(match ? parseIntSafe(match.UserCount) : 0);
            }

            var totalCompletion = completionData.reduce(function (sum, x) { return sum + x; }, 0);
            var current = completionData.length > 0 ? completionData[completionData.length - 1] : 0;
            var previous = completionData.length > 1 ? completionData[completionData.length - 2] : 0;
            var delta = current - previous;
            var trendText = 'Completions: ' + totalCompletion + ' | Last month ' + (delta >= 0 ? '+' : '') + delta;
            setText('#md-trendSummary', trendText);

            if (charts.trend) charts.trend.destroy();

            var ctx = document.getElementById('md-yearlyTrendChart');
            if (!ctx) return;

            charts.trend = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Completions',
                            data: completionData,
                            borderColor: '#2f9f63',
                            backgroundColor: 'rgba(47,159,99,0.12)',
                            borderWidth: 2,
                            tension: 0.35,
                            fill: true,
                            pointRadius: 2
                        },
                        {
                            label: 'Assignments',
                            data: assignmentData,
                            borderColor: '#0b6b69',
                            backgroundColor: 'rgba(11,107,105,0.1)',
                            borderWidth: 2,
                            tension: 0.32,
                            fill: false,
                            pointRadius: 2
                        },
                        {
                            label: 'Users Created',
                            data: userData,
                            borderColor: '#de6f4d',
                            backgroundColor: 'rgba(222,111,77,0.1)',
                            borderWidth: 2,
                            tension: 0.3,
                            fill: false,
                            pointRadius: 2
                        }
                    ]
                },
                options: {
                    maintainAspectRatio: false,
                    interaction: { mode: 'index', intersect: false },
                    scales: {
                        x: {
                            grid: { display: false }
                        },
                        y: {
                            beginAtZero: true,
                            ticks: { precision: 0 },
                            grid: { color: 'rgba(19,47,58,0.08)' }
                        }
                    },
                    plugins: {
                        legend: { position: 'top' }
                    }
                }
            });
        });
    }

    function loadCompletionSnapshot() {
        var url = hdnBaseUrl + "Home/LoadCourseCompletionData";
        makeAjax(url, function (res) {
            if (!res || !res.info) return;

            var total = parseIntSafe(res.info.TotalAssignment);
            var completed = parseIntSafe(res.info.Completed);
            var incomplete = Math.max(total - completed, 0);
            var completionPct = total > 0 ? Math.round((completed / total) * 100) : 0;

            setText('#md-completionPct', completionPct + '% complete');
            setText('#md-completedCount', completed);
            setText('#md-incompleteCount', incomplete);
            setText('#md-totalAssignedCount', total);

            if (charts.completion) charts.completion.destroy();

            var ctx = document.getElementById('md-completionDonutChart');
            if (!ctx) return;

            charts.completion = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Completed', 'Pending'],
                    datasets: [{
                        data: [completed, incomplete],
                        backgroundColor: ['#2f9f63', '#dce4e1'],
                        borderWidth: 0,
                        hoverOffset: 3
                    }]
                },
                options: {
                    maintainAspectRatio: false,
                    cutout: '72%',
                    plugins: {
                        legend: { display: true, position: 'bottom' }
                    }
                }
            });
        });
    }

    function loadCourseAssignments() {
        var url = hdnBaseUrl + "Home/GetDashboardCourseAssignments";
        makeAjax(url, function (res) {
            var info = (res && res.info) ? res.info.slice(0) : [];
            var $list = $('#md-courseLoadList');

            if (!info.length) {
                $list.html('<div class="md-muted">No assignment metrics found.</div>');
                setText('#md-courseCount', '0 courses');
                return;
            }

            info.sort(function (a, b) {
                return parseIntSafe(b.AssignmentPercentage) - parseIntSafe(a.AssignmentPercentage);
            });

            setText('#md-courseCount', info.length + ' courses');

            var top = info.slice(0, 8);
            var html = '';
            for (var i = 0; i < top.length; i++) {
                var course = top[i];
                var p = parseIntSafe(course.AssignmentPercentage);
                var color = p >= 90 ? '#cd4b4b' : (p >= 40 ? '#0b6b69' : '#2f9f63');

                html += '<div class="md-progress-row">';
                html += '  <div class="md-progress-title"><span>' + escapeHtml(course.CourseName || 'Course') + '</span><strong>' + p + '%</strong></div>';
                html += '  <div class="md-progress-track"><div class="md-progress-bar" style="width:' + p + '%; background:' + color + ';"></div></div>';
                html += '</div>';
            }

            $list.html(html);
        });
    }

    function loadQuotaByLocation() {
        var panel = document.getElementById('md-quotaPanel');
        if (!panel || panel.getAttribute('data-is-super-admin') !== '1') {
            $('#md-quotaPanel').hide();
            return;
        }

        var url = hdnBaseUrl + "Home/LoadDashboardUserQuotaPerLocation";
        makeAjax(url, function (res) {
            var info = (res && res.info) ? res.info : [];
            if (!info.length) {
                setText('#md-quotaSummary', 'No quota data');
                return;
            }

            var totalQuota = parseIntSafe(info[0].TotalQuota);
            var usedTotal = info.reduce(function (sum, x) {
                return sum + parseIntSafe(x.UsedUsers);
            }, 0);
            var remaining = Math.max(totalQuota - usedTotal, 0);

            setText('#md-quotaSummary', usedTotal + ' / ' + totalQuota + ' used');

            var labels = [];
            var values = [];
            var colors = [];

            info.forEach(function (x) {
                labels.push(x.LocationName || 'Location');
                values.push(parseIntSafe(x.UsedUsers));
                colors.push('#0b6b69');
            });

            labels.push('Remaining Quota');
            values.push(remaining);
            colors.push('#dce4e1');

            if (charts.quota) charts.quota.destroy();

            var ctx = document.getElementById('md-userQuotaChart');
            if (!ctx) return;

            charts.quota = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Users',
                        data: values,
                        backgroundColor: colors,
                        borderRadius: 6
                    }]
                },
                options: {
                    indexAxis: 'y',
                    maintainAspectRatio: false,
                    scales: {
                        x: {
                            beginAtZero: true,
                            max: totalQuota > 0 ? totalQuota : undefined,
                            ticks: { precision: 0 },
                            grid: { color: 'rgba(19,47,58,0.08)' }
                        },
                        y: {
                            ticks: { autoSkip: false }
                        }
                    },
                    plugins: {
                        legend: { display: false }
                    }
                }
            });
        });
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    return {
        init: init
    };
})();
