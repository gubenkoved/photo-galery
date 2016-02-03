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
    [RoutePrefix("api/albums")]
    public class AlbumsController : ApiController
    {
        private PhotoGaleryFactory _factory;

        public AlbumsController(PhotoGaleryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Returns light information about all albums available.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IEnumerable<AlbumViewModel> GetAlbums()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            return metadataProvider.GetItems()
                .OfType<Album>()
                .Select(a => AlbumViewModel.CreateFor(a));
        }

        /// <summary>
        /// Returns detailed information about specific album including items.
        /// </summary>
        /// <param name="albumId">Id of album</param>
        [HttpGet]
        [Route("{albumId}")]
        public AlbumViewModelExtended GetAlbum(string albumId)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = metadataProvider.GetItems()
                .OfType<Album>()
                .SingleOrDefault(a => a.Id == albumId);

            if (album == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return AlbumViewModelExtended.CreateFor(album);
        }

        /// <summary>
        /// Returns original content of album content item.
        /// </summary>
        [HttpGet]
        [Route("{albumId}/content/{*albumContentItemId}")]
        [Route("{albumId}/content")]
        public HttpResponseMessage GetAlbumContentItem(string albumId, string albumContentItemId)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = metadataProvider.GetItems()
                .OfType<Album>()
                .SingleOrDefault(a => a.Id == albumId);

            if (album == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var contentItem = album.Items
                .SingleOrDefault(i => i.Id == albumContentItemId);

            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (!(contentItem is AlbumContentItem))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var contentProvider = _factory.GetContentProvider();

            var contentResult = contentProvider.GetOrigContent(contentItem as AlbumContentItem);

            return this.ContentStreamResult(
                contentResult.Stream,
                contentResult.MimeType);
        }
    }
}
