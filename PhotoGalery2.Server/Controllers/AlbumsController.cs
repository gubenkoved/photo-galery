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
        /// Returns light information about all albums items available.
        /// </summary>
        [HttpGet]
        [Route("")]
        public AlbumViewModelExtended GetRoot()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var rootAlbum = metadataProvider.GetRoot();

            return new AlbumViewModelExtended().FillBy2(rootAlbum);
        }

        /// <summary>
        /// Returns detailed information about specific album including items.
        /// </summary>
        /// <param name="albumId">Id of album</param>
        [HttpGet]
        public AlbumViewModelExtended GetAlbum(
            [FromUri(Name="aid")] string albumId)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = metadataProvider.GetRoot();

            if (albumId != Album.RootAlbumId)
            {
                album = album
                    .Items
                    .OfType<Album>()
                    .SingleOrDefault(a => a.Id == albumId);
            }

            if (album == null)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.NotFound,
                    $"album with id '{albumId}' was not found");
            }

            return new AlbumViewModelExtended().FillBy2(album);
        }

        /// <summary>
        /// Returns original content of album content item.
        /// </summary>
        [HttpGet]
        [Route("content")]
        public HttpResponseMessage GetAlbumContentItem(
            [FromUri(Name = "aid")] string albumId,
            [FromUri(Name = "cid")] string albumContentItemId)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = metadataProvider.GetRoot();

            if (albumId != Album.RootAlbumId)
            {
                album = album
                    .Items
                    .OfType<Album>()
                    .SingleOrDefault(a => a.Id == albumId);
            }

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
