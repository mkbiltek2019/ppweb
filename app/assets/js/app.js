/*!
app.js
(c) 2017-2019 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'pascalprecht.translate', 'ngMaterial', 'chart.js', 'ngStorage', 'functions', 'charts'])

.config(['$translateProvider', '$translatePartialLoaderProvider', '$httpProvider', function ($translateProvider, $translatePartialLoaderProvider, $httpProvider) {

    $translateProvider.useLoader('$translatePartialLoader', {
         urlTemplate: './assets/json/translations/{lang}/{part}.json'
    });
    $translateProvider.preferredLanguage('en');
    $translatePartialLoaderProvider.addPart('main');
    $translateProvider.useSanitizeValueStrategy('escape');

    //*******************disable catche**********************
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************

}])

.config(function($mdDateLocaleProvider) {
        $mdDateLocaleProvider.formatDate = function(date) {
            return moment(date).format("DD.MM.YYYY");
        }
})

.controller('AppCtrl', ['$scope', '$mdDialog', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', function ($scope, $mdDialog, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions) {
    $rootScope.loginUser = $sessionStorage.loginuser;
    $rootScope.user = $sessionStorage.user;
    $scope.today = new Date();
    $rootScope.unitSystem = 1;

    if ((navigator.userAgent.indexOf("MSIE") != -1 ) || (!!document.documentMode == true )) {
        $rootScope.browserMsg = {
            title: 'you are currently using internet explorer',
            description: 'for a better experience in using the application, please use some of the modern browsers such as google chrome, mozilla firefox, microsoft edge etc.'
        };
    }

    if (angular.isDefined($sessionStorage.user)) {
        if ($sessionStorage.user != null) {
            if ($sessionStorage.user.licenceStatus == 'demo') {
                $rootScope.mainMessage = $translate.instant('you are currently working in a demo version') + '. ' + $translate.instant('some functions are disabled') + '.';
                $rootScope.mainMessageBtn = $translate.instant('activate full version');
            }
        }
    }

    $rootScope.initMyCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/Init',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.myCalculation = JSON.parse(response.data.d);
            $rootScope.myCalculation.recommendedEnergyIntake = null;
            $rootScope.myCalculation.recommendedEnergyExpenditure = null;
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getConfig = function () {
        if (location.search.substring(1, 5) == 'lang') {
            var queryLang = location.search.substring(6);
        }
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              $sessionStorage.config = response.data;
              if (localStorage.language) {
                  $rootScope.setLanguage(localStorage.language);
              } else {
                  $rootScope.setLanguage($rootScope.config.language);
              }
              if (angular.isDefined(queryLang)) {
                  if (queryLang == 'hr' || queryLang == 'ba' || queryLang == 'sr' || queryLang == 'sr_cyrl' || queryLang == 'en') {
                      $rootScope.setLanguage(queryLang);
                  }
              }
              if ($sessionStorage.islogin == true) { $rootScope.loadData(); }
              if (angular.isUndefined($rootScope.myCalculation)) { $rootScope.initMyCalculation() };
              if (localStorage.version) {
                  if (localStorage.version != $rootScope.config.version) {
                      $timeout(function () {
                          openNotificationPopup();
                      }, 600);
                  }
              } else {
                  $timeout(function () {
                      openNotificationPopup();
                  }, 600);
              }
          });
    };
    getConfig();

    $rootScope.getUserSettings = function () {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/GetFile',
            method: "POST",
            data: { foldername: 'users/' + $rootScope.user.userGroupId, filename: 'settings' }
        })
       .then(function (response) {
           $rootScope.settings = angular.fromJson(response.data.d);
           if (response.data.d != null) {
               if ($rootScope.settings.language != '') { $rootScope.config.language = $rootScope.settings.language; }
               if ($rootScope.settings.currency != '') { $rootScope.config.currency = $rootScope.settings.currency; }
           } else {
               $rootScope.settings = {
                   language: $rootScope.config.language,
                   currency: $rootScope.config.currency
               }
           }
           $sessionStorage.settings = $rootScope.settings;
       },
       function (response) {
           functions.alert($translate.instant(response.data.d), '');
       });
    }

    $rootScope.setLanguage = function (x) {
        $translate.use(x);
        $translatePartialLoader.addPart('main');
        $rootScope.config.language = x;
        if (typeof (Storage) !== "undefined") {
            localStorage.language = x;
        }
        $sessionStorage.config.language = x;
        if ($sessionStorage.usergroupid != undefined || $sessionStorage.usergroupid != null) {
            $rootScope.loadData();
        }
    };

    $rootScope.loadFoods = function () {
        $rootScope.loading = true;
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/Load',
            method: "POST",
            data: { userId:$sessionStorage.usergroupid, userType:$rootScope.user.userType, lang:$rootScope.config.language }
        })
        .then(function (response) {
            var data = JSON.parse(response.data.d);
            $rootScope.foods = data.foods;
            $rootScope.myFoods = data.myFoods;
            $rootScope.foodGroups = data.foodGroups;
            $rootScope.loading = false;
        },
        function (response) {
            $rootScope.loading = false;
            alert(response.data.d)
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

    $rootScope.loadGoals = function () {
        $http({
            url: $sessionStorage.config.backend + 'Goals.asmx/Load',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.goals = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    $rootScope.loadActivities = function () {
        $rootScope.loading = true;
        $http({
            url: $sessionStorage.config.backend + 'Activities.asmx/Load',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.activities = JSON.parse(response.data.d);
            angular.forEach($rootScope.activities, function (value, key) {
                $rootScope.activities[key].activity = $translate.instant($rootScope.activities[key].activity).replace('&gt;', '<').replace('&lt;', '>');
            })
            $rootScope.loading = false;
        },
        function (response) {
            $rootScope.loading = false;
            alert(response.data.d)
        });
    };

    $rootScope.loadDiets = function () {
        $http({
            url: $sessionStorage.config.backend + 'Diets.asmx/Load',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.diets = JSON.parse(response.data.d);
            angular.forEach($rootScope.diets, function (value, key) {
                $rootScope.diets[key].diet = $translate.instant($rootScope.diets[key].diet).replace('&gt;', '<').replace('&lt;', '>');;
                $rootScope.diets[key].dietDescription = $translate.instant($rootScope.diets[key].dietDescription).replace('&gt;', '<').replace('&lt;', '>');
                $rootScope.diets[key].note = $translate.instant($rootScope.diets[key].note).replace('&gt;', '<').replace('&lt;', '>');
            })
        },
        function (response) {
            alert(response.data.d)
        });
    };

    $rootScope.loadData = function () {
        if ($sessionStorage.user == null) {
            $scope.toggleTpl('login');
            $rootScope.isLogin = false;
        } else {
            $rootScope.loadFoods();
            $rootScope.loadPals();
            $rootScope.loadGoals();
            $rootScope.loadActivities();
            $rootScope.loadDiets();
        }
    }

    $scope.toggleTpl = function (x) {
        $rootScope.currTpl = './assets/partials/' + x + '.html';
    };

    var checkUser = function () {
        if ($sessionStorage.userid == "" || $sessionStorage.userid == undefined || $sessionStorage.user == null || $sessionStorage.user.licenceStatus == 'expired') {
            $scope.toggleTpl('login');
            $rootScope.isLogin = false;
        } else {
            $scope.toggleTpl('dashboard');
            $scope.activeTab = 0;
            $rootScope.isLogin = true;
        }
    }
    checkUser();

    var validateForm = function () {
        if ($rootScope.clientData.clientId == null) {
            return false;
        }
        if ($rootScope.clientData.height <= 0) {
            functions.alert($translate.instant('height is required'));
            return false;
        }
        if ($rootScope.clientData.weight <= 0) {
            functions.alert($translate.instant('weight is required'));
            return false;
        }
        if ($rootScope.clientData.pal.value <= 0) {
            functions.alert($translate.instant('choose physical activity level'));
            return false;
        }
        return true;
    }
    
    $scope.toggleNewTpl = function (x) {
        if ($rootScope.clientData !== undefined) {
            if (validateForm() == false) {
                return false;
            };
            if ($rootScope.clientData.meals == null) {
                $rootScope.newTpl = 'assets/partials/meals.html';
                $rootScope.selectedNavItem = 'meals';
                functions.alert($translate.instant('choose meals'), '');
                return false;
            }
            if (x == 'menu' && $rootScope.clientData.meals.length > 0 && !$rootScope.isMyMeals && $rootScope.clientData.meals[0].code == 'B') {
                if ($rootScope.clientData.meals[1].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                    $rootScope.newTpl = './assets/partials/meals.html';
                    functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                    return false;
                }
                if ($rootScope.clientData.meals[3].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                    $rootScope.newTpl = './assets/partials/meals.html';
                    functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                    return false;
                }
            }
            if (x == 'menu') {
                if ($rootScope.myMeals !== undefined) {
                    $rootScope.setMealCode();
                }
            }
            $rootScope.saveClientData($rootScope.clientData);
        }
        $rootScope.newTpl = './assets/partials/' + x + '.html';
        $rootScope.selectedNavItem = x;
    };
    $scope.toggleNewTpl('clientsdata');

    $scope.logout = function () {
        $sessionStorage.loginuser = null;
        $sessionStorage.user = null;
        $rootScope.user = null;
        $sessionStorage.userid = "";
        $sessionStorage.username = "";
        $rootScope.isLogin = false;
        $rootScope.client = null;
        $rootScope.isLogin = false;
        $sessionStorage.islogin = false;
        $sessionStorage.usergroupid = null;
        $rootScope.mainMessage = null;
        $rootScope.currTpl = 'assets/partials/login.html';
    }

    $rootScope.saveClientData = function (x) {
        if (validateForm() == false) {
            return false;
        };
        if ($rootScope.clientData.meals == null) {
            $rootScope.newTpl = 'assets/partials/meals.html';
            $rootScope.selectedNavItem = 'meals';
            functions.alert($translate.instant('choose meals'), '');
            return false;
        }
        if ($rootScope.clientData.meals.length > 0 && !$rootScope.isMyMeals && $rootScope.clientData.meals[0].code == 'B') {
            if ($rootScope.clientData.meals[1].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                $rootScope.newTpl = 'assets/partials/meals.html';
                functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                return false;
            }
            if ($rootScope.clientData.meals[3].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                $rootScope.newTpl = 'assets/partials/meals.html';
                functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                return false;
            }
        }
        saveClientData(x);
    }

    var saveClientData = function (x) {
        /*if ($rootScope.user.licenceStatus == 'demo') {
            if ($rootScope.newTpl == 'assets/partials/clientsdata.html') {
                functions.demoAlert('the saving function is disabled in demo version');
            }
            return false;
        }*/
        x.userId = $rootScope.user.userId;
        x.clientId = x.clientId == null ? $rootScope.client.clientId: x.clientId;
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Save',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, x: x, userType: $rootScope.user.userType }
        })
       .then(function (response) {
           $rootScope.clientData.date = new Date($rootScope.clientData.date);
       },
       function (response) {
           alert(response.data.d)
       });
    }

    $scope.hideMsg = function () {
        $rootScope.mainMessage = null;
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

    $scope.changeUnitSystem = function (x) {
        $rootScope.unitSystem = x;
        $rootScope.convertToStandardSystem();
    }

    $scope.standard = {
        height_feet: 0,
        height_inches: 0,
        weight: 0,
        waist: 0,
        hip: 0
    }

    $scope.convertToMetricSystem = function (x) {
        $rootScope.clientData.height = (x.height_feet * 30.48 + x.height_inches * 2.54).toFixed(1);
        $rootScope.clientData.weight = (x.weight * 0.45349237).toFixed(1);
        $rootScope.clientData.waist = (x.waist * 2.54).toFixed(1);
        $rootScope.clientData.hip = (x.hip * 2.54).toFixed(1);
      /*  Cm.Text = Format(Feet * 30.48 + Inches * 2.54, "0")   'visina cm
        Kg.Text = Format(Pounds * 0.45349237, "0")   'masa kg
        Waist_cm.Text = Format(Waist_inches * 2.54, "0")  'opseg struka cm
        Hip_cm.Text = Format(Hip_inches * 2.54, "0")  'opseg bokova cm */
    }

    $rootScope.convertToStandardSystem = function () {
        var height_inches = $rootScope.clientData.height * 0.3937;
        $scope.standard.height_feet = (parseInt(height_inches / 12)).toFixed(0);
        var rest_height_feet = (height_inches / 12) - parseInt(height_inches / 12);
        var rest_height_inches = (rest_height_feet * 12);
        $scope.standard.height_inches = (rest_height_inches).toFixed(0);
        $scope.standard.weight = ($rootScope.clientData.weight / 0.45349237).toFixed(0);
        $scope.standard.waist = ($rootScope.clientData.waist / 2.54).toFixed(0);
        $scope.standard.hip = ($rootScope.clientData.hip / 2.54).toFixed(0);
    }

    $scope.populateDdl = function (from, to) {
        var list = [];
        for (var i = from; i <= to; i++) {
            list.push(i);
        }
        return list;
    }

    $scope.showUpdates = false;
    $scope.toggleUpdates = function () {
        $scope.showUpdates = !$scope.showUpdates;
    };

    $scope.dateDiff = function () {
        if (localStorage.lastvisit) {
            return functions.getDateDiff(localStorage.lastvisit)
        } else {
            return 0;
        }
    }

    var openNotificationPopup = function () {
        $mdDialog.show({
            controller: notificationPoupCtrl,
            templateUrl: 'assets/partials/popup/notification.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: {}
        })
        .then(function (response) {
            window.location.reload(true);
        }, function () {
            window.location.reload(true);
        });
    };

    var notificationPoupCtrl = function ($scope, $rootScope, $mdDialog, $localStorage) {
        $scope.config = $rootScope.config;
        $scope.showUpdates = false;
        $scope.toggleUpdates = function () {
            $scope.showUpdates = !$scope.showUpdates;
        };

        if (typeof (Storage) !== "undefined") {
            localStorage.version = $scope.config.version;
        }

        $scope.hide = function () {
            $mdDialog.hide();
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }

    var socialSharePopup = function () {
        if (typeof (Storage) !== "undefined") {
            if (!localStorage.socailshare) {
                $timeout(function () {
                    openSocialSharePopup();
                }, 600000);
            }
        }
    }
    socialSharePopup();
    
    var openSocialSharePopup = function () {
        $mdDialog.show({
            controller: socialSharePoupCtrl,
            templateUrl: 'assets/partials/popup/socialshare.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: {}
        })
        .then(function (response) {
        }, function () {
        });
    };

    var socialSharePoupCtrl = function ($scope, $rootScope, $mdDialog, $localStorage) {
        localStorage.socailshare = 'ok';

        $scope.hide = function () {
            $mdDialog.hide();
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }

    $scope.reportABug = function () {
        openReportABugPopup();
    }

    var openReportABugPopup = function () {
        $mdDialog.show({
            controller: openReportABugPopupCtrl,
            templateUrl: 'assets/partials/popup/reportabug.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { user: $rootScope.user }
        })
       .then(function (x) {
       }, function () {
       });
    }

    var openReportABugPopupCtrl = function ($scope, $mdDialog, $http, d, $translate, functions) {
        $scope.d = {
            description: null,
            email: d.user.email,
            alert: null
        }

        var send = function (x) {
            $scope.titlealert = null;
            $scope.emailalert = null;
            if (functions.isNullOrEmpty(x.description)) {
                x.alert = $translate.instant('description is required');
                return false;
            }
            if (functions.isNullOrEmpty(x.email)) {
                x.alert = $translate.instant('email is required');
                return false;
            }
            $mdDialog.hide();
            var subject = x.description + ' E-mail: ' + x.email;
            $http({
                url: $sessionStorage.config.backend + 'Mail.asmx/SendMessage',
                method: "POST",
                data: { sendTo: $sessionStorage.config.email, messageSubject: 'BUG - ' + x.title, messageBody: subject, lang: $rootScope.config.language }
            })
            .then(function (response) {
                functions.alert(response.data.d, '');
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            send(x);
        }

    };


    /************************/







}])

.controller('loginCtrl', ['$scope', '$http','$localStorage', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', '$mdDialog', function ($scope, $http, $localStorage, $sessionStorage, $window, $rootScope, functions, $translate, $mdDialog) {
    var webService = 'Users.asmx';

    $scope.toggleTpl = function (x) {
        $scope.tpl = x;
    }
    $scope.toggleTpl('loginTpl');

    $scope.login = function (u, p) {
        $scope.errorMesage = null;
        if (functions.isNullOrEmpty(u) || functions.isNullOrEmpty(p)) {
            $scope.errorLogin = true;
            $scope.errorMesage = $translate.instant('enter user name (email) and password');
            return false;
        }
        $rootScope.loading = true;
        $http({
            url: $rootScope.config.backend + webService + '/Login',
            method: "POST",
            data: {
                userName: u,
                password: p
            }
        })
        .then(function (response) {
            if (JSON.parse(response.data.d).userName == u) {
                $rootScope.user = JSON.parse(response.data.d);
                if ($rootScope.user.userId !== $rootScope.user.userGroupId && $rootScope.user.isActive === false) {
                    $rootScope.loading = false;
                    $scope.errorLogin = true;
                    $scope.errorMesage = $translate.instant('your account is not active') + '. ' + $translate.instant('please contact your administrator');
                    return false;
                }
                $rootScope.loginUser = JSON.parse(response.data.d);
                $sessionStorage.loginuser = $rootScope.loginUser;
                $sessionStorage.userid = $rootScope.user.userId;
                $sessionStorage.usergroupid = $rootScope.user.userGroupId;
                $sessionStorage.username = $rootScope.user.userName;
                $sessionStorage.user = $rootScope.user;
                $sessionStorage.islogin = true;
                $rootScope.isLogin = true;
                $rootScope.loadData();

                if (typeof (Storage) !== "undefined") {
                    localStorage.lastvisit = new Date();
                }

                //   $rootScope.getUserSettings();  //TODO

                if ($rootScope.user.licenceStatus == 'expired') {
                    $rootScope.isLogin = false;
                    functions.alert($translate.instant('your account has expired'), $translate.instant('renew now'));
                    $rootScope.currTpl = './assets/partials/order.html';
                } else {
                    $rootScope.currTpl = './assets/partials/dashboard.html';
                    //var daysToExpite = functions.getDateDiff($rootScope.user.expirationDate);
                    if ($rootScope.user.daysToExpite <= 10 && $rootScope.user.daysToExpite > 0) {
                        $rootScope.mainMessage = $translate.instant('your subscription will expire in') + ' ' + $rootScope.user.daysToExpite + ' ' + ($rootScope.user.daysToExpite == 1 ? $translate.instant('day') : $translate.instant('days')) + '.';
                        $rootScope.mainMessageBtn = $translate.instant('renew subscription');
                    }
                    if ($rootScope.user.daysToExpite == 0) {
                        $rootScope.mainMessage = $translate.instant('your subscription will expire today') + '.';
                        $rootScope.mainMessageBtn = $translate.instant('renew subscription');
                    }
                    if ($rootScope.user.licenceStatus == 'demo') {
                        $rootScope.mainMessage = $translate.instant('you are currently working in a demo version') + '. ' + $translate.instant('some functions are disabled') + '.';
                        $rootScope.mainMessageBtn = $translate.instant('activate full version');
                    }
                    if ($rootScope.config.language == 'en') {
                        $rootScope.unitSystem = 0;
                    } else {
                        $rootScope.unitSystem = 1;
                    }
                }

                /**** TODO (QUERY STRING) *****
                var lang = $sessionStorage.config.language;
                $window.location.href = lang == 'hr' ? '../app/' : '../app/?lang=' + lang;
                ***************/

            } else {
                $rootScope.loading = false;
                $scope.errorLogin = true;
                $scope.errorMesage = $translate.instant('wrong user name or password');
               // $rootScope.currTpl = 'assets/partials/signup.html';  //<< Only for first registration
            }
        },
        function (response) {
            $scope.errorLogin = true;
            $scope.errorMesage = $translate.instant('user was not found');
        });
     }

     $scope.signup = function () {
         $rootScope.currTpl = 'assets/partials/signup.html';
     }

     $scope.forgotPasswordPopup = function () {
         $mdDialog.show({
             controller: $scope.forgotPasswordPopupCtrl,
             templateUrl: 'assets/partials/popup/forgotpassword.html',
             parent: angular.element(document.body),
             targetEvent: '',
             clickOutsideToClose: true,
             fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
             d: ''
         })
         .then(function (response) {
         }, function () {
         });
     };

     $scope.forgotPasswordPopupCtrl = function ($scope, $mdDialog, $http, $translate) {
         $scope.confirm = function (x) {
             forgotPassword(x);
         }

         var forgotPassword = function (x) {
             $http({
                 url: $sessionStorage.config.backend + webService + '/ForgotPassword',
                 method: "POST",
                 data: { email: x, lang: $rootScope.config.language }
             })
           .then(function (response) {
               $mdDialog.hide();
               functions.alert(JSON.parse(response.data.d), '');
           },
           function (response) {
               functions.alert(response.data.d, '');
           });
         }

         $scope.hide = function () {
             $mdDialog.hide();
         };

         $scope.cancel = function () {
             $mdDialog.cancel();
         };
     }

}])

.controller('signupCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, functions, $translate) {
    var webService = 'Users.asmx';
    $scope.showAlert = false;
    $scope.passwordConfirm = '';
    $scope.emailConfirm = '';
    $scope.signupdisabled = false;
    $scope.accept = false;

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.newUser = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }
    init();

    $scope.signup = function () {
        $scope.signupdisabled = true;
        $scope.newUser.userName = $scope.newUser.email;
        if (functions.isNullOrEmpty($scope.newUser.firstName) || functions.isNullOrEmpty($scope.newUser.lastName) || functions.isNullOrEmpty($scope.newUser.email) || functions.isNullOrEmpty($scope.newUser.password) || functions.isNullOrEmpty($scope.passwordConfirm) || functions.isNullOrEmpty($scope.emailConfirm)) {
            functions.alert($translate.instant('all fields are required'), '');
            $scope.signupdisabled = false;
            return false;
        }
        if ($scope.newUser.email != $scope.emailConfirm) {
            $scope.signupdisabled = false;
            functions.alert($translate.instant('emails are not the same'), '');
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            $scope.signupdisabled = false;
            functions.alert($translate.instant('passwords are not the same'), '');
            return false;
        }
        if ($scope.accept == false) {
            $scope.signupdisabled = false;
            functions.alert($translate.instant('you must agree to the terms and conditions'), '');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/Signup',
            method: "POST",
            data: { x: $scope.newUser, lang: $rootScope.config.language }
        })
        .then(function (response) {
            if (response.data.d == 'registration completed successfully') {
                $scope.alertMessage = response.data.d;
                $scope.showAlert = true;
            } else {
                functions.alert($translate.instant(response.data.d), '');
            }
        },
        function (response) {
            $scope.showAlert = false;
            $scope.signupdisabled = false;
            functions.alert($translate.instant(response.data.d), '');
        });
    }

}])

