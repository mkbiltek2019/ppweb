/*!
app.js
(c) 2018 IG PROG, www.igprog.hr
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
            $scope.toggleTpl('login.html');
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
        $rootScope.currTpl = './assets/partials/' + x;
    };

    var checkUser = function () {
        if ($sessionStorage.userid == "" || $sessionStorage.userid == undefined || $sessionStorage.user == null || $sessionStorage.user.licenceStatus == 'expired') {
            $scope.toggleTpl('login.html');
            $rootScope.isLogin = false;
        } else {
            $scope.toggleTpl('dashboard.html');
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
        if ($rootScope.clientData != undefined) {
            if (validateForm() == false) {
                return false;
            };
            if (x == 'menu' && $rootScope.clientData.meals.length > 0) {
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
        if ($rootScope.clientData.meals.length > 0) {
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
        x.userId = $rootScope.user.userId;
        x.clientId = x.clientId == null ? $rootScope.client.clientId : x.clientId;
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Save',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, x: x }
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
        if (x == 'B') { return 'breakfast'; }
        if (x == 'MS') { return 'morning snack'; }
        if (x == 'L') { return 'lunch'; }
        if (x == 'AS') { return 'afternoon snack'; }
        if (x == 'D') { return 'dinner'; }
        if (x == 'MBS') { return 'meal before sleep'; }
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

    var getDateDiff = function (x) {
        var today = new Date();
        var date1 = today;
        var date2 = new Date(x);
        var diffDays = parseInt((date2 - date1) / (1000 * 60 * 60 * 24));
        return diffDays;
    }

    $scope.dateDiff = function () {
        if (localStorage.lastvisit) {
            return getDateDiff(localStorage.lastvisit)
        } else {
            return 0;
        }
    }

    var openNotificationPopup = function () {
        if ($rootScope.config.language == 'en') { return false;}
        $mdDialog.show({
            controller: notificationPoupCtrl,
            templateUrl: 'assets/partials/popup/notification.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { config: $rootScope.config }
        })
        .then(function (response) {
        }, function () {
        });
    };

    var notificationPoupCtrl = function ($scope, $mdDialog, d, $localStorage) {
        $scope.config = d.config;
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

}])

.controller('loginCtrl', ['$scope', '$http','$localStorage', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', '$mdDialog', function ($scope, $http, $localStorage, $sessionStorage, $window, $rootScope, functions, $translate, $mdDialog) {
    var webService = 'Users.asmx';

    $scope.toggleTpl = function (x) {
        $scope.tpl = x;
    }
    $scope.toggleTpl('loginTpl');

    var getDateDiff = function (x) {
        var today = new Date();
        var date1 = today;
        var date2 = new Date(x);
        var diffDays = parseInt((date2 - date1) / (1000 * 60 * 60 * 24));
        return diffDays;
    }

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
                    var daysToExpite = getDateDiff($rootScope.user.expirationDate);
                    if (daysToExpite <= 10 && daysToExpite > 0) {
                        $rootScope.mainMessage = $translate.instant('your subscription will expire in') + ' ' + daysToExpite + ' ' + (daysToExpite == 1 ? $translate.instant('day') : $translate.instant('days')) + '.';
                        $rootScope.mainMessageBtn = $translate.instant('renew subscription');
                    }
                    if (daysToExpite == 0) {
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

             //   $rootScope.loading = false;
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
       // eventObj_old.startDate = startDatePrev == null ? x.startDate : Date.parse(startDatePrev);
        eventObj_old.startDate = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.startDate) ? x.startDate : Date.parse(event.details[0].newSchedulerEvent.lastChange.startDate.prevVal);
        eventObj_old.userId = $rootScope.user.userId;
        remove(eventObj_old);

        $timeout(function () {
             save(eventObj);
        }, 500);
    }

    var save = function (x) {
        if ($rootScope.user.licenceStatus == 'demo' || $rootScope.user.userType < 1) {
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/Save',
            method: "POST",
            data: { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId, x: x }
        })
        .then(function (response) {
            //functions.alert($translate.instant(response.data.d));
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
        //$scope.getSchedulerByRoom();
    }

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId, x: x }
        })
        .then(function (response) {
            //alert($translate.instant(response.data.d))
        },
        function (response) {
            functions.alert($translate.instant(response.data));
        });
    }

    $scope.toggleTpl = function (x) {
        $rootScope.currTpl = './assets/partials/' + x;
    };

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
    init();

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

    $scope.package = function (x) {
        if (angular.isDefined($rootScope.user) && $rootScope.user != null) {
            if ($rootScope.user.licenceStatus == 'demo') {
                return 'demo';
            }
            switch (x) {
                case 0:
                    return 'start';
                    break;
                case 1:
                    return 'standard';
                    break;
                case 2:
                    return 'premium';
                    break;
                default:
                    return '';
            }
        } else {
            return '';
        }
        
    }

    var maxnumberofusers = function () {
        var usertype = $rootScope.user.userType;
        switch (usertype) {
            case 0:
                return 1;
                break;
            case 1:
                return 2;
                break;
            case 2:
                return 5;
                break;
            default:
                return 1;
        }
    }

    $scope.signup = function () {
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.users.length > 0) {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        if ($scope.users.length >= maxnumberofusers()) {
            functions.alert($translate.instant('max number of users is') + ' ' + maxnumberofusers(), '');
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
        $rootScope.user = x;
        $rootScope.currTpl = 'assets/partials/user.html';
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
.controller('dashboardCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions) {
    $rootScope.newTpl = 'assets/partials/clientsdata.html',
    $rootScope.selectedNavItem = 'clientsdata';

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
        $rootScope.currTpl = './assets/partials/' + x;
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
            $rootScope.client.date = new Date(new Date().setHours(0, 0, 0, 0)); // new Date($rootScope.client.date);
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
            //data: { userId: $sessionStorage.usergroupid }
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
            $scope.get(response);
        }, function () {
        });
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, $http, $timeout) {
        $scope.d = d;
        $scope.d.date = new Date($scope.d.date);
        $scope.d.birthDate = new Date($scope.d.birthDate);

        var getDateDiff = function (x) {
            var today = new Date();
            var date2 = today;
            var date1 = new Date(x);
            var diffDays = parseInt((date2 - date1) / (1000 * 60 * 60 * 24));
            return diffDays;
        }

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
                if (getDateDiff(x.birthDate) < 365) {
                    $scope.birthDateRequiredMsq = 'birth date is required';
                    return false;
                } else {
                    $scope.birthDateRequiredMsq = null;
                }
            }
            if ($rootScope.user.licenceStatus == 'demo' && $rootScope.clients.length > 0) {
                functions.demoAlert('in demo version you can enter only one client');
                return false;
            }
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
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            getCalculation();
            $scope.toggleTpl('clientStatictic');
            $scope.clientLog = JSON.parse(response.data.d);
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
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/UpdateClientLog',
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

    var getCalculation = function () {
        //var detailTee = 0;
        //if (angular.isDefined($rootScope.totalDailyEnergyExpenditure)) {
        //    if ($rootScope.totalDailyEnergyExpenditure.duration == 1440) {
        //        detailTee = $rootScope.totalDailyEnergyExpenditure.value;
        //    }
        //}
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            setClientLogGraphData($scope.displayType);
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

    $scope.changeDisplayType = function (x) {
        setClientLogGraphData(x);
    }

    var setClientLogGraphData = function (type) {
        var clientData = [];
        var goalFrom = [];
        var goalTo = [];
        var labels = [];
        $rootScope.clientLogGraphData = charts.createGraph(
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
        if (angular.isDefined($rootScope.calculation.recommendedWeight)) {
            angular.forEach($scope.clientLog, function (x, key) {
                if (type == 0) { clientData.push(x.weight); goalFrom.push($rootScope.calculation.recommendedWeight.min); goalTo.push($rootScope.calculation.recommendedWeight.max); }
                if (type == 1) { clientData.push(x.waist); goalFrom.push(95); }
                if (type == 2) { clientData.push(x.hip); goalFrom.push(97); }
                if (key % (Math.floor($scope.clientLog.length/31)+1) === 0) {
                    labels.push(new Date(x.date).toLocaleDateString());
                } else {
                    labels.push("");
                }
            });
        }
        

    };

    $rootScope.setClientLogGraphData = function (x) {
        setClientLogGraphData(x);
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
            //   $scope.openPdf();
        },
        function (response) {
            $scope.creatingPdf = false;
            alert(response.data.d)
        });

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }



        //var img = null;
        //if (document.getElementById("clientDataChart") != null) {
        //    img = document.getElementById("clientDataChart").toDataURL("image/png").replace(/^data:image\/(png|jpg);base64,/, "");
        //}
        //    $http({
        //        url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
        //        method: "POST",
        //        data: { userId: $sessionStorage.usergroupid, clientId: $rootScope.client.clientId }
        //    })
        //    .then(function (response) {
        //        $rootScope.clientLog = JSON.parse(response.data.d);
        //            $http({
        //                url: $sessionStorage.config.backend + 'PrintPdf.asmx/ClientPdf',
        //                method: "POST",
        //                data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, clientLog: $rootScope.clientLog, lang: $rootScope.config.language, imageData: img }
        //            })
        //          .then(function (response) {
        //              $scope.creatingPdf = false;
        //              var fileName = response.data.d;
        //              $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        //          },
        //          function (response) {
        //              $scope.creatingPdf = false;
        //              alert(response.data.d)
        //          });
        //    },
        //    function (response) {
        //        $scope.creatingPdf = false;
        //        alert(response.data.d)
        //    });
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
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: $rootScope.client.clientId }
        })
        .then(function (response) {
            $rootScope.clientLog = JSON.parse(response.data.d);
            //$rootScope.setClientLogGraphData($scope.type);
            $http({
                url: $sessionStorage.config.backend + 'PrintPdf.asmx/ClientLogPdf',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, clientLog: $rootScope.clientLog, lang: $rootScope.config.language, imageData: img }
            })
          .then(function (response) {
              $scope.creatingPdf = false;
              var fileName = response.data.d;
              $scope.pdfLink1 = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
              //   $scope.openPdf();
          },
          function (response) {
              $scope.creatingPdf = false;
              alert(response.data.d)
          });
        },
        function (response) {
            $scope.creatingPdf = false;
            alert(response.data.d)
        });
    }

    $scope.hidePdfLink1 = function () {
        $scope.pdfLink1 = null;
    }

    $scope.sendingMail = false;
    $scope.sendAppLinkToClientEmail = function (client) {
        if ($scope.sendingMail == true) { return false; }
        if (functions.isNullOrEmpty(client.email)) {
            functions.alert($translate.instant('email is required'), '');
            return false;
        }
        $scope.sendingMail = true;
        var link = $rootScope.config.clientapppageurl + '?uid=' + client.userId + '&cid=' + client.clientId
        var messageSubject = $translate.instant('nutrition program') + '. ' + $translate.instant('app access link')   //'Program Prehrane. link za pristup aplikaciji';
        var messageBody = '<p>' + $translate.instant('dear') + ',' + '</p>' +
            $translate.instant('the app access link to track your body weight and download menus is') + ': ' +
            '<br />' +
            '<strong><a href="' + link + '">' + link + '</a></strong>' + 
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
            functions.alert($translate.instant(response.data.d), '');
        },
        function (response) {
            $scope.sendingMail = false;
            functions.alert($translate.instant(response.data.d), '');
        });
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

    $scope.backToApp = function () {
        $rootScope.currTpl = './assets/partials/dashboard.html';
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
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.clients.length > 0) {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        $http({
            url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Save',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, clientId: $rootScope.client.clientId, activities: x }
        })
      .then(function (response) {
          debugger;
          $rootScope.clientData.dailyActivities = JSON.parse(response.data.d);
         // $rootScope.calculation.tee = $rootScope.totalDailyEnergyExpenditure;
         // functions.alert($translate.instant(response.data.d), '');
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

    var getCalculation = function () {
        //var detailTee = 0;
        //if (angular.isDefined($rootScope.totalDailyEnergyExpenditure)) {
        //    if ($rootScope.totalDailyEnergyExpenditure.duration == 1440) {
        //        detailTee = $rootScope.totalDailyEnergyExpenditure.value;
        //    }
        //}
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
        },
        function (response) {
            if (response.data.d === undefined) {
                functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
            } else {
                functions.alert(response.data.d, '');
            }
        });
    };


    //var getCalculation = function () {
    //    $http({
    //        url: $sessionStorage.config.backend + webService + '/GetCalculation',
    //        method: "POST",
    //        data: { client: $rootScope.clientData }
    //    })
    //    .then(function (response) {
    //        $rootScope.calculation = JSON.parse(response.data.d);
    //        $rootScope.appCalculation = JSON.parse(response.data.d);
    //        if (angular.isDefined($rootScope.totalDailyEnergyExpenditure)) {
    //            if ($rootScope.totalDailyEnergyExpenditure.duration == 1440) {
    //                $rootScope.calculation.tee = $rootScope.totalDailyEnergyExpenditure.value;
    //                $rootScope.appCalculation.tee = $rootScope.totalDailyEnergyExpenditure.value;
    //            }
    //        }

    //        if ($rootScope.clientData.goal.code == undefined || $rootScope.clientData.goal.code == null || $rootScope.clientData.goal.code == 0) {
    //            $rootScope.clientData.goal.code = $rootScope.calculation.goal.code;
    //        }

    //        getCharts();
    //        getGoals();
    //    },
    //    function (response) {
    //        alert(response.data.d)
    //    });
    //};
    getCalculation();

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

    $scope.getGoal = function (x) {

        var energy = 0;
        var activity = 0;
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

                //else {
                //    energy = -300;
                //    activity = 0;
                //}

              //  energy = $rootScope.appCalculation.goal.code == "G1" ? 0 : -300;
               // activity = 0;
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


                //else {
                //    energy = 0;
                //    activity = 0;
                //}

               
                break;
            case "G3":  // povecanje tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 300;
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

                //else


                //{
                //    energy = 0;
                //    activity = $rootScope.calculation.recommendedEnergyExpenditure + 0;
                //}

                //energy = 500;
                //activity = 200;

                break;
            default:
                energy = 0;
                activity = 0;
                break;
        }


        angular.forEach($rootScope.goals, function (value, key) {
            if (value.code == x) {
                $rootScope.clientData.goal.code = value.code;
                $rootScope.clientData.goal.title = value.title;
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
            data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, calculation: $rootScope.calculation, myCalculation: $rootScope.myCalculation, lang: $rootScope.config.language }
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
       // return energy > 0 ? $rootScope.calculation.recommendedEnergyExpenditure - energy : $rootScope.calculation.recommendedEnergyExpenditure;
        return $rootScope.calculation.recommendedEnergyExpenditure - energy;
    }

  //  var energyLeft = getEnergyLeft() > 0 ? getEnergyLeft() : $rootScope.calculation.recommendedEnergyExpenditure;

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

    var load = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.clientData.meals = JSON.parse(response.data.d);
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
    }

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

    if ($rootScope.clientData.meals.length == 0) {
        $rootScope.newTpl = 'assets/partials/meals.html';
        $rootScope.selectedNavItem = 'meals';
        functions.alert($translate.instant('choose meals'), '');
    }

    var getRecommendations = function (x) {

        /****** my recommendations *****/
        //var isMyrecommendations = false;

        //if (angular.isDefined($rootScope.myCalculation)) {
            //$rootScope.calculation.recommendedEnergyIntake = functions.isNullOrEmpty($rootScope.myCalculation.recommendedEnergyIntake) == true
           // isMyrecommendations = !functions.isNullOrEmpty($rootScope.myCalculation.recommendedEnergyIntake)

            //? $rootScope.appCalculation.recommendedEnergyIntake
            //: $rootScope.myCalculation.recommendedEnergyIntake;
            //$rootScope.recommendations.energy = $rootScope.calculation.recommendedEnergyIntake;

        //    $rootScope.calculation.recommendedEnergyExpenditure = functions.isNullOrEmpty($rootScope.myCalculation.recommendedEnergyExpenditure) == true
        //    ? $rootScope.appCalculation.recommendedEnergyExpenditure
        //    : $rootScope.myCalculation.recommendedEnergyExpenditure;
        //}
        /******************************/


        //var detailTee = 0;
        //if (angular.isDefined($rootScope.totalDailyEnergyExpenditure)) {
        //    if ($rootScope.totalDailyEnergyExpenditure.duration == 1440) {
        //        detailTee = $rootScope.totalDailyEnergyExpenditure.value;
        //    }
        //}
        $http({
            url: $sessionStorage.config.backend + webService + '/GetRecommendations',
            method: "POST",
            data: { client: x, myRecommendedEnergyIntake: $rootScope.myCalculation.recommendedEnergyIntake }
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
    getRecommendations(angular.copy($rootScope.clientData));

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
    if ($rootScope.currentMenu === undefined) { init(); }

    var initMenuDetails = function () {
        $http({
            url: $sessionStorage.config.backend + 'Menues.asmx/Init',
            method: "POST",
            data: {}
        })
        .then(function (response) {
            $rootScope.currentMenu = JSON.parse(response.data.d);
            $rootScope.currentMenu.data.meals = $rootScope.clientData.meals;
            angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                $rootScope.currentMenu.data.meals[key].description = '';
            })
            $rootScope.currentMeal = 'B';
            getTotals($rootScope.currentMenu);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    $scope.toggleMeals = function (x) {
        $rootScope.currentMeal = x;
    };
    $rootScope.currentMeal = 'B';

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
            $timeout(function () {
                $http({
                    url: $sessionStorage.config.backend + webService + '/ChangeFoodQuantity',
                    method: "POST",
                    data: { initFood: $rootScope.currentMenu.data.selectedInitFoods[idx], newQuantity: x.quantity, newMass: x.mass, type: type }
                })
                .then(function (response) {
                    $rootScope.currentMenu.data.selectedFoods[idx] = JSON.parse(response.data.d);
                    getTotals($rootScope.currentMenu);
                },
                function (response) {
                    //alert(response.data.d)
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
        $scope.food = d.food != undefined ? d.food : null;
        var initFood = d.food != undefined ? d.food : null;
        $scope.limit = 100;

        $scope.initCurrentFoodGroup = function () {
            $scope.currentGroup = { code: 'A', title: 'all foods' };
        }
        $scope.initCurrentFoodGroup();

        $scope.showMyFoods = function (x) {
            $scope.isShowMyFood = x;
           // $scope.limit = 100;
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

        $scope.getThermalTreatment = function (x, idx) {
            angular.forEach(x, function (value, key) {
                value.isSelected = false;
            })
            x[idx].isSelected = true
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            var data = { food: x, initFood: initFood }
            $mdDialog.hide(data);
        }

        $scope.changeQuantity = function (x, type) {
            if (x.quantity > 0.0001 && isNaN(x.quantity) == false && x.mass > 0.0001 && isNaN(x.mass) == false) {
                var currentFood = $scope.food.food;  // << in case where user change food title
                $timeout(function () {
                    $http({
                        url: $sessionStorage.config.backend + webService + '/ChangeFoodQuantity',
                        method: "POST",
                        data: { initFood: initFood, newQuantity: x.quantity, newMass: x.mass, type: type }
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
            //$rootScope.selectedFoods.splice(idx, 1);
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
            d: { currentMenu: $rootScope.currentMenu, clientData: $rootScope.clientData, totals: $rootScope.totals, settings: $rootScope.printSettings }
        })
        .then(function () {
        }, function () {
        });
    };

    $scope.printPreviewCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.currentMenu = d.currentMenu;
        $scope.clientData = d.clientData;
        $scope.totals = d.totals;
        $scope.settings = d.settings;

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

    };
  
    $scope.get = function () {
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
        } else {
            getMenuPopup();
        }
    }

    var getMenuPopup = function (x) {
        $mdDialog.show({
            controller: getMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/getmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            clientData: $rootScope.clientData,
            config: $rootScope.config
        })
        .then(function (x) {
            $rootScope.currentMenu = x;
            $rootScope.clientData.meals = x.data.meals;
            getTotals($rootScope.currentMenu);
            $rootScope.currentMeal = 'B';
        }, function () {
        });
    };

    var getMenuPopupCtrl = function ($scope, $mdDialog, $http, clientData, config, $translate, $translatePartialLoader, $timeout) {
        $scope.clientData = clientData;
        $scope.config = config;
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
            functions.demoAlert('this function is not available in demo version');
        } else {
            openSaveMenuPopup();
        }
    }

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
            //if (currentMenu.title == '' || currentMenu.title == undefined) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu title'), '');
                openSaveMenuPopup();
                return false;
            }
            currentMenu.diet = d.client.clientData.diet.diet;
            $mdDialog.hide($scope.d.currentMenu);
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Save',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, x: currentMenu, user: $scope.d.user }
            })
          .then(function (response) {
              if (response.data.d != 'error') {
                  $scope.d.currentMenu = JSON.parse(response.data.d);
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
            //if (currentMenu.title == '' || currentMenu.title == undefined) {
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
                functions.alert($translate.instant(response.data.d), '');
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
        if (!angular.isDefined($rootScope.totals)) { return false;}
        $scope.mealsTotals = [];
        $scope.mealsMin = [];
        $scope.mealsMax = [];
        $scope.mealsTitles = [];
        angular.forEach($rootScope.clientData.meals, function (value, key) {
            if (value.isSelected == true && angular.isDefined($rootScope.totals)) {
                $scope.mealsTotals.push($rootScope.totals.mealsTotalEnergy.length > 0 ? $rootScope.totals.mealsTotalEnergy[key].meal.energy : 0);
                $scope.mealsMin.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMin);
                $scope.mealsMax.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMax);
                $scope.mealsTitles.push($translate.instant($rootScope.getMealTitle(value.code)));
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
                $translate.instant('unit servings'),
                [
                    [t.servings.cerealsServ, t.servings.vegetablesServ, t.servings.fruitServ, t.servings.meatServ, t.servings.milkServ, t.servings.fatsServ],
                    [r.servings.cerealsServ, r.servings.vegetablesServ, r.servings.fruitServ, r.servings.meatServ, r.servings.milkServ, r.servings.fatsServ]
                ],
                [$translate.instant('cereals'), $translate.instant('vegetables'), $translate.instant('fruit'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fats')],
                ['#45b7cd', '#33cc33', '#33cc33'],
                [
                     {
                         label: $translate.instant('choosen'),
                         borderWidth: 1,
                         type: 'bar'
                     },
                     {
                         label: $translate.instant('recommended'),
                         borderWidth: 3,
                         hoverBackgroundColor: "rgba(255,99,132,0.4)",
                         hoverBorderColor: "rgba(255,99,132,1)",
                         type: 'line'
                     }
                ],
                false
        );
        //$rootScope.energyGraphData = charts.createGraph(
        //        ['energetska vrijednost'],
        //        [
        //            [t.energy],
        //            [r.energy]
        //        ],
        //        ['energija'],
        //        ['#45b7cd', '#ff6384', '#ff8e72']
        //);
        $rootScope.pieGraphData = charts.createGraph(
                [$translate.instant('nutrients')], //['nutrijenti'],
                [t.carbohydratesPercentage, t.proteinsPercentage, t.fatsPercentage],
                [$translate.instant('carbohydrates'), $translate.instant('proteins'), $translate.instant('fats')],
               // ['ugljikohidrati', 'bjelančevine', 'masti'],
                true
        );
        //$rootScope.otherFoodsGraphData = charts.createGraph(
        //        ['energetska vrijednost'],
        //        [t.otherFoodsEnergy],
        //        ['energija']
        //);


        $rootScope.mealsGraphData = charts.createGraph(
               [$translate.instant('meals')], //['obroci'],
               [ $scope.mealsTotals, $scope.mealsMin, $scope.mealsMax ],
               $scope.mealsTitles,
               ['#45b7cd', '#ff6384', '#33cc33'],
               [
                    {
                        label: $translate.instant('choosen'),
                        borderWidth: 1,
                        type: 'bar'
                    },
                    {
                        label: $translate.instant('recommended'),
                        borderWidth: 3,
                        hoverBackgroundColor: "rgba(255,99,132,0.4)",
                        hoverBorderColor: "rgba(255,99,132,1)",
                        type: 'line'
                    },
                     {
                         label: $translate.instant('recommended'),
                        borderWidth: 3,
                        hoverBackgroundColor: "rgba(255,99,132,0.4)",
                        hoverBorderColor: "rgba(255,99,132,1)",
                        type: 'line'
                    }
               ],
               false
       );
        //TODO
        $rootScope.parametersGraphData = charts.createGraph(
               [$translate.instant('parameters')],
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
                    t.vitaminK],
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
                    r.vitaminK.rda]
               ],
               [
                'fibers',
                'monounsaturatedFats',
                'polyunsaturatedFats',
                'calcium',
                'magnesium',
                'phosphorus',
                'iron',
                'copper',
                'zinc',
                'manganese',
                'selenium',
                'iodine',
                'retinol',
                'vitaminD',
                'vitaminE',
                'vitaminB1',
                'vitaminB2',
                'vitaminB3',
                'vitaminB6',
                'vitaminB12',
                'folate',
                'pantothenicAcid',
                'biotin',
                'vitaminC',
                'vitaminK'],
               ['#45b7cd', '#2fed4f', '#ff8e72'],
               [
                    {
                        label: "Odabrano",
                        borderWidth: 1,
                        type: 'bar'
                    },
                    {
                        label: "RDA",
                        borderWidth: 3,
                        hoverBackgroundColor: "rgba(255,99,132,0.4)",
                        hoverBorderColor: "rgba(255,99,132,1)",
                        type: 'line'
                    }
               ],
               false
        );

        //TODO
        $rootScope.parametersGraphDataUI = charts.createGraph(
               [$translate.instant('parameters')],
               [
                   [
                       t.saturatedFats, t.trifluoroaceticAcid, t.cholesterol
                       //t.saturatedFats / r.saturatedFats.ui * 100,
                       //t.trifluoroaceticAcid / r.trifluoroaceticAcid.ui * 100,
                       //t.cholesterol / r.cholesterol.ui * 100
                   ],
                   [r.saturatedFats.ui, r.trifluoroaceticAcid.ui, r.cholesterol.ui]
               ],
               ['saturatedFats', 'trifluoroaceticAcid', 'cholesterol'],
               ['#f44242', '#ff6384'],
               [
                    {
                        label: "Odabrano",
                        borderWidth: 1,
                        type: 'bar',
                        backgroundColor: "rgb(244, 66, 66)",
                        hoverBackgroundColor: "rgb(244, 66, 66)",
                    },
                    {
                        label: "UI",
                        borderWidth: 3,
                        type: 'line'
                    }
               ]
        );

        //TODO
        $rootScope.parametersGraphDataMDA = charts.createGraph(
               ['parametri'],
               [
                   [t.sodium, t.potassium, t.chlorine
                       //t.sodium / r.sodium.mda * 100,
                       //t.potassium / r.potassium.mda * 100,
                       //t.chlorine / r.chlorine.mda * 100
                   ],
                   [r.sodium.ui],
                   [r.sodium.mda, r.potassium.mda, r.chlorine.mda]
               ],
               ['sodium', 'potassium', 'chlorine'],
               ['#49a5af', '#f44242', '#2fed4f'],
               [
                    {
                        label: "Odabrano",
                        borderWidth: 1,
                        type: 'bar',
                        backgroundColor: "rgb(244, 66, 66)",
                        hoverBackgroundColor: "rgb(244, 66, 66)",
                    },
                    {
                        label: "UI",
                        borderWidth: 10,
                        type: 'line',
                        backgroundColor: "rgb(244, 66, 66)",
                    },
                    {
                        label: "MDA",
                        borderWidth: 3,
                        type: 'line'
                    },
               ],
               false
        );
    }

    var totalEnergyChart = function () {
        var recommended = parseInt($rootScope.recommendations.energy);
        var id = 'energyChart';
        var value = $rootScope.totals.energy.toFixed(0);
        var unit = 'kcal';

        var options = {
            title: 'energy',
            //width: 220,
            //height: 150,
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
            //width: 220,
            //height: 150,
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

        //var title = 'Energija';
        //var min = 0;
        //var max = suggested + (suggested * 0.5);
        //var greenFrom = 0;
        //var greenTo = suggested - (suggested * 0.02);
        //var yellowFrom = suggested - (suggested * 0.02);
        //var yellowTo = suggested;
        //var redFrom = suggested;
        //var redTo = max;
        //var minorTicks = 5;

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
            //width: 220,
            //height: 150,
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
            //width: 220,
            //height: 150,
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
            //width: 220,
            //height: 150,
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
            //width: 220,
            //height: 150,
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
            //width: 220,
            //height: 150,
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
            //width: 220,
            //height: 150,
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
                        data: { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, clientData: $rootScope.clientData, totals: $rootScope.totals, consumers: consumers, lang: $rootScope.config.language, settings: $scope.settings }
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
        $rootScope.newTpl = './assets/partials/myrecipes.html';
        $rootScope.selectedNavItem = 'myrecipes';
        $rootScope.recipeData = data;
        $rootScope.currentMealForRecipe = currentMeal;
    }

}])

.controller('analyticsCtrl', ['$scope', '$http', '$window', '$rootScope', '$mdDialog', 'charts', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, functions, $translate) {

    $scope.toggleTpl = function (x) {
        $scope.analyticsTpl = x;
    };
    $scope.toggleTpl('tablesTpl');

    $scope.printPreview = function () {
        $mdDialog.show({
            controller: $scope.printPreviewCtrl,
            templateUrl: 'assets/partials/popup/printmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: $rootScope.currentMenu }
        })
        .then(function () {
        }, function () {
        });
    };

    $scope.printPreviewCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = d.currentMenu.data.selectedFoods;
        $scope.meals = d.currentMenu.data.meals;
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    };

    var displayCharts = function () {
        var t = $rootScope.totals;
        var r = $rootScope.recommendations;
        $rootScope.servGraphData = charts.createGraph(
                $translate.instant('unit servings'), //['jedinična serviranja'],
                [
                    [t.servings.cerealsServ, t.servings.vegetablesServ, t.servings.fruitServ, t.servings.meatServ, t.servings.milkServ, t.servings.fatsServ],
                    [r.servings.cerealsServ, r.servings.vegetablesServ, r.servings.fruitServ, r.servings.meatServ, r.servings.milkServ, r.servings.fatsServ]
                ],
                [$translate.instant('carbohytrates'), $translate.instant('fruit'), $translate.instant('vegetables'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fat')],
               // ['ugljikohidrati', 'povrće', 'voće', 'meso', 'mlijeko', 'masti'],
                ['#45b7cd', '#ff6384', '#ff8e72'],
                [
                     {
                         label: $translate.instant('choosen'), //"Odabrano",
                         borderWidth: 1,
                         type: 'bar'
                     },
                     {
                         label: $translate.instant('recommended'), // "Preporučeno",
                         borderWidth: 3,
                         hoverBackgroundColor: "rgba(255,99,132,0.4)",
                         hoverBorderColor: "rgba(255,99,132,1)",
                         type: 'line'
                     }
                ],
                false
        );
        $rootScope.energyGraphData = charts.createGraph(
                ['energetska vrijednost'],
                [
                    [t.energy],
                    [r.energy]
                ],
                ['energija'],
                ['#45b7cd', '#ff6384', '#ff8e72'],
                false
        );
        $rootScope.pieGraphData = charts.createGraph(
                ['nutrijenti'],
                [t.carbohydratesPercentage, t.proteinsPercentage, t.fatsPercentage],
                ['ugljikohidrati', 'bjelančevine', 'masti'],
                false
        );
        $rootScope.otherFoodsGraphData = charts.createGraph(
                ['energetska vrijednost'],
                [t.otherFoodsEnergy],
                ['energija'],
                false
        );
        //TODO
        $rootScope.parametersGraphData = charts.createGraph(
               ['parametri'],
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
                    t.vitaminK],
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
                    r.vitaminK.rda]
               ],
               [
                'fibers',
                'monounsaturatedFats',
                'polyunsaturatedFats',
                'calcium',
                'magnesium',
                'phosphorus',
                'iron',
                'copper',
                'zinc',
                'manganese',
                'selenium',
                'iodine',
                'retinol',
                'vitaminD',
                'vitaminE',
                'vitaminB1',
                'vitaminB2',
                'vitaminB3',
                'vitaminB6',
                'vitaminB12',
                'folate',
                'pantothenicAcid',
                'biotin',
                'vitaminC',
                'vitaminK'],
               ['#45b7cd', '#2fed4f', '#ff8e72'],
               [
                    {
                        label: "Odabrano",
                        borderWidth: 1,
                        type: 'bar'
                    },
                    {
                        label: "RDA",
                        borderWidth: 3,
                        hoverBackgroundColor: "rgba(255,99,132,0.4)",
                        hoverBorderColor: "rgba(255,99,132,1)",
                        type: 'line'
                    }
               ],
               false
        );

        //TODO
        $rootScope.parametersGraphDataUI = charts.createGraph(
               ['parametri'],
               [
                   [
                       t.saturatedFats, t.trifluoroaceticAcid, t.cholesterol
                       //t.saturatedFats / r.saturatedFats.ui * 100,
                       //t.trifluoroaceticAcid / r.trifluoroaceticAcid.ui * 100,
                       //t.cholesterol / r.cholesterol.ui * 100
                   ],
                   [r.saturatedFats.ui, r.trifluoroaceticAcid.ui, r.cholesterol.ui]
               ],
               ['saturatedFats', 'trifluoroaceticAcid', 'cholesterol'],
               ['#f44242', '#ff6384'],
               [
                    {
                        label: "Odabrano",
                        borderWidth: 1,
                        type: 'bar',
                        backgroundColor: "rgb(244, 66, 66)",
                        hoverBackgroundColor: "rgb(244, 66, 66)",
                    },
                    {
                        label: "UI",
                        borderWidth: 3,
                        type: 'line'
                    }
               ],
               false
        );

        //TODO
        $rootScope.parametersGraphDataMDA = charts.createGraph(
               ['parametri'],
               [
                   [t.sodium, t.potassium, t.chlorine
                       //t.sodium / r.sodium.mda * 100,
                       //t.potassium / r.potassium.mda * 100,
                       //t.chlorine / r.chlorine.mda * 100
                   ],
                   [r.sodium.ui],
                   [r.sodium.mda, r.potassium.mda, r.chlorine.mda]
               ],
               ['sodium', 'potassium', 'chlorine'],
               ['#49a5af', '#f44242', '#2fed4f'],
               [
                    {
                        label: "Odabrano",
                        borderWidth: 1,
                        type: 'bar',
                        backgroundColor: "rgb(244, 66, 66)",
                        hoverBackgroundColor: "rgb(244, 66, 66)",
                    },
                    {
                        label: "UI",
                        borderWidth: 10,
                        type: 'line',
                        backgroundColor: "rgb(244, 66, 66)",
                    },
                    {
                        label: "MDA",
                        borderWidth: 3,
                        type: 'line'
                    },
               ],
               false
        );
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

    $scope.new = function(){
        init();
    }

    $scope.delete = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete food') + '?')
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
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.clients.length > 0) {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        if (functions.isNullOrEmpty(x.food)) {
            functions.alert($translate.instant('food title is required'), '');
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
        if (x.foodGroup.code == 'OF') { return true;}
        if ( x.servings.cerealsServ > 0 ||
             x.servings.vegetablesServ > 0 ||
             x.servings.fruitServ > 0 ||
             x.servings.meatServ > 0 ||
             x.servings.milkServ > 0 ||
             x.servings.fatsServ > 0
            ) { return false; } else { return true; }
    }

    $scope.search = function () {
        openMyFoodsPopup();
    }

    var openMyFoodsPopup = function () {
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
            d: { foods:$rootScope.foods, myFoods:$rootScope.myFoods, foodGroups:$rootScope.foodGroups, food:food, idx:idx, config:$rootScope.config }
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

//TODO remove recipes

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
            //alert(response.data.d);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.search = function() {
        openMyRecipesPopup();
    }

    var openMyRecipesPopup = function () {
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

    };

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
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.clients.length > 0) {
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
       },
       function (response) {
           functions.alert($translate.instant(response.data.d), '');
       });
    }

    $scope.delete = function (x) {
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
         //  functions.alert($translate.instant(response.data.d), '');
           load();
       },
       function (response) {
           functions.alert($translate.instant(response.data.d), '');
       });
    }

}])

