using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mynt.Extensibility
{
    public static class PluginLoader
    {
        public static Type GetType<T>(string name) where T : class
        {
            var types = GetTypes<T>();
            return types.SingleOrDefault(_ => _.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<T> Create<T>() where T : class
        {
            var types = GetTypes<T>();
            return types.Select(_ => (T)Activator.CreateInstance(_));
        }

        public static IEnumerable<Type> GetTypes<T>() where T:class
        {
            var assemblyNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*")
                .Where(s => s.EndsWith(".dll") || s.EndsWith(".exe"));
            List<Type> results = new List<Type>();
            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(assemblyName);
                    var types = assembly.GetTypes().Where(_ => _.IsInstanceOfType(typeof(T)) ||
                        _.GetInterfaces().Contains(typeof(T))).Where(_ => _.GetConstructor(Type.EmptyTypes) != null);
                    results.AddRange(types);
                }
                catch (Exception ex)
                {

                }
            }

            return results;
        }
    }
}
