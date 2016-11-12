'use strict';

/**
 * @ngdoc function
 * @name appApp.controller:SignupCtrl
 * @description
 * # SignupCtrl
 * Controller of the appApp
 */
angular.module('appApp').controller('SignupCtrl', function($scope, $location, $timeout, authService, $activityIndicator) {
    $activityIndicator.stopAnimating();
    alertify.closeAll();

    $scope.savedSuccessfully = false;
    $scope.message = "";

    $scope.registration = {
        UserName: "",
        Password: "",
        ConfirmPassword: ""
    };

    $scope.signUp = function() {

        authService.saveRegistration($scope.registration).then(function(response) {

                $scope.savedSuccessfully = true;
                $scope.message = "User has been registered successfully, you will be redicted to login page in 3 seconds.";
                startTimer();

            },
            function(response) {
                var errors = [];
                for (var key in response.data.ModelState) {
                    for (var i = 0; i < response.data.ModelState[key].length; i++) {
                        errors.push(response.data.ModelState[key][i]);
                    }
                }
                $scope.message = "Failed to register user due to:" + errors.join(' ');
            });
    };

    var startTimer = function() {
        var timer = $timeout(function() {
            $timeout.cancel(timer);
            $location.path('/login');
        }, 3000);
    }
});
