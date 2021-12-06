$(document).ready(function () {
    fillCities($("#Countries").get(0));
});


function fillCities(select) {
    var fillArea = $("#Cities");
    $.ajax({
        url: '/Home/FillCities',
        type: "GET",
        dataType: "JSON",
        data: { countryId: select.value },
        success: function (cities) {
            fillArea.html("");
            $.each(cities, function (id, value) {
                fillArea.append(
                    $('<option></option>').val(value.value).text(value.text));
            });
        }
    });
}

function showhide() {
    var divid = document.getElementById("thanks_request");
    var divs = document.getElementsByClassName("hideable");
    for (var i = 0; i < divs.length; i = i + 1) {
        $(divs[i]).fadeOut("slow");
    }
    $(divid).attr["style"] = "";
    $(divid).fadeIn("slow");
}