using PhotoGallery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGallery2.Server
{
    public abstract class AlbumItemsPathProvider
    {
        protected abstract Uri GetApiRootUri();

        public abstract Album FindByAlbumPath(Album album, string albumPath);

        public abstract string ConstructAlbumPathSegment(Album album);

        public abstract AlbumContentItem FindByContentItemId(Album album, string contentItemId);

        public abstract string ConstructContentItemPathSegment(AlbumContentItem contentItem);

        public Uri GetAlbumUri(Album album)
        {
            var rootUri = GetApiRootUri();

            string albumPath = ConstructAlbumPathSegment(album);

            var albumUriBuilder = new UriBuilder(rootUri);

            albumUriBuilder.Path += $"albums/{albumPath}";

            return albumUriBuilder.Uri;
        }

        public Uri GetContentItemUri(AlbumContentItem contentItem)
        {
            var rootUri = GetApiRootUri();

            string albumPath = ConstructAlbumPathSegment(contentItem.ParentAlbum);

            string contentItemPathSegment = ConstructContentItemPathSegment(contentItem);

            var contentItemUriBuilder = new UriBuilder(rootUri);

            contentItemUriBuilder.Path += $"albums/{albumPath}/content/{contentItemPathSegment}";

            return contentItemUriBuilder.Uri;
        }

        public Uri GetContentItemThumbUri(AlbumContentItem contentItem)
        {
            var rootUri = GetApiRootUri();

            string albumPath = ConstructAlbumPathSegment(contentItem.ParentAlbum);

            string contentItemPathSegment = ConstructContentItemPathSegment(contentItem);

            var contentItemUriBuilder = new UriBuilder(rootUri);

            contentItemUriBuilder.Path += $"albums/{albumPath}/content/{contentItemPathSegment}/thumbnail";

            //contentItemUriBuilder.Query = $"w={thumbSize.Width}&h={thumbSize.Height}";

            return contentItemUriBuilder.Uri;
        }
    }
}