.controller("schedulerCtrl", ['$scope', '$localStorage', '$http', '$rootScope', '$timeout', '$sessionStorage', 'functions', '$translate', function ($scope, $localStorage, $http, $rootScope, $timeout, $sessionStorage, functions, $translate) {
    var webService = 'Scheduler.asmx';
    $scope.id = '#myScheduler';
    $scope.room = 0;
    $scope.uid = null;

    var showScheduler = function () {
        YUI().use('aui-scheduler', function (Y) {
            var agendaView = new Y.SchedulerAgendaView();
            var dayView = new Y.SchedulerDayView();
            var weekView = new Y.SchedulerWeekView();
            var monthView = new Y.SchedulerMonthView();
            var eventRecorder = new Y.SchedulerEventRecorder({
                on: {
                    save: function (event) {
                        addEvent(this.getTemplateData(), event);
                        //  alert('Save Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                    },
                    edit: function (event) {
                        addEvent(this.getTemplateData(), event);
                       /* var startDatePrev = null;
                        var endDatePrev = null;
                        event.newSchedulerEvent.on("startDateChange", function (event) {
                            startDatePrev = event.prevVal;
                          //  var DateNew = event.newVal;
                            alert(startDatePrev)
                        });
                        addEvent(this.getTemplateData(), event, startDatePrev, endDatePrev);*/
                       //  editEvent(this.getTemplateData(), event);
                       // alert('Edit Event:' + this.isNew() + ' --- ' + this.getContentNode().val() + ' --- ' + JSON.stringify(this.getTemplateData()));
                    },
                    delete: function (event) {
                        removeEvent(this.getTemplateData(), event);
                        // alert('Delete Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                        //  Note: The cancel event seems to be buggy and occurs at the wrong times, so I commented it out.
                    }
                    //endDateChange: function (event) {
                    //    alert('ok')
                    //}
                    //          cancel: function(event) {
                    //              alert('Cancel Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                    //}
                }
            });

            $scope.id = $scope.uid == null ? 'myScheduler' : $scope.uid;

            new Y.Scheduler({
                activeView: weekView,
                boundingBox: '#' + $scope.id,
                date: new Date(),
                eventRecorder: eventRecorder,
                items: $rootScope.events,
                render: true,
                views: [dayView, weekView, monthView, agendaView],
                strings: {
                    agenda: $translate.instant('agenda'),
                    day: $translate.instant('day_'),
                    month: $translate.instant('month'),
                    table: $translate.instant('table'),
                    today: $translate.instant('today'),
                    week: $translate.instant('week'),
                    year: $translate.instant('year')
                },
            }
          );
        });
    }

    var getUsers = function () {
        $http({
            url: $sessionStorage.config.backend +'Users.asmx/GetUsersByUserGroup',
            method: 'POST',
            data: { userGroupId: $rootScope.user.userGroupId }
        })
      .then(function (response) {
          $scope.users = JSON.parse(response.data.d);
          $scope.getSchedulerEvents(null);
      },
      function (response) {
          functions.alert($translate.instant(response.data.d));
      });
    };

    $scope.getSchedulerEvents = function (uid) {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetSchedulerEvents',
            method: 'POST',
            data: { user: $rootScope.user, room: $scope.room, uid: uid }
        })
       .then(function (response) {
           $rootScope.events = JSON.parse(response.data.d);
           $timeout(function () {
               showScheduler();
           }, 200);
       },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    };
    getUsers();

    var addEvent = function (x, event) {
        $rootScope.events.push({
            room: $scope.room,
            clientId: angular.isDefined($rootScope.client) && $rootScope.client != null ? $rootScope.client.clientId : null, //  null,  // << TODO
            content: event.details[0].newSchedulerEvent.changed.content,
            endDate: x.endDate,
            startDate: x.startDate,
            userId: $rootScope.user.userId
        });

        var eventObj = {};
        eventObj.room = $scope.room;
        eventObj.clientId = angular.isDefined($rootScope.client) && $rootScope.client != null ? $rootScope.client.clientId : null;
        eventObj.content = event.details[0].newSchedulerEvent.changed.content == null ? x.content : event.details[0].newSchedulerEvent.changed.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        eventObj.userId = $rootScope.user.userId;

        var eventObj_old = {};
        eventObj_old.room = $scope.room;
        eventObj_old.clientId = angular.isDefined($rootScope.client) && $rootScope.client != null ? $rootScope.client.clientId : null;
        eventObj_old.content = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.content) ? x.content : event.details[0].newSchedulerEvent.lastChange.content.prevVal;
        eventObj_old.endDate = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.endDate) ? x.endDate : Date.parse(event.details[0].newSchedulerEvent.lastChange.endDate.prevVal);
        eventObj_old.startDate = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.startDate) ? x.startDate : Date.parse(event.details[0].newSchedulerEvent.lastChange.startDate.prevVal);
        eventObj_old.userId = $rootScope.user.userId;
        remove(eventObj_old);

        $timeout(function () {
             save(eventObj);
        }, 500);
    }

    var save = function (x) {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('the saving function is disabled in demo version');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/Save',
            method: "POST",
            data: { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId, x: x }
        })
        .then(function (response) {
            getAppointmentsCountByUserId();
        },
        function (response) {
            functions.alert($translate.instant(response.data.d));
        });
    }

    var removeEvent = function (x, event) {
        var eventObj = {};
        eventObj.room = $scope.room;
        eventObj.clientId = '0';
        eventObj.content = x.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        eventObj.userId = $rootScope.user.userId;
        remove(eventObj);
    }

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId, x: x }
        })
        .then(function (response) {
            getAppointmentsCountByUserId();
        },
        function (response) {
            functions.alert($translate.instant(response.data));
        });
    }

    $scope.toggleTpl = function (x) {
        $rootScope.currTpl = './assets/partials/' + x + '.html';
    };

    var getAppointmentsCountByUserId = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetAppointmentsCountByUserId',
            method: 'POST',
            data: { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId },
        }).then(function (response) {
            $rootScope.user.datasum.scheduler = JSON.parse(response.data.d);
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }

}])

.controller('userCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Users.asmx';

    $scope.adminTypes = [
       {
           value: 0,
           text: 'Supervizor'
       },
       {
           value: 1,
           text: 'Admin'
       }
    ];

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $scope.newUser = JSON.parse(response.data.d);
            load();
        },
        function (response) {
            functions.alert($translate.instant(response.data.d));
        });
    }

    var load = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetUsersByUserGroup',
            method: 'POST',
            data: { userGroupId: $sessionStorage.usergroupid }
        })
      .then(function (response) {
          $scope.users = JSON.parse(response.data.d);
      },
      function (response) {
          functions.alert($translate.instant(response.data.d));
      });
    };

    init();

    $scope.adminType = function (x) {
        switch (x) {
            case 0:
                return 'Supervizor';
                break;
            case 1:
                return 'Admin';
                break;
            default:
                return '';
        }
    }

    $scope.signup = function () {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }

        if ($scope.users.length >= $rootScope.user.maxNumberOfUsers) {
            functions.alert($translate.instant('max number of users is') + ' ' + $rootScope.user.maxNumberOfUsers, '');
            return false;
        }

        if (!angular.isDefined($rootScope.user)) { $rootScope.user = $scope.newUser; }

        $scope.newUser.userName = $scope.newUser.email;
        $scope.newUser.companyName = $rootScope.user.companyName;
        $scope.newUser.address = $rootScope.user.address;
        $scope.newUser.postalCode = $rootScope.user.postalCode;
        $scope.newUser.city = $rootScope.user.city;
        $scope.newUser.country = $rootScope.user.country;
        $scope.newUser.pin = $rootScope.user.pin;
        $scope.newUser.phone = $rootScope.user.phone;
        $scope.newUser.userGroupId = $rootScope.user.userGroupId;
        $scope.newUser.expirationDate = $rootScope.user.expirationDate;
        $scope.newUser.isActive = true;
        $scope.newUser.adminType = 1;
        $scope.newUser.userType = $rootScope.user.userType;

        if ($scope.newUser.password == "" || $scope.passwordConfirm == "") {
            functions.alert($translate.instant('enter password'), '');
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            functions.alert($translate.instant('passwords are not the same'), '');
            return false;
        }

        $http({
            url: $sessionStorage.config.backend + webService + '/Signup',
            method: "POST",
            data: { x: $scope.newUser, lang: $rootScope.config.language }
        })
        .then(function (response) {
            load();
            functions.alert($translate.instant(response.data.d));
        },
        function (response) {
            functions.alert($translate.instant(response.data.d));
        });
    }

    $scope.update = function (user) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Update',
            method: 'POST',
            data: {x: user}
        })
       .then(function (response) {
           functions.alert($translate.instant('saved'), '');
       },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }

    $scope.showUser = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Get',
            method: 'POST',
            data: { userId: x }
        }).then(function (response) {
            $rootScope.user = JSON.parse(response.data.d);
            $rootScope.currTpl = 'assets/partials/user.html';
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    };

    $scope.updateUser = function (user) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Update',
            method: 'POST',
            data: { x: user }
        }).then(function (response) {
            functions.alert($translate.instant(response.data.d));
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
              .title($translate.instant('delete user') + '?')
              .textContent(x.firstName + ' ' + x.lastName)
              .targetEvent(x)
              .ok($translate.instant('yes') + '!')
              .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (user) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: 'POST',
            data: { x: user }
        }).then(function (response) {
            load();
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }

    $scope.showpass = false;
    $scope.showPassword = function () {
        $scope.showpass = $scope.showpass == true ? false : true;
    }

    /********* Logo ************/
    var isLogoExists = function () {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/IsLogoExists',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, filename: 'logo.png' },
        }).then(function (response) {
            if (response.data.d == 'TRUE') {
                $scope.showLogo = true;
            } else {
                $scope.showLogo = false;
                $scope.logo = null;
            }
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }
    isLogoExists();

    $scope.logo = '../upload/users/' + $rootScope.user.userGroupId + '/logo.png?v=' + new Date().getTime();
    $scope.upload = function () {
        if ($rootScope.user.adminType != 0) { return false; }
        var content = new FormData(document.getElementById("formUpload"));
        $http({
            url: $sessionStorage.config.backend + '/UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            $scope.showLogo = true;
            $scope.logo = '../upload/users/' + $rootScope.user.userGroupId + '/logo.png?v=' + new Date().getTime();
            if (response.data != 'OK') {
                functions.alert($translate.instant(response.data));
            }
            isLogoExists();
        },
       function (response) {
           functions.alert($translate.instant(response.data));
       });
    }

    $scope.removeLogo = function (x) {
        if (x.adminType != 0) { return false; }
        var confirm = $mdDialog.confirm()
                   .title($translate.instant('remove logo') + '?')
                   .targetEvent(x)
                   .ok($translate.instant('yes') + '!')
                   .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            removeLogo(x);
        }, function () {
        });
    }

    var removeLogo = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/DeleteLogo',
            method: 'POST',
            data: { userId: x.userId, filename: 'logo.png' },
        }).then(function (response) {
            $scope.showLogo = false;
            $scope.logo = null;
            if (response.data.d != 'OK') {
                functions.alert($translate.instant(response.data.d));
            }
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }
    /********* Logo ************/


}])

//-------------- Program Prehrane Controllers---------------
.controller('mainCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions) {
    if ($rootScope.client) {
        if ($rootScope.client.clientId) {
            $rootScope.newTpl = 'assets/partials/clientsdata.html',
            $rootScope.selectedNavItem = 'clientsdata';
        } else {
            $rootScope.newTpl = 'assets/partials/dashboard.html',
            $rootScope.selectedNavItem = 'dashboard';
        }
    } else {
        $rootScope.newTpl = 'assets/partials/dashboard.html',
        $rootScope.selectedNavItem = 'dashboard';
    }

}])
.controller('dashboardCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions) {
	var getUser = function () {
        $http({
            url: $sessionStorage.config.backend + 'Users.asmx/Get',
            method: 'POST',
            data: { userId: $rootScope.user.userId },
        }).then(function (response) {
            $rootScope.user = JSON.parse(response.data.d);
            $scope.expirationDate = new Date($rootScope.user.expirationDate);
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }
	getUser();

}])

