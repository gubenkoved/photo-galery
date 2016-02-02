using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation
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
        
        public override IEnumerable<Album> GetAlbums()
        {
            string[] subDirs = System.IO.Directory.GetDirectories(RootPath);

            foreach (var subDir in subDirs)
            {
                var dirInfo = new System.IO.DirectoryInfo(subDir);

                yield return new Album()
                {
                    Id = dirInfo.Name,
                    Name = dirInfo.Name,
                    Items = GetPhotosIn(subDir),
                };
            }
        }

        public override void PrepareAlbum(Album album)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Photo> GetPhotosIn(string dir)
        {
            foreach(var filePath in System.IO.Directory
                .EnumerateFiles(dir)
                .Where(file => Extensions.Any(ext => file.ToLower().EndsWith(ext)))
                .ToList())
            {
                var fileInfo = new System.IO.FileInfo(filePath);

                yield return new Photo()
                {
                    Id = fileInfo.FullName,
                    Name = fileInfo.Name,
                };
            }
        }
    }
}
