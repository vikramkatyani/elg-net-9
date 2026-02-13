
let moduleWin = null,
    moduleInterval = null, isSessionAlive = true;

const trackingURL = typeof scormTrackingUrl !== 'undefined' ? scormTrackingUrl : (hdnBaseUrl + "SCORM/");

// Expose API globally for SCORM courses to find
window.API = {
    values: [],
    pollTimeout: null,
    LMSInitialize: function (parameter) {
        debug("LMSInitialize (" + parameter + ")");
        const me = this;
        const url = trackingURL + "LMSInitialize";
        const data = {
            CourseId: this.module
        }

        UTILS.makeAsyncAjaxCall(url, data, function (resp) {
            //success
            me.values["cmi.core.lesson_status"] = resp.progress.ProgressStatus
            me.values["cmi.core.student_name"] = resp.progress.UserFistName + ' ' + resp.progress.UserLastName;
            me.values["cmi.core.student_id"] = resp.progress.UserId;
            me.values["cmi.core.module_id"] = resp.progress.CourseId;
            me.values["cmi.suspend_data"] = resp.progress.SuspendData;
            me.values["cmi.core.score.raw"] = resp.progress.Score;
            me.values["cmi.core.lesson_location"] = resp.progress.Bookmark;
            //me.values["cmi.core.passing_score"] = resp.progress.PassingScore;
        }, function (err) {
            //error
            console.log(err);
        })
        return "true";
    },
    LMSGetValue: function (parameter) {
        debug("LMSGetValue (" + parameter + ") = " + this.values[parameter]);

        const result = this.values[parameter];

        return result === null ? "" : result;
    },
    //LMSSetValue: function (parameter, value) {
    //    debug("LMSSetValue (" + parameter + "," + value + ")");
    //    var me = this;
    //    var success = false;
    //    if (value == null) {
    //        value = "";
    //    }
    //    this.values[parameter] = value;

    //    var saveurl = trackingURL + "LMSSaveData";
    //    var savedata = {
    //        CourseId: this.module,
    //        Parameter: parameter,
    //        Value: value
    //    }

    //    if (!parameter.includes("cmi.interactions.")) {
    //        UTILS.makeAsyncAjaxCall(saveurl, savedata, function (e) {
    //            if (e.success == "-1") {
    //                isSessionAlive = false;
    //                if (moduleWin)
    //                    moduleWin.close();
    //            }

    //            success = "true";
    //        }, function (err) {
    //            success = "false";
    //        })
    //        return success

    //        //this.SaveData(parameter, value, function (result, status, jqxr) {
    //        //    debugger;
    //        //    success  = result.d;
    //        //});

    //        //return success ? "true" : "false";
    //    }

    //},
    LMSSetValue: function (parameter, value) {
        debug("LMSSetValue (" + parameter + "," + value + ")");
        const me = this;
        if (value === null) {
            value = "";
        }
        me.values[parameter] = value;
        return "true";
    },
    LMSCommit: function (parameter) {
        debug("LMSCommit (" + parameter + ")");
        const me = this;
        let success = "true";
        const saveurl = trackingURL + "LMSSaveData";
        const savedata = {
            SuspendData: me.values["cmi.suspend_data"],
            Bookmark: me.values["cmi.core.lesson_location"],
            ProgressStatus: me.values["cmi.core.lesson_status"],
            Score: me.values["cmi.core.score.raw"],
            SessionTime: me.values["cmi.core.session_time"],
        }

        UTILS.makeAjaxCall(saveurl, savedata, function (e) {
            if (e.success === "-1") {
                isSessionAlive = false;
                if (moduleWin){
                    moduleWin.close();
                    //moduleWin.document.write(
                    //"<script>alert('Session Lost!LMS Connection lost. Your progress will not be saved.\nPlease close the window and login again.')<\/script>"
                    //);
                }
            }

            success = "true";
        }, function (err) {
            success = "false";
        })
        return success;
    },

    LMSFinish: function (parameter) {
        debug("LMSFinish  (" + parameter + ")");
        const saveurl = trackingURL + "LMSFinish";
        const savedata = {
            CourseId: this.module,
            Parameter: parameter,
            Value: ""
        }
        UTILS.makeAsyncAjaxCall(saveurl, savedata, function (e) {

        }, function (err) {

        })

        if (moduleWin)
            moduleWin.close();
        return "true";
    },
    LMSGetLastError: function () {
        debug("LMSGetLastError()");

        return 0;
    },

    LMSGetErrorString: function (errornumber) {
        debug("LMSGetErrorString (" + errornumber + ")");

        return "";
    },

    LMSGetDiagnostic: function (parameter) {
        debug("LMSGetDiagnostic (" + parameter + ")");

        return "";
    },

    Submit: function (params, callBack, async) {
        const url = trackingURL + "LMSInitialize";
        const data = {
            CourseId: this.module
        }

        UTILS.makeAsyncAjaxCall(url, data, function (e) {
            //success
        }, function (err) {
            //error
        })

        //$.ajax({
        //    type: "POST",
        //    url: "mycourses.aspx/LMSInitialize",
        //    data: '{pram: "' + this.module + '" }',
        //    contentType: "application/json; charset=utf-8",
        //    dataType: "json",
        //    async: async,
        //    success: callBack,
        //    error: function (xhr, ajaxOptions, thrownError) {
        //        //createLog('LMSInitialize-Err', xhr.status)
        //    }
        //});
    },

    SaveData: function (params, value, callBack, async) {

        const url = trackingURL + "LMSSaveData";
        const savedata = {
            CourseId: this.module,
            Parameter: params,
            Value: value
        }

        UTILS.makeAsyncAjaxCall(url, savedata, function (e) {
            //success
        }, function (err) {
            //error
        })

        //$.ajax({
        //    type: "POST",
        //    url: "mycourses.aspx/LMSSaveData",
        //    data: JSON.stringify(savedata),
        //    contentType: "application/json; charset=utf-8",
        //    dataType: "json",
        //    async: async,
        //    success: callBack,
        //    error: function (xhr, ajaxOptions, thrownError) {
        //        //createLog('LMSInitialize-Err', xhr.status)
        //    }
        //});
    }
}

