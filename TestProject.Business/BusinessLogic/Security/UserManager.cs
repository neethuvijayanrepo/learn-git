using TestProject.Business.BusinessObjects;
using TestProject.Data.DataObjects;

namespace TestProject.Business.BusinessLogic.Security
{
    using AutoMapper;
    using NLog;
    using BusinessInterfaces;
    using Data.DataInterfaces;
    using System.Data;
    using Utilities.Common;
    using System.Collections.Generic;
    using BusinessObjects.Security;
    using System;
    using BusinessInterfaces.Security;
    using Data.DataInterfaces.Security;
    using BusinessObjects.Common;

    /// <summary>
    /// Manager for user module.
    /// </summary>
    internal class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IUserSessionRepository _userSessionRepository;

        /// <summary>
        /// User manager constructor.
        /// </summary>
        /// <param name="userRepository">Instance of <see cref="IUserRepository"/></param>
        /// <param name="mapper">Instance of <see cref="IMapper"/></param>
        /// <param name="logger">Instance of <see cref="ILogger"/></param>
        /// <param name="userSessionRepository">Instance of <see cref="IUserSessionRepository"/></param>
        public UserManager(IUserRepository userRepository, IMapper mapper, ILogger logger, IUserSessionRepository userSessionRepository)
        {
            this._userRepository = userRepository;
            this._mapper = mapper;
            this._logger = logger;
            this._userSessionRepository = userSessionRepository;
        }

        /// <summary>
        /// handle the login process
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="iPAddress"></param>
        /// <returns></returns>
        public TestProjectResponse<LoginDto> Login(string userName, string password,string iPAddress)
        {
            TestProjectResponse<LoginDto> response = new TestProjectResponse<LoginDto>();
            try
            {
                var loginDbData = _userRepository.ValidateLogin(userName, password, iPAddress);
                if (loginDbData == null || loginDbData.LoginStatus == (int)loginstatus.failure)
                {
                    response.Message = Messages.MSG_NOT_A_TestProject_USER;
                    return response;
                }
                else if(loginDbData.LoginStatus == (int)loginstatus.locked)
                {
                    response.Message = Messages.LOCKED_ACCOUNT;
                    return response;
                }
                else 
                {
                    response.Output = _mapper.Map<LoginDto>(loginDbData);
                    response.Status = ExecutionStatus.Success;
                }
            }
            catch(Exception ex)
            {
                response.Message = Messages.MSG_UNEXPECTED_LOGIN_PROBLEMS;
                _logger.Error(ex);
            }
            return response;
        }

        /// <summary>
        /// handle the logout process
        /// </summary>
        /// <param name="sessionGuid"></param>
        public void Logout(Guid sessionGuid)
        {
            _userSessionRepository.Logout(sessionGuid);
        }
        
    }
}
