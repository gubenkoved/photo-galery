using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    /// <summary>
    /// Material content of a library that could be displayed.
    /// </summary>
    [DataContract]
    public abstract class AlbumContentItem : AlbumItem
    {
        [DataMember]
        public virtual Size Size { get; set; }
    }
}
