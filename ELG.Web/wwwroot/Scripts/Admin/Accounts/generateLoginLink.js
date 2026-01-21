let loading = false;

const generateLoginLinkHandler = (() => {
    const form = document.querySelector('#adminForgetpwd');
    const emailInput = document.querySelector('#Email');
    const waitMessage = document.querySelector('#wait-message');
    const submitButton = document.querySelector('#btnGenerateLink');
    waitMessage.style.display = 'none';

    /**
     * Display a wait message with countdown timer for 5 minutes
     */
    function showWaitMessage() {
        // Hide link
        submitButton.style.display = 'none';
        waitMessage.style.display = 'block';

        // Start timer for 5 minutes (300 seconds)
        let timeLeft = 300;
        const timerInterval = setInterval(() => {
            timeLeft--;
            const minutes = Math.floor(timeLeft / 60);
            const seconds = timeLeft % 60 < 10 ? `0${timeLeft % 60}` : timeLeft % 60;
            waitMessage.innerHTML = `Email Sent! Please wait for <b>(${minutes}:${seconds}) </b>before sending login link again`;

            if (timeLeft === 0) {
                clearInterval(timerInterval);
                waitMessage.style.display = 'none';
                submitButton.style.display = 'block';
            }
        }, 1000);
    }

    // Handle submit button click
    submitButton.addEventListener('click', (event) => {
        event.preventDefault();
        const btn = submitButton;

        if (!loading) {
            disableButton(btn);
            if (form.checkValidity()) {
                const url = window.hdnBaseUrl ? window.hdnBaseUrl + 'Account/GenerateLoginLink' : 'Account/GenerateLoginLink';
                const data = {
                    Email: emailInput.value
                };

                fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(data)
                })
                .then(response => response.json())
                .then(response => {
                    if (response.Err === 0 || response.err === 0) {
                        showMessage(response.Message || response.message);
                        showWaitMessage();
                    } else {
                        showError(response.Message || response.message);
                    }
                    resetButton(btn);
                })
                .catch(error => {
                    resetButton(btn);
                    showError(error.message);
                });
            } else {
                showError('Please enter valid email.');
                resetButton(btn);
            }
        }
    });
})();


/**
 * Display an error message in the alert box
 * @param {string} message - The error message to display
 */
function showError(message) {
    const alertBox = document.querySelector('#divErrorMessage');
    alertBox.className = 'alert alert-danger';
    alertBox.textContent = message;
    alertBox.style.display = 'block';
}

/**
 * Display a success message in the alert box
 * @param {string} message - The success message to display
 */
function showMessage(message) {
    const alertBox = document.querySelector('#divErrorMessage');
    alertBox.className = 'alert alert-success';
    alertBox.textContent = message;
    alertBox.style.display = 'block';
}

/**
 * Disable button and show loading state
 */
function disableButton(btn) {
    loading = true;
    btn.disabled = true;
    btn.classList.add('disabled');
    btn.textContent = btn.getAttribute('data-loading-text') || 'Loading...';
}

/**
 * Enable button and restore original state
 */
function resetButton(btn) {
    loading = false;
    btn.disabled = false;
    btn.classList.remove('disabled');
    btn.textContent = btn.getAttribute('data-original-text') || 'Generate Login Link';
}
