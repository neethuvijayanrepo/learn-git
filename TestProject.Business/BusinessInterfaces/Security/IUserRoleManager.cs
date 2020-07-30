using System.Collections.Generic;
using TestProject.Business.BusinessObjects.Common;
using TestProject.Business.BusinessObjects.Security;
using TestProject.Utilities.Common;

namespace TestProject.Business.BusinessInterfaces.Security
{
    public interface IUserRoleManager
    {
        /// <summary>
        /// To get paged list of user roles.
        /// </summary>
        /// <param name="resource">user name to be filtered</param>
        /// <param name="role">user role to be filtered</param>
        /// <param name="sortColumn">Column which is to be sorted</param>
        /// <param name="sortDirection">Direction of sort</param>
        /// <param name="pageSize">Listing page size.</param>
        /// <param name="page">The index of page.</param>
        /// <returns>PagedTestProjectResponse object of type <see cref="PagedUserRoleDto"/> collection.</returns>
        //PagedTestProjectResponse<List<PagedUserRoleDto>> GetPagedUserRoleList(string resource, string role,
        //    string sortColumn, TestProjectSortOrder sortDirection = TestProjectSortOrder.ASC,
        //    int pageSize = Constants.DefaultListPageSize,
        //    int page = Constants.DefaultListPage);

        /// <summary>
        /// To get user role details by using userroleid.
        /// </summary>
        /// <param name="userRoleID">Id of userRole which is to be fetched</param>
        /// <returns>TestProjectResponse object of <see cref="UserRoleDto"/> type.</returns>
        TestProjectResponse<UserRoleDto> GetByUserRoleId(short userRoleID);

        /// <summary>
        /// To update user role details.
        /// </summary>
        /// <param name="userRoleData"><see cref="UserRoleDto"/> object with user role details</param>
        /// <returns>TestProjectResponse of type <see cref="UserRoleDto"/></returns>
        //TestProjectResponse<UserRoleDto> Update(UserRoleDto userRoleData);
    }
}
