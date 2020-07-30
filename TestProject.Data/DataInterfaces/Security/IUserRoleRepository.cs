using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Data.DataObjects;

namespace TestProject.Data.DataInterfaces.Security
{
    /// <summary>
    /// Contract for user role repository.
    /// </summary>
    public interface IUserRoleRepository
    {
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
       // IEnumerable<PagedUserRole> GetPagedUserRoleList(string resource, string role, int? pageSize, int? page, string sortColumn, string sortDirection);

        /// <summary>
        /// To get user role details by using userroleid.
        /// </summary>
        /// <param name="userRoleID">Id of userrole which is to be fetched.</param>
        /// <returns>Entity object or null if its not exists.</returns>
        UserRole GetById(object userRoleID);

        /// <summary>
        /// To update role.
        /// </summary>
        /// <param name="entity"><see cref="UserRole"/> object with UserRole details.</param>
        /// <returns><see cref="UserRole"/></returns>
        UserRole Update(UserRole entity);
    }
}
