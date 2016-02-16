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
        private AlbumItemsPathProvider _pathProvider;

        [DataMember]
        public List<AlbumViewModel> AlbumItems { get; set; }

        [DataMember]
        public List<AlbumContentItemViewModel> ContentItems { get; set; }

        [DataMember]
        public Uri ParentUrl { get; set; }

        public AlbumViewModelExtended(AlbumItemsPathProvider pathProvider)
        {
            if (pathProvider == null)
            {
                throw new ArgumentNullException(nameof(pathProvider));
            }

            _pathProvider = pathProvider;

            AlbumItems = new List<AlbumViewModel>();
            ContentItems = new List<AlbumContentItemViewModel>();
        }

        public override void FillBy(Album model)
        {
            base.FillBy(model);

            if (model.ParentAlbum != null)
            {
                ParentUrl = _pathProvider.GetAlbumUri(model.ParentAlbum);
            }

            Url = _pathProvider.GetAlbumUri(model);

            foreach (var albumItem in model.Items)
            {
                if (albumItem is AlbumContentItem)
                {
                    var contentItemVM = new AlbumContentItemViewModel().FillBy2(albumItem as AlbumContentItem);

                    contentItemVM.Url = _pathProvider.GetContentItemUri(albumItem as AlbumContentItem);
                    contentItemVM.ThumUrl = _pathProvider.GetContentItemThumbUri(albumItem as AlbumContentItem, new Size(200, 200));

                    ContentItems.Add(contentItemVM);
                }
                else if (albumItem is Album)
                {
                    var albumVM = new AlbumViewModel().FillBy2(albumItem as Album);

                    albumVM.Url = _pathProvider.GetAlbumUri(albumItem as Album);

                    AlbumItems.Add(albumVM);
                }
            }
        }
    }
}