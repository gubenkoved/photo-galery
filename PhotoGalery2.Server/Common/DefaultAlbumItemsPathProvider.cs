using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGalery2.Server.Common
{
    public class DefaultAlbumItemsPathProvider : AlbumItemsPathProvider
    {
        public const string Separator = ":";

        protected override string ConstructAlbumPathSegment(Album album)
        {
            string path = string.Empty;

            Album current = album;

            while (current != null)
            {
                if (path != string.Empty)
                {
                    path = ":" + path;
                }

                path = $"{current.Id}" + path;

                current = current.ParentAlbum;
            }

            return path;
        }

        protected override string ConstructContentItemPathSegment(AlbumContentItem contentItem)
        {
            return contentItem.Id;
        }

        public override Album FindByAlbumPath(Album album, string albumPath)
        {
            if (albumPath == Album.RootAlbumId)
            {
                return album;
            }

            if (albumPath.StartsWith(Album.RootAlbumId + Separator))
            {
                albumPath = albumPath.Substring((Album.RootAlbumId + Separator).Length);
            }

            string[] albumIds = albumPath.Split(new string[] { Separator }, StringSplitOptions.None)
                .ToArray();

            for (int level = 0; level < albumIds.Length; level++)
            {
                string aid = albumIds[level];

                album = album.Items
                    .OfType<Album>()
                    .SingleOrDefault(a => a.Id == aid);

                if (album == null)
                {
                    return null;
                }
            }

            return album;
        }

        public override AlbumContentItem FindByContentItemId(Album album, string contentItemId)
        {
            return album.Items.OfType<AlbumContentItem>()
                .SingleOrDefault(i => i.Id == contentItemId);
        }

        protected override Uri GetApiRootUri()
        {
            var currentRequest = HttpContext.Current.Request;

            if (currentRequest == null)
            {
                throw new InvalidOperationException("There is no Request context that is required to infer the API root URI.");
            }

            string baseUrl = currentRequest.Url.Scheme + "://"
                + currentRequest.Url.Authority
                + currentRequest.ApplicationPath.TrimEnd('/') + "/"
                + "api/";

            return new Uri(baseUrl);
        }
    }
}