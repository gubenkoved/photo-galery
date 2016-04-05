using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhotoGallery2.Core.Implementation.Naive
{
    public class NaiveContentProvider : ContentProvider
    {
        /// <summary>
        /// Gets or sets the path where thumbnails will be cached.
        /// Uses temp path as the default.
        /// </summary>
        public string ThumbCacheDir { get; set; }
        
        /// <summary>
        /// Gets or sets flag that controls usage of Cache for Thumbnails.
        /// Default value is true;
        /// </summary>
        public bool UseCache { get; set; } = true;

        public NaiveContentProvider()
        {
            ThumbCacheDir = Path.Combine(Path.GetTempPath(), "photo-gallery-cache");
        }

        public override AlbumItemContentResult GetOrigContent(Album album, string contentItemId)
        {
            string path = GetFilePathFor(album, contentItemId);

            FileStream origContentStream = GetFileStreamFor(path);

            return new AlbumItemContentResult(
                contentStream: origContentStream,
                size: null,
                mimeType: MimeMapping.GetMimeMapping(path));
        }

        public override AlbumItemContentResult GetThumbnail(Album album, string contentItemId, Size thumbSize, bool enforceAspectRato)
        {
            // try cached
            string thumbPath = GetCachedThumbPathFor(album, contentItemId, thumbSize, enforceAspectRato);

            if (File.Exists(thumbPath) && UseCache)
            {
                // return from cache
                return new AlbumItemContentResult(
                    contentStream: new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                    size: null,
                    mimeType: MimeMapping.GetMimeMapping(thumbPath));
            }

            string path = GetFilePathFor(album, contentItemId);

            Size resultSize;
            Stream thumbStream;

            if (enforceAspectRato)
            {
                thumbStream = ImageMethods.GenerateThumbinail(path, thumbSize, out resultSize);
            } else
            {
                thumbStream = ImageMethods.GenerateThumbinailExact(path, thumbSize);
                resultSize = thumbSize;
            }

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

        #region Private methods
        private string GetCachedThumbPathFor(Album album, string contentItemId, Size thumbSize, bool enforceAspectRato)
        {
            string fullAlbumPath = string.Join("_",
                GetAlbumsToRoot(album)
                    .OfType<NaiveAlbum>()
                    .Select(na => new DirectoryInfo(na.PhysicalDir).Name));

            string thumbPath = fullAlbumPath + $"_{contentItemId}_{thumbSize.Width}x{thumbSize.Height}{(enforceAspectRato ? "" : "_exact")}.jpg";

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

        private FileStream GetFileStreamFor(string path)
        {
            var fileStream = new System.IO.FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return fileStream;
        }

        private string GetFilePathFor(Album album, string contentItemId)
        {
            if (!(album is NaiveAlbum))
            {
                throw new InvalidOperationException();
            }

            var nAlbum = album as NaiveAlbum;

            return System.IO.Path.Combine(nAlbum.PhysicalDir, contentItemId);
        } 
        #endregion
    }
}
