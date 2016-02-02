using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhotoGalery2.Core.Implementation
{
    public class NaiveContentProvider : ContentProvider
    {
        public override AlbumItemContentResult GetOrigContent(AlbumContentItem albumContentItem)
        {
            string path = albumContentItem.Id;

            var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open);

            return new AlbumItemContentResult(
                contentStream: fileStream,
                size: null,
                mimeType: MimeMapping.GetMimeMapping(path));
        }

        public override AlbumItemContentResult GetThumb(AlbumContentItem albumContentItem, Size thumbSize)
        {
            throw new NotImplementedException();
        }
    }
}
