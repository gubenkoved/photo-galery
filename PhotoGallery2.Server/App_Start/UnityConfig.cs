using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Microsoft.Practices.Unity;
using PhotoGallery2.Core;
using PhotoGallery2.Core.Implementation.Naive;
using PhotoGallery2.Server.Common;
using Unity.WebApi;

namespace PhotoGallery2.Server
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            IoC.Container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            string rootDir = ConfigurationManager.AppSettings["PhotosRootDir"];
            string thumCacheDir = ConfigurationManager.AppSettings["ThumbnailsCacheDir"];
            string[] extensions = ConfigurationManager.AppSettings["Extensions"]
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            IoC.Container.RegisterType<PhotoGaleryFactory, NaivePhotoGaleryFactory>
                (new InjectionConstructor(new NaivePhotoGaleryFactory.SettingsGroup()
                {
                    RootDir       = rootDir,
                    ThumbCacheDir = thumCacheDir,
                    Extensions    = extensions,
                }));

            IoC.Container.RegisterType<AlbumItemsPathProvider, DefaultAlbumItemsPathProvider>();
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(IoC.Container);
        }
    }
}