using TestProject.Business.BusinessObjects.Common;
using TestProject.Business.BusinessObjects.Security;
using System.Collections.Generic;

namespace TestProject.Business.BusinessInterfaces.Security
{
    public interface IRoleManager
    {
        /// <summary>
        /// Gets all active roles
        /// </summary>
        /// <returns><see cref="TestProjectResponse"/> object of <see cref="RoleDto"/> collection type</returns>
        TestProjectResponse<IEnumerable<RoleDto>> GetActiveRoles();
    }
}
