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

    desiredThumbSize: {
        width: 300,
        height: 300
    }
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

    function _updateQueryStringParameter(uri, key, value) {
      var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
      var separator = uri.indexOf('?') !== -1 ? "&" : "?";
      if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
      }
      else {
        return uri + separator + key + "=" + value;
      }
    }

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

            var thumUrl = apiResponse.ContentItems[i].ThumbUrl;

            thumUrl = _updateQueryStringParameter(thumUrl, "w", config.desiredThumbSize.width);
            thumUrl = _updateQueryStringParameter(thumUrl, "h", config.desiredThumbSize.height);

            r.contentItems.push({
                name: apiResponse.ContentItems[i].Name,
                url: apiResponse.ContentItems[i].Url,
                thumbUrl: thumUrl,
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

app.directive('contentItem', function($timeout) {
    return {
        scope: {
            item: '='
        },
        templateUrl: '/app/directives/contentItem.html',
        compile: function (element, transclude, maxPriority)
        {
            element.addClass('contentItem');

            return {
                post: function postLink($scope, $el, $attrs)
                {
                    var img = $el.find('img');

                    img.bind('load', function() {
                        // image is loaded - wait DOM to be rendered - then raise flag
                        $timeout(function(){
                            $scope.$emit('content-item-rendered');
                        });
                    });

                    console.log('[link] content-item');
                }
              }
        }
    };
});

app.directive('spinnerWhileLoading', function() {
    return {
        scope: {
            ngSrc: '@'
        },
        link: function ($scope, $el, $attrs)
        {
            //debugger;

            var spinnerScr = $attrs['spinnerWhileLoading'];

            //debugger;

             $el.on('load', function() {
                $el.show();

                $scope.loaderImg.remove();
                //console.log('remove spiner');
             });
            
             $scope.$watch('ngSrc', function() {
                 //debugger;
                 //console.log('inject spiner');

                 $el.hide();

                 $scope.loaderImg = angular.element('<img></img');
                 $scope.loaderImg.attr('src', spinnerScr);
                 $scope.loaderImg.addClass('loader');
                 $scope.loaderImg.attr('style', 'position: absolute; left: 0; top: 0; right: 0; bottom: 0; margin: auto;');

                 //debugger;

                 $el.parent().append($scope.loaderImg);
                 // Set visibility: false + inject temporary spinner overlay
             });
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

app.directive('itemsPane', function($timeout) {
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
                $scope.container2 = $el[0].children[0];

                var jqContainer = angular.element($scope.container2);

                console.log('rearrange for ' + jqContainer.children().length + ' items');

                $scope.$on('items-pane-item-last-iterated', function ()
                {
                    console.log('handle items-pane-item-last-iterated');

                    angular.forEach(jqContainer.children(), function(item) {

                        var aspect = angular.element(item).scope().aspect;
                    });

                    // thisi speice of shit, but i don't know how to
                    // correctly postpone items rearrangemene until
                    // all content items are linked
                    $timeout(function () {
                        $timeout(function () {
                            $scope.rearrangeItems();
                        });
                    });
                });
            }
        },

        controller: function($scope, $timeout) {

            function _scaleRow(items, targetWidth)
            {
                //debugger;

                targetWidth -= 5;

                var currentWidth = 0;

                angular.forEach(items, function(item) {

                    //console.log('item width: ' + item.clientWidth);
                    //console.dir(item);

                    currentWidth += item.clientWidth;
                });

                var scaleFactor = targetWidth / currentWidth;

                //console.log('current row width: ' + currentWidth + '; target: ' + targetWidth + '; scale at: ' + scaleFactor);

                //scaleFactor *= 0.99;

                //return;

                angular.forEach(items, function(item) {

                    //console.dir(item);

                    var itemTargetWidth = item.clientWidth * scaleFactor;
                    var itemTargetHeight = item.clientHeight * scaleFactor;

                    itemTargetWidth = Math.floor(itemTargetWidth);
                    itemTargetHeight = Math.floor(itemTargetHeight);

                    angular.element(item).css('width', itemTargetWidth + 'px');
                    angular.element(item).css('height', itemTargetHeight + 'px');
                });
            }

            $scope.rearrangeItems = function() {
                var jqContainer = angular.element($scope.container2);

                console.log('rearrange for ' + jqContainer.children().length + ' items');

                // phase 1: Scale to TargetHeight taking into account the item aspect

                angular.forEach(jqContainer.children(), function(item) {
                    console.log("phase 1");

                    var jqItem = angular.element(item);
                    var itemAspect = jqItem.scope().aspect;

                    jqItem.css('height', $scope.targetHeight + 'px');
                    jqItem.css('width', itemAspect * $scope.targetHeight + 'px');

                    // child styling

                    jqItem.children().css('background-color', 'yellow');
                    jqItem.children().css('position', 'absolute');
                    angular.element(jqItem.children()[0]).css({
                        top: $scope.spacing + 'px',
                        left: $scope.spacing + 'px',
                        bottom: 0,
                        right: 0,
                    });

                });

                // phase 2: Scale rows so that they fills the whole width

                //return;

                var prevOffsetLeft = -1;
                var row = [];

                var targetRowWidth = $scope.container2.clientWidth;

                angular.forEach(jqContainer.children(), function(item) {
                    //console.log("phase 2 - " + item.offsetLeft);
                    //console.dir(item);

                    //console.log(item);
                    var jqItem = angular.element(item);
                    var itemScope = jqItem.scope().aspect;

                    if (item.offsetLeft < prevOffsetLeft)
                    {
                        _scaleRow(row, targetRowWidth);
                        row = [];
                        prevOffsetLeft = -1;
                    }

                    row.push(item);

                    prevOffsetLeft = item.offsetLeft;
                });

                // scale last row
                if (row.length >= 3)
                {
                    _scaleRow(row, targetRowWidth);
                }
            }
        }
    };
});

app.directive('itemsPaneItem', function($timeout) {
    return {
        link: function($scope, $el, $attrs, controller, transcludeFn) {

            var jsonStringCfg = $attrs["itemsPaneItem"];
            var cfg = angular.fromJson(jsonStringCfg);

            var aspect = $scope.$eval(cfg.aspect);
            $scope.aspect = aspect;

            $scope.$on(cfg.rendredEvent, function () {
                    console.log('emit item rendered');
                    $scope.$emit('items-pane-item-rendered');
                }
            );

            if ($scope.$last)
            {
                // wait for this last item to render
                $timeout(function() {
                    $scope.$emit('items-pane-item-last-iterated');
                });
            }

            $el.css('display', 'inline-block');
            $el.css('position', 'relative');
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