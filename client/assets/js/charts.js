/*!
charts.js
(c) 2017 IG PROG, www.igprog.hr
*/
angular.module('charts', [])

.factory('charts', [function () {
    return {
        'createGraph': function (s, d, l, c, dso, legend) {
            return {
                series: s,
                data: d,
                labels: l,
                colors: c,
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    legend: {
                        display: legend
                    },
                    scales: {
                        xAxes: [{
                            display: true,
                            scaleLabel: {
                                display: true,
                            },
                            ticks: {
                                beginAtZero: true,
                                stepSize: 10
                            }
                        }],
                        yAxes: [{
                            display: true,
                            scaleLabel: {
                                display: true,
                            },
                            ticks: {
                                beginAtZero: true,
                                stepSize: 10
                            }
                        }]
                    }
                },
                datasetOverride: dso
            }
        },
        'createGuageChart': function (recommendedEnergy, energy) {
            var data = google.visualization.arrayToDataTable([
                  ['Label', 'Value'],
                  ['kcal', 80]
            ]);

            var maxValue = recommendedEnergy + (recommendedEnergy * 0.2);

            var options = {
                title: "Energetska vrijednost",
                min: 0, max: maxValue.toFixed(0),
                greenFrom: recommendedEnergy - (recommendedEnergy * 0.02),
                greenTo: recommendedEnergy + (recommendedEnergy * 0.02),
                yellowFrom: recommendedEnergy + (recommendedEnergy * 0.02),
                yellowTo: recommendedEnergy + (recommendedEnergy * 0.04),
                redFrom: recommendedEnergy + (recommendedEnergy * 0.04),
                redTo: maxValue,
                minorTicks: 5
            };
            data.setValue(0, 1, energy);
            var chart = new google.visualization.Gauge(document.getElementById('energyChart'));

            chart.draw(data, options);

            var chart1 = new google.visualization.Gauge(document.getElementById('energyChart1'));
            chart1.draw(data, options);
            var chart2 = new google.visualization.Gauge(document.getElementById('energyChart2'));
            chart2.draw(data, options);
            var chart3 = new google.visualization.Gauge(document.getElementById('energyChart3'));
            chart3.draw(data, options);

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
