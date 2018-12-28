﻿/*!
functions.js
(c) 2018 IG PROG, www.igprog.hr
*/
angular.module('functions', [])

.factory('functions', ['$rootScope', '$window', '$translate', function ($rootScope, $window, $translate) {
    return {
        'isNullOrEmpty': function (x) {
            var res = false;
            if (x === '' || x == undefined || x == null) {
                res = true;
            }
            return res;
        },
        'getDateDiff': function (x) {
            var today = new Date();
            var date1 = new Date(x);
            var diffDays = Math.abs(parseInt((today - date1) / (1000 * 60 * 60 * 24)));
            return diffDays;
        }
    }
}]);

;
