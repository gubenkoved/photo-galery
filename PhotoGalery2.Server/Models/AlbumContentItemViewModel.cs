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
        public Uri ThumbUrl { get; set; }

        [DataMember]
        public int? Width { get; set; }

        [DataMember]
        public int? Height { get; set; }

        public void FillBy(AlbumContentItem model)
        {
            base.FillBy(model);

            //Width = model.BasicMetadata?.OrigSize.Width;
            //Height = model.BasicMetadata?.OrigSize.Height;
        }
    }
}