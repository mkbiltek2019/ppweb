/*!
app.js
(c) 2017 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'pascalprecht.translate', 'ngMaterial', 'chart.js', 'ngStorage', 'functions', 'charts'])

.config(['$stateProvider', '$urlRouterProvider', '$translateProvider', '$translatePartialLoaderProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $translateProvider, $translatePartialLoaderProvider, $httpProvider) {

    $urlRouterProvider.otherwise('/index');

    $stateProvider
        .state('index', {
            url: '/index',
            templateUrl: 'assets/partials/index.html',
            controller: 'appCtrl',
        })
        .state('dashboard', {
            url: '/dashboard',
            templateUrl: 'assets/partials/dashboard.html',
            controller: 'dashboardCtrl'
        })
        .state('clientsdata', {
            url: '/clientsdata',
            templateUrl: 'assets/partials/clientsdata.html',
            controller: 'clientsCtrl'
        })
        .state('calculation', {
            url: '/calculation',
            templateUrl: 'assets/partials/calculation.html',
            controller: 'calculationCtrl'
        });


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

.config(function($mdDateLocaleProvider) {
        $mdDateLocaleProvider.formatDate = function(date) {
            return moment(date).format("DD.MM.YYYY");
        }
    })

.controller('AppCtrl', ['$scope', '$mdDialog', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', function ($scope, $mdDialog, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions) {
    $rootScope.loginUser = $sessionStorage.loginuser;
    $rootScope.user = $sessionStorage.user;

    if ($rootScope.user != undefined) {
        if ($rootScope.user.licenceStatus == 'demo') {
            $rootScope.mainMessage = $translate.instant('you are currently working in a demo version') + '. ' + $translate.instant('some functions are disabled' + '.');
        }
    }
   
    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              $sessionStorage.config = response.data;
              var queryLang = location.search.substring(6);
              if (angular.isDefined(queryLang)) {
                  if (queryLang == 'hr' || queryLang == 'sr' || queryLang == 'en') {
                      $rootScope.setLanguage(queryLang);
                  }
              }
              if ($sessionStorage.islogin == true) { $rootScope.loadData(); }
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
            data: { userId: $sessionStorage.usergroupid }
        })
        .then(function (response) {
            var data = JSON.parse(response.data.d);
            $rootScope.foods = data.foods;

            angular.forEach($rootScope.foods, function (value, key) {
                $rootScope.foods[key].food = $translate.instant($rootScope.foods[key].food).replace('&gt;', '<').replace('&lt;', '>');
            })
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
        $rootScope.loadFoods();
        $rootScope.loadPals();
        $rootScope.loadGoals();
        $rootScope.loadActivities();
        $rootScope.loadDiets();
    }

    $scope.toggleTpl = function (x) {
        $rootScope.currTpl = './assets/partials/' + x;
    };

    var checkUser = function () {
        if ($sessionStorage.userid == "" || $sessionStorage.userid == undefined || $sessionStorage.user.licenceStatus == 'expired') {
            $scope.toggleTpl('login.html');
            $rootScope.isLogin = false;
        } else {
            $scope.toggleTpl('dashboard.html');
            $scope.activeTab = 0;
            $rootScope.isLogin = true;
        }
    }
    checkUser();

    $scope.toggleNewTpl = function (x) {
        if ($rootScope.clientData != undefined) {

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

    $scope.today = new Date();

    $scope.getDateDiff = function (x) {
        var today = new Date();
        var date1 = today;
        var date2 = new Date(x);
        var diffDays = parseInt((date2 - date1) / (1000 * 60 * 60 * 24));
        return diffDays;
    }

    $scope.showSaveMessage = false;

    $scope.logout = function () {
        $sessionStorage.loginuser = null;
        $sessionStorage.user = null;
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
        //angular.forEach(x.meals, function (value, key) {
        //    x.meals[key].title = $translate.instant(value.title);
        //})
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

    $scope.showTabs = function () {
        if(angular.isUndefined($rootScope.clientData)){return false;}
        var x = $rootScope.clientData;
        if (x.clientId != null && x.height > 0 && x.weight > 0 && x.pal.value > 0) {
            return true;
        } else {
            return false;
        }
    }

}])

.controller('loginCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', '$mdDialog', function ($scope, $http, $sessionStorage, $window, $rootScope, functions, $translate, $mdDialog) {
    var webService = 'Users.asmx';

    $scope.toggleTpl = function (x) {
        $scope.tpl = x;
    }
    $scope.toggleTpl('loginTpl');

    $scope.login = function (u, p) {
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
                $rootScope.loginUser = JSON.parse(response.data.d);
                $sessionStorage.loginuser = $rootScope.loginUser;
                $sessionStorage.userid = $rootScope.user.userId;
                $sessionStorage.usergroupid = $rootScope.user.userGroupId;
                $sessionStorage.username = $rootScope.user.userName;
                $sessionStorage.user = $rootScope.user;
                $sessionStorage.islogin = true;
                $rootScope.isLogin = true;
                $rootScope.loadData();

             //   $rootScope.getUserSettings();  //TODO


                if ($rootScope.user.licenceStatus == 'expired') {
                    $rootScope.isLogin = false;
                    functions.alert($translate.instant('your account has expired'), $translate.instant('renew now'));
                    $rootScope.currTpl = './assets/partials/order.html';
                } else {
                    $rootScope.currTpl = './assets/partials/dashboard.html';
                    if ($rootScope.user.licenceStatus == 'demo') {
                        $rootScope.mainMessage = $translate.instant('you are currently working in a demo version') + '. ' + $translate.instant('some functions are disabled') + '.';
                       // functions.alert($translate.instant('you are currently working in a demo version'), $translate.instant('some options are disabled'));
                    }
                }

                
             //   $rootScope.loading = false;
            } else {
                $rootScope.loading = false;
                $scope.errorLogin = true;
                $scope.errorMesage = $translate.instant('wrong user name or password');
               // $rootScope.currTpl = 'assets/partials/singup.html';  //<< Only fo first registration
            }
        },
        function (response) {
            $scope.errorLogin = true;
            $scope.errorMesage = $translate.instant('user was not found');
        });
     }

     $scope.singup = function () {
         $rootScope.currTpl = 'assets/partials/singup.html';
     }


     $scope.forgotPasswordPopup = function () {
         $mdDialog.show({
             controller: $scope.forgotPasswordPopupCtrl, // RealizationCtrl,
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
                 url: $sessionStorage.config.backend + webService + '/ForgotPassword', // '../Users.asmx/Init',
                 method: "POST",
                 data: { email: x }
             })
           .then(function (response) {
               $mdDialog.hide();
               functions.alert($translate.instant(JSON.parse(response.data.d)), '');
           },
           function (response) {
               functions.alert($translate.instant(response.data.d), '');
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

.controller('singupCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, functions, $translate) {
    var webService = 'Users.asmx';
    $scope.showAlert = false;
    $scope.passwordConfirm = '';

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Init', // '../Users.asmx/Init',
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

    $scope.singup = function () {
        $scope.newUser.userName = $scope.newUser.email;
        if ($scope.newUser.firstName == "" || $scope.newUser.lastName == "" || $scope.newUser.email == "" || $scope.newUser.password == "" || $scope.passwordConfirm == "") {
            functions.alert($translate.instant('all fields are required'), '');
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            functions.alert($translate.instant('passwords are not the same'), '');
            return false;
        }

        $http({
            url: $sessionStorage.config.backend + webService + '/Singup',
            method: "POST",
            data: { x: $scope.newUser }
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
            functions.alert($translate.instant(response.data.d), '');
        });
    }

}])

.controller("schedulerCtrl", ['$scope', '$localStorage', '$http', '$rootScope', '$timeout', '$sessionStorage', 'functions', function ($scope, $localStorage, $http, $rootScope, $timeout, $sessionStorage, functions) {
    var webService = 'Scheduler.asmx';

    $scope.room = 0;
    $scope.getSchedulerByRoom = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetSchedulerByRoom',
            method: 'POST',
            data: { userId: $rootScope.user.userId, room: $scope.room }
        })
       .then(function (response) {

           $rootScope.events = JSON.parse(response.data.d);
          // $timeout(function () {
               showScheduler();
         //  }, 50);

       },
       function (response) {
           alert(response.data.d)
       });
    };
    $scope.getSchedulerByRoom();

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
                       // editEvent(this.getTemplateData(), event);
                      //  alert('Edit Event:' + this.isNew() + ' --- ' + this.getContentNode().val() + ' --- ' + this.getTemplateData());
                    },
                    delete: function (event) {
                        removeEvent(this.getTemplateData(), event);
                       // alert('Delete Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                       //  Note: The cancel event seems to be buggy and occurs at the wrong times, so I commented it out.
                              },
                    //          cancel: function(event) {
                    //              alert('Cancel Event:' + this.isNew() + ' --- ' + this.getContentNode().val());
                    //}
                }
            });

            new Y.Scheduler({
                activeView: weekView,
                boundingBox: '#myScheduler',  //'#' + $scope.room, 
                date: new Date(),
                eventRecorder: eventRecorder,
                items: $rootScope.events,
                render: true,
                views: [dayView, weekView, monthView, agendaView],
                strings: { agenda: 'Dnevni red', day: 'Dan', month: 'Mjesec', table: 'Tablica', today: 'Danas', week: 'Tjedan', year: 'Godina' },
            }
          );
        });
    }

    var addEvent = function (x, event) {
        $rootScope.events.push({
            //'yuid': event.details[0].newSchedulerEvent._yuid,
            'room': $scope.room,
            'clientId': '0',  // << TODO
            'content': event.details[0].newSchedulerEvent.changed.content,
            'endDate': x.endDate,
            'startDate': x.startDate,
            
        });
        var eventObj = {};
        eventObj.room = $scope.room;
        eventObj.clientId = '0';
        eventObj.content = event.details[0].newSchedulerEvent.changed.content == null ? x.content : event.details[0].newSchedulerEvent.changed.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;

        saveEvent(eventObj);

    }

    var saveEvent = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete', // '../Scheduler.asmx/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userId, x: x }  // '{x:' + JSON.stringify(x) + '}'
        })
        .then(function (response) {
            save(x);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var save = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Save',
            method: "POST",
            data: { userId: $rootScope.user.userId, x: x }
        })
        .then(function (response) {
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var removeEvent = function (x, event) {
        var eventObj = {};
        eventObj.room = $scope.room;
        eventObj.clientId = '0';
        eventObj.content = x.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        remove(eventObj);
    }

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userId, x: x }
        })
        .then(function (response) {
            $scope.getSchedulerByRoom();
        },
        function (response) {
            alert(response.data.d)
        });
    }

}])

.controller('userCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Users.asmx';

    $scope.adminTypes = [
       {
           'value': '0',
           'text': 'Supervizor'
       },
       {
           'value': '1',
           'text': 'Admin'
       }
    ];

    $scope.userTypes = [
      {
          'value': '0',
          'text': 'Tip korisnika'
      },
      {
          'value': '1',
          'text': 'Tip korisnika'
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
            data: { userGroupId: $rootScope.user.userGroupId }
        })
      .then(function (response) {
          $scope.users = JSON.parse(response.data.d);
        //  angular.forEach($scope.users, function (x, key) {
          //    x.expirationDate = $rootScope.user.expirationDate;
       //   });
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

    $scope.singup = function () {
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.users.length > 0) {
            functions.demoAlert('this function is not available in demo version');
            return false;
        }
        if ($scope.users.length >= $sessionStorage.config.maxnumberofusers) {
            functions.alert($translate.instant('max number of users is') + ' ' + $sessionStorage.config.maxnumberofusers, '');
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

        if ($scope.newUser.password == "" || $scope.passwordConfirm == "") {
            functions.alert($translate.instant('enter password'), '');
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            functions.alert($translate.instant('passwords are not the same'), '');
            return false;
        }

        $http({
            url: $sessionStorage.config.backend + webService + '/Singup',
            method: "POST",
            data: { x: $scope.newUser }
        })
        .then(function (response) {
            load();
            functions.alert($translate.instant(response.data.d));
           // functions.alert($translate.instant('registration completed successfully'), '');
        },
        function (response) {
            functions.alert($translate.instant(response.data.d));
        });
    }

    $scope.update = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Update',
            method: 'POST',
            data: {x: $rootScope.user}

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
            data: {userId: x}
        })
     .then(function (response) {
         $rootScope.user = JSON.parse(response.data.d);
         $rootScope.currTpl = 'assets/partials/user.html';
     },
     function (response) {
         alert(response.data.d)
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

    $scope.showPassword = function () {
        $scope.showpass = $scope.showpass == true ? false : true;
    }

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
    $scope.toggleSubTpl('pal');

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

    if ($rootScope.clientData == undefined || null) {
        if ($rootScope.client != undefined) {
            init($rootScope.client);
        }
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
        $scope.toggleSubTpl('pal');
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid }
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

    //Test jsPdf
    $scope.testPdf = function () {
        var pdf = new jsPDF('p', 'pt', 'a4');  //One of "portrait" or "landscape" (or shortcuts "p" (Default), "l")
        var options = {
            pagesplit: true
        };

        $("#printr").show();
        pdf.addHTML($("#printr"), 0, 0, options, function () {
            $("#printr").hide();
            //  pdf.save("realizacija.pdf");
            //  pdf.autoPrint();
            pdf.output("dataurlnewwindow"); // this opens a new popup,  after this the PDF opens the print window view but there are browser inconsistencies with how this is handled
        });
    }

    $rootScope.newClient = function () {
        $scope.toggleSubTpl('pal');
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
            fullscreen: $scope.customFullscreen,
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

        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.save = function (x) {
            if ($rootScope.user.licenceStatus == 'demo' && $rootScope.clients.length > 0) {
                functions.demoAlert('in demo version you can enter only one client');
                return false;
            }
            x.userId = $sessionStorage.userid;
            $http({
                url: $sessionStorage.config.backend + webService + '/Save',
                method: 'POST',
                data: { userId: $sessionStorage.usergroupid, x: x }
            })
           .then(function (response) {
               getClients();
               $timeout(function () {
                   $mdDialog.hide(JSON.parse(response.data.d));
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
            data: { userId: $sessionStorage.usergroupid }
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
    if ($rootScope.client == undefined) {
        //if ($scope.clients.length == 0) {
        //    alert($scope.clients.length);
        //    $scope.openPopup();
        //} else {
      //  $scope.search();
     //   $rootScope.newClient();  //open first time
      //  }
    }

    $scope.openSearchPopup = function () {
        $scope.toggleSubTpl('pal');
        $mdDialog.show({
            controller: $scope.searchPopupCtrl,
            templateUrl: 'assets/partials/popup/searchclients.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
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
                  .title($translate.instant('delete input') + ' ?')
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
                data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
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
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Get',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
            //data: {lang: $rootScope.config.language, userId: $sessionStorage.userid, clientId: x.clientId }
        })
        .then(function (response) {
            if (JSON.parse(response.data.d).id != null) {
                $rootScope.clientData = JSON.parse(response.data.d);
                $rootScope.clientData.date = new Date(new Date().setHours(0, 0, 0, 0));
                $scope.getPalDetails($rootScope.clientData.pal.value);
                $rootScope.calculation = [];
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
            //data: { lang: $rootScope.config.databaselanguage, palValue: x }
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
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            //$rootScope.appCalculation = JSON.parse(response.data.d);
            //if ($rootScope.clientData.goal.code == undefined || $rootScope.clientData.goal.code == null || $rootScope.clientData.goal.code == 0) {
            //    $rootScope.clientData.goal.code = $rootScope.calculation.goal.code;
            //}

            //getCharts();
            //getGoals();
            setClientLogGraphData($scope.displayType);
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
                       label: $translate.instant("limit"),
                       borderWidth: 2,
                       backgroundColor: '#e6e6ff',
                       fill: false,
                       type: 'line'
                   },
                   {
                       label: $translate.instant("limit"),
                       borderWidth: 2,
                       backgroundColor: '#e6e6ff',
                       fill: false,
                       type: 'line'
                   }
            ],
            false
        )

        //TODO - goal, date
        angular.forEach($scope.clientLog, function (x, key) {
            if (type == 0) { clientData.push(x.weight); goalFrom.push($rootScope.calculation.recommendedWeight.min); goalTo.push($rootScope.calculation.recommendedWeight.max); }
            //if (type == 0) { clientData.push(x.weight); goal.push(75); }
            if (type == 1) { clientData.push(x.waist); goalFrom.push(95); }
            if (type == 2) { clientData.push(x.hip);; goalFrom.push(97); }
            labels.push(new Date(x.date).toLocaleDateString());
        });

    };

    $scope.getDateFormat = function (x) {
        return new Date(x);
    }

    $scope.change = function (x, scope) {
        switch (scope) {
            case 'height':
                return $rootScope.clientData.height = $rootScope.clientData.height + x;
                    break;
            case 'weight':
                return $rootScope.clientData.weight = $rootScope.clientData.weight + x;
                break;
            case 'waist':
                return $rootScope.clientData.waist = $rootScope.clientData.waist + x;
                break;
            case 'hip':
                return $rootScope.clientData.hip = $rootScope.clientData.hip + x;
                break;
                default:
                    return '';
            }
    }


}])

.controller('detailCalculationOfEnergyExpenditureCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    $rootScope.totalDailyEnergyExpenditure = {
            value: 0,
            duration: 0
        }



    //var get = function () {
    //    $http({
    //        url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Get',
    //        method: "POST",
    //        data: { userId: $rootScope.user.userGroupId, clientId: $rootScope.client.clientId }
    //    })
    //  .then(function (response) {
    //      if (response.data.d != '') {
    //          $scope.dailyActivities = JSON.parse(response.data.d);
    //          $rootScope.totalDailyEnergyExpenditure.value = totalEnergy();
    //          var lastActivity = $scope.dailyActivities[$scope.dailyActivities.length - 1];
    //          $scope.from = {
    //              hour: lastActivity.to.hour,
    //              min: lastActivity.to.min
    //          };
    //          $scope.to = {
    //              hour: lastActivity.to.hour,
    //              min: lastActivity.to.min
    //          }
    //          setTime(lastActivity.to.hour);
    //      } else {
    //          $scope.clearDailyActivities();
    //      }
    //  },
    //  function (response) {
    //      functions.alert($translate.instant(response.data.d), '');
    //  });
    //}

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Init',
            method: "POST",
            data: ""
        })
      .then(function (response) {
          $scope.dailyActivity = JSON.parse(response.data.d);
         // get();
      },
      function (response) {
          functions.alert($translate.instant(response.data.d), '');
      });
    }

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

    $rootScope.detailCalculationOfEnergyExpenditure = function (x) {
        $scope.showDetailCalculationOfEnergyExpenditure = x;
        //if ($rootScope.clientData.dailyActivities.length == 0) {
        //    init();
        //}
        init();
        initTime();
    }
    $rootScope.detailCalculationOfEnergyExpenditure(true);

    $scope.clearDailyActivities = function () {
        $rootScope.clientData.dailyActivities = [];
        $rootScope.totalDailyEnergyExpenditure.value = 0;
        $rootScope.totalDailyEnergyExpenditure.duration = 0;
        initTime();
    }

    var totalEnergy = function () {
        var e = 0;
        angular.forEach($rootScope.clientData.dailyActivities, function (value, key) {
            e = e + value.energy;
        })
        return e;
    }

    var totalDuration = function () {
        var d = 0;
        angular.forEach($rootScope.clientData.dailyActivities, function (value, key) {
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

        $rootScope.clientData.dailyActivities.push(angular.copy($scope.dailyActivity));
        $rootScope.totalDailyEnergyExpenditure.value = totalEnergy(); // $scope.totalDailyEnergyExpenditure + $scope.dailyActivity.energy;
        $rootScope.totalDailyEnergyExpenditure.duration = totalDuration();
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
         // $rootScope.calculation.tee = $rootScope.totalDailyEnergyExpenditure;
          functions.alert($translate.instant(response.data.d), '');
      },
      function (response) {
          functions.alert($translate.instant(response.data.d), '');
      });
    }

    var getTotal = function () {
        if ($rootScope.clientData.dailyActivities == null) {
            $rootScope.clientData.dailyActivities = [];
        }
        if ($rootScope.clientData.dailyActivities.length > 0) {
            $rootScope.totalDailyEnergyExpenditure.value = totalEnergy();
            $rootScope.totalDailyEnergyExpenditure.duration = totalDuration();
            var lastActivity = $rootScope.clientData.dailyActivities[$rootScope.clientData.dailyActivities.length - 1];
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
    getTotal();


    

}])

.controller('calculationCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'charts', '$timeout', 'functions', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, $timeout, functions) {
    var webService = 'Calculations.asmx';
    var getCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            $rootScope.appCalculation = JSON.parse(response.data.d);
            //TODO
            if (angular.isDefined($rootScope.totalDailyEnergyExpenditure)) {
                if ($rootScope.totalDailyEnergyExpenditure.duration == 1440) {
                    $rootScope.calculation.tee = $rootScope.totalDailyEnergyExpenditure.value;
                    $rootScope.appCalculation.tee = $rootScope.totalDailyEnergyExpenditure.value;
                }
            }

            if ($rootScope.clientData.goal.code == undefined || $rootScope.clientData.goal.code == null || $rootScope.clientData.goal.code == 0) {
                $rootScope.clientData.goal.code = $rootScope.calculation.goal.code;
            }

            getCharts();
            getGoals();
        },
        function (response) {
            alert(response.data.d)
        });
    };
    getCalculation();

    $scope.getBmiClass = function (x) {
        if (x < 18.5) { return { text: 'text-info', icon: 'fa fa-exclamation' }; }
        if (x >= 18.5 && x <= 25) { return { text: 'text-success', icon: 'fa fa-check' }; }
        if (x > 25 && x < 30) { return { text: 'text-warning', icon: 'fa fa-exclamation' }; }
        if (x >= 30) { return { text: 'text-danger', icon: 'fa fa-exclamation' }; }
    }

    $scope.getWaistClass = function (x) {
        if (x < 94) { return { text: 'text-success', icon: 'fa fa-check' }; }
        if (x >= 94 && x <= 102) { return { text: 'text-warning', icon: 'fa fa-exclamation' }; }
        if (x > 102) { return { text: 'text-danger', icon: 'fa fa-exclamation' }; }
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
        var options = {
            title: 'WHR',
            min: 0,
            max: 2,
            greenFrom: 0,
            greenTo: 1,
            yellowFrom: 1,
            yellowTo: 1.1,
            redFrom: 1.1,
            redTo: 2,
            minorTicks: 0.1
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var waistChart = function () {
        var id = 'waistChart';
        var value = $rootScope.calculation.waist.value.toFixed(1);
        var unit = 'cm';
        var options = {
            title: 'WHR',
            min: 60,
            max: 160,
            greenFrom: 60,
            greenTo: 94,
            yellowFrom: 94,
            yellowTo: 102,
            redFrom: 102,
            redTo: 160,
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
      
        //$rootScope.calculation.recommendedEnergyIntake = $rootScope.appCalculation.recommendedEnergyIntake + energy;
        //$rootScope.calculation.recommendedEnergyExpenditure = $rootScope.appCalculation.recommendedEnergyExpenditure + activity;
    }

    var isGoalDisabled = function () {
            if ($rootScope.calculation.bmi.value < 18.5) {
                $rootScope.goals[0].isDisabled = true;
            }
            if ($rootScope.calculation.bmi.value > 25) {
                $rootScope.goals[2].isDisabled = true;
            }
    }

}])

.controller('activitiesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Activities.asmx';
  //  $scope.isSport = 1;
    $scope.orderdirection = '-';
    $scope.orderby = function (x) {
        var direction = $scope.orderdirection == '+' ? '-' : '+';
        $scope.order = direction + x;
        $scope.orderdirection = direction;
    }
    $scope.orderby('activity');

    if ($rootScope.activities == undefined) { $rootScope.loadActivities(); };

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
      //  alert(energyLeft);
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
           //   alert(response);
          }, function () {
          });
        } else {
            functions.alert($translate.instant('the selected additional energy expenditure is the same as recommended'), '');
          //  alert('Odabrana dodatna tjesna potrošnja jednaka preporućenoj.')
        }
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = d.activity;
        var energy = d.energy; // $rootScope.calculation.recommendedEnergyExpenditure;

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
           //energy = energy - Math.round(($scope.duration * ($scope.d.factorKcal * $rootScope.clientData.weight)) / 60);
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
    $scope.addFoodBtnIcon = 'fa fa-hand-o-right';
    $scope.addFoodBtn = false;

    $rootScope.selectedFoods = $rootScope.selectedFoods == undefined ? [] : $rootScope.selectedFoods;

    if ($rootScope.clientData.meals.length == 0) {
        $rootScope.newTpl = 'assets/partials/meals.html';
        $rootScope.selectedNavItem = 'meals';
        functions.alert($translate.instant('choose meals'), '');
    }

    var getRecommendations = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/GetRecommendations',
            method: "POST",
            data: { client: x }
        })
       .then(function (response) {
           $rootScope.recommendations = JSON.parse(response.data.d);
           displayCharts();
       },
       function (response) {
           alert(response.data.d)
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
    if ($rootScope.currentMenu == undefined) { init(); }

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
        $timeout(function () {
            $scope.analyticsTpl = x;
            getTotals($rootScope.currentMenu);
        }, 700);
    };
    $scope.toggleAnalytics('chartsTpl');

    $scope.changeQuantity = function (x, type, idx) {
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
                alert(response.data.d)
            });
        }, 400);
    }

    $scope.change = function (x, type, idx) {
        if ($rootScope.currentMenu.data.selectedFoods[idx].quantity + x > 0) {
                if (type == 'quantity') {
                    $rootScope.currentMenu.data.selectedFoods[idx].quantity = $rootScope.currentMenu.data.selectedFoods[idx].quantity + x;
                    $scope.changeQuantity($rootScope.currentMenu.data.selectedFoods[idx], 'quantity', idx);
                }
                if (type == 'mass') {
                    $rootScope.currentMenu.data.selectedFoods[idx].mass = $rootScope.currentMenu.data.selectedFoods[idx].mass + x;
                    $scope.changeQuantity($rootScope.currentMenu.data.selectedFoods[idx], 'mass', idx);
                }
            }
    }

    $scope.openFoodPopup = function (x, idx) {
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.currentMenu.data.selectedFoods.length > 9) {
            functions.demoAlert('in demo version maximum number of choosen foods is 10');
            return false;
        }
        $scope.addFoodBtn = true;
        $scope.addFoodBtnIcon = 'fa fa-spinner fa-spin';
        $mdDialog.show({
            controller: $scope.foodPopupCtrl,
            templateUrl: 'assets/partials/popup/food.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { foods: $rootScope.foods, myFoods: $rootScope.myFoods, foodGroups: $rootScope.foodGroups, food: x, idx: idx, config:$rootScope.config }
        })
        .then(function (x) {
            $scope.addFoodBtnIcon = 'fa fa-hand-o-right';
            $scope.addFoodBtn = false;
            $scope.addFoodToMeal(x.food, x.initFood, idx);
        }, function () {
            $scope.addFoodBtnIcon = 'fa fa-hand-o-right';
            $scope.addFoodBtn = false;
        });
    };

    $scope.foodPopupCtrl = function ($scope, $mdDialog, d, $http, $translate) {
        $scope.d = d;
        $scope.foods = d.foods;
        $scope.myFoods = d.myFoods;
        $scope.foodGroups = d.foodGroups;
        $scope.food = d.food != undefined ? d.food : null;
        var initFood = d.food != undefined ? d.food : null;
        $scope.limit = 20;

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

            //var obj = JSON.parse(x);
            //if (obj.foodGroup.code == 'MYF') {
            //    getMyFoodDetails(x);
            //    return false;
            //}

            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/Get',
                method: "POST",
                data: {userId: $rootScope.user.userId, id: JSON.parse(x).id }
                //data: { lang: $rootScope.config.databaselanguage, id: JSON.parse(x).id }
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

                initFood = angular.copy($scope.food); // JSON.parse(response.data.d);
              //  showServings($scope.food);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var getMyFoodDetails = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userId, id: JSON.parse(x).id }
            })
          .then(function (response) {
              $scope.food = JSON.parse(response.data.d);
              initFood = angular.copy(JSON.parse(response.data.d));
            //  showServings($scope.food);
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
            $timeout(function () {
                $http({
                    url: $sessionStorage.config.backend + webService + '/ChangeFoodQuantity',
                    method: "POST",
                    data: { initFood: initFood, newQuantity: x.quantity, newMass: x.mass, type: type }
                })
                .then(function (response) {
                    $scope.food = JSON.parse(response.data.d);
                },
                function (response) {
                });
            }, 400);
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
            controller: $scope.printPreviewCtrl, // RealizationCtrl,
            templateUrl: 'assets/partials/popup/printmenu.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
            d: { currentMenu: $rootScope.currentMenu, clientData: $rootScope.clientData }
        })
        .then(function () {
        }, function () {
        });
    };

    $scope.printPreviewCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = d.currentMenu.data.selectedFoods;
        $scope.clientData = d.clientData;
        $scope.meals = d.currentMenu.data.meals;

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        //$scope.print = function () {
        //    alert('todo');
        //    window.print();
        //}

        //$scope.pdf = function () {
        //    printPdf();
        //}

        //var printPdf = function () {
        //    var fileName = 'jelovnik';
        //    $http({
        //        url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
        //        method: "POST",
        //        data: { userId: $rootScope.user.userId, fileName: fileName, currentMenu: d.currentMenu, clientData: d.clientData, totals: $rootScope.totals, lang: $rootScope.config.language }
        //    })
        //      .then(function (response) {
        //       //   alert(response.data.d);
        //          //window.open($sessionStorage.config.backend + '/App_Data/users/' + $rootScope.user.userId + '/pdf/' + fileName + '.pdf', + '_blank');
        //          //  window.open($sessionStorage.config.backend + 'pdf/users/' + $rootScope.user.userId + '/pdf/' + fileName + '.pdf');
        //       //   $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userId + '/pdf/' + fileName + '.pdf';
        //      },
        //      function (response) {
        //          alert(response.data.d)
        //      });
        //}
        //printPdf();

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
            d: { currentMenu: $rootScope.currentMenu, client: $rootScope.client, totals: $rootScope.totals, config: $sessionStorage.config }
        })
       .then(function (x) {
           $rootScope.currentMenu = x;
       }, function () {
       });
    }

    var openSaveMenuPopupCtrl = function ($scope, $mdDialog, $http, d, $translate) {
        $scope.d = angular.copy(d);
        var save = function (currentMenu) {
            if (currentMenu.title == '' || currentMenu.title == undefined) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu name'), '');
                openSaveMenuPopup();
                return false;
            }
            currentMenu.diet = d.client.clientData.diet.diet;
            $http({
                url: $sessionStorage.config.backend + 'Menues.asmx/Save',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, x: currentMenu }
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
            if (currentMenu.title == '' || currentMenu.title == undefined) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu name'), '');
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
        if ($rootScope.user.licenceStatus == 'demo') {
            functions.demoAlert('this function is not available in demo version');
        } else {
            openSendMenuPopup();
        }
    }

    var openSendMenuPopup = function () {
        $rootScope.client.clientData = $rootScope.clientData;

        $mdDialog.show({
            controller: openSendMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/sendmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: $rootScope.currentMenu, client: $rootScope.client }
        })
       .then(function (x) {
       }, function () {
       });
    }

    var openSendMenuPopupCtrl = function ($scope, $mdDialog, $http, d, $translate) {
        $scope.d = angular.copy(d);
     
        var send = function () {
            $mdDialog.hide();
            $http({
                url: $sessionStorage.config.backend + 'Mail.asmx/SendMenu',
                method: "POST",
                data: { email: d.client.email, messageSubject: d.currentMenu.title, currentMenu: d.currentMenu }
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

        $scope.confirm = function (x, saveasnew) {
            send();
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
        $scope.mealsTotals = [];
        $scope.mealsMin = [];
        $scope.mealsMax = [];
        $scope.mealsTitles = [];
        angular.forEach($rootScope.clientData.meals, function (value, key) {
            if (value.isSelected == true && angular.isDefined($rootScope.totals)) {
                $scope.mealsTotals.push($rootScope.totals.mealsTotalEnergy.length > 0 ? $rootScope.totals.mealsTotalEnergy[key].meal.energy : 0);
                $scope.mealsMin.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMin);
                $scope.mealsMax.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMax);
                $scope.mealsTitles.push(value.title);
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
                $translate.instant('unit servings'), //['jedinična serviranja'],
                [
                    [t.servings.cerealsServ, t.servings.vegetablesServ, t.servings.fruitServ, t.servings.meatServ, t.servings.milkServ, t.servings.fatsServ],
                    [r.servings.cerealsServ, r.servings.vegetablesServ, r.servings.fruitServ, r.servings.meatServ, r.servings.milkServ, r.servings.fatsServ]
                ],
                [$translate.instant('carbohydrates'), $translate.instant('vegetables'), $translate.instant('fruit'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fats')],

               // ['ugljikohidrati', 'povrče', 'voće', 'meso', 'mlijeko', 'masti'],
                ['#45b7cd', '#33cc33', '#33cc33'],
                [
                     {
                         label: $translate.instant('choosen'), // "Odabrano",
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
        var recommended = $rootScope.recommendations.energy;
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


}])

.controller('analyticsCtrl', ['$scope', '$http', '$window', '$rootScope', '$mdDialog', 'charts', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, functions, $translate) {

    $scope.toggleTpl = function (x) {
        $scope.analyticsTpl = x;
    };
    $scope.toggleTpl('tablesTpl');

    $scope.printPreview = function () {
        $mdDialog.show({
            controller: $scope.printPreviewCtrl, // RealizationCtrl,
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

        //$scope.print = function () {
        //    alert('todo print');
        //}

        //$scope.pdf = function () {
        //  //  alert('todo');

        //    printPdf();

        //}

        //var printPdf = function () {
        //    $http({
        //        url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
        //        method: "POST",
        //        data: { userId: $rootScope.user.userId, fileName: 'testpdf', currentMenu: $rootScope.currentMenu, clientData: $rootScope.clientData, totals: $rootScope.totals, lang: $rootScope.config.language }
        //    })
        //      .then(function (response) {
        //          alert(response.data.d);
        //      },
        //      function (response) {
        //          alert(response.data.d)
        //      });
        //}

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
           // getUnits();
        },
        function (response) {
            alert(response.data.d)
        });
    };
    init();

    //var getUnits = function () {
    //    $http({
    //        url: $sessionStorage.config.backend + 'Foods.asmx/GetUnits',
    //        method: "POST",
    //        data: { lang: $rootScope.config.language }
    //    })
    //  .then(function (response) {
    //      $scope.units = JSON.parse(response.data.d);
    //  },
    //  function (response) {
    //      alert(response.data.d)
    //  });
    //}
    //$scope.units = [
    //    'komad',
    //    'jušna žljica',
    //    'porcija'
    //];

    var load = function () {
        $rootScope.loading = true;
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/Load',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid }
        })
        .then(function (response) {
            var data = JSON.parse(response.data.d);
            $rootScope.myFoods = data.myFoods;
            $rootScope.loading = false;
        },
        function (response) {
            $rootScope.loading = false;
            alert(response.data.d)
        });
    };


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
         load();
         init();
         alert(response.data.d);
     },
     function (response) {
         alert(response.data.d);
     });
    }

    $scope.save = function (x) {
        if ($rootScope.user.licenceStatus == 'demo' && $rootScope.clients.length > 0) {
            functions.demoAlert('this function is not available in demo version');
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
            load();
            functions.alert($translate.instant(response.data.d), '');
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

    $scope.getMyFoodDetails = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'MyFoods.asmx/Get',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, id: x }
        })
      .then(function (response) {
          $scope.myFood = JSON.parse(response.data.d);
      },
      function (response) {
          alert(response.data.d)
      });
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
            $scope.price.netPrice.currency = $sessionStorage.settings.currency;
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
    $scope.fontsize = 14;
  
    $scope.toggleTpl = function (x) {
        $scope.printTpl = x;

    };
    $scope.toggleTpl('menuTpl');
  
    $scope.changeFontSize = function (x) {
        $scope.fs = { 'font-size': + x + 'px' };
    };
    $scope.changeFontSize($scope.fontsize);

    $scope.printWindow = function () {
        window.print();
    };

    var printPdf = function () {
        $http({
            url: $sessionStorage.config.backend + 'PrintPdf.asmx/MenuPdf',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, fileName: null, currentMenu: $rootScope.currentMenu, clientData: $rootScope.clientData, totals: $rootScope.totals, lang: $rootScope.config.language }
        })
          .then(function (response) {
              var fileName = response.data.d;
                $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
          },
          function (response) {
              alert(response.data.d)
          });
    }
    printPdf();

    $scope.openPdf = function () {
        window.open($scope.pdfLink, '_blank');
    }

    var getClient = function () {
        $http({
            url: $sessionStorage.config.backend + 'Clients.asmx/Get',
            method: "POST",
            data: { userId: $sessionStorage.userid, clientId: $rootScope.client.clientId }
        })
          .then(function (response) {
              $scope.client = JSON.parse(response.data.d);
          },
          function (response) {
              alert(response.data.d)
          });
    }
    if ($rootScope.client != undefined) { getClient(); }

    //TODO
    $scope.changeNumberOfConsumers = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/ChangeNumberOfConsumers',
            method: "POST",
            data: { foods: $rootScope.currentMenu.data.selectedFoods, number: x }
        })
       .then(function (response) {
           $scope.foods = JSON.parse(response.data.d);
           //$rootScope.currentMenu.data.selectedFoods[idx] = JSON.parse(response.data.d);
           //getTotals($rootScope.currentMenu);
       },
       function (response) {
           //   alert(response.data.d)
       });
    }
    if ($rootScope.currentMenu != undefined) { $scope.changeNumberOfConsumers($scope.consumers); }

}])

.controller('orderCtrl', ['$scope', '$http', '$rootScope', '$translate', function ($scope, $http, $rootScope, $translate) {
  //  $rootScope.isLogin = false;
    $scope.application = 'Program Prehrane';
    $scope.version = 'WEB';
    $scope.userType = 0;
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = $translate.instant('send order');
    $scope.showUserDetails = $rootScope.user.userName != '' ? false : true;
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
         $scope.user.userName = $rootScope.user.userName;
         $scope.user.password = $rootScope.user.password;
         $scope.user.application = $scope.application;
         $scope.user.version = 'WEB';
         $scope.user.licence = '0';
         $scope.user.licenceNumber = '1';
         $scope.login($scope.user.userName, $scope.user.password);
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

        $scope.user.version = $scope.version;
        unitprice = 550;
        $scope.user.licence = '1';
        $scope.user.licenceNumber = '1';

        totalprice = $scope.user.licenceNumber > 1 ? unitprice * $scope.user.licenceNumber - (unitprice * $scope.user.licenceNumber * 0.1) : unitprice;
        $scope.user.price = totalprice;
        $scope.user.priceEur = totalprice / $rootScope.config.eur;
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
            data: { x: user }
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

//-------------end Program Prehrane Controllers--------------------

;