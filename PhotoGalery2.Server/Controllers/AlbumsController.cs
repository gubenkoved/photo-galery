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
        private AlbumItemsPathProvider _albumItemsPathProvider;

        public AlbumsController(PhotoGaleryFactory factory, AlbumItemsPathProvider albumPathProvider)
        {
            _factory = factory;
            _albumItemsPathProvider = albumPathProvider;
        }

        /// <summary>
        /// Returns light information about all albums items available.
        /// </summary>
        [HttpGet]
        [Route("")]
        public AlbumViewModelExtended GetRoot()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var rootAlbum = metadataProvider.GetRoot();

            return new AlbumViewModelExtended(_albumItemsPathProvider).FillBy2(rootAlbum);
        }

        /// <summary>
        /// Returns detailed information about specific album including items.
        /// </summary>
        /// <param name="albumPath">Id of album</param>
        [HttpGet]
        [Route("{albumPath}")]
        public AlbumViewModelExtended GetAlbum(string albumPath)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = _albumItemsPathProvider.FindByAlbumPath(metadataProvider.GetRoot(), albumPath);

            if (album == null)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.NotFound,
                    $"album with id '{albumPath}' was not found");
            }

            return new AlbumViewModelExtended(_albumItemsPathProvider).FillBy2(album);
        }

        /// <summary>
        /// Returns original content of album content item.
        /// </summary>
        [HttpGet]
        [Route("{albumPath}/content/{contentItemId}")]
        public HttpResponseMessage GetAlbumContentItem(
            string albumPath,
            string contentItemId)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = _albumItemsPathProvider.FindByAlbumPath(metadataProvider.GetRoot(), albumPath);

            if (album == null)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.NotFound,
                    $"album with id '{albumPath}' was not found");
            }

            var contentItem = album.Items
                .SingleOrDefault(i => i.Id == contentItemId);

            if (contentItem == null)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.NotFound,
                    $"content item with id '{contentItemId}' was not found");
            }

            if (!(contentItem is AlbumContentItem))
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.BadRequest,
                    $"item with id '{contentItemId}' is not content item");
            }

            var contentProvider = _factory.GetContentProvider();

            var contentResult = contentProvider.GetOrigContent(album, contentItemId);

            return this.ContentStreamResult(
                contentResult.Stream,
                contentResult.MimeType);
        }
    }
}
