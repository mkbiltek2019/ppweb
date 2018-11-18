/*!
bmi.js
(c) 2018 IG PROG, www.igprog.hr
*/
angular.module('app', ['charts'])

.config(['$httpProvider', function ($httpProvider) {
    //--------------disable catche---------------------
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //-------------------------------------------------
}])

.controller('bmiCtrl', ['$scope', '$timeout', 'charts', function ($scope, $timeout, charts) {
    $scope.d = {
        height: null,
        weight: null,
        bmi: null,
        description: '',
        css: '',
        calculate: function () {
            this.bmi = (this.weight * 10000 / (this.height * this.height)).toFixed(1);
            this.description = getBmiTitle(this.bmi).des;
            this.css = getBmiTitle(this.bmi).css;
            getCharts();
        }
    }

    var getBmiTitle = function (x) {
        var res = {
            des: '',
            css: ''
        }
        if (x < 18.5) { res.des = 'snižena tjelesna masa', res.css = 'info'; }
        if (x >= 18.5 && x <= 25) { res.des = "normalan tjelesna masa"; res.css = 'success'; }
        if (x > 25 && x < 30) { res.des = "povišena tjelesna masa"; res.css = 'warning'; }
        if (x >= 30) { res.des = "gojaznost"; res.css = 'danger'; }
        return res;
    }

    var bmiChart = function () {
        var id = 'bmiChart';
        var value = $scope.d.bmi;
        var unit = 'BMI';
        var options = {
            title: 'BMI',
            min: 15,
            max: 34,
            greenFrom: 18.5,
            greenTo: 25,
            yellowFrom: 25,
            yellowTo: 30,
            redFrom: 30,
            redTo: 34,
            minorTicks: 5
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var getCharts = function () {
        google.charts.load('current', { 'packages': ['gauge'] });
        $timeout(function () {
            bmiChart();
        }, 300);
    }
    getCharts();
}]);