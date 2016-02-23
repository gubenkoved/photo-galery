var app = angular.module('app', ['ngRoute']);

app.config(['$routeProvider',
    function($routeProvider) {
        $routeProvider.
        when('/albums/:albumPath?', {
            templateUrl: 'views/albums.html',
            controller: 'AlbumsController'
        }).
        when('/about', {
            templateUrl: 'views/about.html',
            controller: 'AboutController'
        }).
        when('/404', {
            templateUrl: 'views/404.html',
            controller: 'NotFoundController'
        }).
        when('/test', {
            templateUrl: 'views/test.html'
        }).
        when('/', {
            redirectTo: '/albums'
        }).
        otherwise({
            redirectTo: '/404',
            controller: 'NotFoundController'
        });
    }
]);

app.config(function($httpProvider) {
    $httpProvider.interceptors.push('http404Interceptor');
});

app.constant('config', {
    apiRoot: 'http://localhost:59999/api',
    ver: '1.0.0.0',
});

/* INTERCEPTORS */

app.factory('http404Interceptor', function($q, $window) {
    return {
        'responseError': function(rejection) {

            if (rejection.status == 404) {
                //console.log(rejection.config);

                $window.location = './#/404';
            }

            return $q.reject(rejection);
        }
    };
});

/* SERVICES */

app.service('AlbumsService', ["$http", "config", "$q", function($http, config, $q) {
    var service = {};

    function mapAlbumsResponse(apiResponse) {
        var r = {
            name: apiResponse.Name,
            url: apiResponse.Url,
            parentUrl: apiResponse.ParentUrl,
            fullPath: apiResponse.FullPath,

            albumItems: [],
            contentItems: []
        };

        for (var i = apiResponse.AlbumItems.length - 1; i >= 0; i--) {
            r.albumItems.push({
                name: apiResponse.AlbumItems[i].Name,
                url: apiResponse.AlbumItems[i].Url
            });
        }

        for (var i = apiResponse.ContentItems.length - 1; i >= 0; i--) {
            r.contentItems.push({
                name: apiResponse.ContentItems[i].Name,
                url: apiResponse.ContentItems[i].Url,
                thumbUrl: apiResponse.ContentItems[i].ThumbUrl,
                origWidth: apiResponse.ContentItems[i].OrigWidth,
                origHeight: apiResponse.ContentItems[i].OrigHeight,
            });
        }

        return r;
    }

    service.getAlbumItems = function(albumUrl) {
        if (!albumUrl) {
            return $http.get(config.apiRoot + '/albums')
                .then(function(response) {
                    return mapAlbumsResponse(response.data);
                });
        }

        return $http.get(albumUrl)
            .then(function(response) {
                return mapAlbumsResponse(response.data);
            });
    }

    service.resolve = function(albumPath) {
        console.log('resolve: ' + albumPath);

        if (albumPath) {
            return $http.get(config.apiRoot + '/resolve/' + albumPath)
                .then(function(response) {
                    console.log(response.data);

                    return response.data;
                });
        } else {
            return $q(function(resolve, reject) {
                resolve(config.apiRoot + '/albums');
            });
        }
    }

    return service;
}]);

/* CONTROLLERS */

app.controller('AlbumsController', ['$scope', '$routeParams', '$route', 'AlbumsService',
    function($scope, $routeParams, $route, AlbumsService) {
        $scope.init = function() {
            AlbumsService.resolve($routeParams.albumPath)
                .then(function(targetAlbumUrl) {
                    $scope.getAlbum(targetAlbumUrl);
                });
        }

        $scope.getAlbum = function(albumUrl) {
            var rid = $scope.nextRequestId();
            $scope.waitingForRequestId = rid;

            return AlbumsService.getAlbumItems(albumUrl).then(function(albumModel) {
                if ($scope.waitingForRequestId === rid) {
                    var pRoute = $routeParams;
                    pRoute.albumPath = albumModel.fullPath;
                    $route.updateParams(pRoute);

                    $scope.currentAlbum = albumModel
                } else {
                    console.log(`discarding result of request with id ${rid} since waiting for ${$scope.waitingForRequestId}`);
                }
            });
        }

        $scope.getParentAlbum = function() {
            $scope.getAlbum($scope.currentAlbum.parentUrl);
        }

        $scope.onAlbumClick = function(album) {
            $scope.getAlbum(album.url);
        }

        $scope.debug = function(o) {
            console.log(o);
        }
    }
]);

app.controller('AboutController', ['$scope', 'config', function($scope, config) {
    $scope.config = config;
}]);

app.controller('NotFoundController', ['$scope', function($scope) {}]);


/* DIRECTIVES */

app.directive('contentItem', function() {
    return {
        scope: {
            item: '='
        },
        templateUrl: '/app/directives/contentItem.html',
        compile: function (element, transclude, maxPriority)
        {
            return {
                post: function postLink($scope, $el, $attrs)
                {
                    console.log('[link] content-item');
                }
              }
        }
    };
});

app.directive('albumItem', function() {
    return {
        scope: {
            item: '=',
            click: '&onClick'
        },
        templateUrl: '/app/directives/albumItem.html'
    };
});

app.directive('test', function() {
    return {
        scope: {
            testFunc: '&onClick'
        },
        //templateUrl: '/app/directives/test.html',
        template: '<div>test</div>',
        compile: function (element, transclude, maxPriority)
        {
            element.css('display', 'inline-block');

            return {
                post: function postLink($scope, $el, $attrs)
                {
                    console.log('[test link]');
                }
              }
        }
    };
});

app.directive('itemsPane', function() {
    return {
        transclude: true,
        scope: {
            targetHeight: '@',
            spacing: '@'
        },

        templateUrl: '/app/directives/itemsPane.html',

        link: {
            post: function($scope, $el, $attrs, controller, transcludeFn) {
                
                console.log('[link] itemsPane');

                $el.css('font-size', '0');

                $scope.container = angular.element($el[0].children[0]);

                $scope.$on('last-item-iterated', function ($event)
                {
                    $scope.rearrangeItems();
                    $event.stopPropagation();
                });
            }
        },

        controller: function($scope, $timeout) {
            $scope.rearrangeItems = function() {
                console.log('rearrange for ' + $scope.container.children().length + ' items');

                angular.forEach($scope.container.children(), function(item) {
                    //console.log(item);
                    angular.element(item).css('margin-left', ($scope.spacing || 2) + 'px');
                    angular.element(item).css('margin-top', ($scope.spacing || 2) + 'px');
                    angular.element(item).css('background', 'green');

                    //angular.element(item).css('height', '200px');
                    //angular.element(item).css('width', '200px');

                    console.log(item.clientWidth + 'x' + item.clientHeight);
                });
            }
        }
    };
});

app.directive('itemsPaneItem', function() {
    return {
        link: function($scope, $el, $attrs, controller, transcludeFn) {

            $el.css('display', 'inline-block');

            if ($scope.$last)
            {
                $scope.$emit('last-item-iterated');
            }
        }
    };
});

/* APP RUN */

angular.module('app')
    .run(['$rootScope', function($rootScope) {

        $rootScope.lastRequestId = 0;

        $rootScope.nextRequestId = function() {
            $rootScope.lastRequestId += 1;

            return $rootScope.lastRequestId;
        }
    }]);