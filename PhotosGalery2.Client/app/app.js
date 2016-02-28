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
            templateUrl: 'views/test.html',
            controller: 'TestController'
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
    },

    // this improves image quality by requesting exact size from server
    // that is needed to show in browser w/o any conversion
    // loading on API side is increased however by enablig this
    avoidClientSideResize: true
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

    function _getSpecificSizeThumbUrl(thumbUrl, width, height)
    {
        // if undefined or 0 was requested - return some super small thumb to easier troubleshooting
        width = width || 13;
        height = height || 13;

        if (config.avoidClientSideResize)
        {
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'w', width);
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'h', height);

            thumbUrl = _updateQueryStringParameter(thumbUrl, 'enforceSourceAspectRatio', 'false');
        } else
        {
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'w', config.desiredThumbSize.width);
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'h', config.desiredThumbSize.height);            
        }

        return thumbUrl;
    }

    function mapAlbumsResponse(apiResult) {
        var r = {
            name: apiResult.Name,
            url: apiResult.Url,
            thumbUrl: apiResult.ThumbUrl,
            parentUrl: apiResult.ParentUrl,
            fullPath: apiResult.FullPath,

            albumItems: [],
            contentItems: []
        };

        for (var i = apiResult.AlbumItems.length - 1; i >= 0; i--) {
            var ai = apiResult.AlbumItems[i];
            
            r.albumItems.push({
                name: ai.Name,
                url: ai.Url,
                thumbUrl: ai.ThumbUrl
            });
        }

        for (var i = apiResult.ContentItems.length - 1; i >= 0; i--) {
            var ci = apiResult.ContentItems[i];

            r.contentItems.push({
                name: ci.Name,
                url: ci.Url,
                thumbUrl: ci.ThumbUrl,
                origWidth: ci.OrigWidth,
                origHeight: ci.OrigHeight
            });
        }

        return r;
    }

    service.getSpecificSizeThumbUrl = _getSpecificSizeThumbUrl;

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

        $scope.getSpecificSizeThumbUrl = function (thumbUrl, width, height) {
            return AlbumsService.getSpecificSizeThumbUrl(thumbUrl, width, height);
        }

        $scope.getParentAlbum = function() {
            $scope.getAlbum($scope.currentAlbum.parentUrl);
        }

        $scope.onAlbumClick = function(album) {
            console.log('handle onAlbumClick for');
            console.log(album);
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


app.controller('TestController', ['$scope', function($scope){
    $scope.album = {
        name: 'Test album',
        url: 'http://example.com',
        parentUrl: 'http://example.com',
        fullPath: ':testAlbum',

        albumItems: [
            { name: 'A1', url: '' },
            { name: 'A2', url: '' }
        ],
        contentItems: [
            {name: 'C1', url: '', thumbUrl: 'https://goo.gl/fMLlXF', origWidth: 640, origHeight: 480 },
            {name: 'C2', url: '', thumbUrl: 'https://goo.gl/fMLlXF', origWidth: 640, origHeight: 480 },
            {name: 'C3', url: '', thumbUrl: 'https://goo.gl/fMLlXF', origWidth: 640, origHeight: 480 },
        ]
    }
}]);

/* DIRECTIVES */

app.directive('contentItem', function($timeout) {
    return {
        scope: {
            item: '='
        },
        templateUrl: '/app/directives/contentItem.html',
        //require: '^ablumView'
        compile: function (element, transclude, maxPriority)
        {
            element.addClass('contentItem');

            return function($scope, $el, $attrs)
            {
                var img = $el.find('img');

                $scope.$on('rerender-thumbnails', function (event, args) {
                    //console.log('[thumb-rerender] ' + img.width() + 'x' + img.height());
                    if (!$scope.$$destroyed)
                    {
                        var url = args.thumbUrlResolver($scope.item.thumbUrl, img.width(), img.height());
                        img.attr('src', url);
                    }
                });

                console.log('[post-link] content-item');
            }
        }
    };
});

app.directive('spinner2', function() {
    return {
        scope: {
            ngSrc: '@'
        },
        link: function ($scope, $el, $attrs)
        {
            $scope.spinner = angular.element('<div class="spinner">\
  <div class="rect1"></div>\
  <div class="rect2"></div>\
  <div class="rect3"></div>\
  <div class="rect4"></div>\
  <div class="rect5"></div>\
</div>');
            $scope.spinner.attr('style', 'position: absolute; left: 0; top: 0; right: 0; bottom: 0; margin: auto;');

             $el.on('load', function() {
                $el.show();
                $scope.spinner.remove();
             });
            
             $scope.$watch('ngSrc', function() {
                 $el.hide();

                 //$scope.loaderImg = angular.element('<img class="loader"></img>');
                 //$scope.loaderImg.attr('src', spinnerScr);                 

                 $el.parent().append($scope.spinner);
             });
        }
    };
});

app.directive('albumItem', function() {
    return {
        scope: {
            item: '=',
            onClick: '&'
        },
        templateUrl: '/app/directives/albumItem.html'
    };
});

app.directive('albumView', function ($compile, $timeout, $window) {
    return {
        scope: {
            album: '=',
            spacing: '@',
            targetHeight: '@',
            onAlbumClick: '&',
            specificSizeThumbResolver: '&'
        },
        restrict: 'A',
        templateUrl: '/app/directives/albumView.html',
        controller: function ($scope)
        {
            $scope.targetHeight = $scope.targetHeight || 200;
            $scope.spacing = $scope.spacing || 3;

            function _scaleRow(itemsRow, targetWidth) {
                console.log('scaling row');

                targetWidth -= 5;

                var currentWidth = 0;

                angular.forEach(itemsRow, function(item) {
                    currentWidth += item.width();
                });

                var scaleFactor = targetWidth / currentWidth;

                angular.forEach(itemsRow, function(item) {
                    item = angular.element(item);

                    var itemTargetWidth = item.width() * scaleFactor;
                    var itemTargetHeight = item.height() * scaleFactor;

                    itemTargetWidth = Math.round(itemTargetWidth);
                    itemTargetHeight = Math.round(itemTargetHeight);

                    item.width(itemTargetWidth);
                    item.height(itemTargetHeight);
                });
            }

            function _redrawAllWithTargetHeight() {
                var jqContainer = $scope.contentItemsContainer;

                // phase 1: Scale to TargetHeight taking into account the item aspect

                var items = $scope.itemsRoot.children();

                angular.forEach(items, function(item) {
                    item = angular.element(item);

                    var itemAspect = angular.element(item).scope().aspect || 1;

                    item.css({
                        height: $scope.targetHeight + 'px',
                        width: itemAspect * $scope.targetHeight + 'px',
                        position: 'relative'
                    });

                    item.children().css({
                        position: 'absolute',
                        top: $scope.spacing + 'px',
                        left: $scope.spacing + 'px',
                        bottom: 0,
                        right: 0
                    });
                });
            }

            function _scaleAsRows() {
                var prevOffsetLeft = -1;
                var row = [];
                var rowNumber = 0;

                var targetRowWidth = $scope.itemsRoot.width();

                var items = $scope.itemsRoot.children();

                angular.forEach(items, function(item) {
                    item = angular.element(item);
                    //console.log(contentItemWrapper.offset().left);

                    if (item.offset().left <= prevOffsetLeft)
                    {
                        _scaleRow(row, targetRowWidth);
                        rowNumber += 1;
                        row = [];
                        prevOffsetLeft = -1;
                    }

                    if (item.scope())
                    {
                        item.scope().rowNumber = rowNumber;
                    } else
                    {
                        console.log('WTF');
                        console.log(item);
                    }

                    row.push(item);

                    prevOffsetLeft = item.offset().left;
                });

                // scale last row
                if (row.length >= 3)
                {
                    _scaleRow(row, targetRowWidth);
                }
            }

            $scope.rearrange = function() {
                $scope.rearrangeNeeded = false;

                if (!$scope.itemsRoot.children().length)
                {
                    return;
                }

                console.log('phase 1: Scale all to target height');
                _redrawAllWithTargetHeight();

                // timeout is needed to let browser rebuild DOM after first phase
                $timeout(function () {
                    // phase 2: Scale rows so that they fills the whole width
                    console.log('phase 2: Scale rows to fill parent by width');
                    _scaleAsRows();

                    $timeout(function() {
                        console.log('broadcasting rerender-thumbnails');
                        $scope.$broadcast('rerender-thumbnails', {
                            thumbUrlResolver: function (thumbUrl, desiredWidth, desiredHeight)
                            {
                                return $scope.specificSizeThumbResolver()(thumbUrl, desiredWidth, desiredHeight);
                            }
                        });
                    });
                });
            }
        },
        link: function ($scope, $el, $attr, $controller, $transclude) {
            
            $scope.itemsRoot = $el.find('.items-root');
            $scope.itemsRoot.css({
                'font-size': 0
            });

            $scope.$watch('album', function (newValue, oldValue)
            {
                if (newValue)
                {
                    angular.forEach($scope.album.albumItems, function (albumItemModel)
                    {
                        var albumItemScope = $scope.$new();
                        albumItemScope.albumItem = albumItemModel;

                        var itemWrapper = angular.element('<div></div>');
                        itemWrapper.css({
                            display: 'inline-block',
                            margin: 0,
                            padding: 0
                        });
                        itemWrapper.addClass('item-wrapper');
                        
                        var albumItemDirective = angular.element('<div album-item item="albumItem" on-click="onAlbumClick()(albumItem)"></div>');

                        itemWrapper.append(albumItemDirective);
                        $scope.itemsRoot.append(itemWrapper);

                        $compile(albumItemDirective)(albumItemScope);
                    });

                    angular.forEach($scope.album.contentItems, function (contentItemModel)
                    {
                        var contentItemScope = $scope.$new();
                        contentItemScope.contentItem = contentItemModel;
                        contentItemScope.aspect = contentItemModel.origWidth / contentItemModel.origHeight;

                        var contentItemWrapper = angular.element('<div></div>');
                        contentItemWrapper.css({
                            display: 'inline-block',
                            margin: 0,
                            padding: 0
                        });
                        contentItemWrapper.addClass('item-wrapper');

                        var contentItemDirective = angular.element('<div content-item item="contentItem"></div>');
                        
                        contentItemWrapper.append(contentItemDirective);
                        $scope.itemsRoot.append(contentItemWrapper);

                        // var debugInfo = angular.element('<div> #{{ rowNumber }} </div>');
                        // debugInfo.css({
                        //     'font-size': '14px',
                        //     'font-family': 'Consolas'
                        // });
                        // contentItemWrapper.append(debugInfo);

                        $compile(contentItemWrapper)(contentItemScope);
                    })

                    $scope.rearrange();
                }
            });

            // listen for container size changes
            $scope.$watch(function() {
                return $scope.itemsRoot.width();
            }, function (newValue, oldValue) {
                if (newValue && oldValue && newValue !== oldValue)
                {
                    console.log('viewport width changed from ' + oldValue + ' to ' + newValue);

                    $scope.rearrangeNeeded = true;

                    $timeout(function(){

                        if ($scope.rearrangeNeeded)
                        {
                            $scope.rearrange();
                        }

                    }, 1000);
                }
            });

            angular.element($window).bind('resize', function () {
                //console.log('widnows resized - start digest cycle');
                $scope.$apply();
            });
        }
    };
})

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

/* APP RUN */

angular.module('app')
    .run(['$rootScope', function($rootScope) {

        $rootScope.lastRequestId = 0;

        $rootScope.nextRequestId = function() {
            $rootScope.lastRequestId += 1;

            return $rootScope.lastRequestId;
        }
    }]);