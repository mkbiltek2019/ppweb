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

    $scope.toggleCurrTpl = function (x) {
        $rootScope.currTpl = './assets/partials/' + x;
    };
    $scope.toggleCurrTpl('clientdata');

    $scope.toggleTpl = function (x) {
        $scope.tpl = x;
    };
    $scope.toggleTpl('inputData');

    $scope.toggleSubTpl = function (x) {
        $scope.subTpl = x;
    };
    $scope.toggleSubTpl('clientLog');

    var checkUser = function () {
        $rootScope.currTpl = './assets/partials/clientdata.html';
    }
    checkUser();

  

    $scope.showSaveMessage = false;

    
    $rootScope.saveClientData = function (x) {
        //if ($rootScope.clientData.meals.length > 0) {
        //    if ($rootScope.clientData.meals[1].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
        //        $rootScope.newTpl = 'assets/partials/meals.html';
        //        functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
        //        return false;
        //    }
        //    if ($rootScope.clientData.meals[3].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
        //        $rootScope.newTpl = 'assets/partials/meals.html';
        //        functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
        //        return false;
        //    }
        //}
        saveClientData(x);
    }

    var saveClientData = function (x) {
        x.userId = $rootScope.user.userId;
        x.clientId = x.clientId == null ? $rootScope.client.clientId : x.clientId;
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Save',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, x: x }
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
    //$scope.userId = 'c67066a9-f3c1-432d-88c4-7f6e3278f616';  //TODO
    //$scope.clientId = 'b01ce6b4-c566-4e6f-8382-23d5bf635497';  //TODO

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
          //  functions.alert($translate.instant(response.data.d), '');
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
          //  functions.alert($translate.instant(response.data.d), '');
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
            data: { userId: $scope.userId, x: x }
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
        }, 500);
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
       // $scope.bmi = ($scope.clientData.weight * 10000 / ($scope.clientData.height * $scope.clientData.height)).toFixed(1);
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
                    labels.push(new Date(x.date).toLocaleDateString());
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
       // var txt;
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
            getTotals($scope.menu);
            $scope.toggleTpl('menu');
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.getMealTitle = function (x) {
        if (x == 'B') { return 'breakfast'; }
        if (x == 'MS') { return 'morning snack'; }
        if (x == 'L') { return 'lunch'; }
        if (x == 'AS') { return 'afternoon snack'; }
        if (x == 'D') { return 'dinner'; }
        if (x == 'MBS') { return 'meal before sleep'; }
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

    $scope.pdfLink = null;
    $scope.creatingPdf = false;
    $scope.createMenuPdf = function () {
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
            method: "POST",
            data: { userId: $scope.userId, currentMenu: $scope.menu, clientData: $scope.clientData, totals: $scope.totals, consumers: 1, lang: $rootScope.config.language }
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


    //********* New *****************



}])




//-------------end Program Prehrane Controllers--------------------

;