// Create shorthand reference for internal use
const API = window.API;

// Bridge: handle SCORM requests from cross-origin course windows via postMessage
window.addEventListener('message', function (event) {
    try {
        const data = event.data || {};
        if (!data || data.type !== 'SCORM_REQUEST' || !data.method) return;

        const srcWin = event.source;
        const requestId = data.requestId;
        let result = null;

        switch (data.method) {
            case 'LMSInitialize':
                result = API.LMSInitialize("");
                // Return initial values so content cache can seed
                srcWin && srcWin.postMessage({ type: 'SCORM_RESPONSE', requestId, result, values: API.values || {} }, '*');
                return;
            case 'LMSGetValue':
                result = API.LMSGetValue(data.args && data.args[0]);
                break;
            case 'LMSSetValue':
                result = API.LMSSetValue(data.args && data.args[0], data.args && data.args[1]);
                break;
            case 'LMSCommit':
                result = API.LMSCommit("");
                break;
            case 'LMSFinish':
                result = API.LMSFinish("");
                break;
            default:
                result = null;
        }

        srcWin && srcWin.postMessage({ type: 'SCORM_RESPONSE', requestId, result }, '*');
    } catch (e) {
        console.warn('SCORM bridge error:', e);
    }
});

function Init() {
    debug("init");
    //API.Poll();

    moduleInterval = setInterval(function () {
        if (moduleWin && moduleWin.closed) {
            moduleWin = null;

            if (isSessionAlive === false) {
                const sessionModal = document.querySelector("#sessionExpiredModal");
                if (sessionModal) new bootstrap.Modal(sessionModal).show();
            }
            else {
                API.LMSFinish('finish');
                location.reload();
            }                
        }
    }, 250);
}


function debug(s) {
    //alert (s);
    console.log(s);
}

function getStartPage(url) {
    const xmlFile = "../courses/scorm/" + url + "/imsmanifest.xml";
    let xmlDoc;
    let startPath = "";

    if (typeof window.DOMParser !== "undefined") {
        const xmlhttp = new XMLHttpRequest();
        xmlhttp.open("GET", xmlFile, false);
        if (xmlhttp.overrideMimeType) {
            xmlhttp.overrideMimeType('text/xml');
        }
        xmlhttp.send();
        xmlDoc = xmlhttp.responseXML;
    }
    else {
        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.load(xmlFile);
    }
    const tagObj = xmlDoc.getElementsByTagName("resources");
    for (let i = 0; i < tagObj.length; i++) {
        const resTag = tagObj[i].getElementsByTagName('resource');
        for (let j = 0; j < resTag.length; j++) {
            if (resTag[j].getAttribute("adlcp:scormtype") === "sco") {
                startPath = resTag[j].getAttribute("href")
                return startPath;
            }
        }
    }
    return startPath;
}


