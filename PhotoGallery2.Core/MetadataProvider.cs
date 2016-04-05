using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    public abstract class MetadataProvider
    {
        /// <summary>
        /// Returns the root album - collection of albums available.
        /// </summary>
        public abstract Album GetRoot();
    }
}
