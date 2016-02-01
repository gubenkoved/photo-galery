using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    /// <summary>
    /// Grouping container for AlbumItems.
    /// </summary>
    [DataContract]
    public class Album : AlbumItem
    {
        [DataMember]
        public virtual IList<AlbumItem> Items { get; set; }

        public Album()
        {
            Items = new List<AlbumItem>();
        }
    }
}
