using PhotoGallery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGallery2.Server.Models
{
    public class AlbumContentItemViewModel : AlbumItemViewModel,
         IViewModelFilledInByModel<AlbumContentItem>
    {
        [DataMember]
        public Uri ThumbUrl { get; set; }

        [DataMember]
        public int? OrigWidth { get; set; }

        [DataMember]
        public int? OrigHeight { get; set; }

        public void FillBy(AlbumContentItem model)
        {
            base.FillBy(model);

            OrigWidth = model.BasicMetadata?.OrigSize.Width;
            OrigHeight = model.BasicMetadata?.OrigSize.Height;
        }
    }
}