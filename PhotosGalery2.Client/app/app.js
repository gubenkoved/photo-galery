var settings =
{
    'appRoot': 'http://localhost:55196/api'
};

function AlbumsService() {
    this.getRoot() = function () {
        return 'Hello there ' + name;
    };
}

angular
  .module('app')
  .service('AlbumsService', AlbumsService);