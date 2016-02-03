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
    public class AlbumViewModelExtended : AlbumViewModel,
        IViewModelFilledInByModel<Album>
    {
        [DataMember]
        public List<AlbumViewModel> AlbumItems { get; set; }

        [DataMember]
        public List<AlbumContentItemViewModel> ContentItems { get; set; }

        public AlbumViewModelExtended()
        {
            AlbumItems = new List<AlbumViewModel>();
            ContentItems = new List<AlbumContentItemViewModel>();
        }

        public override void FillBy(Album model)
        {
            base.FillBy(model);

            foreach (var item in model.Items)
            {
                var albumItemViewModel = AlbumItemViewModel.CreateFor(item);

                if (albumItemViewModel is AlbumViewModel)
                {
                    AlbumItems.Add(albumItemViewModel as AlbumViewModel);
                } else if (albumItemViewModel is AlbumContentItemViewModel)
                {
                    ContentItems.Add(albumItemViewModel as AlbumContentItemViewModel);
                }
            }
        }
    }
}