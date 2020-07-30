namespace TestProject.Utilities.Dependency.DependencyInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to locate dependency type exports.
    /// </summary>
    public interface IDependencyType
    {
        /// <summary>
        /// To initialize dependency registration.
        /// </summary>
        /// <param name="registrar">The instance of <see cref="IDependencyRegistrar"/></param>
        void Initialize(IDependencyRegistrar registrar);
    }
}
