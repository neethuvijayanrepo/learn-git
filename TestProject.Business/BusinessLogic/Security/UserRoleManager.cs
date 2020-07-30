using AutoMapper;
using NLog;
using TestProject.Business.BusinessInterfaces.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using TestProject.Business.BusinessObjects.Common;
using TestProject.Business.BusinessObjects.Security;
using TestProject.Utilities.Common;
using TestProject.Data.DataObjects;
using TestProject.Data.DataInterfaces.Security;
using TestProject.Utilities.Extensions;

namespace TestProject.Business.BusinessLogic.Security
{
    /// <summary>
    /// Manager for user role module.
    /// </summary>
    internal class UserRoleManager : IUserRoleManager
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        /// <summary>
        /// The manager
        /// </summary>
        /// <param name="userRoleRepository"><see cref="IUserRoleRepository"/> instance.</param>
        /// <param name="mapper"><see cref="IMapper"/> instance.</param>
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        /// <param name="qcallocationRepository"></param>

        public UserRoleManager(IMapper mapper, IUserRoleRepository userRoleRepository, ILogger logger)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
            _logger = logger;
        }

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
        //public PagedTestProjectResponse<List<PagedUserRoleDto>> GetPagedUserRoleList(string resource, string role,
        //    string sortColumn, TestProjectSortOrder sortDirection = TestProjectSortOrder.ASC, int pageSize = Constants.DefaultListPageSize,
        //    int page = Constants.DefaultListPage)
        //{
        //    PagedTestProjectResponse<List<PagedUserRoleDto>> response = new PagedTestProjectResponse<List<PagedUserRoleDto>>();

        //    try
        //    {
        //        IEnumerable<PagedUserRole> dbData = _userRoleRepository.GetPagedUserRoleList(
        //        resource, role, pageSize, page, sortColumn,
        //            sortDirection.ToString());

        //        if (dbData == null || !dbData.Any())
        //        {
        //            response.Message = EnumExtensions.DisplayName(Messages.UserRoleMessages.MsgErrorUserRoleListRead); 
        //            return response;
        //        }

        //        response.Output = _mapper.Map<List<PagedUserRoleDto>>(dbData);

        //        var firstRow = dbData.FirstOrDefault();
        //        if (firstRow != null)
        //        {
        //            response.CurrentPage = firstRow.CurrentPage;
        //            response.PageSize = pageSize;
        //            response.TotalRows = firstRow.TotalRows;
        //        }

        //        response.Status = ExecutionStatus.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //        response.Message = EnumExtensions.DisplayName(Messages.CommonMessages.MsgErrorOccured);
        //    }

        //    return response;
        //}

        /// <summary>
        /// To get user role details by using userroleid.
        /// </summary>
        /// <param name="userRoleID">Id of userRole which is to be fetched</param>
        /// <returns>TestProjectResponse object of <see cref="UserRoleDto"/> type.</returns>
        public TestProjectResponse<UserRoleDto> GetByUserRoleId(short userRoleID)
        {
            TestProjectResponse<UserRoleDto> response = new TestProjectResponse<UserRoleDto>();

            try
            {
                var userrole = _userRoleRepository.GetById(userRoleID);
                if (userrole == null)
                {
                    response.Message = EnumExtensions.DisplayName(Messages.UserRoleMessages.MsgErrorUserRoleDataNotFound);
                    return response;
                }

                response.Output = _mapper.Map<UserRoleDto>(userrole);
                response.Status = ExecutionStatus.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.Message = EnumExtensions.DisplayName(Messages.CommonMessages.MsgErrorOccured);
            }

            return response;
        }

        /// <summary>
        /// To update user role details.
        /// </summary>
        /// <param name="userRoleData"><see cref="UserRoleDto"/> object with user role details</param>
        /// <returns>TestProjectResponse of type <see cref="UserRoleDto"/></returns>
        //public TestProjectResponse<UserRoleDto> Update(UserRoleDto userRoleData)
        //{
        //    TestProjectResponse<UserRoleDto> response = new TestProjectResponse<UserRoleDto>();

        //    try
        //    {
        //        if (_qcallocationRepository.IsQCAllocatedForThisUser(userRoleData.UserID))
        //        {
        //            response.Message = EnumExtensions.DisplayName(Messages.UserRoleMessages.MsgErrorQCAllocatedForThisUser);
        //            return response;
        //        }

        //        UserRole userRoleDbData = _userRoleRepository.GetById(userRoleData.UserRoleID);

        //        if (userRoleDbData == null)
        //        {
        //            response.Message = EnumExtensions.DisplayName(Messages.UserRoleMessages.MsgErrorRoleAllocationDataNotFound);
        //            return response;
        //        }
                
        //        DateTime currentTime = DateTime.Now;
        //        userRoleDbData.RoleID = userRoleData.RoleID;
        //        userRoleDbData.LU = currentTime;
        //        userRoleDbData.UILU = userRoleData.UILU;

        //        response.Output = _mapper.Map<UserRoleDto>(_userRoleRepository.Update(userRoleDbData));
        //        if (response.Output != null)
        //        {
        //            response.Message = EnumExtensions.DisplayName(Messages.UserRoleMessages.MsgInfoRoleAllocationUpdated);
        //            response.Status = ExecutionStatus.Success;
        //            return response;
        //        }
        //        else
        //        {
        //            response.Message = EnumExtensions.DisplayName(Messages.UserRoleMessages.MsgErrorRoleUpdateFailed);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //        response.Message = EnumExtensions.DisplayName(Messages.CommonMessages.MsgErrorOccured);
        //    }

        //    return response;
        //}

    }
}
