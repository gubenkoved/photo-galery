using PhotoGalery2.Core;
using PhotoGalery2.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PhotoGalery2.Server.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api")]
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
        [Route("albums")]
        public AlbumViewModelExtended GetRoot()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var rootAlbum = metadataProvider.GetRoot();

            var rootAlbumVM = new AlbumViewModelExtended(_albumItemsPathProvider).FillBy2(rootAlbum);

            return DefaultSortAlbum(rootAlbumVM);
        }

        /// <summary>
        /// Returns detailed information about specific album including items.
        /// </summary>
        /// <param name="albumPath">Id of album</param>
        [HttpGet]
        [Route("albums/{albumPath}")]
        public AlbumViewModelExtended GetAlbum(string albumPath)
        {
            //System.Threading.Thread.Sleep(3000);

            var metadataProvider = _factory.GetMetadataProvider();

            var album = _albumItemsPathProvider.FindByAlbumPath(metadataProvider.GetRoot(), albumPath);

            if (album == null)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.NotFound,
                    $"album with id '{albumPath}' was not found");
            }

            var extendedAlbumVM = new AlbumViewModelExtended(_albumItemsPathProvider).FillBy2(album);

            return DefaultSortAlbum(extendedAlbumVM);
        }

        /// <summary>
        /// Returns original content of album content item.
        /// </summary>
        [HttpGet]
        [Route("albums/{albumPath}/content/{contentItemId}")]
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

        /// <summary>
        /// Returns original content of album content item.
        /// </summary>
        [HttpGet]
        [Route("albums/{albumPath}/content/{contentItemId}/thumbnail")]
        public HttpResponseMessage GetAlbumContentItemThumbnail(
            string albumPath,
            string contentItemId,
            [FromUri] int w = 200,
            [FromUri] int h = 200,
            [FromUri] bool enforceSourceAspectRatio = true)
        {
            if (w < 10 || h < 10)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.BadRequest, "Invalid thumbnail size");
            }

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

            var contentResult = contentProvider.GetThumbnail(album, contentItemId,
                new Size(w, h), enforceSourceAspectRatio);

            Request.RegisterForDispose(contentResult.Stream);

            return this.ContentStreamResult(
                contentResult.Stream,
                contentResult.MimeType);
        }

        /// <summary>
        /// Returns original content of album content item.
        /// </summary>
        [HttpGet]
        [Route("resolve/{albumPath}")]
        public Uri Resolve(string albumPath)
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var album = _albumItemsPathProvider.FindByAlbumPath(metadataProvider.GetRoot(), albumPath);

            if (album == null)
            {
                this.ThrowHttpErrorResponseException(HttpStatusCode.NotFound,
                    $"album with id '{albumPath}' was not found");
            }

            return _albumItemsPathProvider.GetAlbumUri(album);
        }

        /// <summary>
        /// Sorts album/content items with default order.
        /// Returns the same album reference - for convenience of chaining.
        /// </summary>
        private AlbumViewModelExtended DefaultSortAlbum(AlbumViewModelExtended album)
        {
            album.AlbumItems = album.AlbumItems.OrderBy(x => x.Name).ToList();
            album.ContentItems = album.ContentItems.OrderBy(x => x.Name).ToList();

            return album;
        }
    }
}
