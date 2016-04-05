using PhotoGallery2.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace PhotoGallery2.Server
{
    public static class ViewModelHelper
    {
        public static TViewModel FillBy2<TViewModel, TModel>(this TViewModel viewModel, TModel model)
            where TViewModel : IViewModelFilledInByModel<TModel>
        {
            viewModel.FillBy(model);

            return viewModel;
        }
    }
}