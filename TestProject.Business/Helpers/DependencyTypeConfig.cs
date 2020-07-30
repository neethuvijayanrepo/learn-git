using TestProject.Utilities.Dependency.DependencyInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using TestProject.Business.BusinessInterfaces.Security;
using TestProject.Business.BusinessLogic.Security;

namespace TestProject.Business.Helpers
{
    [Export(typeof(IDependencyType))]
    public class DependencyTypeConfig : IDependencyType
    {
        public void Initialize(IDependencyRegistrar registrar)
        {
            registrar.RegisterType<IUserManager, UserManager>();
            registrar.RegisterType<IUserRoleManager, UserRoleManager>();
            registrar.RegisterType<IRoleManager, RoleManager>();
        }
    }
}
