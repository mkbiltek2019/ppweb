﻿/*!
app.js
(c) 2017 IG PROG, www.igprog.hr
*/
angular.module('app', ['ngMaterial'])


.controller('appCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
          });
    };
    getConfig();

    $scope.pp5lang = 'hr';
    $scope.setDownloadLink = function (x) {
        if (x == 'hr') { $scope.pp5downloadlink = './download/' + x + '/ProgramPrehrane5.exe'; }
        if (x == 'rs') { $scope.pp5downloadlink = './download/' + x + '/ProgramPrehrane5S.exe'; }
    }
    $scope.setDownloadLink($scope.pp5lang);

    var d = new Date();
    $scope.year = d.getFullYear();

    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = 'Dalje';

    $scope.hashId = function (id) {
        window.location.hash = id;
    }

    $scope.href = function (x) {
        window.open(x,'_blank');
    }
    
    $scope.today = new Date;
    $scope.send = function (g) {
        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem';
        $http({
            url: $rootScope.config.backend + 'Mail.asmx/Send',
            method: 'POST',
            data: { name: g.name, email: g.email, phone: g.phone, address: g.address, type: g.type, message: g.message }
        })
     .then(function (response) {
         $scope.sendicon = 'fa fa-check';
         $scope.sendicontitle = 'Poslano';
     },
     function (response) {
         $scope.sendicon = 'fa fa-exclamation-triangle';
         $scope.sendicontitle = 'Greška.';
         alert(response.data.d);
     });
    }

}])

.controller('webAppCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
     $rootScope.application = 'Program Prehrane';
     $rootScope.version = 'WEB';
}])

.controller('pp5Ctrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $rootScope.application = 'Program Prehrane 5.0';
    $rootScope.version = 'PREMIUM';

    $scope.gotoForm = function () {
        $scope.showUserDetails = true;
    }

}])


.controller('singupCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var init = function () {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.user = JSON.parse(response.data.d);
     },
     function (response) {
         alert(response.data.d);
     });
    }

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              init();
          });
    };
    if ($rootScope.config == undefined) {
        getConfig();
    } else {
        init();
    }

    $scope.singup = function (user) {
        user.userName = user.email;
        if (user.firstName == "" || user.lastName == "" || user.email == "" || user.password == "" || user.passwordConfirm == "") {
            alert('Sva polja su obavezna.');
            return false;
        }
        if (user.password != $scope.passwordConfirm) {
            alert('Lozinke moraju biti jednake.');
            return false;
        }
        $http({
            url: $rootScope.config.backend +  'Users.asmx/Singup',
            method: 'POST',
            data: { x: user }
        })
       .then(function (response) {
           alert(response.data.d);
       },
       function (response) {
           alert(response.data.d);
       });
    }
   
}])

