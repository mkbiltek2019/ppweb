﻿/*!
app.js
(c) 2019-2020 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'pascalprecht.translate', 'chart.js', 'ngStorage', 'functions', 'charts'])

.config(['$stateProvider', '$urlRouterProvider', '$translateProvider', '$translatePartialLoaderProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $translateProvider, $translatePartialLoaderProvider, $httpProvider) {

    $stateProvider
        .state('login', {
            url: '/', templateUrl: './assets/partials/login.html'
        })
        .state('dashboard', {
            url: '/dashboard', templateUrl: './assets/partials/dashboard.html', controller: 'dashboardCtrl'
        })
        .state('client', {
            url: '/client', templateUrl: './assets/partials/client.html'
        })
        .state('inputdata', {
            url: '/inputdata', templateUrl: './assets/partials/inputdata.html', controller: 'inputdataCtrl'
        })
        .state('clientlog', {
            url: '/clientlog', templateUrl: './assets/partials/clientlog.html', controller: 'clientlogCtrl'
        })
        .state('activities', {
            url: '/activities', templateUrl: './assets/partials/activities.html'
        })
        .state('additionalactivities', {
            url: '/additional-activities', templateUrl: './assets/partials/additionalactivities.html'
        })
        .state('menus', {
            url: '/menus', templateUrl: './assets/partials/menus.html'
        })
        .state('menu', {
            url: '/menu', templateUrl: './assets/partials/menu.html'
        })
        .state('info', {
            url: '/info', templateUrl: './assets/partials/info.html'
        })

    $urlRouterProvider.otherwise("/");

    $translateProvider.useLoader('$translatePartialLoader', {
         urlTemplate: './assets/json/translations/{lang}/{part}.json'
    });
    $translateProvider.preferredLanguage('hr');
    $translatePartialLoaderProvider.addPart('main');
    $translateProvider.useSanitizeValueStrategy('escape');


    //--------------disable catche---------------------
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //-------------------------------------------------

}])

.controller('AppCtrl', ['$scope', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', 'charts', '$state', function ($scope, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions, charts, $state) {

    $scope.today = new Date();

    function initPrintSettings() {
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/InitMenuSettings',
            method: "POST",
            data: {}
        })
       .then(function (response) {
           $scope.settings = JSON.parse(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });
    };

    var saveVersion = function () {
        if (typeof (Storage) !== "undefined") {
            localStorage.version = $scope.config.version;
        }
        window.location.reload(true);
    }

    var initChartDays = function () {
        $scope.chartDays = [
           { days: 7, title: 'last 7 days' },
           { days: 14, title: 'last 14 days' },
           { days: 30, title: 'last 30 days' },
           { days: 92, title: 'last 3 months' },
           { days: 180, title: 'last 6 months' },
           { days: 365, title: 'last 12 months' },
           { days: 100000, title: 'all' }
        ]
        $scope.clientLogsDays = $scope.chartDays[2];
    }

    $scope.currLanguageTitle = null
    var getLanguageTitle = function (x) {
        if ($scope.config !== undefined) {
            $scope.currLanguageTitle = $scope.config.languages.find(a => a.code === x).title;
            //angular.forEach($scope.config.languages, function (value, key) {
            //    if (value.code == x) {
            //        $scope.currLanguageTitle = value.title;
            //        return false;
            //    }
            //});
        }
    }

    $scope.setLanguage = function (x) {
        $translate.use(x);
        $translatePartialLoader.addPart('main');
        $scope.config.language = x;
        if (typeof (Storage) !== "undefined") {
            if (localStorage.language !== undefined) {
                if (localStorage.language !== x) {
                    //$timeout(function () {
                    //    setClientLogGraphData(0, 30);
                    //}, 1000);
                }
            }
            localStorage.language = x;
        }
        $sessionStorage.config.language = x;
        getLanguageTitle(x);
        initChartDays();
    };


    $scope.activationCode = null;
    $scope.activateApp = function (x) {
        if (x == null || x == '' || x == 'null') {
            alert($translate.instant('enter the activation code'));
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + 'ClientApp.asmx/Activate',
            method: 'POST',
            data: { code: x }
        })
       .then(function (response) {
           $scope.clientApp = JSON.parse(response.data.d);
           if ($scope.clientApp.code === x) {
               //$scope.setLanguage($scope.clientApp.lang);
               localStorage.code = $scope.clientApp.code;
               //localStorage.language = $scope.clientApp.lang;
               //$sessionStorage.config.language = $scope.clientApp.lang;
               $scope.clientId = $scope.clientApp.clientId;
               $scope.userId = $scope.clientApp.userId;
               initPrintSettings();
               getClient();
               loadPals();
               $scope.loadMenues();
               var lang = $scope.clientApp.lang;
               if ($scope.config.language !== $scope.clientApp.lang) {
                   lang = $scope.config.language;
               }
               if (localStorage.language !== undefined) {
                   lang = localStorage.language;
               }
               $scope.setLanguage(lang);
               $state.go('dashboard');
           } else {
               alert($translate.instant('wrong activation code'));
           }
       },
       function (response) {
           alert(response.data.d);
       });
    }

    var loadPals = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/LoadPal',
            method: "POST",
            data: ''
        })
      .then(function (response) {
          $scope.pals = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };

    var getConfig = function () {
        $scope.userId = null;
        $scope.clientId = null;
        $http.get('./config/config.json')
          .then(function (response) {
              $scope.config = response.data;
              $sessionStorage.config = $scope.config;
              var querystring = location.search;
              if (!functions.isNullOrEmpty(querystring)) {
                  if (querystring.split('&')[0].substring(1, 4) == 'uid') {
                      $scope.userId = querystring.split('&')[0].substring(5);
                  }
                  if (querystring.split('&')[1].substring(0, 3) == 'cid') {
                      $scope.clientId = querystring.split('&')[1].substring(4);
                  }
                  if (querystring.split('&')[2].substring(0, 4) == 'lang') {
                      $scope.config.language = querystring.split('&')[2].substring(5);
                  }
                  $scope.setLanguage($scope.config.language);
              } else {
                  if (typeof (Storage) !== "undefined") {
                      if (localStorage.code !== undefined) {
                          $scope.activateApp(localStorage.code);
                      }
                  }
              }
              if ($scope.userId == null || $scope.clientId == null) {
                  $state.go('login');
                  return false;
              }

              //$sessionStorage.config = $scope.config;
              getClient();
              initPrintSettings();
              loadPals();
              $scope.loadMenues();
              $state.go('dashboard');
              if (localStorage.version) {
                  if (localStorage.version != $scope.config.version) {
                      saveVersion();
                  }
              } else {
                  saveVersion();
              }
          });
    };

    $scope.showTabs = function () {
        if(angular.isUndefined($rootScope.clientData)){return false;}
        var x = $scope.clientData;
        if (x.clientId != null && x.height > 0 && x.weight > 0 && x.pal.value > 0) {
            return true;
        } else {
            return false;
        }
    }

    $scope.hideMsg = function () {
        $rootScope.mainMessage = null;
    }

    //********** New *****************
    var getClient = function () {
        $http({
            url: $sessionStorage.config.backend + 'Clients.asmx/Get',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
        .then(function (response) {
            $scope.client = JSON.parse(response.data.d);
            $scope.client.birthDate = new Date($scope.client.birthDate);
            getClientData();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.activitiesTotalProgress = 0;
    var getActivitiesTotal = function (x) {
        if (x === undefined) { return false; }
        tot = 0;
        for (var i in x) {
            tot += x[i].energy;
        }
        $scope.activitiesTotal = tot.toFixed(2);
        $scope.activitiesTotalProgress = tot / 4;
    }

    var getClientData = function () {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Get',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
        .then(function (response) {
            $scope.clientData = JSON.parse(response.data.d);
            $scope.clientData.date = new Date(new Date().setHours(0, 0, 0, 0));
            getActivitiesTotal($scope.clientData.activities);
            $scope.calculate();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.save = function (x) {
        x.date = functions.dateToString(x.date);
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Save',
            method: "POST",
            data: { userId: $scope.userId, x: x, userType: 0 }
        })
        .then(function (response) {
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.getDateFormat = function (x) {
        return new Date(x);
    }

    var getCalculation = function () {
        if (isNaN($scope.clientData.weight) == true || isNaN($scope.clientData.height) == true || isNaN($scope.clientData.waist) == true || isNaN($scope.clientData.hip) == true) { return false; }
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $scope.clientData, userType: 1 }
        })
        .then(function (response) {
            $scope.calculation = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    $scope.calculate = function () {
        getCalculation();
    }

    getConfig();

    $scope.loading = false;
    $scope.loadMenues = function () {
        $scope.loading = true;
        $http({
            url: $sessionStorage.config.backend + 'Menues.asmx/LoadClientMenues',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
       .then(function (response) {
           $scope.menus = JSON.parse(response.data.d);
           angular.forEach($scope.menus, function (x, key) {
               var date_ = new Date(x.date);
               x.date = date_.toLocaleDateString();
           });
           $scope.loading = false;
       },
       function (response) {
           $scope.loading = false;
           alert(response.data.d);
       });
    }

    $scope.getMenu = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Menues.asmx/Get',
            method: "POST",
            data: { userId: $scope.userId, id: x.id, }
        })
        .then(function (response) {
            $scope.menu = JSON.parse(response.data.d);
            $scope.menu.client.clientData = $scope.clientData;
            getTotals($scope.menu);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $rootScope.getMealTitle = function (x) {
        if (x.code == 'B') { return $translate.instant('breakfast'); }
        else if (x.code == 'MS') { return $translate.instant('morning snack'); }
        else if (x.code == 'L') { return $translate.instant('lunch'); }
        else if (x.code == 'AS') { return $translate.instant('afternoon snack'); }
        else if (x.code == 'D') { return $translate.instant('dinner'); }
        else if (x.code == 'MBS') { return $translate.instant('meal before sleep'); }
        else return x.title;
    }

    var getTotals = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/GetTotals',
            method: "POST",
            data: { selectedFoods: x.data.selectedFoods, meals: x.data.meals }
        })
       .then(function (response) {
           $scope.totals = JSON.parse(response.data.d);
           $scope.totals.price.currency = $scope.config.currency;
       },
       function (response) {
           alert(response.data.d)
       });
    }

    var consumers = 1;

    $scope.pdfLink = null;
    $scope.creatingPdf = false;
    $scope.createMenuPdf = function () {
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
            method: "POST",
            data: { userId: $scope.userId, currentMenu: $scope.menu, totals: $scope.totals, consumers: consumers, lang: $scope.config.language, settings: $scope.settings, date: null, author: null, headerInfo: null }
        })
        .then(function (response) {
            var fileName = response.data.d;
            $scope.creatingPdf = false;
            $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $scope.userId + '/pdf/' + fileName + '.pdf';
        },
        function (response) {
            $scope.creatingPdf = false;
            alert(response.data.d)
        });
    }

    $scope.isSeparatedDes = function (x) {
        return x.includes('~');
    }

    var currDes = null;
    $scope.list = [];
    var currList = [];
    $scope.getTitleDes = function (x) {
        if (currList === x) { return currList; }
        if (!functions.isNullOrEmpty(x) && !$scope.list.includes(x)) {
            $scope.list.push(x);
            var desList = x.split('|');
            var list = [];
            angular.forEach(desList, function (value, key) {
                list.push({
                    title: value.split('~')[0],
                    description: value.split('~')[1],
                })
            });
            currDes = x;
            currList = list;
            return list.length > 0 ? list : x;
        } else {
            currList = x;
            return x;
        }
    }

    $scope.change = function (step, scope) {
        if (scope === 'height') {
            $scope.clientData.height += step;
            $scope.calculate();
        }
        if (scope === 'weight') {
            $scope.clientData.weight += step;
            $scope.calculate();
        }
        if (scope === 'waist') {
            $scope.clientData.waist += step;
            $scope.calculate();
        }
        if (scope === 'hip') {
            $scope.clientData.hip += step;
            $scope.calculate();
        }
    }

    $scope.clientLogDiff = function (type, clientLog, x, idx) {
        var diff = 0;
        if (clientLog.length - idx == 1) return {
            diff: diff.toFixed(1),
            icon: 'fa fa-circle text-success'
        }
        switch (type) {
            case 'weight': diff = (x.weight - clientLog[clientLog.length - idx - 2].weight).toFixed(1);
                break;
            case 'waist': diff = (x.waist - clientLog[clientLog.length - idx - 2].waist).toFixed(1);
                break;
            case 'hip': diff = (x.hip - clientLog[clientLog.length - idx - 2].hip).toFixed(1);
                break;
            default:
                diff = 0;
                break;
        }
        if (diff > 0) {
            return {
                diff: diff,
                icon: 'fa fa-arrow-up text-danger'
            }
        }
        if (diff < 0) {
            return {
                diff: diff,
                icon: 'fa fa-arrow-down text-info'
            }
        }
        if (diff == 0) {
            return {
                diff: diff,
                icon: 'fa fa-circle text-success'
            }
        }
    }
    //********* New *****************

    $scope.show = false;
    $scope.showTitle = 'show';
    $scope.showOther = function () {
        $scope.show = !$scope.show;
        if ($scope.show == true) {
            $scope.showTitle = 'hide';
        } else {
            $scope.showTitle = 'show';
        }
    }

    $scope.showLog = true;
    $scope.showClientLog = function () {
        $scope.showLog = !$scope.showLog;
    }

    $scope.getBmiClass = function (x) {
        if (x < 18.5) { return { css: 'info', icon: 'exclamation' }; }
        if (x >= 18.5 && x <= 25) { return { css: 'success', icon: 'check' }; }
        if (x > 25 && x < 30) { return { css: 'warning', icon: 'exclamation' }; }
        if (x >= 30) { return { css: 'danger', icon: 'exclamation' }; }
    }

    $scope.updateClient = function (x) {
        updateClient(x);
    }

    var updateClient = function (x) {
        var c = angular.copy(x);
        c.birthDate = functions.dateToString(c.birthDate);
        $http({
            url: $sessionStorage.config.backend + 'Clients.asmx/UpdateClient',
            method: 'POST',
            data: { userId: $scope.userId, x: c }
        })
       .then(function (response) {
           document.getElementById("mySidenav").style.width = "0";
       },
       function (response) {
           alert(response.data.d);
       });
    }

    $scope.setGenderTitle = function (x) {
        x.title = x.value == 0 ? 'male' : 'femaile';
    }

    $scope.logout = function () {
        $scope.client = null;
        $scope.userId = null;
        localStorage.clear();
        $state.go('login');
        var uri = window.location.toString();
        if (uri.indexOf("?") > 0) {
            var clean_uri = uri.substring(0, uri.indexOf("?"));
            window.history.replaceState({}, document.title, clean_uri);
            window.location.href = window.location.origin + window.location.pathname;
        }
    }

    $scope.activationCodeInputType = 'password';
    $scope.showPass = function () {
        $scope.activationCodeInputType = $scope.activationCodeInputType == 'password' ? 'type' : 'password'
    }

    /********* Profile Image ************/
    $scope.uploadImg = function () {
        var content = new FormData(document.getElementById("formUpload"));
        $http({
            url: $sessionStorage.config.backend + '/UploadProfileImg.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            $scope.client.profileImg = response.data;
        },
       function (response) {
           alert($translate.instant(response.data));
       });
    }

    $scope.removeProfileImg = function (x) {
        if (confirm($translate.instant('remove image') + '?')) {
            removeProfileImg(x);
        }

    }

    var removeProfileImg = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/DeleteProfileImg',
            method: 'POST',
            data: { x: x },
        }).then(function (response) {
            $scope.client.profileImg = response.data.d;
        },
       function (response) {
           alert($translate.instant(response.data.d));
       });
    }
    /********* Profile Image ************/


}])

