using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PhotoGalery2.Server.Common
{
    public static class FieldsCopier
    {
        public static void Copy<TFrom, TTo>(TFrom source, TTo target) where TTo : TFrom
        {
            foreach (var fieldInfo in typeof(TFrom).GetFields(
                BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object value = fieldInfo.GetValue(source);

                fieldInfo.SetValue(target, value);
            }
        }
    }
}