.controller('clientsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', '$timeout', 'charts', '$filter', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, $timeout, charts, $filter, functions, $translate) {
    var webService = 'Clients.asmx';
    $scope.displayType = 0;

    $scope.toggleTpl = function (x) {
        $scope.clientDataTpl = x;
    };
    $scope.toggleTpl('inputData');

    $scope.toggleSubTpl = function (x) {
        $scope.subTpl = x;
    };

    $scope.toggleCurrTpl = function (x) {
        $rootScope.currTpl = './assets/partials/' + x + '.html';
    };

    var init = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Init',
            method: "POST",
            data: {client:x}
        })
        .then(function (response) {
            $rootScope.clientData = JSON.parse(response.data.d);
            $rootScope.clientData.date = new Date($rootScope.clientData.date);
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    var initClient = function () {
        $http({
            url: $sessionStorage.config.backend + 'Clients.asmx/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.client = JSON.parse(response.data.d);
            $rootScope.client.date = new Date(new Date().setHours(0, 0, 0, 0));
            $scope.d = $rootScope.client;
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }
  
    var getClients = function () {
        $rootScope.loading = true;
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, user: $rootScope.user }
        })
        .then(function (response) {
            $rootScope.clients = JSON.parse(response.data.d);
            $rootScope.loading = false;
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
            $rootScope.loading = false;
        });
    };
    getClients();

    $rootScope.newClient = function () {
        $rootScope.showDetailCalculationOfEnergyExpenditure = false;
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.client = JSON.parse(response.data.d);
            $rootScope.client.date = new Date(new Date().setHours(0, 0, 0, 0));
            $rootScope.clientData = [];
            $rootScope.calculation = [];
            $rootScope.initMyCalculation();
            $scope.d = $rootScope.client;
            $rootScope.goalWeightValue_ = null;
            $scope.openPopup();
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    $scope.openPopup = function () {
        $mdDialog.show({
            controller: $scope.popupCtrl,
            templateUrl: 'assets/partials/popup/client.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            d: $scope.d
        })
        .then(function (response) {
            $rootScope.client = response;
            $rootScope.currTpl = './assets/partials/main.html';
            $scope.toggleNewTpl('clientsdata');
            if ($rootScope.user.licenceStatus == 'demo') {
                init($rootScope.client);
                $rootScope.client.clientId = 'demo';
            } else {
                $scope.get($rootScope.client);
            }
        }, function () {
        });
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, $http, $timeout) {
        $scope.d = d;
        $scope.d.date = new Date($scope.d.date);
        $scope.d.birthDate = new Date($scope.d.birthDate);
        $scope.user = $rootScope.user;

        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.firstNameRequiredMsq = null;
        $scope.firstNameRequiredMsq = null;

        $scope.save = function (x) {
            if (x.firstName == '' || x.firstName == null ) {
                $scope.firstNameRequiredMsq = 'first name is required';
                return false;
            } else {
                $scope.firstNameRequiredMsq = null;
                if (functions.getDateDiff(x.birthDate) < 365) {
                    $scope.birthDateRequiredMsq = 'birth date is required';
                    return false;
                } else {
                    $scope.birthDateRequiredMsq = null;
                }
            }
            //if ($rootScope.user.licenceStatus == 'demo') {
            //    $mdDialog.hide(x);
            //    return false;
            //}
            x.userId = $sessionStorage.userid;
            $http({
                url: $sessionStorage.config.backend + webService + '/Save',
                method: 'POST',
                data: { user: $rootScope.user, x: x, lang: $rootScope.config.language }
            })
           .then(function (response) {
               if (JSON.parse(response.data.d).message != null) {
                   functions.alert($translate.instant(JSON.parse(response.data.d).message), '');
                   return false;
               }
               getClients();
               $timeout(function () {
                   $mdDialog.hide(JSON.parse(response.data.d).data);
               }, 500);
           },
           function (response) {
               functions.alert($translate.instant(response.data.d), '');
           });
        }
    }

    $scope.edit = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Get',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
            $scope.openPopup();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.search = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, user: $rootScope.user }
        })
       .then(function (response) {
           $rootScope.clients = JSON.parse(response.data.d);
           $scope.d = JSON.parse(response.data.d);
           $scope.openSearchPopup();
       },
       function (response) {
           alert(response.data.d)
       });
    }

    $scope.openSearchPopup = function () {
        $mdDialog.show({
            controller: $scope.searchPopupCtrl,
            templateUrl: 'assets/partials/popup/searchclients.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            d: $scope.d
        })
        .then(function (response) {
            $rootScope.client = response;
            $rootScope.currTpl = './assets/partials/main.html';
            $scope.toggleNewTpl('clientsdata');
            $scope.get(response);
        }, function () {
        });
    };

    $scope.searchPopupCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = d;
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        $scope.getDateFormat = function (x) {
            return new Date(x);
        }
        $scope.hide = function (x) {
            $mdDialog.hide(x);
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.get = function (x) {
            $scope.hide(x);
        }

        $scope.addNewClient = function () {
            $mdDialog.cancel();
            $rootScope.newClient();
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                  .title($translate.instant('are you sure you want to delete') + '?')
                  .textContent($translate.instant('client') + ': ' + x.firstName + ' ' + x.lastName)
                  .targetEvent(x)
                  .ok($translate.instant('yes'))
                  .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        };

        var remove = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Delete',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, clientId: x.clientId, user: $rootScope.user }
            })
           .then(function (response) {
               $rootScope.clients = JSON.parse(response.data.d);
               $rootScope.client = [];
               $rootScope.clientData = [];
           },
           function (response) {
               alert(response.data.d)
           });
        }
    }

    $scope.get = function (x) {
        $rootScope.showDetailCalculationOfEnergyExpenditure = false;
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Get',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            if (JSON.parse(response.data.d).id != null) {
                $rootScope.clientData = JSON.parse(response.data.d);
                $rootScope.clientData.date = new Date(new Date().setHours(0, 0, 0, 0));
                $scope.getPalDetails($rootScope.clientData.pal.value);
                getCalculation();
                getMyCalculation();
                if ($rootScope.clientData.dailyActivities.activities == null) {
                    $rootScope.clientData.dailyActivities.activities = [];
                }
                if ($rootScope.clientData.dailyActivities.activities.length > 0) {
                    $rootScope.showDetailCalculationOfEnergyExpenditure = true;
                }
                if ($rootScope.unitSystem == 0 && $rootScope.config.language == 'en') {
                    $rootScope.convertToStandardSystem();
                }
                $rootScope.goalWeightValue_ = null;
            } else {
                init(x);
            }
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.getPalDetails = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetPalDetails',
            method: "POST",
            data: { palValue: x }
        })
      .then(function (response) {
          $rootScope.pal = JSON.parse(response.data.d)
          $rootScope.pal.value = x;
          $rootScope.clientData.pal = $rootScope.pal;
          $scope.toggleTpl('inputData');
      },
      function (response) {
          alert(response.data.d)
      });
    }

    $scope.getClientLog = function (x) {
        //if ($rootScope.user.licenceStatus == 'demo') {
        //    $scope.toggleTpl('clientStatictic');
        //    return false;
        //}
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            $scope.toggleTpl('clientStatictic');
            $scope.clientLog = JSON.parse(response.data.d);
            angular.forEach($scope.clientLog, function (x, key) {
                x.date = new Date(x.date);
                functions.correctDate(x.date);
            });
            if ($rootScope.goalWeightValue_ == null) {
                getCalculation();
            } else {
                setClientLogGraphData($scope.displayType, $scope.clientLogsDays);
            }
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.removeClientLog = function (x, idx) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete record') + '?')
            .textContent($translate.instant('record date') + ': ' + $filter('date')(x.date, "dd.MM.yyyy") + ', ' + $translate.instant('mass') + ': ' + x.weight + ' kg')
            .targetEvent(x)
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            removeClientLog(x);
        }, function () {
        });
    }

    var removeClientLog = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Delete',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientData: x }
        })
        .then(function (response) {
            $scope.getClientLog(x);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.updateClientLog = function (x) {
        var cd = angular.copy(x);
        cd.date = cd.date.toISOString();
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/UpdateClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientData: cd }
        })
        .then(function (response) {
            $scope.getClientLog(x);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            setClientLogGraphData($scope.displayType, $scope.clientLogsDays);
        },
        function (response) {
            if (response.data.d === undefined) {
                functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
            } else {
                functions.alert(response.data.d, '');
            }
        });
    };

    var getMyCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetMyCalculation',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: $rootScope.client.clientId }
        })
        .then(function (response) {
            $rootScope.myCalculation = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    };

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
    initChartDays();

    $scope.changeDisplayType = function (type, clientLogsDays) {
        setClientLogGraphData(type, clientLogsDays);
    }

    var getRecommendedWeight = function (h) {
        return {
            min: Math.round(((18.5 * h * h) / 10000) * 10) / 10,
            max: Math.round(((25 * h * h) / 10000) * 10) / 10
        }
    }

    $scope.changeGoalWeightValue = function (value, type, clientLogsDays) {
        $rootScope.goalWeightValue_ = parseInt(value);
        setClientLogGraphData(type, clientLogsDays);
    }

    var getGoalLog = function (deficit, key, x, firstWeight, firstDate, currDate) {
        var goal = (firstWeight + (functions.getTwoDateDiff(firstDate, currDate)) * deficit / 7000).toFixed(1);
        var value = 0;
        var goalLimit = $rootScope.goalWeightValue_ !== undefined ? parseInt($rootScope.goalWeightValue_) : 0;
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

        //TODO - goal (depending of type, reduction increase, fixed Goal)
        if (angular.isDefined($rootScope.calculation.recommendedWeight)) {
            var days = 30;
            var goal = 0;
            var deficit = ($rootScope.calculation.recommendedEnergyIntake - $rootScope.calculation.recommendedEnergyExpenditure) - $rootScope.calculation.tee;
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
        
        $rootScope.clientLogGraphData = charts.createGraph(
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
                responsive: true, maintainAspectRatio: true, legend: { display: true },
                scales: {
                    xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }],
                    yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }]
                }
            },
            [
                { label: $translate.instant("measured value"), borderWidth: 1, type: 'bar', fill: true },
                { label: $translate.instant("lower limit"), borderWidth: 2, type: 'line', fill: false },
                { label: $translate.instant("upper limit"), borderWidth: 2, type: 'line', fill: false },
                { label: $translate.instant("goal") + ' (2 ' + $translate.instant("kg") + '/' + $translate.instant("mo") + ')', borderWidth: 3, type: 'line', fill: false, strokeColor: "#33ff33", fillColor: "#43ff33" }
            ]
        )

    };

    $rootScope.setClientLogGraphData = function (type, clientLogsDays) {
        setClientLogGraphData(type, clientLogsDays);
    }

    $scope.getDateFormat = function (x) {
        return new Date(x);
    }

    $scope.change = function (x, scope) {
        switch (scope) {
            case 'height':
                return $rootScope.clientData.height = $rootScope.clientData.height * 1 + x;
                    break;
            case 'weight':
                return $rootScope.clientData.weight = $rootScope.clientData.weight * 1 + x;
                break;
            case 'waist':
                return $rootScope.clientData.waist = $rootScope.clientData.waist * 1 + x;
                break;
            case 'hip':
                return $rootScope.clientData.hip = $rootScope.clientData.hip * 1 + x;
                break;
                default:
                    return '';
            }
    }

    $scope.pdfLink = null;
    $scope.creatingPdf = false;
    $scope.printClientPdf = function () {
        if ($scope.creatingPdf == true) {
            return false;
        }
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/ClientPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, lang: $rootScope.config.language }
        }).then(function (response) {
            $scope.creatingPdf = false;
            var fileName = response.data.d;
            $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        },
        function (response) {
            $scope.creatingPdf = false;
            alert(response.data.d)
        });

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }
    }

    $scope.pdfLink1 = null;

    $scope.printClientLogPdf = function () {
        if ($scope.creatingPdf == true) {
            return false;
        }
        $scope.creatingPdf = true;
        var img = null;
        if (document.getElementById("clientDataChart") != null) {
            img = document.getElementById("clientDataChart").toDataURL("image/png").replace(/^data:image\/(png|jpg);base64,/, "");
        }
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/ClientLogPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, clientLog: $scope.clientLog_, lang: $rootScope.config.language, imageData: img }
        })
        .then(function (response) {
            $scope.creatingPdf = false;
            var fileName = response.data.d;
            $scope.pdfLink1 = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        });
    }

    $scope.hidePdfLink1 = function () {
        $scope.pdfLink1 = null;
    }

    $scope.clientLogDiff = function (type, clientLog, x, idx) {
        if (x === undefined) { return false; }
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

}])

.controller('detailCalculationOfEnergyExpenditureCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$timeout', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $timeout) {
    $rootScope.totalDailyEnergyExpenditure = {
        value: 0,
        duration: 0
    }

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Init',
            method: "POST",
            data: ''
        })
      .then(function (response) {
          $scope.dailyActivity = JSON.parse(response.data.d);
      },
      function (response) {
          functions.alert($translate.instant(response.data.d), '');
      });
    }
    init();

    var setTime = function (h) {
        $scope.hours = [];
        $scope.minutes = [];
        for (i = h; i < 25; i++) {
            $scope.hours.push(i);
        }
        for (i = 0; i < 60; i = i + 5) {
            $scope.minutes.push(i);
        }
    }

    var initTime = function () {
        $scope.from = {
            hour: 0,
            min: 0
        };
        $scope.to = {
            hour: 0,
            min: 0
        }
        setTime(0);
    }
    initTime();

    $scope.clearDailyActivities = function () {
        $rootScope.clientData.dailyActivities.activities = [];
        $rootScope.clientData.dailyActivities.energy = 0;
        $rootScope.clientData.dailyActivities.duration = 0;
        $rootScope.totalDailyEnergyExpenditure.value = 0;
        $rootScope.totalDailyEnergyExpenditure.duration = 0;
        $scope.save($rootScope.clientData.dailyActivities.activities);
        initTime();
    }

    $rootScope.detailCalculationOfEnergyExpenditure = function () {
        $rootScope.showDetailCalculationOfEnergyExpenditure = !$rootScope.showDetailCalculationOfEnergyExpenditure;
        init();
        //$scope.clearDailyActivities();
    }

    var totalEnergy = function () {
        var e = 0;
        angular.forEach($rootScope.clientData.dailyActivities.activities, function (value, key) {
            e = e + value.energy;
        })
        return e;
    }

    var totalDuration = function () {
        var d = 0;
        angular.forEach($rootScope.clientData.dailyActivities.activities, function (value, key) {
            d = d + value.duration;
        })
        return d;
    }

    $scope.confirmActivity = function (x) {
        if (timeDiff($scope.from, $scope.to) == 0) {
            functions.alert($translate.instant('the start time and end of activity can not be the same'), '');
            return false;
        }
        $scope.dailyActivity.id = angular.fromJson(x).id;
        $scope.dailyActivity.activity = angular.fromJson(x).activity;
        //$scope.dailyActivity.from = $scope.from.hour + ':' + $scope.from.minute;
        $scope.dailyActivity.from.hour = $scope.from.hour;
        $scope.dailyActivity.from.min = $scope.from.min;
        // $scope.dailyActivity.to = $scope.to.hour + ':' + $scope.to.minute;
        $scope.dailyActivity.to.hour = $scope.to.hour;
        $scope.dailyActivity.to.min = $scope.to.min;
        $scope.dailyActivity.duration = timeDiff($scope.from, $scope.to);
        $scope.dailyActivity.energy = energy(timeDiff($scope.from, $scope.to), angular.fromJson(x).factorKcal);

        $rootScope.clientData.dailyActivities.activities.push(angular.copy($scope.dailyActivity));
        $rootScope.totalDailyEnergyExpenditure.value = totalEnergy(); // $scope.totalDailyEnergyExpenditure + $scope.dailyActivity.energy;
        $rootScope.clientData.dailyActivities.energy = $rootScope.totalDailyEnergyExpenditure.value;
        $rootScope.totalDailyEnergyExpenditure.duration = totalDuration();
        $rootScope.clientData.dailyActivities.duration = $rootScope.totalDailyEnergyExpenditure.duration;

        $scope.from = angular.copy($scope.to);
        setTime($scope.from.hour);
    }

    var timeDiff = function (from, to) {
        return (to.hour * 60 + to.min) - (from.hour * 60 + from.min);
    }

    var energy = function (duration, factor) {
        return duration * factor;
    }

    $scope.save = function (x) {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Save',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, clientId: $rootScope.client.clientId, activities: x }
        })
      .then(function (response) {
          $rootScope.clientData.dailyActivities = JSON.parse(response.data.d);
      },
      function (response) {
          functions.alert($translate.instant(response.data.d), '');
      });
    }

    var getTotal = function () {
        if ($rootScope.clientData.dailyActivities.activities == null) {
            $rootScope.clientData.dailyActivities.activities = [];
        }
        if ($rootScope.clientData.dailyActivities.activities.length > 0) {
            $rootScope.totalDailyEnergyExpenditure.value = totalEnergy();
            $rootScope.totalDailyEnergyExpenditure.duration = totalDuration();
            var lastActivity = $rootScope.clientData.dailyActivities.activities[$rootScope.clientData.dailyActivities.activities.length - 1];
            $scope.from = {
                hour: lastActivity.to.hour,
                min: lastActivity.to.min
            };
            $scope.to = {
                hour: lastActivity.to.hour,
                min: lastActivity.to.min
            }
            setTime(lastActivity.to.hour);
        }
    }
    $timeout(function () {  // TODO, ne ucita prvi put 
        getTotal();
    }, 1000);


    $scope.selectHours = function () {
        if ($scope.to.hour == 24) {
            $scope.to.min = 0;
            $scope.minutes = [];
            $scope.minutes.push(0);
        }
    }

}])

.controller('calculationCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'charts', '$timeout', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, $timeout, functions, $translate) {
    var webService = 'Calculations.asmx';

    $scope.getBmiClass = function (x) {
        if (x < 18.5) { return { text: 'text-info', icon: 'fa fa-exclamation' }; }
        if (x >= 18.5 && x <= 25) { return { text: 'text-success', icon: 'fa fa-check' }; }
        if (x > 25 && x < 30) { return { text: 'text-warning', icon: 'fa fa-exclamation' }; }
        if (x >= 30) { return { text: 'text-danger', icon: 'fa fa-exclamation' }; }
    }

    $scope.getWaistClass = function (x) {
        if (x.value < x.increasedRisk) { return { text: 'text-success', icon: 'fa fa-check' }; }
        if (x.value >= x.increasedRisk && x.value < x.highRisk) { return { text: 'text-warning', icon: 'fa fa-exclamation' }; }
        if (x.value >= x.highRisk) { return { text: 'text-danger', icon: 'fa fa-exclamation' }; }
    }

    var getCharts = function () {
        google.charts.load('current', { 'packages': ['gauge'] });
        $timeout(function () {
            bmiChart();
            whrChart();
            waistChart();
        }, 1000);
    }

    var bmiChart = function () {
        var id = 'bmiChart';
        var value = $rootScope.calculation.bmi.value.toFixed(1);
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

    var whrChart = function () {
        var id = 'whrChart';
        var value = $rootScope.calculation.whr.value.toFixed(1);
        var unit = 'WHR';
        var increasedRisk = $rootScope.calculation.whr.increasedRisk;
        var highRisk = $rootScope.calculation.whr.highRisk;
        var optimal = $rootScope.calculation.whr.optimal;
        var options = {
            title: 'WHR',
            min: 0,
            max: 1.6,
            greenFrom: optimal - 0.1,
            greenTo: increasedRisk,
            yellowFrom: increasedRisk,
            yellowTo: highRisk,
            redFrom: highRisk,
            redTo: 1.6,
            minorTicks: 0.1
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var waistChart = function () {
        var id = 'waistChart';
        var value = $rootScope.calculation.waist.value.toFixed(1);
        var increasedRisk = $rootScope.calculation.waist.increasedRisk;
        var highRisk = $rootScope.calculation.waist.highRisk;
        var unit = 'cm';
        var options = {
            title: 'WHR',
            min: 0,
            max: 140,
            greenFrom: 70,
            greenTo: increasedRisk,
            yellowFrom: increasedRisk,
            yellowTo: highRisk,
            redFrom: highRisk,
            redTo: 140,
            minorTicks: 5
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var getGoals = function () {
        $http({
            url: $sessionStorage.config.backend + 'Goals.asmx/Load',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.goals = JSON.parse(response.data.d);
            isGoalDisabled();
        },
        function (response) {
            alert(response.data.d)
        });
    };

    if ($rootScope.goalWeightValue_ === undefined) { $rootScope.goalWeightValue_ = 0; }
    $scope.changeGoalWeightValue = function (x) {
        $rootScope.goalWeightValue_ = angular.copy(x);
    }

    $scope.getGoal = function (x) {
        var energy = 0;
        var activity = 0;
        $rootScope.goalWeightValue = 0;
        switch (x) {
            case "G1":  // redukcija tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.tee - 300;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 300;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                $rootScope.goalWeightValue = Math.round(angular.copy($rootScope.calculation.recommendedWeight.max));
                break;
            case "G2":  // zadrzavanje postojece tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.tee + $rootScope.appCalculation.recommendedEnergyExpenditure;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake - 300;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                $rootScope.goalWeightValue =  Math.round(angular.copy($rootScope.clientData.weight));
                break;
            case "G3":  // povecanje tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 300 + $rootScope.appCalculation.recommendedEnergyExpenditure;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G4") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 500;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 200;
                }
                $rootScope.goalWeightValue = $rootScope.clientData.weight < $rootScope.calculation.recommendedWeight.min ?  Math.round(angular.copy($rootScope.calculation.recommendedWeight.min)) :  Math.round(angular.copy($rootScope.clientData.weight + 10));  //TODO
                break;
            case "G4":  // povecanje misicne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.tee + $rootScope.calculation.recommendedEnergyExpenditure;
                    activity = $rootScope.calculation.recommendedEnergyExpenditure + 200;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.tee + 500;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 200;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {   //TODO
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 400;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 100;
                }
                $rootScope.goalWeightValue =  Math.round(angular.copy($rootScope.clientData.weight));
                break;
            default:
                energy = 0;
                activity = 0;
                break;
        }
        $scope.changeGoalWeightValue($rootScope.goalWeightValue);

        angular.forEach($rootScope.goals, function (value, key) {
            if (value.code == x) {
                $rootScope.clientData.goal.code = value.code;
                $rootScope.clientData.goal.title = value.title;
                $rootScope.calculation.goal.code = x;
            }
        })

        $rootScope.calculation.recommendedEnergyIntake = Math.round(energy);
        $rootScope.calculation.recommendedEnergyExpenditure = Math.round(activity);
    }

    var isGoalDisabled = function () {
        if ($rootScope.calculation.bmi.value < 18.5) {
            $rootScope.goals[0].isDisabled = true;
        }
        if ($rootScope.calculation.bmi.value > 25) {
            $rootScope.goals[2].isDisabled = true;
        }
    }

    $scope.creatingPdf = false;
    $scope.printPdf = function () {
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/CalculationPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, calculation: $rootScope.calculation, myCalculation: $rootScope.myCalculation, goal: $rootScope.goalWeightValue_, lang: $rootScope.config.language }
        })
        .then(function (response) {
            var fileName = response.data.d;
            $scope.creatingPdf = false;
            $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        },
        function (response) {
            $scope.creatingPdf = false;
            alert(response.data.d)
        });
    }

    $scope.hidePdfLink = function () {
        $scope.pdfLink = null;
    }

    $scope.clearMyCalculation = function () {
        $rootScope.initMyCalculation();
    }

    $scope.saveMyCalculation = function (x) {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('the saving function is disabled in demo version');
            return false;
        }
        var myCalculation = angular.copy($rootScope.calculation);
        myCalculation.recommendedEnergyIntake = x.recommendedEnergyIntake;
        myCalculation.recommendedEnergyExpenditure = x.recommendedEnergyExpenditure;
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/SaveMyCalculation',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: $rootScope.client.clientId, myCalculation: myCalculation }
        })
        .then(function (response) {
            functions.alert($translate.instant(response.data.d), '');
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        }); 
    }

    var getCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            $rootScope.appCalculation = JSON.parse(response.data.d);

            if ($rootScope.clientData.goal.code == undefined || $rootScope.clientData.goal.code == null || $rootScope.clientData.goal.code == 0) {
                $rootScope.clientData.goal.code = $rootScope.calculation.goal.code;
            }

            getCharts();
            getGoals();
            $scope.getGoal($rootScope.clientData.goal.code);

        },
        function (response) {
            if (response.data.d === undefined) {
                functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
            } else {
                functions.alert(response.data.d, '');
            }
        });
    };
    getCalculation();

}])

