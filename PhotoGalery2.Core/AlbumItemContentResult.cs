using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    public class AlbumItemContentResult
    {
        public readonly Stream Stream;
        public readonly string MimeType;
        public readonly Size? Size;

        public AlbumItemContentResult(Stream contentStream, Size? size, string mimeType)
        {
            Stream = contentStream;
            Size = size;
            MimeType = mimeType;
        }
    }
}
