using Microsoft.Practices.Unity;
using PhotoGallery2.Core;
using PhotoGallery2.Core.Implementation;
using PhotoGallery2.Core.Implementation.Naive;
using PhotoGallery2.Server.Common;
using System.Web.Http;
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

            IoC.Container.RegisterType<PhotoGaleryFactory, NaivePhotoGaleryFactory>
                (new InjectionConstructor(new NaivePhotoGaleryFactory.SettingsGroup()
                {
                    RootDir = @"D:\Dropbox\Photos",
                    Extensions = new[] { ".jpg", ".jpeg", ".bmp" },
                    ThumbCacheDir = @"C:\temp\photo-gallery-thumbs-cache"
                }));

            IoC.Container.RegisterType<AlbumItemsPathProvider, DefaultAlbumItemsPathProvider>();
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(IoC.Container);
        }
    }
}