.controller('activitiesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Activities.asmx';
    $scope.orderdirection = '-';
    $scope.orderby = function (x) {
        var direction = $scope.orderdirection == '+' ? '-' : '+';
        $scope.order = direction + x;
        $scope.orderdirection = direction;
    }
    $scope.orderby('activity');

    if ($rootScope.activities == undefined) { $rootScope.loadActivities(); };
    if (angular.isDefined($rootScope.appCalculation) && angular.isDefined($rootScope.myCalculation)) {
        $rootScope.calculation.recommendedEnergyExpenditure = functions.isNullOrEmpty($rootScope.myCalculation.recommendedEnergyExpenditure)
            ? $rootScope.appCalculation.recommendedEnergyExpenditure
            : $rootScope.myCalculation.recommendedEnergyExpenditure;
    } else {
        $rootScope.newTpl = './assets/partials/calculation.html';
        $rootScope.selectedNavItem = 'calculation';
    }
    var getEnergyLeft = function () {
        var energy = 0;
        if ($rootScope.clientData.activities.length > 0) {
            angular.forEach($rootScope.clientData.activities, function (value, key) {
                energy = energy + value.energy;
            })
        }
        return $rootScope.calculation.recommendedEnergyExpenditure - energy;
    }

    $scope.openPopup = function (x) {
        energyLeft = getEnergyLeft();
        if (energyLeft > 10) {  // todo
            $mdDialog.show({
                controller: $scope.popupCtrl,
                templateUrl: 'assets/partials/popup/activity.html',
                parent: angular.element(document.body),
                targetEvent: '',
                clickOutsideToClose: true,
                fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
                d: { activity: x, energy: energyLeft }
            })
          .then(function (response) {
              energyLeft = response;
          }, function () {
          });
        } else {
            functions.alert($translate.instant('the selected additional energy expenditure is the same as recommended'), '');
        }
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = d.activity;
        var energy = d.energy;

        $scope.duration = Math.round((energy / ($scope.d.factorKcal * $rootScope.clientData.weight)) * 60);
        // d = (e / (f * w)) * 60
        // d / 60 = e / (f*w)
        // (d * (f*w)) / 60 = e

        // e = d * (f * w) / 60

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            energy = ($scope.duration * ($scope.d.factorKcal * $rootScope.clientData.weight)) / 60;
           $rootScope.clientData.activities.push({
               'id': x.id,
               'activity': x.activity,
               'duration': $scope.duration,
               'energy': energy
           });
           $mdDialog.hide(energy);
        }
    };

    $scope.removeActivity = function (x, idx) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete choosen activity'))
            .textContent(x.title)
            .targetEvent(x)
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            $rootScope.clientData.activities.splice(idx, 1);
        }, function () {
        });
    }

}])

.controller('dietsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions) {
    var webService = 'Diets.asmx';
   
    var get = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Get',
            method: "POST",
            data: { id: x }
        })
        .then(function (response) {
            var diet = JSON.parse(response.data.d);
            $rootScope.clientData.diet = diet;
        },
        function (response) {
            alert(response.data.d)
        });
    };

    var init = function () {
        var age = $rootScope.clientData.age;
        var diet = '';
        var goal = $rootScope.clientData.goal.code;

        if (age < 14) {
            switch (goal) {
                case 'G1':
                    diet = 'd4';
                    break;
                case 'G2':
                    diet = 'd1';
                    break;
                case 'G3':
                    diet = 'd1';
                    break;
                case 'G4':
                    diet = 'd7';
                    break;
                default:
                    diet = 'd1';
                    break;
            }
        }
        if (age >= 14 && age < 18) {
            switch (goal) {
                case 'G1':
                    diet = 'd5';
                    break;
                case 'G2':
                    diet = 'd2';
                    break;
                case 'G3':
                    diet = 'd2';
                    break;
                case 'G4':
                    diet = 'd7';
                    break;
                default:
                    diet = 'd2';
                    break;
            }
        }
        if (age >= 18) {
            switch (goal) {
                case 'G1':
                    diet = 'd6';
                    break;
                case 'G2':
                    diet = 'd3';
                    break;
                case 'G3':
                    diet = 'd3';
                    break;
                case 'G4':
                    diet = 'd7';
                    break;
                default:
                    diet = 'd3';
                    break;
            }
        }
        get(diet);
    }
    if ($rootScope.clientData.diet.id == null) { init(); }

    $scope.select = function (x) {
        $rootScope.clientData.diet = x;
    };

}])

.controller('mealsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Meals.asmx';

    $scope.toggleMealsTpl = function (x) {
        $scope.tpl = x;
        $rootScope.mealsAreChanged = true;
    }

    var defineMealsType = function () {
        if ($rootScope.currentMenu !== undefined) {
            if ($rootScope.currentMenu.id != null) {
                if ($rootScope.currentMenu.data.meals.length > 0) {
                    if ($rootScope.currentMenu.data.meals[0].code == 'B') {
                        $scope.tpl = 'standardMeals';
                    } else {
                        $scope.tpl = 'myMeals';
                    }
                    return false;
                } 
            }
        }
        if ($rootScope.clientData.myMeals !== undefined && $rootScope.clientData.myMeals != null) {
            if ($rootScope.clientData.myMeals.data != null) {
                if ($rootScope.clientData.myMeals.data.meals.length >= 2) {
                    $scope.tpl = 'myMeals';
                } else {
                    $scope.tpl = 'standardMeals';
                }
            } else {
                $scope.tpl = 'standardMeals';
            }
        } else {
            $scope.tpl = 'standardMeals';
        }
    }
    defineMealsType();

    
    
}])

.controller('standardMealsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
        var webService = 'Meals.asmx';
        $rootScope.isMyMeals = false;

        var load = function () {
            $http({
                url: $sessionStorage.config.backend + webService + '/Load',
                method: "POST",
                data: ''
            })
            .then(function (response) {
                $rootScope.clientData.meals = JSON.parse(response.data.d);
                // TODO translate meals on server side
                angular.forEach($rootScope.clientData.meals, function (value, key) {
                    $rootScope.clientData.meals[key].title = $translate.instant($rootScope.clientData.meals[key].title);
                })
            },
            function (response) {
                alert(response.data.d)
            });
        };
        if ($rootScope.clientData.meals.length == 0) {
            load();
        } else if ($rootScope.clientData.meals[0].code != 'B') {
            load();
        }

    }])

.controller('myMealsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'MyMeals.asmx';
    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: { user: $rootScope.user }
        })
        .then(function (response) {
            $rootScope.myMeals = JSON.parse(response.data.d);
            $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
            $rootScope.clientData.meals = $rootScope.clientData.myMeals.data.meals;
            $rootScope.isMyMeals = true;
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getClientMeals = function () {
        if ($rootScope.currentMenu !== undefined) {
            if ($rootScope.currentMenu.id != null) {
                return false;
            }
        }
        if ($rootScope.clientData.myMeals !== undefined && $rootScope.clientData.myMeals != null) {
            if ($rootScope.clientData.myMeals.data != null) {
                if ($rootScope.clientData.myMeals.data.meals.length > 0) {
                    $rootScope.myMeals = angular.copy($rootScope.clientData.myMeals);
                }
            } else {
                init();
            } 
        } else {
            init();
        }
    }

    var initMyMeals = function () {
        if (!angular.isDefined($rootScope.myMeals)) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Init',
                method: "POST",
                data: { user: $rootScope.user }
            })
            .then(function (response) {
                $rootScope.myMeals = JSON.parse(response.data.d);
                getClientMeals();
                $rootScope.isMyMeals = true;
            },
            function (response) {
                alert(response.data.d)
            });
        } else {
            getClientMeals();
        }
        
    }
    initMyMeals();

    $scope.new = function () {
        if ($rootScope.user.userType < 2) { return false; }
        init();
    }
    
    $scope.getTemplate = function () {
        if ($rootScope.user.userType < 2) { return false; }
        $http({
            url: $sessionStorage.config.backend + webService + '/Template',
            method: "POST",
            data: { user: $rootScope.user, lang: $rootScope.config.language }
        })
        .then(function (response) {
            $rootScope.myMeals = JSON.parse(response.data.d);
            if ($rootScope.user.userType > 2) {
                $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
                $rootScope.clientData.meals = $rootScope.clientData.myMeals.data.meals;
            }
            $rootScope.isMyMeals = true;
            $rootScope.mealsAreChanged = true;
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var setMealCode = function () {
        if ($rootScope.myMeals !== undefined) {
            if ($rootScope.myMeals.data != null) {
                if ($rootScope.myMeals.data.meals.length > 0) {
                    angular.forEach($rootScope.myMeals.data.meals, function (value, key) {
                        value.code = 'MM' + key;
                        $rootScope.myMeals.data.energyPerc[key].meal.code = value.code;
                    })
                    $rootScope.isMyMeals = true;
                    $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
                }
            }
        } 
    }

    $scope.add = function () {
        if ($rootScope.myMeals === undefined) {
            init();
        } else {
            addNewRow();
        }
    }

    $rootScope.setMealCode = function () {
        if ($rootScope.isMyMeals) {
            setMealCode();
        }
    }

    var addNewRow = function () {
        if ($rootScope.user.userType < 2) { return false; }
        if ($rootScope.myMeals.data.meals.length >= 8) {
            functions.alert($translate.instant('you have reached the maximum number of meals'), '');
            return false;
        }
        $rootScope.myMeals.data.meals.push({
            code: "",
            title: "",
            description: "",
            isSelected: true,
            isDisabled: false
        });
        $rootScope.myMeals.data.energyPerc.push({
            meal: {
                code: "",
                energyMinPercentage: 0,
                energyMaxPercentage: 0,
                energyMin: 0,
                energyMax: 0
            }
        });
        setMealCode();
    }

    $scope.removeMeal = function (idx) {
        if ($rootScope.user.userType < 2) { return false; }
        $rootScope.myMeals.data.meals.splice(idx, 1);
        $rootScope.myMeals.data.energyPerc.splice(idx, 1);
        setMealCode();
    }

    $scope.save = function () {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('the saving function is disabled in demo version');
            return false;
        }
        if ($rootScope.user.userType < 2) {
            return false;
        }
        if ($rootScope.myMeals.data.meals.length < 3) {
            functions.alert($translate.instant('choose at least 3 meals'), '');
            return false;
        }
        if (functions.isNullOrEmpty($rootScope.myMeals.title)) {
            functions.alert($translate.instant('title is required'), '');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/Save',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, x: $rootScope.myMeals }
        })
        .then(function (response) {
            if (response.data.d != 'error') {
                $rootScope.myMeals = JSON.parse(response.data.d);
                $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
                $rootScope.isMyMeals = true;
            } else {
                functions.alert($translate.instant('meals with the same name already exists'), '');
            }
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    $scope.remove = function (id) {
        if ($rootScope.user.userType < 2) {
            return false;
        }
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete meals') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(id);
        }, function () {
        });
    };

    remove = function (id) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, id: id }
        })
        .then(function (response) {
            $scope.mealsList = JSON.parse(response.data.d);
            init();
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    $scope.search = function () {
        openMyMealsPopup();
    }

    var openMyMealsPopup = function () {
        if ($rootScope.user.userType < 2) {
            return false;
        }
        $mdDialog.show({
            controller: getMyMealsPopupCtrl,
            templateUrl: 'assets/partials/popup/mymeals.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (response) {
            $rootScope.myMeals = response;
            $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
            $rootScope.clientData.meals = $rootScope.clientData.myMeals.data.meals;
            $rootScope.isMyMeals = true;
            $rootScope.mealsAreChanged = true;
        }, function () {
        });
    };

    var getMyMealsPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + 'MyMeals.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
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
        load();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'MyMeals.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
            .then(function (response) {
                $scope.meals = JSON.parse(response.data.d);
                $mdDialog.hide($scope.meals);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.confirm = function (x) {
            get(x);
        }

    };

}])

