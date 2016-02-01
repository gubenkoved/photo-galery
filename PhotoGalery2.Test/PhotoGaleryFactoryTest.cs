using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoGalery2.Core;

namespace PhotoGalery2.Test
{
    [DeploymentItem("TestData")]
    [TestClass]
    public abstract class PhotoGaleryFactoryTest
    {
        private PhotoGaleryFactory _factory;

        public abstract PhotoGaleryFactory CreateFactoryToTest();


        [TestInitialize]
        public void Init()
        {
            _factory = CreateFactoryToTest();

            if (_factory == null)
            {
                throw new InvalidOperationException();
            }
        }

        [TestMethod]
        public void GetAlbumsTest()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var albums = metadataProvider.GetAlbums();

            Assert.IsNotNull(albums);
            Assert.IsTrue(albums.Count() == 2);
        }
    }
}
