using Grand.Core.Plugins;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grand.Core.Infrastructure
{
    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }
        static AppDomain()
        {
            CurrentDomain = new AppDomain();

        }

        private List<Assembly> assemblies;

        public Assembly[] GetAssemblies()
        {
            if (assemblies == null)
            {
                assemblies = new List<Assembly>();
                var dependencies = DependencyContext.Default.RuntimeLibraries;
                foreach (var library in dependencies)
                {
                    if (IsCandidateCompilationLibrary(library))
                    {
                        var assembly = Assembly.Load(new AssemblyName(library.Name));
                        assemblies.Add(assembly);
                    }
                }
                var plugins = PluginManager.ReferencedPlugins.ToList();
                foreach (var item in plugins)
                {
                    assemblies.Add(item.ReferencedAssembly);
                }
            }
            return assemblies.ToArray();
        }

        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            return compilationLibrary.Name == ("Grand")
                || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith("Grand"));
        }


    }
}
