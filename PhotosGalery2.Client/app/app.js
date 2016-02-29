var app = angular.module('app', ['ngRoute', 'LocalStorageModule', 'angular-inview']);

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
        when('/settings', {
            templateUrl: 'views/settings.html',
            controller: 'SettingsController'
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

app.constant('defaultConfig', {
    apiRoot: 'http://192.168.1.2:59999/api',
    ver: '1.0.0.0',

    desiredThumbSize: {
        width: 300,
        height: 300
    },

    // this improves image quality by requesting exact size from server
    // that is needed to show in browser w/o any conversion
    // loading on API side is increased however by enablig this
    avoidClientSideResize: true,

    'albumView.spacing': 5,
    'albumView.targetHeight': 200
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

app.service('ConfigService', function (defaultConfig, localStorageService) {
    var service = {}

    service.get = function()
    {
        return localStorageService.get('config') || angular.extend({}, defaultConfig);
    }

    service.save = function(config)
    {
        localStorageService.set('config', config);
    }

    service.restoreDefault = function(config)
    {
        localStorageService.set('config', angular.extend({}, defaultConfig));
    }

    return service;
});

app.service('AlbumsService', function($http, ConfigService, $q) {
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

    service.getSpecificSizeThumbUrl = function (thumbUrl, width, height, keepSourceAspectRatio)
    {
        // if undefined or 0 was requested - return some super small thumb to easier troubleshooting
        width = width || 13;
        height = height || 13;

        if (ConfigService.get().avoidClientSideResize)
        {
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'w', width);
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'h', height);

            if (keepSourceAspectRatio !== true)
            {
                thumbUrl = _updateQueryStringParameter(thumbUrl, 'enforceSourceAspectRatio', 'false');
            }
        } else
        {
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'w', ConfigService.get().desiredThumbSize.width);
            thumbUrl = _updateQueryStringParameter(thumbUrl, 'h', ConfigService.get().desiredThumbSize.height);            
        }

        return thumbUrl;
    }

    service.getAlbumItems = function(albumUrl) {
        if (!albumUrl) {
            return $http.get(ConfigService.get().apiRoot + '/albums')
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
            return $http.get(ConfigService.get().apiRoot + '/resolve/' + albumPath)
                .then(function(response) {
                    console.log(response.data);

                    return response.data;
                });
        } else {
            return $q(function(resolve, reject) {
                resolve(ConfigService.get().apiRoot + '/albums');
            });
        }
    }

    return service;
});

/* CONTROLLERS */

app.controller('AlbumsController',
    function($scope, $routeParams, $route, $location, AlbumsService, ConfigService) {

        //debugger;
        $scope.spacing = ConfigService.get()['albumView.spacing'];
        $scope.targetHeight = ConfigService.get()['albumView.targetHeight'];

        $scope.init = function() {
            console.log('AlbumsController.init');
            
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
            console.log('handle onAlbumClick for');
            console.log(album);
            $scope.getAlbum(album.url);
        }

        $scope.debug = function(o) {
            console.log(o);
        }
    }
);

app.controller('AboutController', function($scope, ConfigService) {
    $scope.config = ConfigService.get();
});

app.controller('SettingsController', function($scope, ConfigService) {
    $scope.config = ConfigService.get();

    $scope.saveConfig = function ()
    {
        ConfigService.save($scope.config);
    }

    $scope.restoreDefault = function ()
    {
        ConfigService.restoreDefault($scope.config);
        $scope.config = ConfigService.get();
    }
});

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

    $scope.testFunc = function ()
    {
        console.log('test func!');
    }

    //$scope.
}]);

/* DIRECTIVES */

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

app.directive('contentItem', function(AlbumsService) {
    return {
        scope: {
            item: '='
        },
        //templateUrl: '/app/directives/contentItem.html',
        template:
            '<div item-name="{{ item.name }}" style="width: 100%; height: 100%">' +
                '<img spinner2 />' +
            '</div>',
        //require: '^ablumView'
        link: function($scope, $el, $attrs) {
            var img = $el.find('img');

            $scope.$on('rerender-thumbnails', function (event, args) {
                
                if (!$scope.$$destroyed)
                {
                    var currentWidth = img.width();
                    var currentHeight = img.height();

                    if (!$scope.lastRenderedWith
                        || $scope.lastRenderedWith.width !== currentWidth
                        || $scope.lastRenderedWith.height !== currentHeight)
                    {
                        console.log('[thumb-rerender-ci]');

                        var url = AlbumsService.getSpecificSizeThumbUrl($scope.item.thumbUrl, currentWidth, currentHeight);
                        img.attr('src', url);

                        $scope.lastRenderedWith = {
                            width: currentWidth,
                            height: currentHeight
                        };
                    } else
                    {
                        //console.log('[thumb-rerender-ci] skip');
                    }
                }
            });

            console.log('[post-link] content-item');
        }
    };
});

