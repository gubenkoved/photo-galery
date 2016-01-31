using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGallery.Models
{
    public class Album
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int PhotoCount { get; set; }
    }
}