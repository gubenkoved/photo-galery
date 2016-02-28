using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    public abstract class ContentProvider
    {
        /// <summary>
        /// Returns content with original content for requested item.
        /// </summary>
        public abstract AlbumItemContentResult GetOrigContent(Album album, string contentItemId);

        /// <summary>
        /// Returns stream with content of thumbnail image for request album item.
        /// </summary>
        /// <param name="thumbSize">Max requested thumbnail size.</param>
        public abstract AlbumItemContentResult GetThumbnail(Album album, string contentItemId, Size thumbSize, bool enforceAspectRatio = true);
    }
}
