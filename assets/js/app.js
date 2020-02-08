/*!
app.js
(c) 2018-2020 IG PROG, www.igprog.hr
*/
angular.module('app', ['ngMaterial'])

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
            data: { name: g.name, email: g.email, phone: g.phone, address: g.address, type: g.type, message: g.message, lang: $rootScope.config.language }
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

    $scope.showCustomers = true;
    $scope.toggleCustomers = function () {
        $scope.showCustomers = !$scope.showCustomers;
    };

    $scope.premiumUsers = 5;
    $scope.premiumUsers_ = 5;
    var maxUsers = function () {
        $scope.maxUsers = [];
        for (var i = 5; i < 101; i++) {
            $scope.maxUsers.push(i);
        }
    }
    maxUsers();

    $scope.premiumPriceOneYear = 1850;
    $scope.premiumPriceTwoYear = 2960;
    $scope.getPremiumPrice = function (x) {
        $scope.premiumPriceOneYear = x > 5 ? 1850 + ((x- 5) * 500) : 1850;
        $scope.premiumPriceTwoYear = x > 5 ? 2960 + ((x - 5) * 500) : 2960;
        $scope.premiumUsers = x;
        $scope.premiumUsers_ = x;
    }

}])

.controller('webAppCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $rootScope.application = 'Program Prehrane Web';
    $rootScope.version = 'STANDARD';
}])

.controller('pp5Ctrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $rootScope.application = 'Program Prehrane 5.0';
    $rootScope.version = 'PREMIUM';

    $scope.gotoForm = function () {
        $scope.showUserDetails = true;
    }

}])

.controller('signupCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.accept = false;
    $scope.msg = { title: null, css: null, icon: null }
    $scope.hidebutton = false;
    $scope.signupok = false;

    var init = function () {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.user = JSON.parse(response.data.d);
         $scope.passwordConfirm = '';
         $scope.emailConfirm = '';

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

    $scope.signup = function (user) {
        $scope.msg = { title: null, css: null, icon: null }
        user.userName = user.email;
        if (user.firstName == "" || user.lastName == "" || user.email == "" || user.password == "" || $scope.passwordConfirm == "" || $scope.emailConfirm == "") {
            $scope.msg.title = 'Sva polja su obavezna.';
            $scope.msg.css = 'danger';
            $scope.msg.icon = 'exclamation';
            return false;
        }
        if (user.email != $scope.emailConfirm) {
            $scope.msg.title = 'Email adrese moraju biti jednake.';
            $scope.msg.css = 'danger';
            $scope.msg.icon = 'exclamation';
            return false;
        }
        if (user.password != $scope.passwordConfirm) {
            $scope.msg.title = 'Lozinke moraju biti jednake.';
            $scope.msg.css = 'danger';
            $scope.msg.icon = 'exclamation';
            return false;
        }
        if ($scope.accept == false) {
            $scope.msg.title = 'Morate prihvatiti uvjete korištenja.';
            $scope.msg.css = 'danger';
            $scope.msg.icon = 'exclamation';
            return false;
        }
        $scope.hidebutton = true;
        $scope.signupok = false;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Signup',
            method: 'POST',
            data: { x: user, lang: $rootScope.config.language }
        })
       .then(function (response) {
           if (response.data.d == 'the email address you have entered is already registered') {
               $scope.msg.title = 'E-mail adresa koju ste upisali je već registrirana';
               $scope.msg.css = 'danger';
               $scope.msg.icon = 'exclamation';
               $scope.hidebutton = false;
               $scope.signupok = false;
           }
           if (response.data.d == 'registration completed successfully') {
               $scope.msg.title = 'Registracija upješno završena';
               $scope.msg.css = 'success';
               $scope.msg.icon = 'check';
               $scope.hidebutton = true;
               $scope.signupok = true;
               window.location.hash = 'registration';
           }
       },
       function (response) {
           alert(response.data.d);
           $scope.hidebutton = false;
           $scope.signupok = false;
       });
    }
   
}])