.controller('orderCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.application = $rootScope.application;
    $scope.version = $rootScope.version;
    $scope.userType = 0;
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = 'Dalje';
    $scope.showUserDetails = false;
    $scope.showErrorAlert = false;
    $scope.showPaymentDetails = false;

    var init = function () {
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.user = JSON.parse(response.data.d);
         $scope.user.application = $scope.application;
         $scope.user.version = 'PREMIUM';
         $scope.user.licence = '0';
         $scope.user.licenceNumber = '1';
         $scope.calculatePrice();
     },
     function (response) {
         alert(response.data.d);
     });
    }

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              init();
          });
    };
    if ($rootScope.config == undefined) {
        getConfig();
    } else {
        init();
    }

    $scope.changeUserType = function (x) {
        $scope.userType = x;
    }

    $scope.calculatePrice = function () {
        var unitprice = 0;
        var totalprice = 0;

        if ($scope.user.application == 'Program Prehrane') {
            $scope.user.version = $scope.version;
            unitprice = 550;
            $scope.user.licence = '1';
            $scope.user.licenceNumber = '1';
        } else {
            $scope.user.version = $scope.user.version == '' ? 'PREMIUM' : $scope.user.version;
            if ($scope.user.version == 'START') {
                if ($scope.user.licence == '1') {
                    unitprice = 350;
                } else {
                    unitprice = 650;
                }
            }
            if ($scope.user.version == 'PREMIUM') {
                if ($scope.user.licence == '1') {
                    unitprice = 550;
                } else {
                    unitprice = 950;
                }
            }
        }

        totalprice = $scope.user.licenceNumber > 1 ? unitprice * $scope.user.licenceNumber - (unitprice * $scope.user.licenceNumber * 0.1) : unitprice;
        $scope.user.price = totalprice;
        $scope.user.priceEur = totalprice / $rootScope.config.eur;
    }
   
    $scope.order = function (application, version) {
        init();
        window.location.hash = 'order';
        $scope.user.application = application;
        $scope.user.version = version;
        $scope.calculatePrice();
    }

    $scope.setApplication = function (x) {
        $scope.user.application = x;
        $scope.calculatePrice();
    }

    $scope.sendOrder = function (user) {
        $scope.showErrorAlert = false;
        if (user.email == '' || user.firstName == '' || user.lastName == '' || user.address == '' || user.postalCode == '' || user.city == '' || user.country == '') {
            $scope.showErrorAlert = true;
            $scope.errorMesage = 'Sva polja su obavezna.';
            return false;
        }
        if ($scope.userType == 1) {
            if (user.companyName == '' || user.pin == '') {
                $scope.showErrorAlert = true;
                $scope.errorMesage = 'Sva polja su obavezna.';
                return false;
            }
        }

        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem';
        $scope.isSendButtonDisabled = true;
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/SendOrder',
            method: 'POST',
            data: { x: user }
        })
       .then(function (response) {
           $scope.showAlert = true;
           $scope.showPaymentDetails = true;
           window.location.hash = 'orderform';
       },
       function (response) {
           $scope.showAlert = false;
           $scope.showPaymentDetails = false;
           $scope.sendicon = 'fa fa-paper-plane-o';
           $scope.sendicontitle = 'Pošalji';
           alert(response.data.d);
       });
    }

    $scope.login = function (u, p) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Login',
            method: "POST",
            data: {
                userName: u,
                password: p
            }
        })
        .then(function (response) {
            var user = JSON.parse(response.data.d);
            if (user.userName == u) {
                $scope.user.firstName = user.firstName;
                $scope.user.lastName = user.lastName;
                $scope.user.companyName = user.companyName;
                $scope.user.address = user.address;
                $scope.user.postalCode = user.postalCode;
                $scope.user.city = user.city;
                $scope.user.country = user.country;
                $scope.user.pin = user.pin;
                $scope.user.email = user.email;
                $scope.showUserDetails = true;
                $scope.showErrorAlert = false;
            } else {
                $scope.showErrorAlert = true;
                $scope.errorMesage = 'Korisnik nije pronađen.'
            }
        },
        function (response) {
            $scope.errorLogin = true;
            $scope.showErrorAlert = true;
            $scope.errorMesage = 'Korisnik nije pronađen.'
            $scope.showUserDetails = false;
        });
    }

    $scope.registration = function () {
        window.location.hash = 'registration';
    }

    $scope.gotoForm = function () {
        $scope.showUserDetails = true;
    }


}])

.controller('contactCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-paper-plane-o';
    $scope.sendicontitle = 'Pošalji';

    $scope.d = {
        name: '',
        email: '',
        message: ''
    }

    $scope.send = function (d) {
        if ($rootScope.config.backend == undefined) { $rootScope.getConfig(); }
        $scope.isSendButtonDisabled = true;
        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem';
        $http({
            url: $rootScope.config.backend + 'Mail.asmx/Send',
            method: 'POST',
            data: { name: d.name, email: d.email, messageSubject: 'Program Prehrane - Upit', message: d.message }
        })
       .then(function (response) {
           $scope.showAlert = true;
           $scope.sendicon = 'fa fa-paper-plane-o';
           $scope.sendicontitle = 'Pošalji';
           window.location.hash = 'contact';
       },
       function (response) {
           $scope.showAlert = false;
           $scope.sendicon = 'fa fa-paper-plane-o';
           $scope.sendicontitle = 'Pošalji';
           alert(response.data.d);
       });
    }

}])


;