.controller('dashboardCtrl', ['$scope', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', 'charts', function ($scope, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions, charts) {
    
    var getClientData = function () {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Get',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
        .then(function (response) {
            $scope.clientData = JSON.parse(response.data.d);
            $scope.clientData.date = new Date(new Date().setHours(0, 0, 0, 0));
            //getActivitiesTotal($scope.clientData.activities);
            getCalculation();
            getClientLog();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getCalculation = function () {
        if (isNaN($scope.clientData.weight) == true || isNaN($scope.clientData.height) == true || isNaN($scope.clientData.waist) == true || isNaN($scope.clientData.hip) == true) { return false; }
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $scope.clientData, userType: 1 }
        })
        .then(function (response) {
            $scope.calculation = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    var getClientLog = function () {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
        .then(function (response) {
            $scope.clientLog = JSON.parse(response.data.d);
            //angular.forEach($scope.clientLog, function (x, key) {
            //    x.date = new Date(x.date);
            //});
            setClientLogGraphData(0, $scope.clientLogsDays);
            //drawChart();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var setClientLogGraphData = function (type, clientLogsDays) {
        $scope.clientLog_ = [];
        var clientLog = [];
        var labels = [];
        if (!angular.isDefined($scope.calculation)) { return false; }
        if (angular.isDefined($scope.calculation.recommendedWeight)) {
            var days = 30;
            var goal = 0;
            var deficit = ($scope.calculation.recommendedEnergyIntake - $scope.calculation.recommendedEnergyExpenditure) - $scope.calculation.tee;
            if (clientLogsDays !== undefined) {
                days = clientLogsDays.days;
                $scope.clientLogsDays = clientLogsDays;
            }
            angular.forEach($scope.clientLog, function (x, key) {
                if (functions.getDateDiff(x.date) <= days) {
                    $scope.clientLog_.push(x);
                        clientLog.push(x.weight);
                    if (key % (Math.floor($scope.clientLog.length / 31) + 1) === 0) {
                        labels.push(new Date(x.date).toLocaleDateString());
                    } else {
                        labels.push("");
                    }
                }
            });

            $scope.clientLogGraphData = charts.createGraph(
                [$translate.instant("")],
                [clientLog],
                labels,
                ['#3399ff'],
                {
                    responsive: true, maintainAspectRatio: false, legend: { display: true },
                    scales: {
                        xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }],
                        yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true } }]
                    }
                },
                [
                    { label: $translate.instant("weight") + ' (' + $translate.instant("kg") + ')', borderWidth: 3, type: 'line', fill: true }
                ]
            )

        }

    };

    getClientData();

}])

