/*!
functions.js
(c) 2017 IG PROG, www.igprog.hr
*/
angular.module('functions', [])

.factory('functions', ['$mdDialog', '$rootScope', '$window', '$translate', function ($mdDialog, $rootScope, $window, $translate) {
    return {
        'alert': function (title, content) {
            var confirm = $mdDialog.confirm()
            .title(title)
            .textContent(content)
            .targetEvent('')
            .ok($translate.instant('ok'))
            .cancel('');
            $mdDialog.show(confirm).then(function () {
            }, function () {
            });
        },
        'demoAlert': function (alert) {
                var confirm = $mdDialog.confirm()
                   .title($translate.instant(alert))
                   .textContent($translate.instant('activate full version'))
                   .ok($translate.instant('yes'))
                   .cancel($translate.instant('not now'));
                $mdDialog.show(confirm).then(function () {
                    $rootScope.currTpl = './assets/partials/order.html';
                }, function () {
                });
        }
    }
}]);

;
