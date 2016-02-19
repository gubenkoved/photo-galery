﻿var app = angular.module('app', ['ngRoute']);

app.config(['$routeProvider',
  function ($routeProvider) {
      $routeProvider.
        when('/albums/:albumPath?', {
            templateUrl: 'views/albums.html',
            controller: 'AlbumsController'
        }).
        when('/about', {
            templateUrl: 'views/about.html',
            controller: 'AboutController'
        }).
        otherwise({
            redirectTo: '/albums'
        });
  }]);

app.constant('config',
{
    apiRoot: 'http://localhost:55196/api',
    ver: '1.0.0.0',
});

/* SERVICES */

app.service('AlbumsService', ["$http", "config", "$q", function ($http, config, $q)
  {
    var service = {};

    function mapAlbumsResponse(apiResponse)
    {
        var r =
        {
            name: apiResponse.Name,
            url: apiResponse.Url,
            parentUrl: apiResponse.ParentUrl,
            fullPath: apiResponse.FullPath,

            albumItems: [],
            contentItems: []
        };

        for (var i = apiResponse.AlbumItems.length - 1; i >= 0; i--)
        {
            r.albumItems.push(
                {
                    name: apiResponse.AlbumItems[i].Name,
                    url: apiResponse.AlbumItems[i].Url
                });
        }

        for (var i = apiResponse.ContentItems.length - 1; i >= 0; i--)
        {
            r.contentItems.push(
                {
                    name: apiResponse.ContentItems[i].Name,
                    url: apiResponse.ContentItems[i].Url,
                    thumbUrl: apiResponse.ContentItems[i].ThumbUrl,
                    width: apiResponse.ContentItems[i].Width,
                    height: apiResponse.ContentItems[i].Height,
                });
        }

        return r;
    }

    service.getAlbumItems = function (albumUrl)
    {
        if (!albumUrl)
        {
            return $http.get(config.apiRoot + '/albums')
                .then(function (response) {
                    return mapAlbumsResponse(response.data);
                });            
        }

        return $http.get(albumUrl)
            .then(function (response) {
                return mapAlbumsResponse(response.data);
            });
    }

    service.resolve = function (albumPath)
    {
        console.log('resolve: ' + albumPath);

        if (albumPath)
        {
            return $http.get(config.apiRoot + '/resolve/' + albumPath)
                .then( function (response)
                {
                    console.log(response.data);

                    return response.data;
                });
        } else
        {
            return $q(function(resolve, reject)
            {
                resolve(config.apiRoot + '/albums');
            });
        }
    }

    return service;
}]);

/* CONTROLLERS */

app.controller('AlbumsController',
    ['$scope', '$routeParams', '$route', 'AlbumsService',
    function ($scope, $routeParams, $route, AlbumsService)
{
    $scope.init = function ()
    {
        AlbumsService.resolve($routeParams.albumPath)
            .then(function (targetAlbumUrl)
            {
                $scope.getAlbum(targetAlbumUrl);
            });
    }

    $scope.getAlbum = function(albumUrl)
    {
        var rid = $scope.nextRequestId();
        $scope.waitingForRequestId = rid;

        return AlbumsService.getAlbumItems(albumUrl).then(function (albumModel)
        {
            if ($scope.waitingForRequestId === rid)
            {
                var pRoute = $routeParams;
                pRoute.albumPath = albumModel.fullPath;
                $route.updateParams(pRoute);

                $scope.currentAlbum = albumModel
            } else
            {
                console.log(`discarding result of request with id ${rid} since waiting for ${$scope.waitingForRequestId}`);
            }
        });
    }

    $scope.getParentAlbum = function()
    {
        $scope.getAlbum($scope.currentAlbum.parentUrl);
    }

    $scope.onAlbumClick = function (album)
    {
        $scope.getAlbum(album.url);
    }

    $scope.debug = function (o)
    {
        console.log(o);
    }
}]);

app.controller('AboutController', ['$scope', 'config', function ($scope, config) {
    $scope.config = config;
}]);


/* DIRECTIVES */

app.directive('contentItem', function() {
    return {
        scope: {
             item: '='
        },
        templateUrl: '/app/directives/contentItem.html'
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
        templateUrl: '/app/directives/test.html'
  };
});

/* APP RUN */

angular.module('app')
    .run(['$rootScope', function ($rootScope) {
        
        $rootScope.lastRequestId = 0;

        $rootScope.nextRequestId = function()
        {
            $rootScope.lastRequestId += 1;

            return $rootScope.lastRequestId;
        }
    }]);