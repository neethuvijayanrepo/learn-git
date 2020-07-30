using AutoMapper;
using NLog;
using TestProject.Business.BusinessInterfaces.Security;
using TestProject.Data.DataInterfaces.Security;
using System;
using System.Collections.Generic;
using TestProject.Business.BusinessObjects.Common;
using TestProject.Business.BusinessObjects.Security;
using TestProject.Utilities.Common;
using TestProject.Utilities.Extensions;

namespace TestProject.Business.BusinessLogic.Security
{
    /// <summary>
    /// Manager for role module.
    /// </summary>
    internal class RoleManager : IRoleManager
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        /// <summary>
        /// The manager
        /// </summary>
        /// <param name="roleRepository"><see cref="IRoleRepository"/> instance.</param>
        /// <param name="mapper"><see cref="IMapper"/> instance.</param>
        /// <param name="logger"><see cref="ILogger"/> instance.</param>
        public RoleManager(IMapper mapper, IRoleRepository roleRepository, ILogger logger)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets all active roles
        /// </summary>
        /// <returns><see cref="TestProjectResponse"/> object of <see cref="RoleDto"/> collection type</returns>
        public TestProjectResponse<IEnumerable<RoleDto>> GetActiveRoles()
        {
            TestProjectResponse<IEnumerable<RoleDto>> response = new TestProjectResponse<IEnumerable<RoleDto>>();

            try
            {
                var dbData = _roleRepository.GetActiveRoles();
                if (dbData == null)
                {
                    response.Message = EnumExtensions.DisplayName(Messages.CommonMessages.MsgErrorOccured);
                    return response;
                }

                response.Output = _mapper.Map<List<RoleDto>>(dbData);
                response.Status = ExecutionStatus.Success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.Message = EnumExtensions.DisplayName(Messages.CommonMessages.MsgErrorOccured);
            }

            return response;
        }
    }
}
