using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    /// <summary>
    /// Material content of a library that could be displayed.
    /// </summary>
    [DataContract]
    public abstract class AlbumContentItem : AlbumItem
    {
        public virtual IEnumerable<IMetadata> MetatdataCollection { get; set; }

        public BasicMetadata BasicMetadata
        {
            get
            {
                return MetatdataCollection.OfType<BasicMetadata>().FirstOrDefault();
            }
        }
    }
}