.controller('printCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    $scope.consumers = 1;

    $scope.printWindow = function () {
        window.print();
    };

    $scope.pdfLink = null;
    var printMenuPdf = function () {
        if (angular.isDefined($rootScope.currentMenu)) {
            var currentMenu = angular.copy($rootScope.currentMenu);
            currentMenu.data.selectedFoods = $scope.foods;
            $http({
                url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, clientData: $rootScope.clientData, totals: $rootScope.totals, consumers: $scope.consumers, lang: $rootScope.config.language }
            })
              .then(function (response) {
                  var fileName = response.data.d;
                  $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
                 // $scope.openPdf();
              },
              function (response) {
                  alert(response.data.d)
              });
        }
    }

    var printMenuDetailsPdf = function () {
        if (angular.isDefined($rootScope.currentMenu)) {
            var currentMenu = angular.copy($rootScope.currentMenu);
            $http({
                url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuDetailsPdf',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, calculation: $rootScope.calculation, totals: $rootScope.totals, recommendations: $rootScope.recommendations, lang: $rootScope.config.language }
            })
              .then(function (response) {
                  var fileName = response.data.d;
                  $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
              },
              function (response) {
                  alert(response.data.d)
              });
        }
    }

    $scope.creatingPdf = false;
    var printClientPdf = function () {
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/ClientPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, clientLog: $rootScope.clientLog,  lang: $rootScope.config.language }
        })
          .then(function (response) {
              var fileName = response.data.d;
              $scope.creatingPdf = false;
              $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
           //   $scope.openPdf();
          },
          function (response) {
              $scope.creatingPdf = false;
              alert(response.data.d)
          });
    }

    $scope.printPdf = function () {
        if ($scope.printTpl == 'menuTpl') { printMenuPdf(); }
        if ($scope.printTpl == 'menuAnalysisTpl') { printMenuDetailsPdf(); }
        if ($scope.printTpl == 'clientTpl') { printClientPdf(); }
    }

    $scope.openPdf = function () {
        if ($scope.pdfLink != null) {
            window.open($scope.pdfLink, window.innerWidth <= 800 && window.innerHeight <= 600 ? '_self' : '_blank');
        }
    }

    $scope.type = 0;
    $scope.setType = function (x) {
        $scope.type = x;
        $rootScope.setClientLogGraphData($scope.type);
    }

    var getClientLog = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            $rootScope.clientLog = JSON.parse(response.data.d);
            $rootScope.setClientLogGraphData($scope.type);
        },
        function (response) {
            alert(response.data.d)
        });
   }

    var getClient = function () {
        $http({
            url: $sessionStorage.config.backend + 'Clients.asmx/Get',
            method: "POST",
            data: { userId: $sessionStorage.userid, clientId: $rootScope.client.clientId }
        })
          .then(function (response) {
              $scope.client = JSON.parse(response.data.d);
              getClientLog($scope.client);
          },
          function (response) {
              alert(response.data.d)
          });
    }
    if ($rootScope.client != undefined) { getClient(); }

    $scope.toggleTpl = function (x) {
        $scope.pdfLink = null;
        $scope.printTpl = x;
        $scope.printPdf();
        if (x == 'weeklyMenuTpl') {
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
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
    };

    $scope.changeNumberOfConsumers = function (x) {
        $scope.consumers = x;
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/ChangeNumberOfConsumers',
            method: "POST",
            data: { foods: $rootScope.currentMenu.data.selectedFoods, number: x }
        })
       .then(function (response) {
           $scope.foods = JSON.parse(response.data.d);
           $scope.toggleTpl('menuTpl');
       },
       function (response) {
           //   alert(response.data.d)
       });
    }
    if (angular.isDefined($rootScope.currentMenu)) { $scope.changeNumberOfConsumers($scope.consumers); }

    $scope.menuList = [];
    $scope.getMenuList = function (id1, id2, id3, id4, id5, id6, id7) {
        $scope.menuList = [id1, id2, id3, id4, id5, id6, id7];
    }

    $scope.creatingPdf = false;
    $scope.pageSizes = ['A4', 'A3', 'A2', 'A1'];
    $rootScope.printSettings.pageSize = 'A3';
    $rootScope.printSettings.showDescription = false;
    $rootScope.printSettings.orientation = 'L';

    $scope.printWeeklyMenu = function (consumers, printSettings) {
        if ($scope.menuList.length == 0) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/WeeklyMenuPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, menuList: $scope.menuList, clientData: $rootScope.clientData, consumers: consumers, lang: $rootScope.config.language, settings: printSettings }
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

}])

