var UTILS = {};

const menu_parents = document.querySelectorAll('.parent-menu-a');

menu_parents.forEach(parent => {
    parent.addEventListener('click', function (e) {
        e.preventDefault(); // Prevent default link behavior
        this.parentElement.classList.toggle('open'); // Toggle the 'open' class on the parent
    });
});
//function toggleMenu() {
//    const sidebar = document.querySelector('.learner-menu-bar');
//    const content = document.querySelector('.learner-main-content');

//    sidebar.classList.toggle('open'); // Show or hide sidebar
//    //content.classList.toggle('dimmed'); // Dim or restore content
//}
function toggleMenu() {
    const menu = document.querySelector(".learner-menu-bar");
    const hamburger = document.querySelector(".hamburger");

    menu.classList.toggle("open");
    hamburger.classList.toggle("open");
}

UTILS.activateNavigationLink = function (link) {
    var $leftMenu = $('#learner-menu')
    $leftMenu.find('.menu-link-container').each(function () {
        $(this).removeClass('show');
    });
    $leftMenu.find('li.nav-item').each(function () {
        $(this).removeClass('active');
        $(this).removeClass('open');
    });
    $('#' + link).addClass('active open');
    //$('#' + link).children('a.nav-link.collapsed').removeClass('collapsed');

}

UTILS.activateMenuNavigationLink = function (link) {
    var $leftMenu = $('#learner-menu')
    $leftMenu.find('a.collapse-item').each(function () {
        $(this).removeClass('active');
        $(this).removeClass('open');
    });
    $('#' + link).addClass('active open');
    //$('#' + link).parent().parent().addClass('show');

}

 UTILS.disableButton = function($btn) {
    var loadingText = $btn.data("loading-text")
    $btn.attr('disabled', 'disabled');
     $btn.html("<i class='fa fa-spinner fa-spin'></i> "+loadingText);
}

UTILS.resetIconButton = function ($btn, $icon) {
    var originalText = $btn.data("original-text")
    var icon = $btn.data("button-icon");
    $btn.removeAttr('disabled');
    $btn.html('<i class="fa fa-fw fa-' + icon + '"></i><span>' + originalText+'</span>');
}

UTILS.resetButton = function ($btn) {
    var originalText = $btn.data("original-text")
    $btn.removeAttr('disabled');
    $btn.html(originalText);
}

UTILS.makeAjaxCall = function (url, data, successcallback, errorcallback) {
    $.ajax({
        type: "POST",
        url: url,
        data: JSON.stringify(data),
        contentType: "application/json",
        success: function (data, text) {
            if (typeof successcallback == "function") {
                successcallback(data);
            };
        },
        error: function (request, status, error) {
            if (typeof errorcallback == "function") {
                errorcallback(request);
            };
        }
    });
};

UTILS.makeAsyncAjaxCall = function (url, data, successcallback, errorcallback) {
    $.ajax({
        type: "POST",
        url: url,
        data: JSON.stringify(data),
        contentType: "application/json",
        async: false,
        success: function (data, text) {
            if (typeof successcallback == "function") {
                successcallback(data);
            };
        },
        error: function (request, status, error) {
            if (typeof errorcallback == "function") {
                errorcallback(request);
            };
        }
    });
};

