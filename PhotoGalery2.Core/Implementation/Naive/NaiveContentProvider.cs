using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhotoGalery2.Core.Implementation.Naive
{
    public class NaiveContentProvider : ContentProvider
    {
        public override AlbumItemContentResult GetOrigContent(Album album, string contentItemId)
        {
            if (!(album is NaiveAlbum))
            {
                throw new InvalidOperationException();
            }

            var nAlbum = album as NaiveAlbum;

            string path = System.IO.Path.Combine(nAlbum.PhysicalDir, contentItemId);

            var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open);

            return new AlbumItemContentResult(
                contentStream: fileStream,
                size: null,
                mimeType: MimeMapping.GetMimeMapping(path));
        }

        public override AlbumItemContentResult GetThumb(Album album, string contentItemId, Size thumbSize)
        {
            throw new NotImplementedException();
        }
    }
}