.controller('orderCtrl', ['$scope', '$http', '$rootScope', '$translate', function ($scope, $http, $rootScope, $translate) {
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
        $scope.user.price = totalprice;
        $scope.user.priceEur = totalprice / $rootScope.config.eur;
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
           $scope.showAlert = true;
           $scope.showPaymentDetails = true;
       },
       function (response) {
           $scope.showAlert = false;
           $scope.showPaymentDetails = false;
           $scope.sendicon = 'fa fa-paper-plane-o';
           $scope.sendicontitle = $translate.instant('send');
           alert(response.data.d);
       });
    }

    $scope.registration = function () {
        window.location.hash = 'registration';
    }


}])

//.controller('helpCtrl', ['$scope', '$rootScope', '$translate', function ($scope, $rootScope, $translate) {

//}])

.controller('infoCtrl', ['$scope', '$rootScope', '$translate', function ($scope, $rootScope, $translate) {
    $scope.package = function (x) {
        if(angular.isUndefined(x)){
            return '';
        }
        if ($rootScope.user.licenceStatus == 'demo') {
            return 'demo';
        }
        switch (x) {
            case 0:
                return 'start';
                break;
            case 1:
                return 'standard';
                break;
            case 2:
                return 'premium';
                break;
            default:
                return '';
        }
    }
}])

