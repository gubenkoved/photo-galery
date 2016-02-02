using PhotoGalery2.Core;
using PhotoGalery2.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PhotoGalery2.Server.Controllers
{
    [RoutePrefix("albums")]
    public class AlbumsController : ApiController
    {
        private PhotoGaleryFactory _factory;

        public AlbumsController(PhotoGaleryFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<AlbumViewModel> GetAlbums()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            return metadataProvider.GetAlbums()
                .Select(a => AlbumViewModel.CreateFor(a));
        }

        [HttpGet]
        [Route("{albumId}")]
        public AlbumViewModelExtended GetAlbum(string albumId)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = metadataProvider.GetAlbums()
                .SingleOrDefault(a => a.Id == albumId);

            if (album == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return AlbumViewModelExtended.CreateFor(album);
        }
    }
}
