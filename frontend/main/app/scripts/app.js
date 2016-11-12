'use strict';

/**
 * @ngdoc overview
 * @name appApp
 * @description
 * # appApp
 *
 * Main module of the application.
 */
angular.module('appApp', [
    'ngAnimate',
    'ngRoute',
    'ngSanitize',
    'ngTouch',
    'ngActivityIndicator',
    'LocalStorageModule',
    'inspiracode.crudFactory',
    'CRUDServices',
    // 'ngTagsInput',
], function($httpProvider) {
    $httpProvider.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';
    $httpProvider.defaults.headers.put['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';
}).config(function($routeProvider, appConfig, $activityIndicatorProvider, $httpProvider, localStorageServiceProvider) {

    localStorageServiceProvider.setPrefix(appConfig.APP_NAME);

    $routeProvider
        .when('/', {
            templateUrl: 'views/main.html',
            controller: 'MainCtrl',
            controllerAs: 'main'
        })
        .when('/about', {
            templateUrl: 'views/about.html',
            controller: 'AboutCtrl',
            controllerAs: 'about'
        })
        .when('/login', {
            templateUrl: 'views/login.html',
            controller: 'LoginCtrl',
            controllerAs: 'loginn'
        })
        .when('/signup', {
            templateUrl: 'views/signup.html',
            controller: 'SignupCtrl',
            controllerAs: 'signup'
        })
        .otherwise({
            redirectTo: '/'
        });

    $activityIndicatorProvider.setActivityIndicatorStyle('CircledWhite');
    alertify.set('notifier', 'position', 'top-left');
    alertify.set('notifier', 'delay', 2);

    $httpProvider.interceptors.push('authInterceptorService');
}).run(function(authService, $rootScope, $location) {

    authService.fillAuthData();

    // register listener to watch route changes
    $rootScope.$on('$routeChangeSuccess', function(event, next, current) {
        alertify.closeAll();
        $('.modal').modal('hide');
        $('.modal-backdrop.fade.in').remove();

        
        

        var authentication = authService.authentication;
        if (!authentication || !authentication.isAuth) {
            // no logged user, we should be going to #login
            if (next.templateUrl == "views/login.html") {
                // already going to #login, no redirect needed
            } else {
                $location.path('/login');
            }
        } else {
            // var tokenPayload = jwtHelper.decodeToken(jwt);
            // LoginService.update(tokenPayload.data.userId, tokenPayload.data.userName);
        }



    });

    $rootScope.$on('$routeChangeSuccess', function() {
        $rootScope.activePath = $location.path();
    });


    $rootScope.logOut = function() {
        authService.logOut();
    };

});
