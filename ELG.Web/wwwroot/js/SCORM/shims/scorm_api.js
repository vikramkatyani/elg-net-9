(function(){
    // Simple SCORM 1.2 shim that talks to opener via postMessage
    var _cache = {};
    var _pending = {};
    var _reqId = 0;

    function nextId(){ return (++_reqId) + '_' + Date.now(); }
    function hasOpener(){ try { return !!window.opener && !window.opener.closed; } catch(e){ return false; } }
    function post(method, args){
        if(!hasOpener()) return null;
        var id = nextId();
        try {
            window.opener.postMessage({ type: 'SCORM_REQUEST', method: method, args: args || [], requestId: id }, '*');
        } catch(e) { /* ignore */ }
        return id;
    }

    window.addEventListener('message', function(event){
        var data = event.data || {};
        if(data.type !== 'SCORM_RESPONSE' || !data.requestId) return;
        var id = data.requestId;
        if(_pending[id]){
            try { _pending[id](data); } catch(e){}
            delete _pending[id];
        }
        if(data.values && typeof data.values === 'object'){
            for(var k in data.values){ _cache[k] = data.values[k]; }
        }
    });

    // API adapter (SCORM 1.2)
    var API = {
        LMSInitialize: function(param){
            var id = post('LMSInitialize', [param||""]);
            if(id){ _pending[id] = function(resp){ if(resp && resp.values){ for(var k in resp.values){ _cache[k] = resp.values[k]; } } }; }
            return "true"; // optimistic
        },
        LMSGetValue: function(element){
            // return from local cache synchronously (typical usage)
            var v = _cache[element];
            return (v === undefined || v === null) ? "" : v;
        },
        LMSSetValue: function(element, value){
            _cache[element] = (value == null ? "" : value);
            post('LMSSetValue', [element, value]);
            return "true";
        },
        LMSCommit: function(param){
            post('LMSCommit', [param||""]);
            return "true";
        },
        LMSFinish: function(param){
            post('LMSFinish', [param||""]);
            return "true";
        },
        LMSGetLastError: function(){ return 0; },
        LMSGetErrorString: function(){ return ""; },
        LMSGetDiagnostic: function(){ return ""; }
    };

    // Pipwerks-like facade expected by many packages
    var SCORM = {
        initialized: false,
        init: function(){ var r = API.LMSInitialize(""); this.initialized = (r === "true"); return this.initialized; },
        get: function(p){ return API.LMSGetValue(p); },
        set: function(p, v){ return (API.LMSSetValue(p, v) === "true"); },
        save: function(){ return (API.LMSCommit("") === "true"); },
        quit: function(){ return (API.LMSFinish("") === "true"); }
    };

    // Expose globals expected by SCORM content
    window.API = window.API || API;
    window.SCORM = window.SCORM || SCORM;
})();
