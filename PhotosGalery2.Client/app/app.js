var app = angular.module('app', []);

app.constant('config',
    {
        apiRoot: 'http://localhost:55196/api',
        ver: '1.0.0.0'
    });

app.service('AlbumsService', ["$http", "config", function ($http, config)
  {
    var service = {};

    function mapAlbumsResponse(apiResponse)
    {
        var r =
        {
            name: apiResponse.Name,
            url: apiResponse.Url,
            parentUrl: apiResponse.ParentUrl,

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
                    thumbUrl: apiResponse.ContentItems[i].ThumbUrl
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

    return service;
}]);

app.controller('AlbumsController', ['$scope', 'AlbumsService', function ($scope, AlbumsService)
    {
        $scope.getAlbum = function(albumUrl)
        {
            return AlbumsService.getAlbumItems(albumUrl).then(function (albumModel)
            {
                $scope.currentAlbum = albumModel
            });
        }

        $scope.getParentAlbum = function()
        {
            $scope.getAlbum($scope.currentAlbum.parentUrl);
        }
    }]);

/* APP RUN */

angular.module('app')
    .run(['$rootScope', function ($rootScope) {
        $rootScope.hello = 'Hello World!'
    }]);