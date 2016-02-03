using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation.Naive
{
    public class NaivePhotoGaleryFactory : PhotoGaleryFactory
    {
        public class SettingsGroup
        {
            public string RootDir { get; set; }

            public IEnumerable<string> Extensions { get; set; }
        }

        public SettingsGroup Settings { get; private set; }

        public NaivePhotoGaleryFactory(SettingsGroup settings)
        {
            Settings = settings;
        }

        public override ContentProvider GetContentProvider()
        {
            return new NaiveContentProvider();
        }

        public override MetadataProvider GetMetadataProvider()
        {
            return new NaiveMetadataProvider(Settings.RootDir, Settings.Extensions);
        }
    }
}
