// const widget_baseURL = 'http://localhost:61544';
const widget_baseURL = 'https://learning.fraud-sentinel.com';

const macWidgetHandler = (function () {

    async function loadWidget(widgetId) {
        try {
            let elem = `${widgetId}_mac`;
            let learnerId = window.parent.parent.GetStudentID() ?? 1;
            let url = `${widget_baseURL}/Learner/Widget/ViewWidget_MAC?widgetId=${widgetId}&learnerId=${learnerId}`;
            const response = await fetch(url);
            const widgetHtml = await response.text();
            document.getElementById(elem).innerHTML += widgetHtml;
        } catch (error) {
            console.error("Error loading widget:", error);
        }
    }

    async function saveUserResponses(btn) {
        try {
            btn.disabled = true;
            btn.classList.remove("active");
            btn.classList.add("disabled");
            let widgetId = btn.id.split('_').pop();
            let responseElement1 = document.getElementById('txt_resp_1_' + widgetId);
            let responseElement2 = document.getElementById('txt_resp_2_' + widgetId);
            let responseElement3 = document.getElementById('txt_resp_3_' + widgetId);
            let showModelRespBtn = document.getElementById('btn_show_ma_' + widgetId);

            let responseText1 = responseElement1.value.trim();
            let responseText2 = responseElement2.value.trim();
            let responseText3 = responseElement3.value.trim();

            if (!responseText1 || !responseText2 || !responseText3) {
                alert("Response cannot be empty. Please enter a valid answer.");
                btn.disabled = false;
                return;
            }

            let learnerId = window.parent.parent.GetStudentID() ?? 1;
            const payload = {
                LearnerID: learnerId,
                QueWidgetID: widgetId,
                Response_1: responseText1,
                Response_2: responseText2,
                Response_3: responseText3
            };

            let url = `${widget_baseURL}/Learner/Widget/SaveResponse_mac`;
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

            responseElement1.disabled = true;
            responseElement2.disabled = true;
            responseElement3.disabled = true;
            btn.style.display = "none";
            showModelRespBtn.style.display = "block";

        } catch (error) {
            console.error("Error saving response:", error);
        }
    }

    function checkInputs(widgetId) {
        const textAreas = document.querySelectorAll(`.elg-wdgt-ans-input`);
        const submitButton = document.getElementById(`btn_sbmt_${widgetId}`);

        let allFilled = Array.from(textAreas).every(textArea => textArea.value.trim() !== "");

        if (allFilled) {
            submitButton.classList.remove("disabled");
            submitButton.classList.add("active");
            submitButton.disabled = false;
            submitButton.addEventListener('click', macWidgetHandler.saveUserResponses(this));

        } else {
            submitButton.classList.remove("active");
            submitButton.classList.add("disabled");
            submitButton.disabled = true;
            submitButton.removeEventListener('click', macWidgetHandler.saveUserResponses(this));
        }
    }

    async function saveFeedback(btn) {
        try {
            btn.disabled = true;
            btn.classList.remove("active");
            btn.classList.add("disabled");
            let widgetId = btn.id.split('_').pop();
            let rb = document.querySelectorAll("input[name='feedback_radioGroup']");
            let feedbackElem = document.getElementById('txt_feedback_' + widgetId);
            let feedbackText = feedbackElem.value.trim();
            let feedback = document.querySelector("input[name='feedback_radioGroup']:checked")?.value.trim() || null;

            if (!feedbackText || !feedback) {
                alert("Feedback cannot be empty. Please provide valid feedback.");
                btn.disabled = false;
                return;
            }

            let learnerId = window.parent.parent.GetStudentID() ?? 1;
            const payload = {
                LearnerID: learnerId,
                QueWidgetID: widgetId,
                FeedBackResponse: feedback,
                FeedBackResponseText: feedbackText
            };

            let url = `${widget_baseURL}/Learner/Widget/SaveFeedback_mac`;
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

            feedbackElem.disabled = true;
            rb.forEach(radio => {
                radio.disabled = true;
            });

        } catch (error) {
            console.error("Error saving response:", error);
        }
    }
    function validateFeedback(widgetId) {
        let radioButtons = document.querySelectorAll("input[name='feedback_radioGroup']");
        let textArea = document.querySelector(".elg-wdgt-resp-input");
        let submitButton = document.getElementById(`btn_submit_ma_${widgetId}`);
        let radioSelected = Array.from(radioButtons).some(radio => radio.checked);
        let textEntered = textArea.value.trim().length > 0;

        if (radioSelected && textEntered) {
            submitButton.classList.remove("disabled");
            submitButton.classList.add("active");
            submitButton.disabled = false;

        } else {
            submitButton.classList.remove("active");
            submitButton.classList.add("disabled");
            submitButton.disabled = true;
        }
    }
    function showModelAnswer(btn) {
        let widgetId = btn.id.split('_').pop();
        let modelAnswerContainer = document.getElementById(`model-response-holder_${widgetId}`);
        modelAnswerContainer.style.display = 'block';
        document.querySelectorAll(".elg-wdgt-mashow-button").forEach(el => {
            el.style.display = "none";
        });
        //document.getElementById(`btn_hide_ma_${widgetId}`).style.display = "block";
        document.getElementById(`btn_submit_ma_${widgetId}`).style.display = "block";
    }
    function hideModelAnswer(btn) {
        let widgetId = btn.id.split('_').pop();
        let modelAnswerContainer = document.getElementById(`model-response-holder_${widgetId}`);
        modelAnswerContainer.style.display = 'none';
        document.querySelectorAll(".elg-wdgt-mashow-button").forEach(el => {
            el.style.display = "none";
        });
        document.getElementById(`btn_show_ma_${widgetId}`).style.display = "block";

    } 

    return {
        loadWidget: loadWidget,
        saveUserResponses: saveUserResponses,
        checkInputs: checkInputs,
        showModelAnswer: showModelAnswer,
        hideModelAnswer: hideModelAnswer,
        validateFeedback: validateFeedback,
        saveFeedback: saveFeedback
    };
})();