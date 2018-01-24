using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mynt.Extensibility
{
    public static class PluginLoader
    {
        public static IEnumerable<T> Create<T>() where T : class
        {
            var assemblyNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll");

            List<T> results = new List<T>();
            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(assemblyName);
                    var types = assembly.GetTypes().Where(_ => _.IsInstanceOfType(typeof(T)) ||
                        _.GetInterfaces().Contains(typeof(T))).Where(_ => _.GetConstructor(Type.EmptyTypes) != null);
                    results.AddRange(types.Select(_ => (T)Activator.CreateInstance(_)));
                }
                catch (Exception ex)
                {

                }
            }

            return results;
        }
    }
}
