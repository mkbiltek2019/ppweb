/*!
app.js
(c) 2018 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'pascalprecht.translate', 'chart.js', 'ngStorage', 'functions', 'charts'])

.config(['$stateProvider', '$urlRouterProvider', '$translateProvider', '$translatePartialLoaderProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $translateProvider, $translatePartialLoaderProvider, $httpProvider) {

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



.controller('AppCtrl', ['$scope', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', 'charts', function ($scope, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions, charts) {

    var querystring = location.search;
    if (!functions.isNullOrEmpty(querystring)) {
        if (querystring.split('&')[0].substring(1, 4) == 'uid') {
            $scope.userId = querystring.split('&')[0].substring(5);
        }
        if (querystring.split('&')[1].substring(0, 3) == 'cid') {
            $scope.clientId = querystring.split('&')[1].substring(4);
        }
    }
    
    $scope.today = new Date();

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              $sessionStorage.config = response.data;
              getClient();
              var queryLang = location.search.substring(6);
              if (angular.isDefined(queryLang)) {
                  if (queryLang == 'hr' || queryLang == 'sr' || queryLang == 'en') {
                      $rootScope.setLanguage(queryLang);
                  }
              }
              if ($sessionStorage.islogin == true) { $rootScope.loadData(); }
          });
    };
    

    $rootScope.loadPals = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/LoadPal',
            method: "POST",
            data: ''
        })
      .then(function (response) {
          $rootScope.pals = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    };

    $rootScope.loadData = function () {
        $rootScope.loadPals();
    }

    $rootScope.saveClientData = function (x) {
        saveClientData(x);
    }

    var saveClientData = function (x) {
        x.userId = $rootScope.user.userId;
        x.clientId = x.clientId == null ? $rootScope.client.clientId : x.clientId;
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Save',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, x: x, userType:0 }
        })
       .then(function (response) {
           $scope.clientData.date = new Date($rootScope.clientData.date);
       },
       function (response) {
           alert(response.data.d)
       });
    }

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
            getClientData();
        },
        function (response) {
            alert(response.data.d)
        });
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
            $scope.calculate();
            getClientLog();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getClientLog = function () {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $scope.userId, clientId: $scope.clientId }
        })
        .then(function (response) {
            $scope.clientLog = JSON.parse(response.data.d);
            setClientLogGraphData(0);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.save = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Save',
            method: "POST",
            data: { userId: $scope.userId, x: x, userType: 0 }
        })
        .then(function (response) {
            getClientLog();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.updateClientLog = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/UpdateClientLog',
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

    $scope.getDateFormat = function (x) {
        return new Date(x);
    }

    var getCharts = function () {
        google.charts.load('current', { 'packages': ['gauge'] });
        $timeout(function () {
            bmiChart();
            //setClientLogGraphData();
        }, 1000);
    }

    var bmiChart = function () {
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
  //  getCharts();

    $scope.displayType = 0;
    var getCalculation = function () {
        if (isNaN($scope.clientData.weight) == true || isNaN($scope.clientData.height) == true || isNaN($scope.clientData.waist) == true || isNaN($scope.clientData.hip) == true) { return false; }
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $scope.clientData }
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
        getCharts();

    }

    var setClientLogGraphData = function (type) {
        var clientData = [];
        var goalFrom = [];
        var goalTo = [];
        var labels = [];
        $scope.clientLogGraphData = charts.createGraph(
            [$translate.instant('tracking of anthropometric measures')],
            [
                clientData,
                goalFrom,
                goalTo
            ],
            labels,
            ['#3399ff', '#ff3333', '#33ff33'],
            [
                   {
                       label: $translate.instant("measured value"),
                       borderWidth: 5,
                       type: 'line',
                       fill: true
                   },
                   {
                       label: $translate.instant("lower limit"),
                       borderWidth: 2,
                       backgroundColor: '#e6e6ff',
                       fill: false,
                       type: 'line'
                   },
                   {
                       label: $translate.instant("upper limit"),
                       borderWidth: 2,
                       backgroundColor: '#e6e6ff',
                       fill: false,
                       type: 'line'
                   }
            ],
            true
        )

        //TODO - goal
        if (angular.isDefined($scope.calculation.recommendedWeight)) {
            angular.forEach($scope.clientLog, function (x, key) {
                if (type == 0) { clientData.push(x.weight); goalFrom.push($scope.calculation.recommendedWeight.min); goalTo.push($scope.calculation.recommendedWeight.max); }
                if (type == 1) { clientData.push(x.waist); goalFrom.push(95); }
                if (type == 2) { clientData.push(x.hip); goalFrom.push(97); }
                if (key % (Math.floor($scope.clientLog.length / 31) + 1) === 0) {
                    labels.push(new Date(x.date).toLocaleDateString());  //TODO check with server date time (one day less)
                } else {
                    labels.push("");
                }
            });
        }


    };

    $scope.setClientLogGraphData = function (x) {
        setClientLogGraphData(x);
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
           $scope.menues = JSON.parse(response.data.d);
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
            $scope.toggleTpl('menu');
        },
        function (response) {
            alert(response.data.d)
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
           $scope.totals.price.currency = $rootScope.config.currency;
       },
       function (response) {
           alert(response.data.d)
       });
    }

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
    initPrintSettings();

    var consumers = 1;

    $scope.pdfLink = null;
    $scope.creatingPdf = false;
    $scope.createMenuPdf = function () {
        debugger;
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
            method: "POST",
            data: { userId: $scope.userId, currentMenu: $scope.menu, totals: $scope.totals, consumers: consumers, lang: $rootScope.config.language, settings: $scope.settings }
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

    $scope.toggleCurrTpl = function (x) {
        $scope.currTpl = './assets/partials/' + x;
    };
    $scope.toggleCurrTpl('clientdata.html');

    $scope.toggleTpl = function (x) {
        $scope.tpl = x;
        getCharts();
    };
    $scope.toggleTpl('inputData');

    $scope.toggleSubTpl = function (x) {
        $scope.subTpl = x;
    };
    $scope.toggleSubTpl('clientLog');



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