.controller('menuCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'charts', '$timeout', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, $timeout, functions, $translate) {
    var webService = 'Foods.asmx';
    $scope.addFoodBtnIcon = 'fa fa-plus';
    $scope.addFoodBtn = false;

    function initPrintSettings() {
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/InitMenuSettings',
            method: "POST",
            data: {}
        })
       .then(function (response) {
           $rootScope.printSettings = JSON.parse(response.data.d);
       },
       function (response) {
           alert(response.data.d)
       });
    };
    initPrintSettings();

    $rootScope.selectedFoods = $rootScope.selectedFoods == undefined ? [] : $rootScope.selectedFoods;

    if ($rootScope.clientData.meals.length < 3) {
        $rootScope.newTpl = 'assets/partials/meals.html';
        $rootScope.selectedNavItem = 'meals';
        functions.alert($translate.instant('choose at least 3 meals'), '');
    }

    var getRecommendations = function (clientData) {
        var energyPerc = null;
        if (clientData.myMeals !== undefined && clientData.myMeals != null) {
            if (clientData.myMeals.data != null) {
                if (clientData.myMeals.data.meals.length >= 2 && $rootScope.isMyMeals == true) {
                    clientData.meals = clientData.myMeals.data.meals;
                    energyPerc = clientData.myMeals.data.energyPerc; // $rootScope.myMeals.data.energyPerc;
                }
            }
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/GetRecommendations',
            method: "POST",
            data: { client: clientData, myRecommendedEnergyIntake: $rootScope.myCalculation.recommendedEnergyIntake, myMealsEnergyPerc: energyPerc }
        })
       .then(function (response) {
           $rootScope.recommendations = JSON.parse(response.data.d);
           displayCharts();
       },
       function (response) {
           if (response.data.d === undefined) {
               functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
           } else {
               functions.alert(response.data.d, '');
           }
       });
    };

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $scope.food = response.data.d.food;
            initMenuDetails();
        },
        function (response) {
            alert(response.data.d)
        });
    };
    
    var initMenuDetails = function () {
        $http({
            url: $sessionStorage.config.backend + 'Menues.asmx/Init',
            method: "POST",
            data: {}
        })
        .then(function (response) {
            $rootScope.currentMenu = JSON.parse(response.data.d);
            $rootScope.currentMenu.client = $rootScope.client;
            $rootScope.currentMenu.client.clientData = $rootScope.clientData;  //TODO sredit
            $rootScope.currentMenu.data.meals = $rootScope.clientData.meals;

            angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                $rootScope.currentMenu.data.meals[key].description = '';
            })

            $rootScope.currentMeal = 'B';
            if ($rootScope.currentMenu !== undefined) {
                if ($rootScope.currentMenu.data !== null) {
                    if ($rootScope.currentMenu.data.meals.length > 0) {
                        $rootScope.currentMeal = $rootScope.currentMenu.data.meals[0].code;
                    }
                }
            }

            getTotals($rootScope.currentMenu);
            getRecommendations($rootScope.currentMenu.client.clientData);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    $scope.toggleMeals = function (x) {
        $rootScope.currentMeal = x;
    };

    if ($rootScope.mealsAreChanged) {
        $rootScope.mealsAreChanged = false;
        init();
    } else {
        if ($rootScope.currentMenu === undefined) {
            init();
        } else {
            var oldMeals = $rootScope.currentMenu.data.meals;
            $rootScope.currentMenu.data.meals = angular.copy($rootScope.clientData.meals);
            angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                if (key >= $rootScope.currentMenu.data.meals.length || key >= oldMeals.length) { return false; }
                if (oldMeals[key].code == value.code && $rootScope.currentMenu.data.selectedFoods.length > 0) {
                    value.description = oldMeals[key].description;
                }
            })
            $rootScope.currentMenu.client = $rootScope.client;
            $rootScope.currentMenu.client.clientData = $rootScope.clientData;  //TODO sredit
        }
    }

    $scope.toggleAnalytics = function (x) {
        $scope.loading = true;
        $timeout(function () {
            $scope.loading = false;
            $scope.analyticsTpl = x;
            getTotals($rootScope.currentMenu);
        }, 700);
    };
    $scope.toggleAnalytics('chartsTpl');

    $scope.changeQuantity = function (x, type, idx) {
        if (x.quantity > 0.0001 && isNaN(x.quantity) == false && x.mass > 0.0001 && isNaN(x.mass) == false) {
			$scope.selectedThermalTreatment = null; //TODO
            angular.forEach($rootScope.currentMenu.data.selectedFoods[idx].thermalTreatments, function (value, key) {
                if (value.isSelected == true) {
                    $scope.selectedThermalTreatment = value;
                }
            })
            $timeout(function () {
                $http({
                    url: $sessionStorage.config.backend + webService + '/ChangeFoodQuantity',
                    method: "POST",
                    data: { initFood: $rootScope.currentMenu.data.selectedInitFoods[idx], newQuantity: x.quantity, newMass: x.mass, type: type, thermalTreatment: $scope.selectedThermalTreatment }
                })
                .then(function (response) {
                    $rootScope.currentMenu.data.selectedFoods[idx] = JSON.parse(response.data.d);
                    getTotals($rootScope.currentMenu);
                },
                function (response) {
                });
            }, 600);
        }
    }

    $scope.change = function (x, type, idx) {
        if ($rootScope.currentMenu.data.selectedFoods[idx].quantity + x > 0) {
                if (type == 'quantity') {
                    $rootScope.currentMenu.data.selectedFoods[idx].quantity = $rootScope.currentMenu.data.selectedFoods[idx].quantity * 1 + x;
                    $scope.changeQuantity($rootScope.currentMenu.data.selectedFoods[idx], 'quantity', idx);
                }
                if (type == 'mass') {
                    $rootScope.currentMenu.data.selectedFoods[idx].mass = $rootScope.currentMenu.data.selectedFoods[idx].mass * 1  + x;
                    $scope.changeQuantity($rootScope.currentMenu.data.selectedFoods[idx], 'mass', idx);
                }
            }
    }

    $scope.openFoodPopup = function (x, idx) {
        $scope.addFoodBtn = true;
        $scope.addFoodBtnIcon = 'fa fa-spinner fa-spin';
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.currentMenu.data.selectedFoods.length > 9) {
            functions.demoAlert('in demo version maximum number of choosen foods is 10');
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
            return false;
        }
        $mdDialog.show({
            controller: $rootScope.foodPopupCtrl,
            templateUrl: 'assets/partials/popup/food.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { foods: $rootScope.foods, myFoods: $rootScope.myFoods, foodGroups: $rootScope.foodGroups, food: x, idx: idx, config:$rootScope.config }
        })
        .then(function (x) {
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
            $scope.addFoodToMeal(x.food, x.initFood, idx);
        }, function () {
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
        });
    };

    $rootScope.foodPopupCtrl = function ($scope, $mdDialog, d, $http, $translate) {
        $scope.d = d;
        $scope.foods = d.foods;
        $scope.myFoods = d.myFoods;
        $scope.foodGroups = d.foodGroups;
        var initFood = null;

        var initFoodForEdit = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/InitFoodForEdit',
                method: "POST",
                data: { food: x }
            })
            .then(function (response) {
                initFood = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var isEditMode = false;
        if (d.food == null) {
            $scope.food = null;
            initFood = null;
            isEditMode = false;
        } else {
            $scope.food = d.food;
            initFoodForEdit(d.food);
            isEditMode = true;
        }

        $scope.limit = 100;

        $scope.initCurrentFoodGroup = function () {
            $scope.currentGroup = { code: 'A', title: 'all foods' };
        }
        $scope.initCurrentFoodGroup();

        var initThermalTreatment = function () {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/InitThermalTreatment',
                method: "POST",
                data: ''
            })
            .then(function (response) {
                $scope.selectedThermalTreatment = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }
        initThermalTreatment();

        $scope.showMyFoods = function (x) {
            $scope.isShowMyFood = x;
        }

        $scope.getFoodDetails = function (x) {
            if ($scope.isShowMyFood == true) {
                getMyFoodDetails(x);
                return false;
            }
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userId, id: JSON.parse(x).id }
            })
            .then(function (response) {
                $scope.food = JSON.parse(response.data.d);
                $scope.food.food = $translate.instant($scope.food.food);
                $scope.food.unit = $translate.instant($scope.food.unit);
                $scope.food.foodGroup.title = $translate.instant($scope.food.foodGroup.title);
                $scope.food.meal.title = $translate.instant($scope.food.meal.title);
                angular.forEach($scope.food.thermalTreatments, function (value, key) {
                    $scope.food.thermalTreatments[key].thermalTreatment.title = $translate.instant($scope.food.thermalTreatments[key].thermalTreatment.title);
                })
                initFood = angular.copy($scope.food);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var getMyFoodDetails = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: JSON.parse(x).id }
            })
            .then(function (response) {
                $scope.food = JSON.parse(response.data.d);
                $scope.food.unit = $translate.instant($scope.food.unit);
                initFood = angular.copy($scope.food);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var currThermalTreatmentIdx = 0;
        $scope.getThermalTreatment = function (x, idx) {
            if (functions.isNullOrEmpty(idx)) {
                idx = currThermalTreatmentIdx;
            }
            angular.forEach(x, function (value, key) {
                value.isSelected = false;
            })
            $scope.selectedThermalTreatment = x[idx];
            x[idx].isSelected = true;
            currThermalTreatmentIdx = idx;
            if (isEditMode) {
                isEditMode = false;
            } else {
                includeThermalTreatment($scope.selectedThermalTreatment);
            }
        }

        var includeThermalTreatment = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/IncludeThermalTreatment',
                method: "POST",
                data: { initFood: initFood, food: $scope.food, thermalTreatment: x }
            })
            .then(function (response) {
                $scope.food = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            var data = { food: x, initFood: initFood }
            $mdDialog.hide(data);
        }

        $scope.changeQuantity = function (x, type) {
            isEditMode = false;
            if (x.quantity > 0.0001 && isNaN(x.quantity) == false && x.mass > 0.0001 && isNaN(x.mass) == false) {
                var currentFood = $scope.food.food;  // << in case where user change food title
                $timeout(function () {
                    $http({
                        url: $sessionStorage.config.backend + webService + '/ChangeFoodQuantity',
                        method: "POST",
                        data: { initFood: initFood, newQuantity: x.quantity, newMass: x.mass, type: type, thermalTreatment: $scope.selectedThermalTreatment }
                    })
                    .then(function (response) {
                        $scope.food = JSON.parse(response.data.d);
                        $scope.food.food = currentFood; // << in case where user change food title
                    },
                    function (response) {
                    });
                }, 600);
            }
        }

        $scope.change = function (x, type) {
            if ($scope.food.quantity + x > 0) {
                if (type == 'quantity') {
                    $scope.food.quantity = $scope.food.quantity + x;
                    $scope.changeQuantity($scope.food, 'quantity');
                }
                if (type == 'mass') {
                    $scope.food.mass = $scope.food.mass + x;
                    $scope.changeQuantity($scope.food, 'mass');
                }
            }
        }

        $scope.showFoodSubGroups = function (x) {
            if(x.parent == 'A') {
                $scope.currentMainGroup = x.group.code;
            }
        }
       
        $scope.changeFoodGroup = function (x) {
            $scope.searchFood = '';
            $scope.limit = $scope.foods.length + 1;
            $scope.showMyFoods(false);
            $scope.currentGroup = {
                code: x.code,
                title: x.title
            };
        }

        $scope.checkIf = function (x) {
            if (x.foodGroup.code == $scope.currentGroup.code || $scope.currentGroup.code == 'A' || $scope.isShowMyFood == true) {
                return true;
            } else {
                if ($scope.currentGroup.code == $scope.currentMainGroup) {
                    if (x.foodGroup.parent == $scope.currentGroup.code) {
                        return true;
                    }
                } else {
                    return false;
                }
            }
        }

        $scope.loadMore = function () {
            $scope.limit = $scope.limit + $scope.foods.length;
        }

    };

    $scope.addFoodToMeal = function (x, initFood, idx) {
        if (x.food != undefined || x.food != null) {
            x.meal.code = $rootScope.currentMeal;

            angular.forEach($rootScope.clientData.meals, function (value, key) {
                if (value.code == x.meal.code) {
                    x.meal.title = $translate.instant(value.title);
                }
            })

            if (idx == undefined) {
                $rootScope.currentMenu.data.selectedFoods.push(x);
                $rootScope.currentMenu.data.selectedInitFoods.push(initFood);
            } else {
                $rootScope.currentMenu.data.selectedFoods[idx] = x;
                $rootScope.currentMenu.data.selectedInitFoods[idx] = initFood;
            }
            
            initFood.meal.code = $rootScope.currentMeal;

            $scope.food = [];
            $scope.choosenFood = "";
            $scope.thermalTreatment = "";
            getTotals($rootScope.currentMenu);
        }
    }

    $scope.new = function () {
        init();
    }

    $scope.delete = function () {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete menu') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            init();
            alert('TODO');
        }, function () {
        });
    };

    $scope.removeFood = function (x, idx) {
        var confirm = $mdDialog.confirm()
             .title($translate.instant('delete food') + '?')
             .textContent(x.food)
             .targetEvent(x)
             .ok($translate.instant('yes'))
             .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            $rootScope.currentMenu.data.selectedFoods.splice(idx, 1);
            $rootScope.currentMenu.data.selectedInitFoods.splice(idx, 1);
            getTotals($rootScope.currentMenu);
        }, function () {
        });
    }

    $scope.printPreview = function () {
        $mdDialog.show({
            controller: $scope.printPreviewCtrl,
            templateUrl: 'assets/partials/popup/printmenu.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen,
            d: { currentMenu: $rootScope.currentMenu, clientData: $rootScope.clientData, client: $rootScope.client, totals: $rootScope.totals, settings: $rootScope.printSettings, config: $rootScope.config }
        })
        .then(function () {
        }, function () {
        });
    };

    $scope.printPreviewCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.currentMenu = d.currentMenu;
        $scope.clientData = d.clientData;
        $scope.client = d.client;
        $scope.totals = d.totals;
        $scope.settings = d.settings;
        $scope.config = d.config;

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.consumers = 1;
        $scope.changeNumberOfConsumers = function (x) {
            $scope.consumers = x;
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/ChangeNumberOfConsumers',
                method: "POST",
                data: { foods: $scope.currentMenu.data.selectedFoods, number: x }
            })
           .then(function (response) {
               $scope.foods = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        }
        if (angular.isDefined($scope.currentMenu)) { $scope.changeNumberOfConsumers($scope.consumers); }


        $scope.copyToClipboard = function (id) {
            var el = document.getElementById(id);
            var range = document.createRange();
            range.selectNodeContents(el);
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
            document.execCommand('copy');
        }

        $scope.getMealTitle = function (x) {
            return $rootScope.getMealTitle(x);
        }

        $scope.getServDescription = function (x) {
            var des = "";
            if (x.cerealsServ > 0) { des = servDes(des, x.cerealsServ, "cereals_"); }
            if (x.vegetablesServ > 0) { des = servDes(des, x.vegetablesServ, "vegetables_"); }
            if (x.fruitServ > 0) { des = servDes(des, x.fruitServ, "fruit_"); }
            if (x.meatServ > 0) { des = servDes(des, x.meatServ, "meat_"); }
            if (x.milkServ > 0) { des = servDes(des, x.milkServ, "milk_"); }
            if (x.fatsServ > 0) { des = servDes(des, x.fatsServ, "fats_"); }
            return des;
        }

        function servDes(des, serv, title) {
            return (functions.isNullOrEmpty(des) ? '' : (des + ', ')) + serv + ' serv. ' + $translate.instant(title);
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
    };
  
    $scope.get = function () {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
        } else {
            getMenuPopup();
        }
    }

    var getMenuPopup = function () {
        $mdDialog.show({
            controller: getMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/getmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            data: { config: $rootScope.config, clientData: $rootScope.clientData }
        })
        .then(function (x) {
            $rootScope.currentMenu.id = x.id;
            $rootScope.currentMenu.title = x.title;
            $rootScope.currentMenu.note = x.note;
            $rootScope.currentMenu.userId = x.userId;
            $rootScope.currentMenu.data = x.data;
            $rootScope.currentMenu.client.clientData = $rootScope.clientData;
            $rootScope.currentMenu.client.clientData.meals = x.data.meals;
            $rootScope.currentMenu.client.clientData.myMeals = x.client.clientData.myMeals;
            $rootScope.isMyMeals = false;
            if ($rootScope.currentMenu.client.clientData.myMeals != null) {
                if ($rootScope.currentMenu.client.clientData.myMeals.data != null) {
                    if ($rootScope.currentMenu.client.clientData.myMeals.data.meals.length >= 2) {
                        $rootScope.isMyMeals = true;
                    }
                }
            }
            
            getRecommendations(angular.copy($rootScope.currentMenu.client.clientData));
            getTotals($rootScope.currentMenu);
            $rootScope.currentMeal = x.data.meals[0].code; 
        }, function () {
        });
    };

    var getMenuPopupCtrl = function ($scope, $mdDialog, $http, data, $translate, $translatePartialLoader, $timeout) {
        $scope.config = data.config;
        $scope.clientData = data.clientData;
        $scope.loadType = 0;
        $scope.type = 0;
        $scope.appMenues = false;
        $scope.toTranslate = false;
        $scope.toLanguage = '';
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.loading = true;
            $scope.appMenues = false;
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
            })
           .then(function (response) {
               $scope.d = JSON.parse(response.data.d);
               angular.forEach($scope.d, function (x, key) {
                   x.date = new Date(x.date);
                   functions.correctDate(x.date);
               });
               $scope.loading = false;
           },
           function (response) {
               $scope.loading = false;
               alert(response.data.d);
           });
        }
        load();

        $scope.load = function () {
            load();
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                 .title($translate.instant('remove menu') + '?')
                 .textContent(x.title)
                 .targetEvent(x)
                 .ok($translate.instant('yes'))
                 .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        }

        var remove = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Delete',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
          .then(function (response) {
              $scope.d = JSON.parse(response.data.d);
          },
          function (response) {
              alert(response.data.d)
          });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id,  }
            })
            .then(function (response) {
                var menu = JSON.parse(response.data.d);
                $mdDialog.hide(menu);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.loadAppMenues = function () {
            $scope.appMenues = true;
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/LoadAppMenues',
                method: "POST",
                data: { lang: $rootScope.config.language }
            })
           .then(function (response) {
               $scope.d = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        }

        var getAppMenu = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/GetAppMenu',
                method: "POST",
                data: { id: x.id, lang: $rootScope.config.language, toTranslate: $scope.toTranslate }
            })
            .then(function (response) {
                var menu = JSON.parse(response.data.d);
                if ($scope.toTranslate == true) {
                    translateFoods(menu);
                }
                $mdDialog.hide(menu);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.confirm = function (x) {
            $scope.appMenues == true ? getAppMenu(x) : get(x);
        }

        $scope.setToTranslate = function (x) {
            $scope.toTranslate = x;
        }

        $scope.setToLanguage = function (x) {
            $scope.toLanguage = x;
        }

        var translateFoods = function (menu) {
            $rootScope.setLanguage($scope.toLanguage);
             $timeout(function () {
                 angular.forEach(menu.data.selectedFoods, function (value, key) {
                     value.food = $translate.instant(value.food);
                     value.unit = $translate.instant(value.unit);
                 })
                 angular.forEach(menu.data.selectedInitFoods, function (value, key) {
                     value.food = $translate.instant(value.food);
                     value.unit = $translate.instant(value.unit);
                 })
                 $mdDialog.hide(menu);
                 $rootScope.setLanguage('hr');
              }, 500);
        }

    };

    $scope.save = function () {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            return false;
        }
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('the saving function is disabled in demo version');
            return false;
        } else {
            openSaveMenuPopup();
        }
    }

    //TODO remove client from params
    var openSaveMenuPopup = function () {
        $rootScope.client.clientData = $rootScope.clientData;
        $mdDialog.show({
            controller: openSaveMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/savemenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: $rootScope.currentMenu, client: $rootScope.client, totals: $rootScope.totals, config: $sessionStorage.config, user: $rootScope.user }
        })
       .then(function (x) {
           $rootScope.currentMenu = x;
       }, function () {
       });
    }

    var openSaveMenuPopupCtrl = function ($scope, $mdDialog, $http, d, $translate) {
        $scope.d = angular.copy(d);
        var save = function (currentMenu) {
            if (functions.isNullOrEmpty(currentMenu.title)) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu title'), '');
                openSaveMenuPopup();
                return false;
            }
            currentMenu.diet = d.client.clientData.diet.diet;
            var myMeals = null;
            if (currentMenu.data.meals.length > 2) {
                if (currentMenu.data.meals[0].code != 'B') {
                    myMeals = $scope.d.client.clientData.myMeals;
                }
            } 
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Save',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, x: currentMenu, user: $scope.d.user, myMeals: myMeals }
            })
          .then(function (response) {
              if (response.data.d != 'error') {
                  $scope.d.currentMenu = JSON.parse(response.data.d);
                  $mdDialog.hide($scope.d.currentMenu);
              } else {
                  functions.alert($translate.instant('there is already a menu with the same name'), '');
              }
          },
          function (response) {
              functions.alert($translate.instant(response.data.d), '');
          });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x, saveasnew) {
            x.client = d.client;
            x.userId = d.client.userId;
            x.id = saveasnew == true ? null : x.id;
            x.energy = d.totals.energy;
            x.date = new Date(new Date().setHours(0, 0, 0, 0));
            save(x);
        }

        var saveAppMenu = function (currentMenu) {
            if (functions.isNullOrEmpty(currentMenu.title)) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu title'), '');
                openSaveMenuPopup();
                return false;
            }
            currentMenu.diet = d.client.clientData.diet.diet;
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/SaveAppMenu',
                method: "POST",
                data: { x: currentMenu, lang: $rootScope.config.language }
            })
            .then(function (response) {
                functions.alert('ok', '');
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }

        $scope.saveAppMenu = function (x) {
            x.energy = d.totals.energy;
            saveAppMenu(x);
        }
    };

    $scope.send = function () {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            return false;
        }
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        if ($rootScope.user.userType < 1) {
            functions.demoAlert('this function is available only in standard and premium package');
            return false;
        }
        openSendMenuPopup();
    }

    var openSendMenuPopup = function () {
        $rootScope.client.clientData = $rootScope.clientData;
        $mdDialog.show({
            controller: openSendMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/sendmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: $rootScope.currentMenu, client: $rootScope.client, user: $rootScope.user }
        })
       .then(function (x) {
       }, function () {
       });
    }

    var openSendMenuPopupCtrl = function ($scope, $mdDialog, $http, d, $translate, functions) {
        $scope.d = angular.copy(d);

        var send = function (x) {
            $scope.titlealert = null;
            $scope.emailalert = null;
            if (functions.isNullOrEmpty(x.currentMenu.title)) {
                $scope.titlealert = $translate.instant('menu title is required');
                return false;
            }
            if (functions.isNullOrEmpty(x.client.email)) {
                $scope.emailalert = $translate.instant('email is required');
                return false;
            }
            $mdDialog.hide();
            $http({
                url: $sessionStorage.config.backend + 'Mail.asmx/SendMenu',
                method: "POST",
                data: { email: x.client.email, currentMenu: x.currentMenu, user: $scope.d.user, lang: $rootScope.config.language }
            })
            .then(function (response) {
                functions.alert(response.data.d, '');
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            send(x);
        }

    };

    var getTotals = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetTotals',
            method: "POST",
            data: { selectedFoods: x.data.selectedFoods, meals: x.data.meals }
        })
       .then(function (response) {
           $rootScope.totals = JSON.parse(response.data.d);
           $rootScope.totals.price.currency = $rootScope.config.currency;
           displayCharts();
       },
       function (response) {
           alert(response.data.d)
       });
    }

    var displayCharts = function () {
        if ($rootScope.recommendations === undefined || $rootScope.totals === undefined) { return false; }
        $scope.mealsTotals = [];
        $scope.mealsMin = [];
        $scope.mealsMax = [];
        $scope.mealsTitles = [];
        angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
            if (value.isSelected == true && angular.isDefined($rootScope.totals)) {
                if (angular.isDefined($rootScope.totals.mealsTotalEnergy)) {
                    if (key < $rootScope.recommendations.mealsRecommendationEnergy.length) {
                        $scope.mealsTotals.push($rootScope.totals.mealsTotalEnergy.length > 0 ? $rootScope.totals.mealsTotalEnergy[key].meal.energy.toFixed(1) : 0);
                        if ($rootScope.recommendations !== undefined) {
                            if (angular.isDefined($rootScope.recommendations.mealsRecommendationEnergy)) {
                                $scope.mealsMin.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMin);
                                $scope.mealsMax.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMax);
                            }
                        }
                        $scope.mealsTitles.push($translate.instant($rootScope.getMealTitle(value)));
                    }
                }
            }
        })

        totalEnergyChart();
        otherFoodChart();
        carbohydratesChart();
        proteinsChart();
        saturatedFatsChart();
        trifluoroaceticAcidChart();
        cholesterolChart();
        fatsChart();

        var t = $rootScope.totals;
        var r = $rootScope.recommendations
        $rootScope.servGraphData = charts.createGraph(
                [$translate.instant('cereals'), $translate.instant('vegetables'), $translate.instant('fruit'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fats')],
                [
                    [t.servings.cerealsServ, t.servings.vegetablesServ, t.servings.fruitServ, t.servings.meatServ, t.servings.milkServ, t.servings.fatsServ],
                    [r.servings.cerealsServ, r.servings.vegetablesServ, r.servings.fruitServ, r.servings.meatServ, r.servings.milkServ, r.servings.fatsServ]
                ],
                [$translate.instant('cereals'), $translate.instant('vegetables'), $translate.instant('fruit'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fats')],
                ['#45b7cd', '#33cc33', '#33cc33'],
                {
                    responsive: true, maintainAspectRatio: true, legend: { display: true },
                    scales: {
                        xAxes: [{ display: true, scaleLabel: { display: false }, ticks: { beginAtZero: true } }],
                        yAxes: [{ display: true, scaleLabel: { display: false }, ticks: { beginAtZero: true } }]
                    }
                },
                [
                     {
                         label: $translate.instant('choosen'),
                         borderWidth: 1,
                         type: 'bar',
                         fill: true
                     },
                     {
                         label: $translate.instant('recommended'),
                         borderWidth: 3,
                         hoverBackgroundColor: "rgba(255,99,132,0.4)",
                         hoverBorderColor: "rgba(255,99,132,1)",
                         type: 'line',
                         fill: true
                     }
                ]
        );

        $rootScope.pieGraphData = charts.createGraph(
                [$translate.instant('carbohydrates'), $translate.instant('proteins'), $translate.instant('fats')],
                [t.carbohydratesPercentage, t.proteinsPercentage, t.fatsPercentage],
                [$translate.instant('carbohydrates') + ' (%)', $translate.instant('proteins') + ' (%)', $translate.instant('fats') + ' (%)'],
                ['#45b7cd', '#ff6384', '#33cc33'],
                { responsive: true, maintainAspectRatio: true, legend: { display: true },
                scales: {
                    xAxes: [{ display: false, scaleLabel: { display: false }, ticks: { beginAtZero: true } }],
                    yAxes: [{ display: false, scaleLabel: { display: false }, ticks: { beginAtZero: true } }]}
                },
                []
        );

        var mealsGraphData = function (displayLegend) {
            return charts.createGraph(
              $scope.mealsTitles,
              [$scope.mealsTotals, $scope.mealsMin, $scope.mealsMax],
              $scope.mealsTitles,
              ['#45b7cd', '#ff6384', '#33cc33'],
              {
                  responsive: true, maintainAspectRatio: true, legend: { display: displayLegend },
                  scales: {
                      xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true } }],
                      yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true, stepSize: 200 } }]
                  }
              },
              [
                   {
                       label: $translate.instant('choosen') + ' (' + $translate.instant('kcal') + ')',
                       borderWidth: 1,
                       type: 'bar',
                       fill: true
                   },
                   {
                       label: $translate.instant('recommended') + ' ' + $translate.instant('from') + ' (' + $translate.instant('kcal') + ')',
                       borderWidth: 3,
                       hoverBackgroundColor: "rgba(255,99,132,0.4)",
                       hoverBorderColor: "rgba(255,99,132,1)",
                       type: 'line',
                       fill: false
                   },
                    {
                        label: $translate.instant('recommended') + ' ' + $translate.instant('to') + ' (' + $translate.instant('kcal') + ')',
                        borderWidth: 3,
                        hoverBackgroundColor: "rgba(255,99,132,0.4)",
                        hoverBorderColor: "rgba(255,99,132,1)",
                        type: 'line',
                        fill: false
                    }
              ]
            );

        }  
        $rootScope.mealsGraphData_menu = mealsGraphData(false);
        $rootScope.mealsGraphData_analysis = mealsGraphData(true);


        $scope.parametersGraphDataOther = charts.stackedChart(
            [$translate.instant('choosen')],
            [
                [t.starch, t.totalSugar, t.glucose, t.fructose, t.saccharose, t.maltose, t.lactose]
            ],
            [
                $translate.instant('starch') + ' (' + $translate.instant('g') + ')',
                $translate.instant('total sugar') + ' (' + $translate.instant('g') + ')',
                $translate.instant('glucose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('fructose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('saccharose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('maltose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('lactose') + ' (' + $translate.instant('g') + ')'
            ],
            ['#33cc33'],
            '');

        //TODO
        $scope.parametersGraphData = charts.stackedChart(
            [$translate.instant('choosen'), $translate.instant('recommended dietary allowance') + ' (' + $translate.instant('rda').toUpperCase() + ')'],
            [
                [
                    t.fibers,
                    t.monounsaturatedFats,
                    t.polyunsaturatedFats,
                    t.calcium,
                    t.magnesium,
                    t.phosphorus,
                    t.iron,
                    t.copper,
                    t.zinc,
                    t.manganese,
                    t.selenium,
                    t.iodine,
                    t.retinol,
                    t.vitaminD,
                    t.vitaminE,
                    t.vitaminB1,
                    t.vitaminB2,
                    t.vitaminB3,
                    t.vitaminB6,
                    t.vitaminB12,
                    t.folate,
                    t.pantothenicAcid,
                    t.biotin,
                    t.vitaminC,
                    t.vitaminK
                ],
                [
                    r.fibers.rda,
                    r.monounsaturatedFats.rda,
                    r.polyunsaturatedFats.rda,
                    r.calcium.rda,
                    r.magnesium.rda,
                    r.phosphorus.rda,
                    r.iron.rda,
                    r.copper.rda,
                    r.zinc.rda,
                    r.manganese.rda,
                    r.selenium.rda,
                    r.iodine.rda,
                    r.retinol.rda,
                    r.vitaminD.rda,
                    r.vitaminE.rda,
                    r.vitaminB1.rda,
                    r.vitaminB2.rda,
                    r.vitaminB3.rda,
                    r.vitaminB6.rda,
                    r.vitaminB12.rda,
                    r.folate.rda,
                    r.pantothenicAcid.rda,
                    r.biotin.rda,
                    r.vitaminC.rda,
                    r.vitaminK.rda
                ]
            ],
            [
                $translate.instant('fibers') + ' (' + $translate.instant('g') + ')',
                $translate.instant('monounsaturated fats') + ' (' + $translate.instant('g') + ')',
                $translate.instant('polyunsaturated fats') + ' (' + $translate.instant('g') + ')',
                $translate.instant('calcium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('magnesium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('phosphorus') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('iron') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('copper') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('zinc') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('manganese') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('selenium') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('iodine') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('retinol') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('vitamin D') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('vitamin E') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B1') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B2') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B3') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B6') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B12') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('folate') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('pantothenic acid') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('biotin') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('vitamin C') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin K') + ' (' + $translate.instant('ug') + ')',
            ],
            ['#45b7cd', '#33cc33'],
            $translate.instant('parameters'));

        $scope.parametersGraphDataUI = charts.stackedChart(
            [$translate.instant('choosen'), $translate.instant('upper intake level') + ' (' + $translate.instant('ul').toUpperCase() + ')'],
            [
                [t.saturatedFats, t.trifluoroaceticAcid, t.cholesterol],
                [r.saturatedFats.ui, r.trifluoroaceticAcid.ui, r.cholesterol.ui]
            ],
            [
                $translate.instant('saturated fats') + ' (' + $translate.instant('g') + ')',
                $translate.instant('trifluoroacetic acid') + ' (' + $translate.instant('g') + ')',
                $translate.instant('cholesterol') + ' (' + $translate.instant('mg') + ')'
            ],
            ['#f44242', '#33cc33'],
            '');

        //TODO
        $scope.parametersGraphDataMDA = charts.stackedChart(
            [$translate.instant('choosen'), $translate.instant('upper intake level') + ' (' + $translate.instant('ul').toUpperCase() + ')', $translate.instant('minimum dietary allowance') + ' (' + $translate.instant('mda').toUpperCase() + ')'],
            [
                [t.sodium, t.potassium, t.chlorine],
                [r.sodium.ui],
                [r.sodium.mda, r.potassium.mda, r.chlorine.mda]
            ],
            [
                $translate.instant('sodium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('potassium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('chlorine') + ' (' + $translate.instant('mg') + ')'
            ],
            ['#45b7cd', '#33cc33'],
            '');

    }

    var totalEnergyChart = function () {
        var recommended = parseInt($rootScope.recommendations.energy);
        var id = 'energyChart';
        var value = $rootScope.totals.energy.toFixed(0);
        var unit = 'kcal';

        var options = {
            title: 'energy',
            min: 0,
            max: recommended + (recommended * 0.06),
            greenFrom: recommended - (recommended * 0.02),
            greenTo: recommended + (recommended * 0.02),
            yellowFrom: recommended + (recommended * 0.02),
            yellowTo: recommended + (recommended * 0.04),
            redFrom: recommended + (recommended * 0.04),
            redTo: recommended + (recommended * 0.06),
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isEnergyOk = function () {
            if (value < recommended - (recommended * 0.02)) {
                return 'fa fa-minus';
            }
            if (value >= recommended - (recommended * 0.02) && value <= recommended + (recommended * 0.04)) {
                return 'fa fa-check';
            }
            if (value > recommended + (recommended * 0.04)) {
                return 'fa fa-exclamation';
            }
        }
    }

    var otherFoodChart = function () {
        var suggested = $rootScope.recommendations.servings.otherFoodsEnergy;
        var id = 'otherFoodChart';
        var value = $rootScope.totals.servings.otherFoodsEnergy.toFixed(0);
        var unit = 'kcal';

        var options = {
            title: 'energy',
            min: 0,
            max: suggested + (suggested * 0.5),
            greenFrom: 0,
            greenTo: suggested - (suggested * 0.02),
            yellowFrom: suggested - (suggested * 0.02),
            yellowTo: suggested,
            redFrom: suggested,
            redTo: suggested + (suggested * 0.5),
            minorTicks: 5
        };
        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isOtherFoodOk = function () {
            return value < suggested ? 'label label-success fa fa-check' : 'label label-danger fa fa-exclamation';
        }
    }

    var carbohydratesChart = function () {
        var recommended = {
            from: $rootScope.recommendations.carbohydratesPercentageMin,
            to: $rootScope.recommendations.carbohydratesPercentageMax
        }
        var id = 'carbohydratesChart';
        var value = $rootScope.totals.carbohydratesPercentage.toFixed(0);
        var unit = '%';

        var options = {
            title: 'carbohidrates',
            min: 0,
            max: 100,
            greenFrom: recommended.from,
            greenTo: recommended.to - (recommended.to * 0.02),
            yellowFrom: recommended.to - (recommended.to * 0.02),
            yellowTo: recommended.to,
            redFrom: recommended.to,
            redTo: 100,
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isCarbohydratesOk = function () {
            if (value < recommended.from) {
                return 'fa fa-minus';
            }
            if (value >= recommended.from && value <= recommended.to) {
                return 'fa fa-check';
            }
            if (value > recommended.to) {
                return 'fa fa-plus';
            }
        }
    }

    var proteinsChart = function () {
        var recommended = {
            from: $rootScope.recommendations.proteinsPercentageMin,
            to: $rootScope.recommendations.proteinsPercentageMax
        }
        var id = 'proteinsChart';
        var value = $rootScope.totals.proteinsPercentage.toFixed(0);
        var unit = '%';

        var options = {
            title: 'proteins',
            min: 0,
            max: 100,
            greenFrom: recommended.from,
            greenTo: recommended.to - (recommended.to * 0.02),
            yellowFrom: recommended.to - (recommended.to * 0.02),
            yellowTo: recommended.to,
            redFrom: recommended.to,
            redTo: 100,
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isProteinsOk = function () {
            if (value < recommended.from) {
                return 'fa fa-minus';
            }
            if (value >= recommended.from && value <= recommended.to) {
                return 'fa fa-check';
            }
            if (value > recommended.to) {
                return 'fa fa-plus';
            }
        }
    }

    var fatsChart = function () {
        var recommended = {
            from: $rootScope.recommendations.fatsPercentageMin,
            to: $rootScope.recommendations.fatsPercentageMax
        }
        var id = 'fatsChart';
        var value = $rootScope.totals.fatsPercentage.toFixed(0);
        var unit = '%';

        var options = {
            title: 'fats',
            min: 0,
            max: 100,
            greenFrom: recommended.from,
            greenTo: recommended.to - (recommended.to * 0.02),
            yellowFrom: recommended.to - (recommended.to * 0.02),
            yellowTo: recommended.to,
            redFrom: recommended.to,
            redTo: 100,
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isFatsOk = function () {
            if (value < recommended.from) {
                return 'fa fa-minus';
            }
            if (value >= recommended.from && value <= recommended.to) {
                return 'fa fa-check';
            }
            if (value > recommended.to) {
                return 'fa fa-plus';
            }
        }
    }

    var saturatedFatsChart = function () {
        var id = 'saturatedFatsChart';
        var value = $rootScope.totals.saturatedFats;
        unit = 'mg';
        var options = {
            title: 'saturated fats',
            min: 0,
            max: Math.round($rootScope.recommendations.saturatedFats.ui + $rootScope.recommendations.saturatedFats.ui * 0.4),
            greenFrom: 0,
            greenTo: $rootScope.recommendations.saturatedFats.ui - $rootScope.recommendations.saturatedFats.ui * 0.2,
            yellowFrom: $rootScope.recommendations.saturatedFats.ui - $rootScope.recommendations.saturatedFats.ui * 0.2,
            yellowTo: $rootScope.recommendations.saturatedFats.ui,
            redFrom: $rootScope.recommendations.saturatedFats.ui,
            redTo: $rootScope.recommendations.saturatedFats.ui + $rootScope.recommendations.saturatedFats.ui * 0.4,
            minorTicks: 5
        };
        charts.guageChart(id, value, unit, options);
    }

    var trifluoroaceticAcidChart = function () {
        var id = 'trifluoroaceticAcidChart';
        var value = $rootScope.totals.trifluoroaceticAcid;
        unit = 'mg';
        var options = {
            title: 'trifluoroacetic acid',
            min: 0,
            max: Math.round($rootScope.recommendations.trifluoroaceticAcid.ui + $rootScope.recommendations.trifluoroaceticAcid.ui * 0.4),
            greenFrom: 0,
            greenTo: $rootScope.recommendations.trifluoroaceticAcid.ui - $rootScope.recommendations.trifluoroaceticAcid.ui * 0.2,
            yellowFrom: $rootScope.recommendations.trifluoroaceticAcid.ui - $rootScope.recommendations.trifluoroaceticAcid.ui * 0.2,
            yellowTo: $rootScope.recommendations.trifluoroaceticAcid.ui,
            redFrom: $rootScope.recommendations.trifluoroaceticAcid.ui,
            redTo: $rootScope.recommendations.trifluoroaceticAcid.ui + $rootScope.recommendations.trifluoroaceticAcid.ui * 0.4,
            minorTicks: 5
        };
        charts.guageChart(id, value, unit, options);
    }

    var cholesterolChart = function () {
        var id = 'cholesterolChart';
        var value = $rootScope.totals.cholesterol;
        unit = 'mg';
        var options = {
            title: 'cholesterol',
            min: 0,
            max: Math.round($rootScope.recommendations.cholesterol.ui + $rootScope.recommendations.cholesterol.ui * 0.4),
            greenFrom: 0,
            greenTo: $rootScope.recommendations.cholesterol.ui - $rootScope.recommendations.cholesterol.ui * 0.2,
            yellowFrom: $rootScope.recommendations.cholesterol.ui - $rootScope.recommendations.cholesterol.ui * 0.2,
            yellowTo: $rootScope.recommendations.cholesterol.ui,
            redFrom: $rootScope.recommendations.cholesterol.ui,
            redTo: $rootScope.recommendations.cholesterol.ui + $rootScope.recommendations.cholesterol.ui * 0.4,
            minorTicks: 5
        };
        charts.guageChart(id, value, unit, options);
    }
   
    var mealEnergyChart = function (idx) {
        var recommended = $rootScope.recommendations.energy;
        var id = 'mealEnergyChart_' + idx;
        var value = angular.isDefined($rootScope.totals) ? $rootScope.totals.mealsTotalEnergy[idx].meal.energy : 0;

        $scope.testmealenergy = value;

        var title = 'Energija';
        var min = 0;
        var max = recommended + (recommended * 0.2);
        var greenFrom = recommended - (recommended * 0.02);
        var greenTo = recommended + (recommended * 0.02);
        var yellowFrom = recommended + (recommended * 0.02);
        var yellowTo = recommended + (recommended * 0.04);
        var redFrom = recommended + (recommended * 0.04);
        var redTo = max;
        var minorTicks = 5;

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, title, min, max, greenFrom, greenTo, yellowFrom, yellowTo, redFrom, redTo, minorTicks));
    }

    $scope.moveUp = function (idx) {
        var _idx = idx;
        if (idx > 0) {
            angular.forEach($rootScope.currentMenu.data.selectedFoods, function (value, key) {
                if (value.meal.code == $rootScope.currentMeal) {
                    if (key < idx) {
                        _idx = key + 1;
                    }
                }
            });
            var tmp = $rootScope.currentMenu.data.selectedFoods[_idx - 1];
            $rootScope.currentMenu.data.selectedFoods[_idx - 1] = $rootScope.currentMenu.data.selectedFoods[idx];
            $rootScope.currentMenu.data.selectedFoods[idx] = tmp;
            var tmp_init = $rootScope.currentMenu.data.selectedInitFoods[_idx - 1];
            $rootScope.currentMenu.data.selectedInitFoods[_idx - 1] = $rootScope.currentMenu.data.selectedInitFoods[idx];
            $rootScope.currentMenu.data.selectedInitFoods[idx] = tmp_init;
        }
    }

    $scope.moveDown = function (idx) {
        var _idx = idx;
        if (idx < $rootScope.currentMenu.data.selectedFoods.length - 1) {
            var keepGoing = true;
            angular.forEach($rootScope.currentMenu.data.selectedFoods, function (value, key) {
                if (value.meal.code == $rootScope.currentMeal && keepGoing == true) {
                    if (key > idx && key > 0) {
                        _idx = key - 1;
                        keepGoing = false;
                    }
                }
            });
            var tmp = $rootScope.currentMenu.data.selectedFoods[_idx + 1];
            $rootScope.currentMenu.data.selectedFoods[_idx + 1] = $rootScope.currentMenu.data.selectedFoods[idx];
            $rootScope.currentMenu.data.selectedFoods[idx] = tmp;
            var tmp_init = $rootScope.currentMenu.data.selectedInitFoods[_idx + 1];
            $rootScope.currentMenu.data.selectedInitFoods[_idx + 1] = $rootScope.currentMenu.data.selectedInitFoods[idx];
            $rootScope.currentMenu.data.selectedInitFoods[idx] = tmp_init;
        }
    }

    $scope.filterMeal = function (x) {
        if (x.meal.code == $rootScope.currentMeal) {
            return true;
        } else {
            return false;
        }
    }

    $scope.parameterStyle = function (total, r) {
        if (!angular.isDefined(total) || !angular.isDefined(r)) { return false; }
        if (r.mda != null) {
            if (total < r.mda) { return 'background-color:#9bc1ff; color:white' }
        }
        if (r.ui != null) {
            if (total > r.ui) { return 'background-color:#f94040; color:white' }
        }
    }

    $scope.openRecipePopup = function () {
        openRecipePopup();
    }

    var openRecipePopup = function () {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        $mdDialog.show({
            controller: getRecipePopupCtrl,
            templateUrl: 'assets/partials/popup/getrecipe.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            clientData: $rootScope.clientData,
            config: $rootScope.config
        })
        .then(function (recipe) {
            angular.forEach(recipe.data.selectedFoods, function (value, key) {
                var idx = $rootScope.currentMenu.data.selectedFoods.length;
                $scope.addFoodToMeal(value, recipe.data.selectedInitFoods[key], idx);
            });
            getTotals($rootScope.currentMenu);
        }, function () {
        });
    };

    var getRecipePopupCtrl = function ($scope, $mdDialog, $http, clientData, config, $translate, $translatePartialLoader, $timeout) {
        $scope.clientData = clientData;
        $scope.config = config;
        $scope.user = $rootScope.user;
        $scope.loadType = 0;
        $scope.type = 0;
        $scope.appRecipes = false;
        $scope.toTranslate = false;
        $scope.toLanguage = '';
        $scope.showDescription = true;
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            if ($rootScope.user.licenceStatus == 'demo') {
                return false;
            }
            $scope.loading = true;
            $scope.appRecipes = false;
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
            })
           .then(function (response) {
               $scope.appRecipes = false;
               $scope.d = JSON.parse(response.data.d);
               $scope.loading = false;
           },
           function (response) {
               $scope.loading = false;
               alert(response.data.d);
           });
        }
        load();

        $scope.load = function () {
            load();
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.toggleMyRecipeTpl = function () {
            $mdDialog.cancel();
            $rootScope.newTpl = './assets/partials/myrecipes.html';
            $rootScope.selectedNavItem = 'myrecipes';
        }

        $scope.get = function (x) {
            get(x);
        }

        var get = function (x, showDescription) {
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
            .then(function (response) {
                $scope.recipe = JSON.parse(response.data.d);
                if (showDescription == true) {
                    angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                        if (value.code == $rootScope.currentMeal) {
                            value.description = value.description == '' ? $scope.recipe.title + '.\n' + $scope.recipe.description : value.description + '\n' + $scope.recipe.title + '.\n' + $scope.recipe.description;
                        }
                    });
                }
                $mdDialog.hide($scope.recipe);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.loadAppRecipes = function () {
            $scope.appRecipes = true;
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/LoadAppRecipes',
                method: "POST",
                data: { lang: $rootScope.config.language }
            })
           .then(function (response) {
               $scope.d = JSON.parse(response.data.d);
           },
           function (response) {
               alert(response.data.d)
           });
        }

        var getAppRecipe = function (x, showDescription) {
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/GetAppRecipe',
                method: "POST",
                data: { id: x.id, lang: $rootScope.config.language, toTranslate: $scope.toTranslate }
            })
            .then(function (response) {
                $scope.recipe = JSON.parse(response.data.d);
                if (showDescription == true) {
                    angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                        if (value.code == $rootScope.currentMeal) {
                            value.description = value.description == '' ? $scope.recipe.description : value.description + '\n' + $scope.recipe.description;
                        }
                    });
                }
                $mdDialog.hide($scope.recipe);

                //**********TODO - translate recipes*****************
                //var menu = JSON.parse(response.data.d);
                //if ($scope.toTranslate == true) {
                //    translateFoods(menu);
                //}
                //$mdDialog.hide(menu);
                //****************************************************
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.confirm = function (x, showDescription) {
            $scope.appRecipes == true ? getAppRecipe(x, showDescription) : get(x, showDescription);
        }

        $scope.setToTranslate = function (x) {
            $scope.toTranslate = x;
        }

        $scope.setToLanguage = function (x) {
            $scope.toLanguage = x;
        }

        var translateFoods = function (menu) {
            $rootScope.setLanguage($scope.toLanguage);
            $timeout(function () {
                angular.forEach(menu.data.selectedFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                angular.forEach(menu.data.selectedInitFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                $mdDialog.hide(menu);
                $rootScope.setLanguage('hr');
            }, 500);
        }

    };

    $scope.toggleMenu = function (x) {
        $scope.loading = true;
        $timeout(function () {
            $scope.loading = false;
            $scope.menuTpl = x;
            if (x == 'dailyMenuTpl') {
                getTotals($rootScope.currentMenu);
            }
        }, 700);
    }
    $scope.toggleMenu('dailyMenuTpl');

    var openPrintPdfPopup = function () {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            return false;
        }
        $mdDialog.show({
            controller: printPdfCtrl,
            templateUrl: 'assets/partials/popup/pdf.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: $rootScope.currentMenu, settings: $rootScope.printSettings },
            config: $rootScope.config
        })
        .then(function (r) {
            alert(r);
        }, function () {
        });
    };

    var printPdfCtrl = function ($scope, $rootScope, $mdDialog, $http, d, config, $translate, $translatePartialLoader, $timeout) {
        $scope.settings = d.settings;
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.pdfLink == null;
        $scope.consumers = 1;
        $scope.creatingPdf = false;
        $scope.printMenuPdf = function (consumers) {
            if (angular.isDefined($rootScope.currentMenu)) {
                $scope.creatingPdf = true;
                $http({
                    url: $sessionStorage.config.backend + 'Foods.asmx/ChangeNumberOfConsumers',
                    method: "POST",
                    data: { foods: $rootScope.currentMenu.data.selectedFoods, number: consumers }
                })
                .then(function (response) {
                    var foods = JSON.parse(response.data.d);
                    var currentMenu = angular.copy($rootScope.currentMenu);
                    currentMenu.data.selectedFoods = foods;
                    $http({
                        url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
                        method: "POST",
                        data: { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, totals: $rootScope.totals, consumers: consumers, lang: $rootScope.config.language, settings: $scope.settings }
                    })
                    .then(function (response) {
                        var fileName = response.data.d;
                        $scope.creatingPdf = false;
                        $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
                    },
                    function (response) {
                        $scope.creatingPdf = false;
                        alert(response.data.d)
                    });
                },
                function (response) {
                    alert(response.data.d)
                });
            }
        }

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }
    };

    $scope.openPrintPdfPopup = function () {
        openPrintPdfPopup();
    }

    $scope.pdfLink == null;
    $scope.creatingPdf1 = false;
    $scope.printMenuDetailsPdf = function () {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            functions.alert($translate.instant('menu is empty') + '!', '');
            return false;
        }
        $scope.creatingPdf1 = true;
        if (angular.isDefined($rootScope.currentMenu)) {
            var currentMenu = angular.copy($rootScope.currentMenu);
            $http({
                url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuDetailsPdf',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, calculation: $rootScope.calculation, totals: $rootScope.totals, recommendations: $rootScope.recommendations, lang: $rootScope.config.language }
            })
              .then(function (response) {
                  var fileName = response.data.d;
                  $scope.creatingPdf1 = false;
                  $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
              },
              function (response) {
                  $scope.creatingPdf1 = false;
                  alert(response.data.d);
              });
        }
    }

    $scope.hidePdfLink = function () {
        $scope.pdfLink = null;
    }
    //----------------------------------------

    $scope.saveRecipeFromMenu = function (data, currentMeal) {
        if (data.selectedFoods.length == 0) {
            return false;
        }
        $rootScope.newTpl = './assets/partials/myrecipes.html';
        $rootScope.selectedNavItem = 'myrecipes';
        $rootScope.recipeData = data;
        $rootScope.currentMealForRecipe = currentMeal;
    }

    $scope.getMealTotal = function (x) {
        var total = null;
        angular.forEach($rootScope.totals.mealsTotalEnergy, function (value, key) {
            if (value.meal.code == x) {
                total = value.meal;
            }
        })
        return total
    }

    $scope.getMealRecommendation = function (x) {
        var recommendations = null;
        angular.forEach($rootScope.recommendations.mealsRecommendationEnergy, function (value, key) {
            if (value.meal.code == x) {
                recommendations = value.meal;
            }
        })
        return recommendations
    }

    $scope.toggleParamTpl = function (x) {
        $scope.parametersTpl = x;
    }
    $scope.toggleParamTpl('parametersChartTpl');

    $scope.checkTotal = function (total, min, max) {
        var icon = 'pull-right fa fa-';
        if (total > max) {
            return icon + 'chevron-circle-right text-danger';
        } else if (total < min) {
            return icon + 'chevron-circle-left text-info';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.checkEnergy = function (total, r) {
        var icon = 'pull-right fa fa-';
        if ((total / r) - 1 > 0.05) {
            return icon + 'chevron-circle-right text-danger';
        } else if ((total / r) - 1 < -0.05) {
            return icon + 'chevron-circle-left text-info';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.checkServ = function (total, r) {
        var icon = 'pull-right fa fa-';
        if ((total - r) > 1) {
            return icon + 'chevron-circle-right text-danger';
        } else if ((total - r) < -1) {
            return icon + 'chevron-circle-left text-info';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.checkOtherFoods = function (total, r) {
        var icon = 'pull-right fa fa-';
        if (total > r) {
            return icon + 'chevron-circle-right text-danger';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    /********Shopping list - TODO*******/
    $scope.shoppingList = [];
    $scope.getShoppingList = function (x) {
        openShoppingListPopup(x);
    }

    var openShoppingListPopup = function (x) {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            return false;
        }
        $mdDialog.show({
            controller: shoppingListPdfCtrl,
            templateUrl: 'assets/partials/popup/shoppinglist.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: x }
        })
        .then(function (r) {
            alert(r);
        }, function () {
        });
    };

    var shoppingListPdfCtrl = function ($scope, $rootScope, $mdDialog, $http, d, $translate, $translatePartialLoader) {
        $scope.currentMenu = d.currentMenu;
        var createShoppingList = function(x){
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/ShoppingList',
                method: "POST",
                data: { x: x, lang: $rootScope.config.language }
            })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
        }
        createShoppingList($scope.currentMenu);



        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    };

    $scope.openPrintPdfPopup = function () {
        openPrintPdfPopup();
    }


}])

.controller('myFoodsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'MyFoods.asmx';

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/Init',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            var res = JSON.parse(response.data.d);
            $scope.myFood = res.food;
            $scope.units = res.units;
            $scope.mainFoodGroups = res.foodGroups;
            $('.selectpicker').selectpicker({
                style: 'btn-default',
                size: 4
            });
        },
        function (response) {
            alert(response.data.d)
        });
    };
    init();

    $scope.new = function () {
        init();
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete food') + '?')
            .textContent(x.food)
            .targetEvent(x)
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, id: x.id }
        })
     .then(function (response) {
         loadMyFoods();
         init();
     },
     function (response) {
         functions.alert($translate.instant(response.data.d), '');
     });
    }

    var loadMyFoods = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid }
        })
        .then(function (response) {
            var data = JSON.parse(response.data.d);
            $rootScope.myFoods = data.foods;
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    $scope.save = function (x) {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        if (functions.isNullOrEmpty(x.food)) {
            functions.alert($translate.instant('food title is required'), '');
            return false;
        }
        if (functions.isNullOrEmpty(x.unit)) {
            functions.alert($translate.instant('choose unit'), '');
            return false;
        }
        if (checkIsOtherFood(x) == true) {
            x.servings.cerealsServ = 0;
            x.servings.vegetablesServ = 0;
            x.servings.fruitServ = 0;
            x.servings.meatServ = 0;
            x.servings.milkServ = 0;
            x.servings.fatsServ = 0;
            x.servings.otherFoodsServ = 1;
            x.servings.otherFoodsEnergy = x.energy;
            x.foodGroup.code = 'OF';
        };
        $http({
            url: $sessionStorage.config.backend + webService + '/Save',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, x: x }
        })
        .then(function (response) {
            if (response.data.d != 'there is already a food with the same name') {
                functions.alert($translate.instant(response.data.d), '');
                loadMyFoods();
            } else {
                functions.alert($translate.instant('there is already a food with the same name'), '');
            }
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });

    };

    var checkIsOtherFood = function (x) {
        if (x.foodGroup.code == 'OF') {
            return true;
        }
        if (x.servings.cerealsServ > 0 ||
             x.servings.vegetablesServ > 0 ||
             x.servings.fruitServ > 0 ||
             x.servings.meatServ > 0 ||
             x.servings.milkServ > 0 ||
             x.servings.fatsServ > 0) {
            return false;
        } else {
            return true;
        }
    }

    $scope.search = function () {
        openMyFoodsPopup();
    }

    var openMyFoodsPopup = function () {
        if ($rootScope.user.licenceStatus == 'demo') { return false; }
        $mdDialog.show({
            controller: getMyFoodsPopupCtrl,
            templateUrl: 'assets/partials/popup/myfoods.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (d) {
            $scope.myFood = d;
        }, function () {
        });
    };

    var getMyFoodsPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Load',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid }
            })
            .then(function (response) {
                var data = JSON.parse(response.data.d);
                $scope.d = data.foods;
                $scope.loading = false;
            },
            function (response) {
                $scope.loading = false;
                alert(response.data.d)
            });
        };
        load();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
          .then(function (response) {
              var myFood = JSON.parse(response.data.d);
              $mdDialog.hide(myFood);
          },
          function (response) {
              alert(response.data.d)
          });
        }

        $scope.confirm = function (x) {
            get(x);
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                .title($translate.instant('delete food') + '?')
                .textContent(x.food)
                .targetEvent(x)
                .ok($translate.instant('yes'))
                .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
                openMyFoodsPopup();
            }, function () {
                openMyFoodsPopup();
            });
        };

        var remove = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Delete',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
             .then(function (response) {
                 loadMyFoods();
                 init();
             },
             function (response) {
                 functions.alert($translate.instant(response.data.d), '');
             });
        }
    };

}])

