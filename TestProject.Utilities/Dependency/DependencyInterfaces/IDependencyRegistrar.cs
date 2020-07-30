using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Utilities.Dependency.DependencyInterfaces
{
    /// <summary>
    /// Interface for dependency registration.
    /// </summary>
    public interface IDependencyRegistrar
    {
        /// <summary>
        /// To register a dependency type.
        /// </summary>
        /// <typeparam name="TFrom">Dependency from type</typeparam>
        /// <typeparam name="TTo">Dependency to type</typeparam>
        void RegisterType<TFrom, TTo>() where TTo : TFrom;
    }
}
