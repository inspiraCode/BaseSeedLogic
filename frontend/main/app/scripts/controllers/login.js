'use strict';

/**
 * @ngdoc function
 * @name appApp.controller:LoginCtrl
 * @description
 * # LoginCtrl
 * Controller of the appApp
 */
angular.module('appApp').controller('LoginCtrl', function($scope, $location, authService, $activityIndicator, appConfig) {

    $scope.appName = appConfig.APP_NAME;

    $activityIndicator.stopAnimating();
    alertify.closeAll();

    $scope.loginData = {
        userName: "",
        password: ""
    };

    $scope.message = "";

    $scope.login = function() {
        $activityIndicator.startAnimating();
        authService.login($scope.loginData).then(function(response) {
                $location.path('/');
            },
            function(err) {
                $scope.message = err.error_description;
            });
    };
});