function launchModulePopUp(courseid, url) {
    let startPath = "";
    API.module = parseInt(courseid);

    // Open a placeholder window synchronously to avoid Safari popup blocking
    // (must be directly in the user-initiated click handler call stack)
    let popup = null;
    try {
        popup = window.open('', '_blank');
    } catch (e) {
        popup = null;
    }
    moduleWin = popup;
    
    // Pre-initialize API data before opening window
    const initUrl = trackingURL + "LMSInitialize";
    const initData = {
        CourseId: API.module
    };

    UTILS.makeAsyncAjaxCall(initUrl, initData, function (resp) {
        // Pre-populate values so they're ready when course checks
        API.values["cmi.core.lesson_status"] = resp.progress.ProgressStatus;
        API.values["cmi.core.student_name"] = resp.progress.UserFistName + ' ' + resp.progress.UserLastName;
        API.values["cmi.core.student_id"] = resp.progress.UserId;
        API.values["cmi.core.module_id"] = resp.progress.CourseId;
        API.values["cmi.suspend_data"] = resp.progress.SuspendData;
        API.values["cmi.core.score.raw"] = resp.progress.Score;
        API.values["cmi.core.lesson_location"] = resp.progress.Bookmark;
        
        // Check if URL is absolute (starts with http/https) or relative
        let courseUrl = url;
        if (!url.startsWith('http://') && !url.startsWith('https://')) {
            courseUrl = "../courses/" + url;
        }

        // Ensure SCORM shim exists at course root before opening (fixes cross-origin API + missing scorm_api.js)
        const ensureUrl = hdnBaseUrl + "EnsureScormShim";
        UTILS.makeAsyncAjaxCall(ensureUrl, { url: courseUrl }, function () {
            // Open via LMS proxy so relative resource requests (e.g., scorm_api.js) don't need SAS tokens
            try {
                // hdnBaseUrl typically ends with "/Learner/Home/"; trim "Home/" to get area base
                const learnerBase = hdnBaseUrl.replace(/Home\/?$/i, "");
                const proxyBase = learnerBase + "CourseProxy/" + API.module + "/index.html?baseUrl=" + encodeURIComponent(courseUrl);
                if (moduleWin && !moduleWin.closed) {
                    moduleWin.location.href = proxyBase;
                } else {
                    window.location.href = proxyBase;
                }
            } catch (e) {
                // Fallback to direct course URL if proxy construction fails
                if (moduleWin && !moduleWin.closed) {
                    moduleWin.location.href = courseUrl;
                } else {
                    window.location.href = courseUrl;
                }
            }
        }, function () {
            // Even if shim ensure fails, attempt to open via proxy
            try {
                const learnerBase = hdnBaseUrl.replace(/Home\/?$/i, "");
                const proxyBase = learnerBase + "CourseProxy/" + API.module + "/index.html?baseUrl=" + encodeURIComponent(courseUrl);
                if (moduleWin && !moduleWin.closed) {
                    moduleWin.location.href = proxyBase;
                } else {
                    window.location.href = proxyBase;
                }
            } catch (e) {
                if (moduleWin && !moduleWin.closed) {
                    moduleWin.location.href = courseUrl;
                } else {
                    window.location.href = courseUrl;
                }
            }
        });
    }, function (err) {
        console.error("Failed to initialize course data:", err);
        // Still open window even if initialization fails
        let courseUrl = url;
        if (!url.startsWith('http://') && !url.startsWith('https://')) {
            courseUrl = "../courses/" + url;
        }
        if (moduleWin && !moduleWin.closed) {
            moduleWin.location.href = courseUrl;
        } else {
            window.location.href = courseUrl;
        }
    });
}


window.onbeforeunload = function (e) {
    if (moduleWin)
        return "This will close the active module!";
}


window.onunload = function () {
    clearInterval(moduleInterval);

    if (moduleWin)
        moduleWin.close();
}

function debug(s) {
    //alert (s);
    console.log(s);
}



Init();

