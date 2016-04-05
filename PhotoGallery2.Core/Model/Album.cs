using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    /// <summary>
    /// Grouping container for AlbumItems.
    /// </summary>
    [DataContract]
    public class Album : AlbumItem
    {
        public const string RootAlbumId = ":root";

        [DataMember]
        public virtual IEnumerable<AlbumItem> Items { get; set; }

        public Album()
        {
            Items = new List<AlbumItem>();
        }
    }
}
