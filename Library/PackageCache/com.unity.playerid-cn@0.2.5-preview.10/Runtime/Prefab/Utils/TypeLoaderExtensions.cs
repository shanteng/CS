using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.PlayerIdentity.UI
{
    internal static class TypeLoaderExtensions
    {
        public static IEnumerable<Type> GetAllTypesWithInterface<T>()
        {
            IEnumerable<Type> ret = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var vendorTypes = asm.GetTypesWithInterface<T>().ToList<Type>();
                ret = ret.Concat(vendorTypes);
            }
            return ret;
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static IEnumerable<Type> GetTypesWithInterface<T>(this Assembly asm)
        {
            var it = typeof(T);
            return from lt in asm.GetLoadableTypes()
                   where it.IsAssignableFrom(lt) && !lt.IsAbstract && !lt.IsInterface
                   select lt;
        }
    }
}