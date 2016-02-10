var settings =
{
    'appRoot': 'http://localhost:55196/api'
};

angular.module('app', [])
  .service('AlbumsService', ["$http", function ($http)
  {
      
  }]);

/* APP RUN */

angular.module('app')
    .run(['$rootScope', function ($rootScope) {
        $rootScope.hello = 'Hello World!'
    }]);