using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using TestProject.Utilities.Dependency.DependencyInterfaces;
using TestProject.Utilities.Dependency.DependencyLogic;
using Unity;
using Unity.Lifetime;

namespace TestProject.Utilities.Dependency
{
    /// <summary>
    /// The dependency loader.
    /// </summary>
    public static class DependencyLoader
    {
        /// <summary>
        /// To Load dependencies in assemblies of specific path with specified lifetime manager.
        /// </summary>
        /// <param name="container">The <see cref="IUnityContainer"/> instance</param>
        /// <param name="path">Path of assembly collection.</param>
        /// <param name="pattern">Pattern to filter assemblies.</param>
        public static void Load<TLifeTime>(IUnityContainer container, string path, string pattern) where TLifeTime: ITypeLifetimeManager
        {
            try
            {
                var dirCat = new DirectoryCatalog(path, pattern);
                var importDef = BuildImportDefinition();

                using (var aggregateCatalog = new AggregateCatalog())
                {
                    aggregateCatalog.Catalogs.Add(dirCat);
                    using (var compositionContainer = new CompositionContainer(aggregateCatalog))
                    {
                        IEnumerable<Export> exports = compositionContainer.GetExports(importDef);
                        IEnumerable<IDependencyType> dependencyTypes =
                            exports.Select(export => export.Value as IDependencyType).Where(m => m != null);

                        var registrar = new DependencyRegistrar(container, typeof(TLifeTime));
                        foreach (IDependencyType module in dependencyTypes)
                        {
                            module.Initialize(registrar);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                var logger = LogManager.GetCurrentClassLogger();

                foreach (Exception loaderException in typeLoadException.LoaderExceptions)
                {
                    logger.Error(loaderException);
                }

                logger.Error(typeLoadException);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }
        }

        /// <summary>
        /// Definition for dependency type import.
        /// </summary>
        /// <returns><see cref="ImportDefinition"/> instance</returns>
        private static ImportDefinition BuildImportDefinition()
        {
            return new ImportDefinition(
                def => true, typeof(IDependencyType).FullName, ImportCardinality.ZeroOrMore, false, false);
        }
    }
}
