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
        /// <summary>
        /// Gets or sets the path where thumbnails will be cached.
        /// Uses temp path as the default.
        /// </summary>
        public string ThumbCacheDir { get; set; }

        public NaiveContentProvider()
        {
            ThumbCacheDir = Path.Combine(Path.GetTempPath(), "photo-gallery-cache");
        }

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
            // try cached
            string thumbPath = GetCachedThumbPathFor(album, contentItemId, thumbSize);

            if (File.Exists(thumbPath))
            {
                // return from cache
                return new AlbumItemContentResult(
                    contentStream: new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                    size: null,
                    mimeType: MimeMapping.GetMimeMapping(thumbPath));
            }

            string path;
            using (FileStream origContentStream = GetFileStreamFor(album, contentItemId, out path))
            {
                Size resultSize;
                Stream thumbStream = ImageMethods.GenerateThumbinail(origContentStream, thumbSize, out resultSize);

                // save in cache
                if (!Directory.Exists(ThumbCacheDir))
                {
                    Directory.CreateDirectory(ThumbCacheDir);
                }

                using (var fileStream = File.Create(thumbPath))
                {
                    thumbStream.Seek(0, SeekOrigin.Begin);
                    thumbStream.CopyTo(fileStream);
                }

                thumbStream.Seek(0, SeekOrigin.Begin);

                return new AlbumItemContentResult(
                    contentStream: thumbStream,
                    size: resultSize,
                    mimeType: MimeMapping.GetMimeMapping(path));
            }
        }

        private string GetCachedThumbPathFor(Album album, string contentItemId, Size thumbSize)
        {
            string fullAlbumPath = string.Join("_",
                GetAlbumsToRoot(album)
                    .OfType<NaiveAlbum>()
                    .Select(na => new DirectoryInfo(na.PhysicalDir).Name));

            string thumbPath = fullAlbumPath + $"_{contentItemId}_{thumbSize.Width}x{thumbSize.Height}.jpg";

            return Path.Combine(ThumbCacheDir, thumbPath);
        }

        private List<Album> GetAlbumsToRoot(Album album)
        {
            var result = new List<Album>();

            while (album.ParentAlbum != null)
            {
                result.Add(album);

                album = album.ParentAlbum;
            }

            result.Add(album);

            result.Reverse();

            return result;
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
