using Microsoft.Practices.Unity;
using PhotoGalery2.Core;
using PhotoGalery2.Core.Implementation;
using PhotoGalery2.Core.Implementation.Naive;
using System.Web.Http;
using Unity.WebApi;

namespace PhotoGalery2.Server
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<PhotoGaleryFactory, NaivePhotoGaleryFactory>
                (new InjectionConstructor(new NaivePhotoGaleryFactory.SettingsGroup()
                {
                    RootDir = @"C:\Users\nrj\Documents\Dropbox\Photos",
                    Extensions = new[] { ".jpg", ".jpeg", ".bmp" },
                }));
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}