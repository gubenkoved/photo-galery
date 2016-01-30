using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace PhotoGallery.Models
{
    public class Photo
    {        
        public string FileName { get; set; }
        public string ThumbUri { get; set; }
        public string OriginalUri { get; set; }

        public static string GetThumbFileNameFor(string album, string photoName)
        {
            string id = album + "_" + photoName;

            foreach (var c in "\\/:*?\"<>|")
            {
                id = id.Replace(c, '-');
            }

            return id;
        }
    }
}