app.directive('albumItem', function(AlbumsService) {
    return {
        scope: {
            item: '=',
            onClick: '&'
        },
        //templateUrl: '/app/directives/albumItem.html',
        template:
            '<div class="album-item-content" ng-click="onClick()(item)" style="width: 100%; height: 100%">'+
                '<img spinner2 />' +
                '<a>{{ item.name }}</a>' +
            '</div>',
        link: function($scope, $el, $attrs) {
            var img = $el.find('img');

            $scope.$on('rerender-thumbnails', function (event, args) {
                console.log('[thumb-rerender-ai]');
                if (!$scope.$$destroyed)
                {
                    if ($scope.item.thumbUrl)
                    {
                        var s = Math.max(img.width(), img.height())
                        var url = AlbumsService.getSpecificSizeThumbUrl($scope.item.thumbUrl, s, s, true);

                        img.attr('src', url);
                    }
                }
            });

            console.log('[post-link] album-item');
        }
    };
});

app.directive('albumView', function ($compile, $timeout, $window, $q) {
    return {
        scope: {
            album: '=',
            spacing: '@',
            targetHeight: '@',
            onAlbumClick: '&',
            lazyLoadingBatchSize: '@'
        },
        restrict: 'A',
        templateUrl: '/app/directives/albumView.html',
        controller: function ($scope)
        {
            $scope.targetHeight = $scope.targetHeight || 200;
            $scope.spacing = $scope.spacing || 3;
            $scope.lazyLoadingBatchSize = $scope.lazyLoadingBatchSize || 12;

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

            // returns promise that is resolved when DOM is finally rendered
            function _redrawWithTargetHeight(items) {
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

                // return promise that is resolved as soon as DOm is rendered
                return $timeout(function() {});
            }

            function _scaleAsRows(items) {

                console.log('_scaleAsRows for ' + items.length);

                var prevOffsetLeft = -1;
                var row = [];
                var rowNumber = 0;

                var targetRowWidth = $scope.itemsRoot.width();

                angular.forEach(items, function(item) {

                    item = angular.element(item);

                    if (item.offset().left <= prevOffsetLeft) {
                        _scaleRow(row, targetRowWidth);
                        rowNumber += 1;
                        row = [];
                        prevOffsetLeft = -1;
                    }

                    if (item.scope()) {
                        item.scope().rowNumber = rowNumber;
                    }

                    row.push(item);

                    prevOffsetLeft = item.offset().left;
                });

                // returm promise that will wait for DOM render
                return $timeout(function() {});
            }

            $scope._buffer = []

            // takes batch of items and adds them to DOM and aligns properly without leaving non-filled row;
            // maintains rest of items from batch that are not rendered in buffer and automatically pickes them
            // up on next call;
            $scope.appendRearrageBatch = function (items)
            {
                if (!items || !items.length)
                {
                    return $q(function(resolve, reject) {
                        resolve(null)
                    });
                }

                console.log('appendRearrageBatch for ' + items.length);

                if (!(items instanceof Array))
                {
                    items = items.toArray();
                }

                //debugger;
                // pick up the rest of items from last run
                items = items.concat($scope._buffer);

                //console.log('appendRearrageBatch2 for ' + items.length);

                return _redrawWithTargetHeight(items)
                    .then(_scaleAsRows(items))
                    .then(function () {
                        // figure out last row items to be subject of repeated rearrange on next call

                        $scope._buffer = [];
                        var lastRowNum = angular.element(items[items.length - 1]).scope().rowNumber;

                        angular.forEach(items, function(item) {
                            if (angular.element(item).scope().rowNumber === lastRowNum) {
                                $scope._buffer.push(item);
                            }
                        });
                    })
                    .then(function () {
                        console.log('broadcasting rerender-thumbnails');
                        $scope.$broadcast('rerender-thumbnails');
                    });
            }

            $scope.rearrangeAll = function ()
            {
                // ToDo: Exclude _buffer items from here
                var items = $scope.itemsRoot.children();

                return _redrawWithTargetHeight(items)
                    .then(_scaleAsRows(items))
                    .then(function () {
                        console.log('broadcasting rerender-thumbnails');
                        $scope.$broadcast('rerender-thumbnails');
                    });
            }

            $scope.lastContentItemIndex = -1;
            $scope.getNewContentItemsBatchToRender = function ()
            {
                var start = $scope.lastContentItemIndex + 1;
                var to = start + $scope.lazyLoadingBatchSize;
                $scope.lastContentItemIndex = to;

                var items = [];

                angular.forEach($scope.album.contentItems.slice(start, to), function (contentItemModel)
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

                    items.push(contentItemWrapper);
                })

                return items;
            }

            $scope.loadMoreContentItems = function ()
            {
                var loadFunc = function () {
                    if ($scope.lastContentItemIndex >= $scope.album.contentItems.length)
                    {
                        console.log('[lazy-loading] all set!');
                    } else
                    {
                        console.log('[lazy-loading] request more items');
                    }

                    var toRender = $scope.getNewContentItemsBatchToRender();
                    return $scope.appendRearrageBatch(toRender);
                };

                if (!$scope.album)
                {
                    $scope.$watch('album', function (newValue)
                    {
                        if (newValue)
                            loadFunc();
                    });
                } else
                {
                    return loadFunc();
                }
            }
        },
        link: function ($scope, $el, $attr) {
            
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

                    console.log('rearrange after link');

                    // render album items
                    $scope.appendRearrageBatch($scope.itemsRoot.children());
                        //.then($scope.loadMoreContentItems);
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
                            $scope.rearrangeAll();
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

app.run(['$rootScope', function($rootScope) {
    $rootScope.lastRequestId = 0;
    $rootScope.nextRequestId = function() {
        $rootScope.lastRequestId += 1;

        return $rootScope.lastRequestId;
    }
}]);