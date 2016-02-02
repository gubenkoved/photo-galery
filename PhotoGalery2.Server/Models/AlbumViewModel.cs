using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    [DataContract]
    public class AlbumViewModel
    {
        [DataMember]
        public string Name { get; set; }

        public static AlbumViewModel CreateFor(Album album)
        {
            return new AlbumViewModel()
            {
                Name = album.Name,
            };
        }
    }
}