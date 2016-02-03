using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    public interface IViewModelFilledInByModel<TModel>
    {
        void FillBy(TModel model);
    }
}