/*!
instal.min.js
(c) 2017 IG PROG, http://www.igprog.hr
*/
angular.module('app', [])

.controller('instalCtrl', ['$scope', '$http', function ($scope, $http) {
    // http://localhost:56396/instal.html?app:ProgramPrehrane5.0S&version:demo

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $scope.config = response.data;
              init();
          });
    };
    getConfig();

    var querystring = location.search;
    var arr = querystring.split("&");
    $scope.application = arr[0].substring(5);
    $scope.version = arr[1].substring(8);
    var d = new Date();
    $scope.year = d.getFullYear();


    var init = function () {
        $http({
            url: $scope.config.backend + 'Instal.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.d = JSON.parse(response.data.d);
         $scope.d.application = $scope.application;
         $scope.d.version = $scope.version;
         $scope.d.action = $scope.version == 'demo' ? 'instalacija' : 'aktivacija';
         save();
     },
     function (response) {
         alert(response.data.d);
     });
    }

    var save = function () {
        $http({
            url: $scope.config.backend + 'Instal.asmx/Save',
            method: 'POST',
            data: { x: $scope.d }
        })
        .then(function (response) {
        },
        function (response) {
            alert(response.data.d);
        });
    }
  
}])

;