.controller('settingsCtrl', ['$scope', '$http', '$rootScope', '$translate', '$sessionStorage', 'functions', function ($scope, $http, $rootScope, $translate, $sessionStorage, functions) {
    var webService = 'Files.asmx';
    if(angular.isDefined($sessionStorage.settings)){$rootScope.settings = $sessionStorage.settings;}

    $scope.save = function (d) {
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
    $scope.consumers = 1;

    $scope.printWindow = function () {
        window.print();
    };

    $scope.pdfLink = null;
    $scope.creatingPdf = false;
    var printMenuPdf = function () {
        $scope.creatingPdf = true;
        if (angular.isDefined($rootScope.currentMenu)) {
            var currentMenu = angular.copy($rootScope.currentMenu);
            currentMenu.data.selectedFoods = $scope.foods;
            $http({
                url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, clientData: $rootScope.clientData, totals: $rootScope.totals, consumers: $scope.consumers, lang: $rootScope.config.language }
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
    }

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
           $scope.menues = JSON.parse(response.data.d);
           if ($scope.menues.length == 0) {
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

    $scope.menuList = [];
    $scope.getMenuList = function (id1, id2, id3, id4, id5, id6, id7) {
        $scope.menuList = [id1, id2, id3, id4, id5, id6, id7];
        $scope.pdfLink = null;
    }

    $scope.creatingPdf = false;
    $scope.pageSizes = ['A4', 'A3', 'A2', 'A1'];
    $rootScope.printSettings.pageSize = 'A3';
    $rootScope.printSettings.showDescription = false;
    $rootScope.printSettings.orientation = 'L';

    $scope.printWeeklyMenu = function (consumers, printSettings) {
        if ($scope.menuList.length == 0) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/WeeklyMenuPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, menuList: $scope.menuList, clientData: $rootScope.clientData, consumers: consumers, lang: $rootScope.config.language, settings: printSettings }
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



