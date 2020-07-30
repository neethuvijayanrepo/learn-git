namespace TestProject.Data.Repositories.Security
{
    using DataObjects;
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Common;
    using DataInterfaces.Common;
    using DataInterfaces.Security;

    /// <summary>
    /// The repository for user session module.
    /// </summary>
    internal sealed class UserSessionRepository : TestProjectRepository<UserAccountSession>, IUserSessionRepository
    {
        private readonly ITestProjectContext _context;

        /// <summary>
        /// The <see cref="UserSessionRepository"/> constructor.
        /// </summary>
        /// <param name="context">The instance of DB Context <see cref="ITestProjectContext"/></param>
        public UserSessionRepository(ITestProjectContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Update the value of LogoutDateTime using instance of DB
        /// </summary>
        /// <param name="sessionGuid">Current session Id</param>
        public void Logout(Guid sessionGuid)
        {
            var session = _context.UserAccountSessions.FirstOrDefault(x => x.SG == sessionGuid);
            if (session == null)
            {
                return;
            }

            session.LogoutDateTime = DateTime.Now;
            Update(session);
        }

    }
}
