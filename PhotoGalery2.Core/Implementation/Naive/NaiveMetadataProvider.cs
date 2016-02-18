using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation.Naive
{
    public class NaiveMetadataProvider : MetadataProvider
    {
        private static ILog _log = LogManager.GetLogger<ImageMethods>();
        private static MemoryCache _metadataCache = new MemoryCache("NaiveMetadataProvider.MetadataCache");

        private TimeSpan _metadataCacheTTL = TimeSpan.FromMinutes(60);

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
            var rootAlbum = new NaiveAlbum()
            {
                Id = Album.RootAlbumId,
                Name = Album.RootAlbumId,
                PhysicalDir = RootPath,
            };

            int dirsTraversed = 0;
            int contentItemsTraversed = 0;

            _log.Info(x => x("Populating items recursively in {0}", RootPath));

            rootAlbum.Items = GetItemsRecoursive(rootAlbum, RootPath, ref dirsTraversed, ref contentItemsTraversed);

            _log.Info(x => x("Traversed {0} directories and {1} content files", dirsTraversed, contentItemsTraversed));

            return rootAlbum;
        }

        private IEnumerable<AlbumItem> GetItemsRecoursive(Album currentAlbum, string currentDir,
            ref int dirsTraversed, ref int contentItemsTraversed)
        {
            dirsTraversed += 1;

            var result = new List<AlbumItem>();

            string[] subDirs = System.IO.Directory.GetDirectories(currentDir);

            foreach (var subDir in subDirs)
            {
                var dirInfo = new System.IO.DirectoryInfo(subDir);

                var subAlbum = new NaiveAlbum()
                {
                    Id = dirInfo.Name,
                    Name = dirInfo.Name,
                    ParentAlbum = currentAlbum,
                    PhysicalDir = subDir,
                };

                subAlbum.Items = GetItemsRecoursive(subAlbum, subDir, ref dirsTraversed, ref contentItemsTraversed);

                result.Add(subAlbum);
            }

            foreach(var photo in GetPhotosStraghtIn(currentDir))
            {
                photo.ParentAlbum = currentAlbum;

                contentItemsTraversed += 1;

                result.Add(photo);
            }

            return result;
        }

        private IEnumerable<Photo> GetPhotosStraghtIn(string dir)
        {
            var result = new List<Photo>();

            foreach(var filePath in System.IO.Directory
                .EnumerateFiles(dir)
                .Where(file => Extensions.Any(ext => file.ToLower().EndsWith(ext)))
                .ToList())
            {
                var fileInfo = new System.IO.FileInfo(filePath);

                result.Add(new NaivePhoto()
                {
                    Id = fileInfo.Name,
                    Name = fileInfo.Name,
                    LazyMetadata = new Lazy<IEnumerable<IMetadata>>(() => PopulateMetadataFor(filePath)),
                });
            }

            return result;
        }

        private IEnumerable<IMetadata> PopulateMetadataFor(string filePath)
        {
            var basicMedatdata = _metadataCache.Get(filePath) as BasicMetadata;
            if (basicMedatdata == null)
            {
                basicMedatdata = ImageMethods.GetBasicMetadata(filePath);
                _metadataCache.Add(filePath, basicMedatdata, DateTimeOffset.UtcNow.Add(_metadataCacheTTL));
            }

            var contentItemMetadata = new List<IMetadata>()
            {
                basicMedatdata,
            };

            return contentItemMetadata;
        }
    }
}
