using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation.Naive
{
    public class NaiveMetadataProvider : MetadataProvider
    {
        public IEnumerable<string> Extensions { get; set; }
        public string RootPath { get; private set; }

        public NaiveMetadataProvider(string rootPath, IEnumerable<string> extensions)
        {
            if (!System.IO.Directory.Exists(rootPath))
            {
                throw new InvalidOperationException($"Directory '{rootPath}' doesn't exist");
            }

            RootPath = rootPath;
            Extensions = extensions;
        }
        
        public override Album GetRoot()
        {
            var album = new Album()
            {
                Id = Album.RootAlbumId,
                Name = Album.RootAlbumId,
                Items = GetItemsRecoursive(RootPath),
            };

            return album;
        }

        public override void PrepareAlbum(Album album)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<AlbumItem> GetItemsRecoursive(string currentDir)
        {
            string[] subDirs = System.IO.Directory.GetDirectories(currentDir);

            foreach (var subDir in subDirs)
            {
                var dirInfo = new System.IO.DirectoryInfo(subDir);

                var subItems = GetItemsRecoursive(subDir);

                yield return new NaiveAlbum()
                {
                    Id = dirInfo.Name,
                    Name = dirInfo.Name,
                    Items = subItems,
                    PhysicalDir = subDir,
                };
            }

            foreach(var photo in GetPhotosStraghtIn(currentDir))
            {
                yield return photo;
            }
        }

        private IEnumerable<Photo> GetPhotosStraghtIn(string dir)
        {
            foreach(var filePath in System.IO.Directory
                .EnumerateFiles(dir)
                .Where(file => Extensions.Any(ext => file.ToLower().EndsWith(ext)))
                .ToList())
            {
                var fileInfo = new System.IO.FileInfo(filePath);

                yield return new Photo()
                {
                    Id = fileInfo.Name,
                    Name = fileInfo.Name,
                };
            }
        }
    }
}
