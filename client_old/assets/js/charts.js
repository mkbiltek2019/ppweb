/*!
charts.js
(c) 2019 IG PROG, www.igprog.hr
*/
angular.module('charts', [])

.factory('charts', [function () {
    return {
        'createGraph': function (series, data, labels, colors, options, datasetOverride) {
            return {
                series: series,
                data: data,
                labels: labels,
                colors: colors,
                options: options,
                datasetOverride: datasetOverride
            }
        },
        'guageChart': function (id, value, unit, options) {
            var data = google.visualization.arrayToDataTable([
                  ['Label', 'Value'],
                  [unit, 80]
            ]);
            data.setValue(0, 1, value);
            var chart = new google.visualization.Gauge(document.getElementById(id));
            chart.draw(data, options);
        }
    }
}]);

;
