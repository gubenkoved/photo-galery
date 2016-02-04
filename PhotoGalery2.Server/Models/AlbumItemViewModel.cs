using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    [DataContract]
    [KnownType(typeof(AlbumViewModel))]
    [KnownType(typeof(AlbumContentItemViewModel))]
    [KnownType(typeof(AlbumViewModelExtended))]
    public class AlbumItemViewModel : IViewModelFilledInByModel<AlbumItem>
    {
        internal Album Parent { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Url { get; set; }

        public void FillBy(AlbumItem model)
        {
            Id = model.Id;
            Name = model.Name;
        }
    }
}