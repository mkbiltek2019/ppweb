/*!
admin.js
(c) 2017 IG PROG, www.igprog.hr
*/
angular.module('app', [])

.controller('adminCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
          });
    };
    getConfig();

    $scope.islogin = false;
    var d = new Date();
    $scope.year = d.getFullYear();

    $scope.toggleTpl = function (x) {
        $scope.tpl = x;
    };
    $scope.toggleTpl('login');

    init = function () {
        $scope.user = {
            username: '',
            password: ''
        }
    }
    init();

    $scope.login = function (u) {
        $http({
            url: $rootScope.config.backend + 'Admin.asmx/Login',
            method: 'POST',
            data: {username: u.username, password: u.password }
        })
         .then(function (response) {
             $scope.islogin = JSON.parse(response.data.d);
             if ($scope.islogin == true) {
                 $scope.toggleTpl('programPrehraneWeb');
             } else {
                 alert('error login');
             }
         },
         function (response) {
             $scope.islogin = false;
             alert(response.data.d);
         });
    }

    $scope.logout = function () {
        $scope.islogin = false;
        $scope.toggleTpl('login');
        init();
    }

}])

.controller('applicationCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var load = function () {
        $http({
            url: $rootScope.config.backend + 'Instal.asmx/Load',
            method: 'POST',
            data: ''
        })
         .then(function (response) {
             $scope.d = JSON.parse(response.data.d);
         },
         function (response) {
             alert(response.data.d);
         });
        }
    load();

}])

.controller('webAppCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    $scope.showDetails = false;

    var load = function () {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Load',
            method: 'POST',
            data: ''
        })
            .then(function (response) {
                $scope.d = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d);
            });
    }
    load();

    var total = function () {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Total',
            method: 'POST',
            data: ''
        })
            .then(function (response) {
                $scope.t = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d);
            });
    }
    total();

    $scope.update = function (user) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Update',
            method: 'POST',
            data: { x: user }
        })
            .then(function (response) {
                load();
                total();
                alert(response.data.d);
            },
            function (response) {
                alert(response.data.d);
            });
    }

    $scope.remove = function(user) {
        var r = confirm("Briši " + user.firstName + " "  + user.lastName + "?");
        if (r == true) {
            remove(user);
        } else {
        }
    }

    var remove = function (user) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Delete',
            method: 'POST',
            data: { x: user }
        })
            .then(function (response) {
                load();
                total();
                alert(response.data.d);
            },
            function (response) {
                alert(response.data.d);
            });
    }

    $scope.idxStart = 0;
    $scope.idxEnd = 10;
    $scope.setPage = function (x) {
        $scope.idxStart = 0 + x;
        $scope.idxEnd = 10 + x;
    }

    $scope.showAllPages = function () {
        $scope.idxStart = 0;
        $scope.idxEnd = $scope.d.length;
    }

}])

.controller('ordersCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var load = function () {
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/Load',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.orders = JSON.parse(response.data.d);
     },
     function (response) {
         alert(response.data.d);
     });
    }
    load();

}])


;
