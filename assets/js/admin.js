﻿/*!
admin.js
(c) 2018-2019 IG PROG, www.igprog.hr
*/
angular.module('app', [])

.config(['$httpProvider', function ($httpProvider) {
    //*******************disable catche**********************
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************
}])

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
        $rootScope.tpl = x;
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

.controller('webAppCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {
    $scope.showDetails = false;
    $scope.showActive = false;
    $scope.loading = false;
    $scope.limit = 10;
    $scope.page = 1;
    $scope.searchQuery = '';
    $scope.showUsers = true;

    function setYears() {
        $scope.years = [];
        for (var i = 2017; i <= $scope.year; i++) {
            $scope.years.push(i);
        }
        $scope.year = new Date().getFullYear();
    }
    setYears();

    var total = function (year) {
        $scope.loading = true;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Total',
            method: 'POST',
            data: { year: year }
        })
        .then(function (response) {
            $scope.t = JSON.parse(response.data.d);
            $scope.loading = false;
        },
        function (response) {
            $scope.loading = false;
            alert(response.data.d);
        });
    }

    $scope.total = function (year) {
        $scope.showUsers = false;
        google.charts.load('current', { packages: ['corechart'] });
        total(year);
    }

    function drawChart() {
        $scope.loadingChart = true;
        var data = new google.visualization.DataTable();
        data.addColumn('string', 'Mjesec');
        data.addColumn('number', 'Registracije');
        data.addColumn('number', 'Aktivacije');
        data.addColumn('number', 'Postotak');

            var tl = $scope.t.monthly;
            angular.forEach(tl, function (value, key) {
                data.addRows([
                       [value.month, value.registration, value.activation, value.percentage]
                ]);
            })
            var options = {
                chart: {
                    title: 'Pregled registracija i aktivacija'
                },
                height: (tl.length * 55) + 2,
                chartArea: {
                    height: tl.length * 55,
                    width: 350
                }
            };
            var chart = new google.visualization.BarChart(document.getElementById('chart_ppweb'));
            chart.draw(data, options);
            $scope.loadingChart = false;
    }

    var load = function () {
        $scope.loading = true;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Load',
            method: 'POST',
            data: { limit: $scope.limit, page: $scope.page }
        })
        .then(function (response) {
            $scope.loading = false;
            $scope.d = JSON.parse(response.data.d);
        },
        function (response) {
            $scope.loading = false;
            alert(response.data.d);
        });
    }

    $scope.search = function (searchQuery, showActive) {
        $scope.loading = true;
        $scope.showUsers = true;
        $scope.page = 1;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Search',
            method: 'POST',
            data: { query: searchQuery, limit: $scope.limit, page: $scope.page, activeUsers: showActive }
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
            $scope.loading = false;
        },
        function (response) {
            $scope.loading = false;
            alert(response.data.d);
        });
    }
    $scope.search(null, false);

    $scope.update = function (user) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Update',
            method: 'POST',
            data: { x: user }
        })
        .then(function (response) {
            load();
            total($scope.year);
            alert(response.data.d);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.currUser = null;
    $scope.info = function (x) {
        $scope.currUser = x;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/GetUserSum',
            method: 'POST',
            data: { userGroupId: x.userGroupId, userId: x.userId, userType: x.userType, adminType: x.adminType }
        })
        .then(function (response) {
            $scope.userTotal = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.remove = function(user) {
        var r = confirm("Briši " + user.firstName + " "  + user.lastName + "?");
        if (r == true) {
            remove(user);
        }
    }

    var remove = function (user) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/DeleteAllUserGroup',
            method: 'POST',
            data: { x: user }
        })
        .then(function (response) {
            load();
            total($scope.year);
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

    $scope.nextPage = function() {
        $scope.page = $scope.page + 1;
        load();
    }

    $scope.prevPage = function () {
        if ($scope.page > 1) {
            $scope.page = $scope.page - 1;
            load();
        }
    }

    $scope.updateInfo = function (x) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/UpdateUserInfoFromOrdersTbl',
            method: 'POST',
            data: { email: x }
        })
        .then(function (response) {
            alert(response.data.d);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.countMyFoodsWithSameIdAsAppFoods = function (x) {
        functions.post('MyFoods', 'CountMyFoodsWithSameIdAsAppFoods', {userId: x.userGroupId}).then(function(d) {
            alert(d);
        });
    }

    $scope.fixMyFoodsId = function (x) {
        functions.post('MyFoods', 'FixMyFoodsId', {userId: x.userGroupId}).then(function(d) {
            alert(d);
        });
    }

    $scope.createSubusers = function (x, prefix) {
        functions.post('Users', 'CreateSubusers', { x: x, prefix: prefix }).then(function (d) {
            alert(d);
        });
    }

}])

.controller('ordersCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {
    var load = function () {
        functions.post('Orders', 'Load', {}).then(function(d) {
            $scope.orders = d;
        });
    }
    load();

    $scope.createInvoice = function (order, tpl) {
        functions.post('Invoice', 'InitPP', {order: order}).then(function(d) {
            $rootScope.i = d;
            $rootScope.tpl = tpl;
        });
    }

}])

