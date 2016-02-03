using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoGalery2.Core;
using PhotoGalery2.Core.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using PhotoGalery2.Core.Implementation.Naive;

namespace PhotoGalery2.Test
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
