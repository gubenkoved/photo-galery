using System;
using System.Collections.Generic;
using System.IO;
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
            string path;
            FileStream origContentStream = GetFileStreamFor(album, contentItemId, out path);

            return new AlbumItemContentResult(
                contentStream: origContentStream,
                size: null,
                mimeType: MimeMapping.GetMimeMapping(path));
        }

        public override AlbumItemContentResult GetThumbnail(Album album, string contentItemId, Size thumbSize)
        {
            string path;
            using (FileStream origContentStream = GetFileStreamFor(album, contentItemId, out path))
            {
                var thumbGen = new ThumbnailGenerator();

                Size resultSize;
                Stream thumbStream = thumbGen.GenerateThumbinail(origContentStream, thumbSize, out resultSize);

                return new AlbumItemContentResult(
                    contentStream: thumbStream,
                    size: resultSize,
                    mimeType: MimeMapping.GetMimeMapping(path));
            }
        }

        private FileStream GetFileStreamFor(Album album, string contentItemId, out string path)
        {
            if (!(album is NaiveAlbum))
            {
                throw new InvalidOperationException();
            }

            var nAlbum = album as NaiveAlbum;

            path = System.IO.Path.Combine(nAlbum.PhysicalDir, contentItemId);

            var fileStream = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return fileStream;
        }
    }
}
