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
        public void GetRootItemsTest()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var rootItems = metadataProvider.GetRoot().Items;

            Assert.IsNotNull(rootItems);
            Assert.AreEqual(3, rootItems.Count());
            Assert.AreEqual(2, rootItems.OfType<Album>().Count());
            Assert.AreEqual(1, rootItems.OfType<Album>().Count());
        }

        [TestMethod]
        public void GetSubItemsTest()
        {
            var metadataProvider = _factory.GetMetadataProvider();

            var rootItems = metadataProvider.GetRoot().Items;

            var a2Album = rootItems.OfType<Album>()
                .SingleOrDefault(a => a.Name == "A2");

            Assert.IsNotNull(rootItems);
            Assert.IsNotNull(a2Album);

            Assert.AreEqual(2, a2Album.Items.Count());
            Assert.AreEqual(1, a2Album.Items.OfType<Album>().Count());
            Assert.AreEqual(1, a2Album.Items.OfType<Album>().Count());
        }
    }
}