.controller('orderCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.application = $rootScope.application;
    $scope.version = $rootScope.version;
    $scope.userType = 1;
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
         $scope.user.version = $rootScope.application == 'Program Prehrane' ? 'PREMIUM' : 'STANDARD';
         $scope.user.licence = 1; // $rootScope.application == 'Program Prehrane' ? '1' : '0';
         $scope.user.licenceNumber = 1;
         $scope.user.userType = $scope.userType;
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

    var maxUsers = function () {
        $scope.maxUsers = [];
        for (var i = 5; i < 101; i++) {
            $scope.maxUsers.push(i);
        }
    }
    maxUsers();

    $scope.premiumUsers = 5;

    $scope.setPremiumUsers = function (x) {
        $scope.premiumUsers = x;
        $scope.calculatePrice();
    }

    $scope.calculatePrice = function () {
        var unitprice = 0;
        var totalprice = 0;

        if ($scope.user.application == 'Program Prehrane Web') {
            if ($scope.user.userType == 0) { unitprice = 550; $scope.user.version = 'START'; }
            if ($scope.user.userType == 1) { unitprice = 950; $scope.user.version = 'STANDARD'; }
            if ($scope.user.userType == 2) { unitprice = 1850; $scope.user.version = 'PREMIUM'; }

            if ($scope.user.licence > 1) {
                unitprice = unitprice * $scope.user.licence - ((unitprice * $scope.user.licence) * ($scope.user.licence / 10))
            }

            $scope.user.licenceNumber = 1;
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
        var additionalUsers = $scope.premiumUsers > 5 && $scope.user.userType == 2 ? ($scope.premiumUsers - 5) * 500 : 0;  // 500kn/additional user;
        $scope.user.price = totalprice + additionalUsers;
        $scope.user.priceEur = (totalprice + additionalUsers) / $rootScope.config.eur;
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
            $scope.errorMessage = 'Sva polja su obavezna.';
            return false;
        }
        if ($scope.userType == 1) {
            if (user.companyName == '' || user.pin == '') {
                $scope.showErrorAlert = true;
                $scope.errorMessage = 'Sva polja su obavezna.';
                return false;
            }
        }

        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem';
        $scope.isSendButtonDisabled = true;
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/SendOrder',
            method: 'POST',
            data: { x: user, lang: $rootScope.config.language }
        })
       .then(function (response) {
           if (response.data.d == 'error') {
               $scope.showAlert = false;
               $scope.showPaymentDetails = false;
               $scope.isSendButtonDisabled = false;
               $scope.sendicon = 'fa fa-paper-plane-o';
               $scope.sendicontitle = 'Pošalji';
               alert("GREŠKA! Narudžba nije poslana.");
           } else {
               $scope.showAlert = true;
               $scope.showPaymentDetails = true;
               window.location.hash = 'orderform';
           }
       },
       function (response) {
           $scope.showAlert = false;
           $scope.showPaymentDetails = false;
           $scope.isSendButtonDisabled = false;
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
            if (user.userId != null) {
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
                $scope.errorMessage = 'Pogrešna kombinacija korisničkog imena i lozinke'
            }
        },
        function (response) {
            $scope.errorLogin = true;
            $scope.showErrorAlert = true;
            $scope.errorMessage = 'Pogrešna kombinacija korisničkog imena i lozinke'
            $scope.showUserDetails = false;
        });
    }

    $scope.registration = function () {
        window.location.hash = 'registration';
    }

    $scope.gotoForm = function () {
        $scope.showUserDetails = true;
    }

    $scope.forgotPassword = function (x) {
        $scope.showErrorAlert = false;
        $scope.showSuccessAlert = false;
        if (x == null || x == '') {
            $scope.showErrorAlert = true;
            $scope.errorMessage = 'Upišite E-mail koji ste koristili za registraciju.'
        } else {
            $http({
                url: $rootScope.config.backend + 'Users.asmx/ForgotPassword',
                method: "POST",
                data: { email: x, lang: $rootScope.config.language }
            })
          .then(function (response) {
              $scope.showSuccessAlert = true;
              $scope.successMessage = JSON.parse(response.data.d);
          },
          function (response) {
              $scope.showErrorAlert = true;
              $scope.errorMessage = JSON.parse(response.data.d);
          });
        }
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
            data: { name: d.name, email: d.email, messageSubject: 'Program Prehrane - Upit', message: d.message, lang: $rootScope.config.language }
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

.controller('foodCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $scope.config = response.data;
              load(null);
          });
    };

    $scope.loading = false;
    var load = function (x) {
        $scope.loading = true;
        $http({
            url: $scope.config.backend + 'Foods.asmx/LoadFoods',
            method: 'POST',
            data: { lang: 'hr' }
        })
       .then(function (response) {
           $scope.loading = false;
           $scope.foods = JSON.parse(response.data.d);
       },
       function (response) {
           $scope.loading = false;
           alert(response.data.d);
       });
    };

    getConfig();

    $scope.limit = 50;
    $scope.showMore = function (x) {
        $scope.limit += x;
    }

}])

;