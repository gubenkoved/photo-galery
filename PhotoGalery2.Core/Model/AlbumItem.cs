using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    [DataContract]
    public abstract class AlbumItem
    {
        [IgnoreDataMember]
        public AlbumItem Parent { get; set; }

        [DataMember]
        public virtual string Id { get; set; }

        [DataMember]
        public virtual string Name { get; set; }
    }
}
