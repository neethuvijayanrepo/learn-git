using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Utilities.Dependency.DependencyInterfaces;
using Unity;
using Unity.Lifetime;

namespace TestProject.Utilities.Dependency.DependencyLogic
{
    /// <summary>
    /// The dependency registrar logic
    /// </summary>
    internal class DependencyRegistrar : IDependencyRegistrar
    {
        private readonly IUnityContainer _container;
        private readonly Type _lifeTimeType;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="container">The <see cref="IUnityContainer"/> instance.</param>
        /// <param name="lifeTimeType">The life time type of <see cref="ITypeLifetimeManager"/> kind.</param>
        public DependencyRegistrar(IUnityContainer container, Type lifeTimeType)
        {
            this._container = container;
            this._lifeTimeType = lifeTimeType;
        }

        /// <summary>
        /// To register dependency with default lifetime.
        /// </summary>
        /// <typeparam name="TFrom">Dependency from type</typeparam>
        /// <typeparam name="TTo">Dependency to type</typeparam>
        public void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            this._container.RegisterType<TFrom, TTo>((ITypeLifetimeManager) Activator.CreateInstance(_lifeTimeType));
        }
    }
}
