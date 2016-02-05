using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    public class AlbumContentItemViewModel : AlbumItemViewModel,
         IViewModelFilledInByModel<AlbumContentItem>
    {
        [DataMember]
        public Uri ThumUrl { get; set; }

        public void FillBy(AlbumContentItem model)
        {
            base.FillBy(model);
        }
    }
}