using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    public abstract class MetadataProvider
    {
        /// <summary>
        /// Returns the collection of albums available.
        /// </summary>
        public abstract IEnumerable<Album> GetAlbums();

        /// <summary>
        /// Prepares album to being viewed. Could imply building up help data structures with
        /// precalculated metadata.
        /// </summary>
        public abstract void PrepareAlbum(Album album);
    }
}
