using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Utilities.Common;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using TestProject.Utilities.Dependency.DependencyInterfaces;
using TestProject.Data.DataInterfaces.Common;
using TestProject.Data.DataInterfaces.Security;
using TestProject.Data.Repositories.Security;
using TestProject.Data.DataObjects;
namespace TestProject.Data.Helpers
{
    [Export(typeof(IDependencyType))]
    public class DependencyTypeConfig : IDependencyType
    {
        public void Initialize(IDependencyRegistrar registrar)
        {
            registrar.RegisterType<ITestProjectContext, TestProjectEntities>();
            registrar.RegisterType<IUserRepository, UserRepository>();
            registrar.RegisterType<IUserSessionRepository, UserSessionRepository>();
            registrar.RegisterType<IUserRoleRepository, UserRoleRepository>();
            registrar.RegisterType<IRoleRepository, RoleRepository>();
        }
    }
}
