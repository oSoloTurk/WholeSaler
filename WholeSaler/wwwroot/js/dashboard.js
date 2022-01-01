function fillChart(chart, labels, series) {
    new Chartist.Line(chart, {
        labels: labels,
        series: series,

    }, {
        fullWidth: true,
        chartPadding: {
            right: 40
        },
        plugins: [
            Chartist.plugins.tooltip({
                appendToBody: false,
            })
            ]
    });

}

$(document).ready(function () {
    var labels, series;
    $.ajax({
        url: document.URL.match("Admin") != null ? 'AdminDashboardChart' : 'CustomerDashboardChart',
        type: 'GET',
        success: function (data) {
            labels = data["labels"];
            series = data["series"];
            fillChart("#ct-chart", labels, series);
        }
    });
});