.controller('invoiceCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.searchInvoices = null;
    $scope.getTotal = function (x) {
        var total = 0;
        angular.forEach(x, function (value, key) {
            total += value.unitPrice * value.qty;
        })
        $scope.total = total;
        return total;
    }

    var getLocalDateAndTime = function () {
        var date = new Date();
        var month = parseInt(date.getMonth()) + 1;
        var min = parseInt(date.getMinutes()) < 10 ? '0' + date.getMinutes() : date.getMinutes();
        var res = date.getDate() + '.' + month + '.' + date.getFullYear() + ', ' + date.getHours() + ':' + min;
        return res;
    }

    var initForm = function () {
        $scope.isInvoice = false;
        $scope.pdfTempLink = null;
        $scope.pdfLink = null;
        $scope.loading = false;
        $scope.loading_1 = false;
        $scope.loading_2 = false;
        $scope.invoices = [];
        $scope.showInvoices = false;
        $scope.total = angular.isDefined($rootScope.i) ? $scope.getTotal($rootScope.i.items) : 0;
        $scope.year = new Date().getFullYear();
        $scope.isForeign = false;
        $scope.clientLeftSpacing = 300;
        $scope.isOffer = false;
        //$scope.totPrice_eur = 0;
    }
    initForm();

    $scope.init = function () {
        $scope.showInvoices = false;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $rootScope.i = JSON.parse(response.data.d);
         $rootScope.i.dateAndTime = getLocalDateAndTime();
         initForm();
     },
     function (response) {
         alert(response.data.d);
     });
    }
    if(angular.isUndefined($rootScope.i)) { $scope.init(); }

    $scope.load = function (year, search) {
        $scope.showInvoices = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Load',
            method: 'POST',
            data: { year: year, search: search }
        })
     .then(function (response) {
         $scope.invoices = JSON.parse(response.data.d);
     },
     function (response) {
         alert(response.data.d);
     });
    }

    $scope.get = function (x) {
        $scope.showInvoices = false;
        $rootScope.i = x;
        $scope.getTotal($rootScope.i.items);
    }

    $scope.copy = function (x) {
        $scope.showInvoices = false;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $rootScope.i = JSON.parse(response.data.d);
         initForm();
         $rootScope.i.firstName = x.firstName;
         $rootScope.i.lastName = x.lastName;
         $rootScope.i.companyName = x.companyName;
         $rootScope.i.address = x.address;
         $rootScope.i.postalCode = x.postalCode;
         $rootScope.i.city = x.city;
         $rootScope.i.country = x.country;
         $rootScope.i.pin = x.pin;
         $rootScope.i.note = x.note;
         $rootScope.i.items = x.items;
         $scope.getTotal($rootScope.i.items);
     },
     function (response) {
         alert(response.data.d);
     });
    }

    $scope.add = function () {
        $rootScope.i.items.push({
            title: '',
            qty: 1,
            unitPrice: 0
        })
        $scope.getTotal($rootScope.i.items);
    }

    $scope.remove = function (idx) {
        $rootScope.i.items.splice(idx, 1);
        $scope.getTotal($rootScope.i.items);
    }

    $scope.totPrice_eur = 0;
    $scope.createPdf = function (i, isForeign, totPrice_eur, clientLeftSpacing, isOffer) {
        if ($rootScope.i.firstName == null && $rootScope.i.lastName == null && $rootScope.i.companyName == null) {
            alert('Upiši ime ili naziv');
            return false;
        }
        if (i.number == '' || i.number == null) {
            alert('enter order number');
            return false;
        }
        $scope.loading = true;
        $scope.pdfTempLink = null;
        $scope.pdfLink = null;
        $scope.tempFileName = null;
        $http({
            url: $rootScope.config.backend + 'PrintPdf.asmx/InvoicePdf',
            method: 'POST',
            data: { invoice: i, isForeign: isForeign, totPrice_eur: totPrice_eur, clientLeftSpacing: clientLeftSpacing, isOffer: isOffer }
        })
     .then(function (response) {
         $scope.loading = false;
         $scope.tempFileName = response.data.d;
         $scope.pdfTempLink = $rootScope.config.backend + 'upload/invoice/temp/' + $scope.tempFileName + '.pdf';
     },
     function (response) {
         $scope.loading = false;
         alert(response.data.d);
     });
    }

    $scope.save = function (i) {
        if ($rootScope.i.firstName == null && $rootScope.i.lastName == null && $rootScope.i.companyName == null) {
            alert('Upiši ime ili naziv');
            return false;
        }
        $scope.loading_1 = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Save',
            method: 'POST',
            data: { x: i, pdf: $scope.tempFileName }
        })
     .then(function (response) {
         $scope.loading_1 = false;
         $rootScope.i = JSON.parse(response.data.d);
         $scope.fileName = $rootScope.i.year + '/' + $rootScope.i.fileName; //  response.data.d;
         $scope.pdfLink = $rootScope.config.backend + 'upload/invoice/' + $scope.fileName + '.pdf';
     },
     function (response) {
         $scope.loading_1 = false;
         alert(response.data.d);
     });
    }

    $scope.saveDb = function (i) {
        if ($rootScope.i.firstName == null && $rootScope.i.lastName == null && $rootScope.i.companyName == null) {
            alert('Upiši ime ili naziv');
            return false;
        }
        $scope.loading_2 = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/SaveDb',
            method: 'POST',
            data: { x: i }
        })
     .then(function (response) {
         $scope.loading_2 = false;
         $rootScope.i = JSON.parse(response.data.d);
        // alert(response.data.d);
     },
     function (response) {
         $scope.loading_2 = false;
         alert(response.data.d);
     });
    }

    $scope.setPaidAmount = function (x) {
        $scope.getTotal(x.items);
        if (x.isPaid == true) {
            $scope.i.paidAmount = $scope.total;
        } else {
            $scope.i.paidAmount = 0;
            $scope.i.paidDate = '';
        }
    }

}])

.factory('functions', ['$http', function ($http) {
    return {
        post: function (service, webmethod, data) {
            return $http({
                url: '../' + service + '.asmx/' + webmethod, method: 'POST', data: data
            }).then(function (response) {
                return JSON.parse(response.data.d);
            },function (response) {
                return response.data.d;
            });
        }
    }
}]);


;
