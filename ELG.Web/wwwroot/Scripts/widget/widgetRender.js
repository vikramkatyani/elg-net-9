//const widget_baseURL = 'http://localhost:61544';
const widget_baseURL = 'https://learning.fraud-sentinel.com';

window.addEventListener("storage", function (event) {
    if (event.key === "refreshWidgetId") {
        if (window.name !== "sourceIframe") {
            console.log(`Reloading widget in iframe: ${event.newValue}`);
            location.reload();
        }
    }
});



const widgetHandler = (function () {

    async function loadWidget(widgetId, elem, mode) {
        try {
            let learnerId = window.parent.parent.parent.GetStudentID() ?? 1;
            let url = `${widget_baseURL}/Learner/Widget/ViewWidget?widgetId=${widgetId}&learnerId=${learnerId}&mode=${mode}`;
            const response = await fetch(url);
            const widgetHtml = await response.text();
            document.getElementById(elem).innerHTML += widgetHtml;
        } catch (error) {
            console.error("Error loading widget:", error);
        }
    }

    async function saveUserFirstResponse_txt(btn) {
        try {
            btn.disabled = true;
            let widgetId = btn.id.split('_').pop();
            let responseElement = document.getElementById('txt_bfr_' + widgetId);

            if (!responseElement) {
                alert("Error: Input field not found.");
                return;
            }

            let responseText = document.getElementById('txt_bfr_' + widgetId).value.trim();

            if (!responseText) {
                alert("Response cannot be empty. Please enter a valid answer.");
                btn.disabled = false;
                return;
            }

            let learnerId = window.parent.parent.GetStudentID() ?? 1;
            const payload = {
                LearnerID: learnerId,
                QueWidgetID: widgetId,
                Response: responseText,
                ResponseFor: 'tiw-bfr'
            };

            let url = `${widget_baseURL}/Learner/Widget/SaveResponse_tiw`;
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const result = await response.json();
            console.log("Response saved successfully:", result);

            //after submiting answer successfuly
            responseElement.disabled = true;
            btn.style.display = "none";
            document.getElementById('bfr_resp_msg_' + widgetId).display = "block";

            //refresh 
            localStorage.setItem("refreshWidgetId", widgetId + "_" + Date.now());

        } catch (error) {
            console.error("Error saving response:", error);
        }
    }

    async function saveUserLastResponse_txt(btn) {
        try {
            btn.disabled = true;
            let widgetId = btn.id.split('_').pop();
            let responseElement = document.getElementById('txt_afr_' + widgetId);

            if (!responseElement) {
                alert("Error: Input field not found.");
                return;
            }

            let responseText = document.getElementById('txt_afr_' + widgetId).value.trim();

            if (!responseText) {
                alert("Response cannot be empty. Please enter a valid answer.");
                btn.disabled = false;
                return;
            }

            let learnerId = window.parent.parent.GetStudentID() ?? 1
            const payload = {
                LearnerID: learnerId,
                QueWidgetID: widgetId,
                Response: responseText,
                ResponseFor: 'tiw-afr'
            };

            let url = `${widget_baseURL}/Learner/Widget/SaveResponse_tiw`;
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const result = await response.json();
            console.log("Response saved successfully:", result);

            //after submiting answer successfuly
            responseElement.disabled = true;
            btn.style.display = "none";
            document.getElementById('bfr_resp_msg_' + widgetId).display = "block";


        } catch (error) {
            console.error("Error saving response:", error);
        }
    }


    return {
        loadWidget: loadWidget,
        saveUserFirstResponse_txt: saveUserFirstResponse_txt,
        saveUserLastResponse_txt: saveUserLastResponse_txt
    }
})();