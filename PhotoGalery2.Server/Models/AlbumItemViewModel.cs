using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PhotoGalery2.Server.Models
{
    [DataContract]
    public class AlbumItemViewModel
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }


        public static AlbumItemViewModel CreateFor(AlbumItem albumItem)
        {
            return new AlbumItemViewModel()
            {
                Id = albumItem.Id,
                Name = albumItem.Name,
            };
        }
    }
}