.controller('myRecipesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Recipes.asmx';
    $scope.addFoodBtnIcon = 'fa fa-plus';
    $scope.addFoodBtn = false;

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + 'Recipes.asmx/Init',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $scope.recipe = JSON.parse(response.data.d);
            $scope.currentRecipe = null;
            recipeFromMenu();
            load();
        },
        function (response) {
            alert(response.data.d)
        });
    };

    var load = function () {
        if ($rootScope.user.licenceStatus == 'demo') { return false; }
        $rootScope.loading = true;
        $http({
            url: $sessionStorage.config.backend + 'Recipes.asmx/Load',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid }
        })
        .then(function (response) {
            $scope.recipes = JSON.parse(response.data.d);
            $rootScope.loading = false;
        },
        function (response) {
            $rootScope.loading = false;
            alert(response.data.d)
        });
    };

    var recipeFromMenu = function () {
        if (angular.isDefined($rootScope.recipeData)) {
            if ($rootScope.recipeData != null) {
                if (angular.isDefined($rootScope.recipeData.selectedFoods)) {
                    angular.forEach($rootScope.recipeData.selectedFoods, function (value, key) {
                        if (value.meal.code == $rootScope.currentMealForRecipe) {
                            $scope.recipe.data.selectedFoods.push(value);
                            $scope.recipe.data.selectedInitFoods.push($rootScope.recipeData.selectedFoods[key]);
                        }
                    })
                    angular.forEach($rootScope.recipeData.meals, function (value, key) {
                        if (value.code == $rootScope.currentMealForRecipe) {
                            $scope.recipe.description = value.description;
                        }
                    })
                }
            }
        }
    }

    init();

    $scope.add = function (x) {
        $scope.recipe.push(x);
    }

    $scope.getTotEnergy = function (x) {
        var sum = 0;
        angular.forEach(x, function (value, key) {
            sum += value.energy;
        })
        return sum;
    }

    $scope.openFoodPopup = function (food, idx) {
        $scope.addFoodBtn = true;
        $scope.addFoodBtnIcon = 'fa fa-spinner fa-spin';
        $mdDialog.show({
            controller: $rootScope.foodPopupCtrl,
            templateUrl: 'assets/partials/popup/food.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { foods: $rootScope.foods, myFoods: $rootScope.myFoods, foodGroups: $rootScope.foodGroups, food: food, idx: idx, config: $rootScope.config }
        })
    .then(function (x) {
        $scope.food = x;
        if (idx == null) {
            $scope.recipe.data.selectedFoods.push(x.food);
            $scope.recipe.data.selectedInitFoods.push(x.initFood);
        } else {
            $scope.recipe.data.selectedFoods[idx] = x.food;
            $scope.recipe.data.selectedInitFoods[idx] = x.initFood;
        }
        $scope.addFoodBtnIcon = 'fa fa-plus';
        $scope.addFoodBtn = false;
        }, function () {
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
        });
    }

    $scope.new = function () {
        $rootScope.recipeData = null;
        init();
    }

    $scope.get = function (id) {
        if (id == null) {
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + 'Recipes.asmx/Get',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, id: id }
        })
        .then(function (response) {
            $scope.recipe = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.save = function (recipe) {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('the saving function is disabled in demo version');
            return false;
        }
        if (recipe.title == '' || recipe.title == null) {
            functions.alert($translate.instant('enter recipe title'), '');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + 'Recipes.asmx/Save',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, x: recipe }
        })
        .then(function (response) {
            if (response.data.d != 'there is already a recipe with the same name') {
                $scope.recipe = JSON.parse(response.data.d);
                load();
            } else {
                functions.alert($translate.instant('there is already a recipe with the same name'), '');
            }
        },
        function (response) {
            alert(response.data.d);
        });
    }
    
    $scope.removeFood = function (idx) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete food') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            $scope.recipe.data.selectedFoods.splice(idx, 1);
            $scope.recipe.data.selectedInitFoods.splice(idx, 1);
        }, function () {
        });
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete recipe') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, id: x.id }
        })
        .then(function (response) {
            init();
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.search = function() {
        openMyRecipesPopup();
    }

    var openMyRecipesPopup = function () {
        if ($rootScope.user.licenceStatus == 'demo') { return false; }
        $mdDialog.show({
            controller: getMyRecipesPopupCtrl,
            templateUrl: 'assets/partials/popup/myrecipes.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (recipe) {
            $scope.recipe = recipe;
        }, function () {
        });
    };

    var getMyRecipesPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            if ($rootScope.user.licenceStatus == 'demo') {
                return false;
            }
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
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
        load();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
            .then(function (response) {
                $scope.recipe = JSON.parse(response.data.d);
                $mdDialog.hide($scope.recipe);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.confirm = function (x) {
            get(x);
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                .title($translate.instant('delete recipe') + '?')
                .textContent(x.title)
                .targetEvent(x)
                .ok($translate.instant('yes'))
                .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
                openMyRecipesPopup();
            }, function () {
                openMyRecipesPopup();
            });
        };

        var remove = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Delete',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
            .then(function (response) {
                init();
            },
            function (response) {
                alert(response.data.d);
            });
        }

    };

    $scope.saveRecipeAsMyFood = function (recipe) {
        if (recipe.data.selectedFoods.length == 0) { return false; }
        saveRecipeAsMyFoodPopup(recipe);
    }

    var saveRecipeAsMyFoodPopup = function (recipe) {
        $mdDialog.show({
            controller: saveRecipeAsMyFoodPopupCtrl,
            templateUrl: 'assets/partials/popup/saverecipeasmyfood.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            data: recipe
        })
        .then(function (recipe) {
            $scope.recipe = recipe;
        }, function () {
        });
    }

    var saveRecipeAsMyFoodPopupCtrl = function ($scope, $mdDialog, $http, data, functions, $rootScope) {
        $scope.d = {
            recipe: data,
            units: [],
            unit: null,
            titleAlert: false,
            unitAlert: false
        }

        var init = function () {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/Init',
                method: "POST",
                data: ''
            })
            .then(function (response) {
                var res = JSON.parse(response.data.d);
                $scope.d.units = res.units;
            },
            function (response) {
                alert(response.data.d)
            });
        };
        init();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            if ($rootScope.user.licenceStatus == 'demo') {
                functions.demoAlert('the saving function is disabled in demo version');
                return false;
            }
            if (functions.isNullOrEmpty(x.recipe.title)) {
                $scope.d.titleAlert = true;
            } else if (functions.isNullOrEmpty(x.unit)) {
                $scope.d.unitAlert = true;
            } else {
                save(x);
            }
        }

        var save = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/SaveAsFood',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, recipe: x.recipe, unit: x.unit }
            })
            .then(function (response) {
                loadMyFoods();
                $mdDialog.hide(x.recipe);
                functions.alert($translate.instant(response.data.d), '');
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }

        var loadMyFoods = function () {
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
            })
            .then(function (response) {
                var data = JSON.parse(response.data.d);
                $rootScope.myFoods = data.foods;
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }
    }

}])

