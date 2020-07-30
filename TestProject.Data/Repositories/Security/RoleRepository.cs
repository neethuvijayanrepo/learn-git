using TestProject.Data.DataInterfaces.Common;
using TestProject.Data.DataInterfaces.Security;
using TestProject.Data.DataObjects;
using TestProject.Data.Repositories.Common;
using TestProject.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Data.Repositories.Security
{
        internal sealed class RoleRepository : TestProjectRepository<Role>, IRoleRepository
        {
            private readonly ITestProjectContext _context;

            /// <summary>
            /// The repository
            /// </summary>
            /// <param name="context">The DB context instance</param>
            public RoleRepository(ITestProjectContext context) : base(context)
            {
                _context = context;
            }

             /// <summary>
             /// Gets all active roles
             /// </summary>
             /// <returns>Collection of <see cref="Role"/> objects.</returns>
             public IEnumerable<Role> GetActiveRoles()
             {
                 return Get(x => x.Status == (int)EntityStatus.Active);
             }
       }
}
