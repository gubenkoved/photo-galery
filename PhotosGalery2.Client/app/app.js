var app = angular.module('app', []);

app.constant('config',
    {
        apiRoot: 'http://localhost:55196/api',
        ver: '1.0.0.0'
    });

app.service('AlbumsService', ["$http", "config", function ($http, config)
  {
    var service = {};

    service.getRootItems = function ()
    {
        console.log('test');

        return $http.get(config.apiRoot + '/albums')
            .then(function (response) {
                return response.data;
            });
    }

    return service;
}]);

app.controller('AlbumsController', ['$scope', 'AlbumsService', function ($scope, AlbumsService)
    {
        $scope.getRootItems = function ()
        {
            return AlbumsService.getRootItems().then(function (items)
            {
                $scope.albumItems = items.AlbumItems;
            });
        }
    }]);

/* APP RUN */

angular.module('app')
    .run(['$rootScope', function ($rootScope) {
        $rootScope.hello = 'Hello World!'
    }]);