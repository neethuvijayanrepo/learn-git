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
    internal sealed class UserRoleRepository : TestProjectRepository<UserRole>, IUserRoleRepository
    {
        private readonly ITestProjectContext _context;

        /// <summary>
        /// The repository
        /// </summary>
        /// <param name="context">The DB context instance</param>
        public UserRoleRepository(ITestProjectContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// To get paged list of users with role.
        /// </summary>
        /// <param name="resource">user name filter parameter.</param> 
        /// <param name="role">user role filter parameter.</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <param name="page">Current page index</param>
        /// <param name="sortColumn">Column to be sorted.</param>
        /// <param name="sortDirection">Sort direction</param>
        /// <returns><see cref="PagedUserRole"/> object collection</returns>
        //public IEnumerable<PagedUserRole> GetPagedUserRoleList(string resource, string role, int? pageSize, int? page, string sortColumn, string sortDirection)
        //{
        //    var dbData = _context.usp_UserRole_GetPagedList(resource, role, pageSize, page, sortColumn, sortDirection);
        //    return dbData?.ToList();
        //}

        /// <summary>
        /// To get user role details by using userroleid.
        /// </summary>
        /// <param name="userRoleID">Id of userrole which is to be fetched.</param>
        /// <returns>Entity object or null if its not exists.</returns>
        public override UserRole GetById(object userRoleID)
        {
            string[] includeRefs = { "Role", "User2" };
            var dbData = Get(x => x.UserRoleID == (short)userRoleID && Constants.ViewableEntityStatuses.Contains(x.Status), includeRefs);
            return dbData?.FirstOrDefault();
        }
    }
}
