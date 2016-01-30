using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace PhotoGallery
{
    public static class Settings
    {
        public static string GalleryRoot
        {
            get
            {
                return ConfigurationManager.AppSettings["GalleryRoot"];
            }
        }

        public static string ThumbsRoot
        {
            get
            {
                return ConfigurationManager.AppSettings["ThumbsRoot"];
            }
        }

        public static int ThumbWidth
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["ThumbWidth"]);
            }
        }

        public static int ThumbHeight
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["ThumbHeight"]);
            }
        }

        public static int MaxViewWidth
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["MaxViewWidth"]);
            }
        }

        public static int MaxViewHeight
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["MaxViewHeight"]);
            }
        }

        public static IEnumerable<string> PhotosExtexsions
        {
            get
            {
                return ConfigurationManager.AppSettings["PhotosExtexsions"].Split('|');
            }
        }
    }
}