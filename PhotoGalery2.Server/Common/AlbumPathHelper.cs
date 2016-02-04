using PhotoGalery2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGalery2.Server
{
    public class AlbumPathHelper
    {
        public const string Separator = ":";

        public static Album Find(Album album, string path)
        {
            if (path == Album.RootAlbumId)
            {
                return album;
            }

            if (path.StartsWith(Album.RootAlbumId + Separator))
            {
                path = path.Substring((Album.RootAlbumId + Separator).Length);
            }

            string[] albumIds = path.Split(new string[] { Separator }, StringSplitOptions.None)
                .ToArray();

            for (int level = 0; level < albumIds.Length; level++)
            {
                string aid = albumIds[level];

                album = album.Items
                    .OfType<Album>()
                    .SingleOrDefault(a => a.Id == aid);

                if (album == null)
                {
                    return null;
                }
            }

            return album;
        }

        public static string ConstructAlbumPathFor(Album album)
        {
            string path = string.Empty;

            Album current = album;

            while (current != null)
            {
                path = $":{current.Id}" + path;

                current = current.Parent;
            }

            path = Album.RootAlbumId + path;

            return path;
        }

        public static string GetApiRoot()
        {
            return "";
        }
    }
}