.controller('pricesCtrl', ['$scope', '$http', '$sessionStorage', '$rootScope', '$translate', 'functions', '$mdDialog', function ($scope, $http, $sessionStorage, $rootScope, $translate, functions, $mdDialog) {
    var webService = 'Prices.asmx';
    $scope.foodListType = 0;
    $scope.getFoodList = function (x) {
        $scope.foodList = x == 0 ? $rootScope.foods : $rootScope.myFoods;
    }
    $scope.getFoodList($scope.foodListType);

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService +'/Init',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $scope.price = JSON.parse(response.data.d);
            $scope.price.netPrice.currency = $sessionStorage.config.currency;
            load();
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    };
    init();

    var load = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId }
        })
       .then(function (response) {
           $scope.prices = JSON.parse(response.data.d);
       },
       function (response) {
           functions.alert($translate.instant(response.data.d), '');
       });
    }

    $scope.selectFood = function (x) {
        var obj = JSON.parse(x);
        $scope.price.food.id = obj.id;
        $scope.price.food.title = obj.food;
        $scope.calculateUnitPrice(x);
    }

    $scope.calculateUnitPrice = function (x) {
        if (angular.isObject(x)) {
            x.unitPrice.value = x.netPrice.value * (1 / x.mass.value) * 1000;
        }
    }

    $scope.save = function (x) {
        if (x.food.title == null) {
            return false;
        }
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/Save',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, x: x }
        })
       .then(function (response) {
           load();
           functions.alert($translate.instant(response.data.d), '');
       },
       function (response) {
           functions.alert($translate.instant(response.data.d), '');
       });
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete input') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, x: x }
        })
       .then(function (response) {
           load();
       },
       function (response) {
           functions.alert($translate.instant(response.data.d), '');
       });
    }

}])

