using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    public class AlbumContentItemViewModel : AlbumItemViewModel,
         IViewModelFilledInByModel<AlbumContentItem>
    {
        public void FillBy(AlbumContentItem model)
        {
            base.FillBy(model);
        }
    }
}