using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    [DataContract]
    public class BasicMetadata : IMetadata
    {
        public Size OrigSize { get; set; }
    }
}
