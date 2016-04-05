﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    [DataContract]
    public abstract class AlbumItem
    {
        [IgnoreDataMember]
        public Album ParentAlbum { get; set; }

        [DataMember]
        public virtual string Id { get; set; }

        [DataMember]
        public virtual string Name { get; set; }
    }
}
