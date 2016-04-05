using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoGallery2.Server
{
    /// <summary>
    /// Container to access IoC without injection
    /// </summary>
    public static class IoC
    {
        private static UnityContainer _container;

        public static UnityContainer Container
        {
            get { return _container; }
            set
            {
                if (_container != null)
                {
                    throw new InvalidOperationException("Container is already there");
                }

                _container = value;
            }
        }

        public static T Build<T>()
        {
            return _container.Resolve<T>();
        }
    }
}