using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoGallery2.Core;
using PhotoGallery2.Core.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using PhotoGallery2.Core.Implementation.Naive;

namespace PhotoGallery2.Test
{
    [TestClass]
    public class NaivePhotoGaleryTest : PhotoGaleryFactoryTest
    {
        public override PhotoGaleryFactory CreateFactoryToTest()
        {
            string absolutePathToRoot = Path.GetFullPath("TestGaleryRoot");

            return new NaivePhotoGaleryFactory(new NaivePhotoGaleryFactory.SettingsGroup()
            {
                RootDir = absolutePathToRoot,
                Extensions = new string[] { ".png", ".jpg" },
            });
        }
    }
}
