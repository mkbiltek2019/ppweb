﻿/*!
admin.js
(c) 2018 IG PROG, www.igprog.hr
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

.controller('webAppCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.showDetails = false;
    $scope.showActive = false;
    $scope.loading = false;
    $scope.limit = 10;
    $scope.page = 1;
    $scope.searchQuery = '';
    $scope.showUsers = true;

    var total = function () {
        $scope.loading = true;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Total',
            method: 'POST',
            data: ''
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
    total();

    $scope.total = function () {
        $scope.showUsers = false;
        google.charts.load('current', { 'packages': ['line'] });
        google.charts.setOnLoadCallback(drawChart);
        total();
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
            total();
            alert(response.data.d);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.info = function (userId) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/GetUserSum',
            method: 'POST',
            data: { userId: userId }
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


    //google.charts.load('current', { 'packages': ['line'] });
    //google.charts.setOnLoadCallback(drawChart);

    function drawChart() {
        $scope.loadingChart = true;
        var data = new google.visualization.DataTable();
        data.addColumn('number', 'Aktivacije');
        data.addColumn('number', 'Postotak licenci');
        data.addColumn('number', 'Licence');
        $http({
            url: $rootScope.config.backend + 'Users.asmx/TotalList',
            method: 'POST',
            data: ''
        })
        .then(function (response) {
            var tl = JSON.parse(response.data.d);
            angular.forEach(tl, function (value, key) {
                data.addRows([
                       [key, value.licencepercentage, value.licence]
                ]);
            })
            var options = {
                chart: {
                    title: 'Pregled registracija i aktivacija'
                },
                height: 250
            };
            var chart = new google.charts.Line(document.getElementById('chart_ppweb'));
            chart.draw(data, google.charts.Line.convertOptions(options));
            $scope.loadingChart = false;
        },
        function (response) {
            $scope.loadingChart = false;
            alert(response.data.d);
        });
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

    $scope.createInvoice = function (order, tpl) {
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/InitPP',
            method: 'POST',
            data: { order: order }
        })
     .then(function (response) {
         $rootScope.i = JSON.parse(response.data.d);
         $rootScope.tpl = tpl;
     },
     function (response) {
         alert(response.data.d);
     });
    }

}])

.controller('invoiceCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.getTotal = function (x) {
        var total = 0;
        angular.forEach(x, function (value, key) {
            total += value.unitPrice * value.qty;
        })
        $scope.total = total;
        return total;
    }
    //if (angular.isDefined($rootScope.i)) {
    //    $scope.getTotal($rootScope.i.items);
    //}


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
         initForm();
     },
     function (response) {
         alert(response.data.d);
     });
    }
    if(angular.isUndefined($rootScope.i)) { $scope.init(); }

    $scope.load = function (year) {
        $scope.showInvoices = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Load',
            method: 'POST',
            data: { year: year }
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
    $scope.createPdf = function (i, isForeign, totPrice_eur) {
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
            data: { invoice: i, isForeign: isForeign, totPrice_eur: totPrice_eur }
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


;
