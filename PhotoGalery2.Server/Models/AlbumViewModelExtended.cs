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

            foreach (var albumItem in model.Items)
            {
                if (albumItem is AlbumContentItem)
                {
                    var contentItemVM = new AlbumContentItemViewModel().FillBy2(albumItem as AlbumContentItem);

                    ContentItems.Add(contentItemVM);
                }
                else if (albumItem is Album)
                {
                    var albumVM = new AlbumViewModel().FillBy2(albumItem as Album);

                    albumVM.Url = AlbumPathHelper.ConstructAlbumPathFor(albumItem as Album);

                    AlbumItems.Add(albumVM);
                }
            }
        }
    }
}