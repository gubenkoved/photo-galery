using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    public abstract class PhotoGaleryFactory
    {
        public abstract MetadataProvider GetMetadataProvider();
        public abstract ContentProvider GetContentProvider();
    }
}
