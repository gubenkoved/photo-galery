using PhotoGallery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGallery2.Server.Models
{
    [DataContract]
    public class AlbumViewModel : AlbumItemViewModel,
         IViewModelFilledInByModel<Album>
    {
        [DataMember]
        public Uri ThumbUrl { get; set; }

        public virtual void FillBy(Album model)
        {
            base.FillBy(model);
        }
    }
}