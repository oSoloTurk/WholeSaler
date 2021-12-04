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
                    $('<option></option>').val(id).text(value.text));
            });
        }
    });
}