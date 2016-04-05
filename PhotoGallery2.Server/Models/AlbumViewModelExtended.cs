using PhotoGallery2.Core;
using PhotoGallery2.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGallery2.Server.Models
{
    [DataContract]
    public class AlbumViewModelExtended : AlbumViewModel,
        IViewModelFilledInByModel<Album>
    {
        private AlbumItemsPathProvider _pathProvider;

        [DataMember]
        public string FullPath { get; set; }

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

            FullPath = _pathProvider.ConstructAlbumPathSegment(model);

            Url = _pathProvider.GetAlbumUri(model);

            foreach (var albumItem in model.Items)
            {
                if (albumItem is AlbumContentItem)
                {
                    var contentItemVM = new AlbumContentItemViewModel().FillBy2(albumItem as AlbumContentItem);

                    contentItemVM.Url = _pathProvider.GetContentItemUri(albumItem as AlbumContentItem);
                    contentItemVM.ThumbUrl = _pathProvider.GetContentItemThumbUri(albumItem as AlbumContentItem);

                    ContentItems.Add(contentItemVM);
                }
                else if (albumItem is Album)
                {
                    var albumVM = new AlbumViewModel().FillBy2(albumItem as Album);

                    albumVM.Url = _pathProvider.GetAlbumUri(albumItem as Album);

                    var someAlbumContentItem = TryFindSomeContentItem(albumItem as Album);

                    if (someAlbumContentItem != null)
                    {
                        albumVM.ThumbUrl = _pathProvider.GetContentItemThumbUri(someAlbumContentItem);
                    }

                    AlbumItems.Add(albumVM);
                }
            }
        }

        private static AlbumContentItem TryFindSomeContentItem(Album album)
        {
            var contentItem = album.Items.OfType<AlbumContentItem>().FirstOrDefault();

            if (contentItem != null)
            {
                return contentItem;
            }

            foreach (var nestAlbum in album.Items.OfType<Album>())
            {
                return TryFindSomeContentItem(nestAlbum);
            }

            return null;
        }
    }
}