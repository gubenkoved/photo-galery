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
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public void FillBy(AlbumItem model)
        {
            Id = model.Id;
            Name = model.Name;
        }

        public static AlbumItemViewModel CreateFor(AlbumItem albumItem)
        {
            if (albumItem is AlbumContentItem)
            {
                return new AlbumContentItemViewModel().FillBy2(albumItem as AlbumContentItem);
            } else if (albumItem is Album)
            {
                return new AlbumViewModel().FillBy2(albumItem as Album);
            }

            throw new NotImplementedException();
        }
    }
}