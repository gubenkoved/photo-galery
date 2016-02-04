using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGalery2.Server.Common
{
    public abstract class AlbumItemsPathProvider
    {
        public abstract Album FindByAlbumPath(Album album, string albumPath);

        public abstract string ConstructAlbumPath(Album album);

        public abstract AlbumContentItem FindByContentItemId(Album album, string contentItemId);

        public abstract string ConstructContentItemId(AlbumContentItem contentItem);
    }
}