using Microsoft.Practices.Unity;
using PhotoGalery2.Core;
using PhotoGalery2.Core.Implementation;
using PhotoGalery2.Core.Implementation.Naive;
using PhotoGalery2.Server.Common;
using System.Web.Http;
using Unity.WebApi;

namespace PhotoGalery2.Server
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
                    RootDir = @"C:\Users\nrj\Documents\Dropbox\Photos",
                    Extensions = new[] { ".jpg", ".jpeg", ".bmp" },
                    ThumbCacheDir = @"C:\temp\photo-gallery-thumbs-cache"
                }));

            IoC.Container.RegisterType<AlbumItemsPathProvider, DefaultAlbumItemsPathProvider>();
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(IoC.Container);
        }
    }
}