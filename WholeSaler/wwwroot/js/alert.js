/*$(document).ready(function () {
    fillAlerts();
});

 * recoded as viewcomponent
 * you can see called position in views/shared/loggedtopbar
 * you can see referenced component in views/shared/components/alert/default
 
function fillAlerts() {
    var fillArea = $("#alerts");
    $.ajax({
        url: '/api/Alert',
        type: "GET",
        success: function (alerts) {
            $("#alert-counter").html(alerts["alertCount"]);
            fillArea.html("");
            $.ajax({
                url: location.protocol + '//' + location.host + '/Templates/AlertTemplates/AlertTemplate.html',
                success: function (response) {
                    fillArea.html("");
                    var alertTemplate = response;
                    if (alerts["alertCount"] > 0) {
                        for (var key in alerts.elements) {
                            let alert = alerts.elements[key];
                            let compilingAlert = alertTemplate;
                            var message = "";
                            $.ajax({
                                url: location.protocol + '//' + location.host + '/api/Translate/',
                                data: { word: alert["message"] },
                                success: function (data) {
                                    message = data;
                                    compilingAlert = compilingAlert.replace("{0}", alert["redirect"]).replace("{1}", alert["date"]).replace("{2}", message);
                                    fillArea.append(compilingAlert);
                                }
                            });
                        }
                    } else {
                        $.ajax({
                            url: location.protocol + '//' + location.host + '/api/Translate/',
                            data: { word: "Here is empty!" },
                            success: function (data) {
                                fillArea.append('<hr/><div class="text-center">' + data + '</div><hr/>');
                            }
                        });
                        fillArea.append()
                    }
                }
            });
        }
    });
}*/

function clearAlerts() {
    $.ajax({
        url: '/api/Alert',
        type: "Delete",
        success: function () {
            $.ajax({
                url: location.protocol + '//' + location.host + '/api/Translate/',
                data: { word: "Here is empty!" },
                success: function (data) {
                    var fillArea = $("#alerts");
                    $("#alert-counter").html("0");
                    fillArea.html("");
                    fillArea.append('<hr/><div class="text-center">' + data + '</div><hr/>');
                }
            });
        }
    });
}