.controller('inputdataCtrl', ['$scope', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', 'charts', function ($scope, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions, charts) {
    google.charts.load('current', { 'packages': ['gauge'] });
    var bmiChart = function () {
        if (!angular.isDefined($scope.calculation)) { return false; }
        var id = 'bmiChart';
        var value = $scope.calculation.bmi.value.toFixed(1);
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

    $timeout(function () {
        bmiChart();
    }, 1000);

}])

.controller('clientlogCtrl', ['$scope', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', 'charts', function ($scope, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions, charts) {
    $scope.displayType = 0;

    var getClientLog = function () {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
        .then(function (response) {
            $scope.clientLog = JSON.parse(response.data.d);
            angular.forEach($scope.clientLog, function (x, key) {
                x.date = new Date(x.date);
            });
            setClientLogGraphData(0, $scope.clientLogsDays);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    getClientLog();

    $scope.updateClientLog = function (x) {
        var cd = angular.copy(x);
        cd.date = functions.dateToString(cd.date);
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/UpdateClientLog',
            method: "POST",
            data: { userId: $scope.userId, clientData: cd }
        })
        .then(function (response) {
            getClientLog();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getRecommendedWeight = function (h) {
        return {
            min: Math.round(((18.5 * h * h) / 10000) * 10) / 10,
            max: Math.round(((25 * h * h) / 10000) * 10) / 10
        }
    }

    $scope.changeGoalWeightValue = function (value, type, clientLogsDays) {
        $scope.goalWeightValue_ = parseInt(value);
        setClientLogGraphData(type, clientLogsDays);
    }

    var getGoalLog = function (deficit, key, x, firstWeight, firstDate, currDate) {
        var goal = (firstWeight + (functions.getTwoDateDiff(firstDate, currDate)) * deficit / 7000).toFixed(1);
        var value = 0;
        var goalLimit = $scope.goalWeightValue_ !== undefined ? parseInt($scope.goalWeightValue_) : 0;
        if (goalLimit == 0) {
            if (deficit == 0) {
                goalLimit = x.weight;
            } else if (deficit > 0) {
                goalLimit = (getRecommendedWeight(x.height).min + getRecommendedWeight(x.height).max) / 2;
            } else {
                goalLimit = getRecommendedWeight(x.height).max;
            }
        }
        if (key == 0) {
            value = x.weight;
        }
        if (deficit > 0) {
            if (goal <= goalLimit) {
                value = goal;
            } else {
                value = goalLimit;
            }
        } else {
            if (goal >= goalLimit) {
                value = goal;
            } else {
                value = goalLimit;
            }
        }
        return value;
    }

    var setClientLogGraphData = function (type, clientLogsDays) {
        $scope.clientLog_ = [];
        var clientLog = [];
        var goalFrom = [];
        var goalTo = [];
        var goalWeight = [];
        var labels = [];
        if (!angular.isDefined($scope.calculation)) { return false; }
        if (angular.isDefined($scope.calculation.recommendedWeight)) {
            var days = 30;
            var goal = 0;
            var deficit = ($scope.calculation.recommendedEnergyIntake - $scope.calculation.recommendedEnergyExpenditure) - $scope.calculation.tee;
            if (clientLogsDays !== undefined) {
                days = clientLogsDays.days;
                $scope.clientLogsDays = clientLogsDays;
            }
            angular.forEach($scope.clientLog, function (x, key) {
                if (functions.getDateDiff(x.date) <= days) {
                    $scope.clientLog_.push(x);
                    if (type == 0) {
                        clientLog.push(x.weight);
                        goalFrom.push(getRecommendedWeight(x.height).min);
                        goalTo.push(getRecommendedWeight(x.height).max);
                        /********** goal **********/
                        goal = getGoalLog(deficit, key, x, $scope.clientLog[0].weight, $scope.clientLog[0].date, x.date);
                        goalWeight.push(goal);
                        /**************************/
                    }
                    if (type == 1) { clientLog.push(x.waist); goalFrom.push(95); }
                    if (type == 2) { clientLog.push(x.hip); goalFrom.push(97); }
                    if (key % (Math.floor($scope.clientLog.length / 31) + 1) === 0) {
                        labels.push(new Date(x.date).toLocaleDateString());
                    } else {
                        labels.push("");
                    }
                }
            });
        }

        $scope.clientLogGraphData = charts.createGraph(
            [$translate.instant("measured value"), $translate.instant("lower limit"), $translate.instant("upper limit"), $translate.instant("goal")],
            [
                clientLog,
                goalFrom,
                goalTo,
                goalWeight
            ],
            labels,
            ['#3399ff', '#ff3333', '#33ff33', '#ffd633'],
            {
                responsive: true, maintainAspectRatio: false, legend: { display: true },
                scales: {
                    xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }],
                    yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }]
                }
            },
            [
                { label: $translate.instant("measured value"), borderWidth: 1, type: 'bar', fill: true },
                { label: $translate.instant("lower limit"), borderWidth: 2, type: 'line', fill: false },
                { label: $translate.instant("upper limit"), borderWidth: 2, type: 'line', fill: false },
                { label: $translate.instant("goal") + ' (2 ' + $translate.instant("kg") + '/' + $translate.instant("mo") + ')', borderWidth: 3, type: 'line', fill: false }
            ]
        )
    };

    $scope.setClientLogGraphData = function (type, clientLogsDays) {
        setClientLogGraphData(type, clientLogsDays);
    }

    $scope.removeClientLog = function (x) {
        var r = confirm($translate.instant('delete record') + '?');
        if (r == true) {
            removeClientLog(x);
        }
    }

    var removeClientLog = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Delete',
            method: "POST",
            data: { userId: $scope.userId, clientData: x }
        })
        .then(function (response) {
            getClientLog();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.changeDisplayType = function (type, clientLogsDays) {
        setClientLogGraphData(type, clientLogsDays);
    }

    }])

//-------------end Program Prehrane Controllers--------------------

.directive('allowOnlyNumbers', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {
            elm.on('keydown', function (event) {
                var $input = $(this);
                var value = $input.val();
                //value = value.replace(/[^0-9]/g, '')
                value = value.replace(',', '.');
                $input.val(value);
                if (event.which == 64 || event.which == 16) {
                    // to allow numbers  
                    return false;
                } else if (event.which >= 48 && event.which <= 57) {
                    // to allow numbers  
                    return true;
                } else if (event.which >= 96 && event.which <= 105) {
                    // to allow numpad number  
                    return true;
                } else if ([8, 13, 27, 37, 38, 39, 40].indexOf(event.which) > -1) {
                    // to allow backspace, enter, escape, arrows  
                    return true;
                }
                else if (event.which == 110 || event.which == 188 || event.which == 190) {
                    // to allow ',' and '.'
                    return true;
                } else if (event.which == 46) {
                    // to allow delete
                    return true;
                }
                else {
                    event.preventDefault();
                    // to stop others  
                    return false;
                }
            });
        }
    }
});


;