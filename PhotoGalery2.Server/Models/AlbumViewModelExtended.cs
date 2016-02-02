using PhotoGalery2.Core;
using PhotoGalery2.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    [DataContract]
    public class AlbumViewModelExtended : AlbumViewModel
    {
        [DataMember]
        public List<AlbumItemViewModel> Items { get; set; }

        public AlbumViewModelExtended()
        {
            Items = new List<AlbumItemViewModel>();
        }

        public new static AlbumViewModelExtended CreateFor(Album album)
        {
            var result = new AlbumViewModelExtended()
            {
                Items = album.Items.Select(x => AlbumItemViewModel.CreateFor(x)).ToList(),
            };

            FieldsCopier.Copy(AlbumViewModel.CreateFor(album), result);

            return result;
        }
    }
}