UTILS.Alert = {
    show: function ($ref, type, message) {
        // Support both jQuery objects and plain DOM elements
        var isJQ = $ref && (typeof $ref.removeClass === 'function') && (typeof $ref.addClass === 'function');
        if (isJQ) {
            $ref.removeClass();
            $ref.addClass("show");
            $ref.html(message);
        } else if ($ref) {
            $ref.className = '';
            $ref.classList.add('show');
            $ref.innerHTML = message || '';
        }

        var addClasses = function (classes) {
            if (isJQ) {
                $ref.addClass(classes);
            } else if ($ref) {
                classes.split(' ').forEach(function (c) { if (c) $ref.classList.add(c); });
            }
        };

        switch (type) {
            case "error":
                addClasses("alert alert-danger");
                break;
            case "success":
                addClasses("alert alert-success");
                break;
            case "info":
                addClasses("alert alert-info");
                break;
            case "warning":
                addClasses("alert alert-warning");
                break;
        }
    },
    hide: function ($ref) {
        // Support both jQuery objects and plain DOM elements
        var isJQ = $ref && (typeof $ref.removeAttr === 'function') && (typeof $ref.addClass === 'function');
        if (isJQ) {
            $ref.removeAttr('class');
            $ref.addClass("hide");
            $ref.html("");
        } else if ($ref) {
            $ref.className = '';
            $ref.classList.add('hide');
            $ref.innerHTML = '';
        }
    },
    showFloatingAlert: function ($ref, type, message) {
        var messageAlertTimeOut;
        var isJQ = $ref && (typeof $ref.removeClass === 'function');
        if (isJQ) {
            $ref.removeClass();
            $ref.empty();
        } else if ($ref) {
            $ref.className = '';
            $ref.innerHTML = '';
        }
        
        var classes = '';
        switch (type) {
            case "error":
                classes = "alert alert-error-dark floating-alert";
                break;
            case "success":
                classes = "alert alert-success-dark floating-alert";
                break;
            case "info":
                classes = "alert alert-info-dark floating-alert";
                break;
            case "warning":
                classes = "alert alert-warning";
                break;
        }
        
        if (isJQ) {
            $ref.addClass(classes);
            $ref.append(message);
            $ref.fadeIn();
        } else if ($ref) {
            $ref.className = classes;
            $ref.innerHTML = message || '';
            $ref.style.display = '';
        }
        
        messageAlertTimeOut = setInterval(function () { hideAlert($ref); }, 3000);

        function hideAlert($ref) {
            if (isJQ) {
                $ref.fadeOut();
            } else if ($ref) {
                $ref.style.display = 'none';
            }
        };
    }
};

UTILS.data = {
    getAllLocations: function (callback) {
        var data = { };
        UTILS.makeAsyncAjaxCall(hdnBaseUrl +"common/LocationList", data, callback);
    },
    getCompanyDepartments: function (callback) {
        var data = {};
        UTILS.makeAsyncAjaxCall(hdnBaseUrl + "common/CompanyDepartmentList", data, callback);
    },
    getAllDepartments: function (locationId, callback) {
        var data = { location: locationId };
        UTILS.makeAsyncAjaxCall(hdnBaseUrl +"common/DepartmentList", data, callback);
    },
    getAllCourses: function (callback) {
        var data = { };
        UTILS.makeAsyncAjaxCall(hdnBaseUrl+"common/CourseList", data, callback);
    },
    getAllSubModules: function (courseID, callback) {
        var data = { courseID: courseID };
        UTILS.makeAsyncAjaxCall(hdnBaseUrl +"common/SubModuleList", data, callback);
    },
    getAllWidgetCourses: function (callback) {
        var data = { };
        UTILS.makeAsyncAjaxCall(hdnBaseUrl +"common/WidgetCourseList", data, callback);
    },
    getAllRiskAssessments: function (callback) {
        var data = {};
        UTILS.makeAsyncAjaxCall(hdnBaseUrl + "common/RiskAssessmentList", data, callback);
    },
    getAllClassrooms: function (callback) {
        var data = {};
        UTILS.makeAsyncAjaxCall(hdnBaseUrl + "common/ClassroomList", data, callback);
    },
    getAllDocumentCategory: function (callback) {
        var data = {};
        UTILS.makeAsyncAjaxCall("/common/DocumentCategoryList", data, callback);
    },
    getAllDocumentSubCategoryForCategory: function (categoryId, callback) {
        var data = { CategoryID: categoryId };
        UTILS.makeAsyncAjaxCall("/common/GetSubCategoriesForCategory", data, callback);
    },
    getAllDocuments: function (callback) {
        var data = {};
        UTILS.makeAsyncAjaxCall(hdnBaseUrl + "common/DocumentList", data, callback);
    },
    getAllRAGroups: function (callback) {
        var data = {};
        UTILS.makeAsyncAjaxCall(hdnBaseUrl + "common/RAGroupList", data, callback);
    }
};

UTILS.getImageFromBuffer = function(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

UTILS.LOADER = '<div id="loader"><div class="spinner-border" style="width:3rem; height:3rem;" role="status"> <span class="sr-only"> Loading...</span ></div></div>';
UTILS.NORECORD = '<div class="row col-md-12 alert alert-info">No record(s) found</div>';