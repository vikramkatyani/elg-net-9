//$(document).ready(function () {
//    var widgetId = '1';
//    $.ajax({
//        url: 'http://localhost:61544/Widget/GetWidget/' + widgetId,
//        method: 'GET',
//        success: function (response) {
//            $('#widget_div').html(response);
//        },
//        error: function (error) {
//            console.log('Error fetching widget:', error);
//        }
//    });
//});

let baseURL = 'http://localhost:61544/';

document.addEventListener('DOMContentLoaded', function () {
    var widgetId = '1'; // Replace with actual widget ID

    var xhr = new XMLHttpRequest();
    xhr.open('GET', baseURL+'Widget/GetWidget/' + widgetId, true);

    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            document.getElementById('widget_div').innerHTML = xhr.responseText;
        } else {
            console.error('Error fetching widget:', xhr.statusText);
        }
    };

    xhr.onerror = function () {
        console.error('Error fetching widget:', xhr.statusText);
    };

    xhr.send();
});

function saveResponse() {
    let response = document.getElementById('responseText').value;
    let widgetId = '1';// document.getElementById('widgetId').value;
    var xhr = new XMLHttpRequest();
    xhr.open('POST', baseURL +'Widget/SaveResponse', true);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            alert('Response saved: ' + response);
        } else {
            console.error('Error saving response:', xhr.statusText);
        }
    };
    xhr.onerror = function () {
        console.error('Error saving response:', xhr.statusText);
    };
    var data = JSON.stringify({ WidgetId: widgetId, Response: response });
    xhr.send(data);
}

function viewResponse() {
    var widgetId = '1'; // Replace with actual widget ID

    var xhr = new XMLHttpRequest();
    xhr.open('GET', baseURL +'Widget/GetWidgetResponse/' + widgetId, true);

    xhr.onload = function () {
        if (xhr.status >= 200 && xhr.status < 300) {
            document.getElementById('responseText').value = xhr.responseText;
        } else {
            console.error('Error fetching widget:', xhr.statusText);
        }
    };

    xhr.onerror = function () {
        console.error('Error fetching widget:', xhr.statusText);
    };

    xhr.send();
}
