using TestProject.Data.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Data.DataInterfaces.Security
{
    /// <summary>
    /// Contract for role repository.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Gets all active roles
        /// </summary>
        /// <returns>Collection of <see cref="Role"/> objects.</returns>
        IEnumerable<Role> GetActiveRoles();
    }


}