.controller('orderCtrl', ['$scope', '$http', '$rootScope', '$translate', 'functions', function ($scope, $http, $rootScope, $translate, functions) {
    $scope.application = $translate.instant('nutrition program');
    $scope.version = 'STANDARD';
    $scope.userType = 1;
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = $translate.instant('send order');
    $scope.showUserDetails = $rootScope.user.userName != '' ? false : true;
    $scope.showErrorAlert = false;
    $scope.showPaymentDetails = false;

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
                $scope.user.userType = user.userType;
                $scope.showUserDetails = true;
                $scope.showErrorAlert = false;
                $scope.calculatePrice();
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

    var init = function () {
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.user = JSON.parse(response.data.d);
         $scope.user.userName = $rootScope.user.userName;
         $scope.user.password = $rootScope.user.password;
         $scope.user.application = $scope.application;
         $scope.user.version = $scope.version;
         $scope.user.licence = 1;
         $scope.user.licenceNumber = 1;
         $scope.user.userType = $scope.userType;
         $scope.login($scope.user.userName, $scope.user.password);
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

        $scope.user.version = $scope.version;
        if ($scope.user.userType == 0) { unitprice = $rootScope.config.packages[0].price; $scope.user.version = $rootScope.config.packages[0].title; }
        if ($scope.user.userType == 1) { unitprice = $rootScope.config.packages[1].price; $scope.user.version = $rootScope.config.packages[1].title; }
        if ($scope.user.userType == 2) { unitprice = $rootScope.config.packages[2].price; $scope.user.version = $rootScope.config.packages[2].title; }

        if ($scope.user.licence > 1) {
            unitprice = unitprice * $scope.user.licence - ((unitprice * $scope.user.licence) * ($scope.user.licence / 10))
        }

        $scope.user.licenceNumber = 1;
        totalprice = $scope.user.licenceNumber > 1 ? unitprice * $scope.user.licenceNumber - (unitprice * $scope.user.licenceNumber * 0.1) : unitprice;
        var additionalUsers = $scope.premiumUsers > 5 && $scope.user.userType == 2 ? ($scope.premiumUsers - 5) * 50 : 0;  // 50kn/additional user;
        $scope.user.price = totalprice + additionalUsers;
        $scope.user.priceEur = (totalprice + additionalUsers) / $rootScope.config.eur;
    }

    if ($rootScope.config == undefined) {
        getConfig();
    } else {
        init();
    }

    $scope.order = function (application, version) {
        init();
        window.location.hash = 'orderform';
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
            $scope.errorMesage = $translate.instant('all fields are required');;
            return false;
        }
        if ($scope.userType == 1) {
            if (user.companyName == '' || user.pin == '') {
                $scope.showErrorAlert = true;
                $scope.errorMesage = $translate.instant('all fields are required');;
                return false;
            }
        }

        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = $translate.instant('sending');
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
               $scope.sendicon = 'fa fa-paper-plane-o';
               $scope.sendicontitle = $translate.instant('send');
               $scope.isSendButtonDisabled = false;
               functions.alert($translate.instant('order is not sent'), '');
           } else {
               $scope.showAlert = true;
               $scope.showPaymentDetails = true;
           }
       },
       function (response) {
           $scope.showAlert = false;
           $scope.showPaymentDetails = false;
           $scope.sendicon = 'fa fa-paper-plane-o';
           $scope.isSendButtonDisabled = false;
           $scope.sendicontitle = $translate.instant('send');
           alert(response.data.d);
       });
    }

    $scope.registration = function () {
        window.location.hash = 'registration';
    }

    $scope.backToApp = function () {
        $rootScope.currTpl = './assets/partials/dashboard.html';
    }

}])

.controller('infoCtrl', ['$scope', '$rootScope', '$translate', function ($scope, $rootScope, $translate) {

}])

.controller('settingsCtrl', ['$scope', '$http', '$rootScope', '$translate', '$sessionStorage', 'functions', function ($scope, $http, $rootScope, $translate, $sessionStorage, functions) {
    var webService = 'Files.asmx';
    if(angular.isDefined($sessionStorage.settings)){$rootScope.settings = $sessionStorage.settings;}

    $scope.save = function (d) {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('the saving function is disabled in demo version');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/SaveJsonToFile',
            method: "POST",
            data: { foldername: 'users/' + $rootScope.user.userGroupId, filename: 'settings', json: JSON.stringify(d) }
        })
     .then(function (response) {
         $rootScope.config.language = d.language;
         $rootScope.config.currency = d.currency;
         functions.alert($translate.instant('settings saved successfully'), '');
     },
     function (response) {
         functions.alert($translate.instant(response.data.d), '');
     });
    }

}])

.controller('weeklyMenuCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'WeeklyMenus.asmx';
    $scope.consumers = 1;

    $scope.getDay = function (x) {
        switch(x) {
            case 0: return $translate.instant('monday');
            case 1: return $translate.instant('tuesday');
            case 2: return $translate.instant('wednesday');
            case 3: return $translate.instant('thursday');
            case 4: return $translate.instant('friday');
            case 5: return $translate.instant('saturday');
            case 6: return $translate.instant('sunday');
            default: return '';
        }
    }

    var emptyMenuList = true;
    var isEmptyList = function (x) {
        emptyMenuList = true;
        angular.forEach(x, function (value, key) {
            if (!functions.isNullOrEmpty(value)) {
                emptyMenuList = false;
                return false;
            }
        });
    }
    $scope.isEmptyList = function () {
        isEmptyList($scope.weeklyMenu.menuList);
        return emptyMenuList;
    }

    var init = function () {
        $scope.loading = true;
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: { user: $rootScope.user, client: $rootScope.client, lang: $rootScope.config.language }
        })
       .then(function (response) {
           $scope.weeklyMenu = JSON.parse(response.data.d);

           $scope.loading = false;
       },
       function (response) {
           $scope.loading = false;
           alert(response.data.d);
       });
    }
    init();

    $scope.new = function () {
        init();
    }

    $scope.printWindow = function () {
        window.print();
    };

    $scope.pdfLink = null;
    $scope.creatingPdf = false;

    $scope.openPdf = function () {
        if ($scope.pdfLink != null) {
            window.open($scope.pdfLink, window.innerWidth <= 800 && window.innerHeight <= 600 ? '_self' : '_blank');
        }
    }
  
    var getMenues = function () {
        $scope.loading = true;
        $http({
            url: $sessionStorage.config.backend + 'Menues.asmx/Load',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId }
        })
       .then(function (response) {
           $scope.menus = JSON.parse(response.data.d);
           if ($scope.menus.length == 0) {
               functions.alert($translate.instant('first you need to create daily menus'), '');
           }
           $scope.loading = false;
       },
       function (response) {
           $scope.loading = false;
           alert(response.data.d);
       });
    }
    getMenues();

    $scope.creatingPdf = false;
    $scope.pageSizes = ['A4', 'A3', 'A2', 'A1'];
    $rootScope.printSettings.pageSize = 'A3';
    $rootScope.printSettings.showTitle = false;
    $rootScope.printSettings.showDescription = false;
    $rootScope.printSettings.orientation = 'L';

    $scope.printWeeklyMenu = function (consumers, printSettings) {
        if (emptyMenuList) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/WeeklyMenuPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, weeklyMenu: $scope.weeklyMenu, consumers: consumers, lang: $rootScope.config.language, settings: printSettings }
        })
          .then(function (response) {
              var fileName = response.data.d;
              $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
              $scope.creatingPdf = false;
          },
          function (response) {
              $scope.creatingPdf = false;
              alert(response.data.d)
          });
    }

    $scope.hidePdfLink = function () {
        $scope.pdfLink = null;
    }

    //********* search ************
    $scope.search = function () {
        openSearchMenuPopup();
    }

    var openSearchMenuPopup = function () {
        $mdDialog.show({
            controller: openSearchMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/searchweeklymenus.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: {}
        })
       .then(function (response) {
           $scope.weeklyMenu = response;
           $scope.weeklyMenu.client = $rootScope.client;
           $scope.weeklyMenu.diet = $rootScope.clientData.diet;
       }, function () {
       });
    }

    var openSearchMenuPopupCtrl = function ($scope, $mdDialog, $http, $translate, functions) {
        var webService = 'WeeklyMenus.asmx';
        $scope.type = 0;
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + webService + '/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, lang: $rootScope.config.language }
            })
           .then(function (response) {
               $scope.d = JSON.parse(response.data.d);
               $scope.loading = false;
           },
           function (response) {
               $scope.loading = false;
               functions.alert(response.data.d, '');
           });
        }
        load();

        $scope.load = function () {
            load();
        }

        $scope.remove = function (x) {
            if (emptyMenuList) return false;
            var confirm = $mdDialog.confirm()
                 .title($translate.instant('remove menu') + '?')
                 .textContent(x.title)
                 .targetEvent(x)
                 .ok($translate.instant('yes'))
                 .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        }

        var remove = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Delete',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id }
            })
          .then(function (response) {
              $scope.d = JSON.parse(response.data.d);
          },
          function (response) {
              alert(response.data.d)
          });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id, lang: $rootScope.config.language }
            })
            .then(function (response) {
                var menu = JSON.parse(response.data.d);
                $mdDialog.hide(menu);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            get(x);
        }

    };

    //********* save ************
    $scope.save = function () {
        if (emptyMenuList) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        openSaveMenuPopup();
    }

    var openSaveMenuPopup = function () {
        $mdDialog.show({
            controller: openSaveMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/saveweeklymenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { weeklyMenu: $scope.weeklyMenu }
        })
       .then(function (response) {
           $scope.weeklyMenu = response;
       }, function () {
       });
    }

    var openSaveMenuPopupCtrl = function ($scope, $mdDialog, d, $http, $translate, functions) {
        var webService = 'WeeklyMenus.asmx';
        $scope.d = d.weeklyMenu;

        var save = function (x) {
            if ($rootScope.user.licenceStatus == 'demo') {
                functions.demoAlert('the saving function is disabled in demo version');
                return false;
            }
            if (functions.isNullOrEmpty(x.title)) {
                functions.alert($translate.instant('enter menu title'), '');
                return false;
            }
            $http({
                url: $sessionStorage.config.backend + webService + '/Save',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, x: $scope.d }
            })
          .then(function (response) {
              if (response.data.d != 'error') {
                  $scope.d = JSON.parse(response.data.d);
                  $mdDialog.hide($scope.d);
              } else {
                  functions.alert($translate.instant('there is already a menu with the same name'), '');
              }
          },
          function (response) {
              functions.alert($translate.instant(response.data.d), '');
          });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x, saveasnew) {
            x.id = saveasnew == true ? null : x.id;
            x.date = new Date(new Date().setHours(0, 0, 0, 0));
            save(x);
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: x.id, }
            })
            .then(function (response) {
                $scope.d = JSON.parse(response.data.d);
                $mdDialog.hide($scope.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

    };

    //********* remove ************

    $scope.remove = function (x) {
        if (emptyMenuList) { return false; }
        var confirm = $mdDialog.confirm()
             .title($translate.instant('remove menu') + '?')
             .textContent(x.title)
             .targetEvent(x)
             .ok($translate.instant('yes'))
             .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    }

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, id: x.id }
        })
      .then(function (response) {
          $scope.d = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
    }

    //********* send ************
    $scope.send = function () {
        openSendMenuPopup();
    }

    var openSendMenuPopup = function () {
        if (emptyMenuList) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        if ($scope.pdfLink == null) { return false;}
        $mdDialog.show({
            controller: openSendMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/sendweeklymenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { client: $rootScope.client, user: $rootScope.user, pdfLink: $scope.pdfLink }
        })
       .then(function (response) {
       }, function () {
       });
    }

    var openSendMenuPopupCtrl = function ($scope, $mdDialog, $http, d, $translate, functions) {
        $scope.d = angular.copy(d);

        $scope.menu = {
            title: '',
            note: ''
        }

        var send = function () {
            $scope.titlealert = null;
            $scope.emailalert = null;
            if (functions.isNullOrEmpty($scope.menu.title)) {
                $scope.titlealert = $translate.instant('menu title is required');
                return false;
            }
            if (functions.isNullOrEmpty($scope.d.client.email)) {
                $scope.emailalert = $translate.instant('email is required');
                return false;
            }
            $mdDialog.hide();
            $http({
                url: $sessionStorage.config.backend + 'Mail.asmx/SendWeeklyMenu',
                method: "POST",
                data: { email: $scope.d.client.email, user: $scope.d.user, pdfLink: $scope.d.pdfLink, title: $scope.menu.title, note: $scope.menu.note, lang: $rootScope.config.language }
            })
            .then(function (response) {
                functions.alert(response.data.d, '');
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function () {
            send();
        }
    };

}])

.controller('clientAppCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$timeout', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $timeout) {
    var webService = 'ClientApp.asmx';
    $scope.show = false;
    $scope.showTitle = $translate.instant('show access data');

    $scope.toggle = function (client, clientApp) {
        $scope.show = !$scope.show;
        if ($scope.show == true) {
            if (clientApp.id == null) {
                $scope.getActivationCode(client, clientApp);
            }
            $scope.showTitle = $translate.instant('hide access data');
        } else {
            $scope.showTitle = $translate.instant('show access data');
        }
    };

    $scope.get = function (x) {
        $scope.show = false;
        $scope.showTitle = $translate.instant('show access data');
        $http({
            url: $sessionStorage.config.backend + webService + '/Get',
            method: "POST",
            data: { clientId: x.clientId }
        })
        .then(function (response) {
            $scope.clientApp = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.client = null;
    $scope.getActivationCode = function (client, clientApp) {
        if (clientApp.id == null) {
            clientApp.clientId = client.clientId;
            clientApp.userId = $rootScope.user.userGroupId;
            clientApp.lang = $rootScope.config.language;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/GetActivationCode',
            method: "POST",
            data: { x: clientApp }
        })
        .then(function (response) {
            $scope.clientApp = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.clientAppUrl = function (x) {
        if (x !== undefined) {
            return $rootScope.config.clientapppageurl + '?uid=' + $rootScope.user.userGroupId + '&cid=' + x.clientId + '&lang=' + $rootScope.config.language;
        } else {
            return;
        }
    }

    $scope.sendingMail = false;
    $scope.sendAppLinkToClientEmail = function (client) {
        if ($scope.sendingMail == true) { return false; }
        if (functions.isNullOrEmpty(client.email)) {
            functions.alert($translate.instant('email is required'), '');
            return false;
        }
        $scope.sendingMail = true;
        var link = $scope.clientAppUrl(client);
        var messageSubject = $translate.instant('nutrition program') + '. ' + $translate.instant('application access data');
        var messageBody = '<p>' + $translate.instant('dear') + ',' + '</p>' +
            $translate.instant('we send you the access data to track your body weight and download menus') + '.' +
            '<br />' +
            '<br />' +
            $translate.instant('web application') + ': ' + '<strong><a href="' + $rootScope.config.clientapppageurl + '">' + $rootScope.config.clientapppageurl + '</a></strong>' +
            '<br />' +
            $translate.instant('or') + ' ' + $translate.instant('android application') + ': ' + '<strong>' + '<a href="' + $rootScope.config.clientapp_apk + '">' + $rootScope.config.clientapp_apk + '</a></strong>' +
            '<br />' +
            '<iframe src="https://www.appsgeyser.com/social_widget/social_widget.php?width=100&height=100&apkName=Program Prehrane Klijent_8297899&simpleVersion=yes" width="180" height="220" vspace="0" hspace="0" frameborder="no" scrolling="no" seamless="" allowtransparency="true"></iframe>' +
            '<br />' +
            $translate.instant('activation code') + ': ' + '<strong>' + $scope.clientApp.code + '</strong>' +
            '<br />' +
            '<hr />' +
             $translate.instant('or') + ' ' +  $translate.instant('web application') + ' (' + $translate.instant('without activation code') + '): ' + '<strong><a href="' + link + '">' + link + '</a></strong>' +
            '<br />' +
            '<br />' +
            '<i>* ' + $translate.instant('this is an automatically generated email – please do not reply to it') + '</i>' +
            '<br />' +
            '<p>' + $translate.instant('best regards') + '</p>' +
            '<a href="' + $rootScope.config.webpageurl + '">' + $rootScope.config.webpage + '</a>'
        $http({
            url: $sessionStorage.config.backend + 'Mail.asmx/SendMessage',
            method: "POST",
            data: { sendTo: client.email, messageSubject: messageSubject, messageBody: messageBody, lang: $rootScope.config.language }
        })
        .then(function (response) {
            $scope.sendingMail = false;
            functions.alert(response.data.d, '');
        },
        function (response) {
            $scope.sendingMail = false;
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    $scope.backToApp = function () {
        $rootScope.currTpl = './assets/partials/dashboard.html';
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
                } else if (event.which == 110 || event.which == 188 || event.which == 190) {
                    // to allow ',' and '.'
                    return true;
                } else if (event.which == 46) {
                    // to allow delete
                    return true;
                } else {
                    event.preventDefault();
                    // to stop others  
                    return false;
                }
            });
        }
    }
});


;



