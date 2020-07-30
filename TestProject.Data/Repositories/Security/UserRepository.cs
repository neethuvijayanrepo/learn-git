using TestProject.Data.DataInterfaces.Common;
using TestProject.Data.DataInterfaces.Security;
using TestProject.Data.DataObjects;
using TestProject.Data.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Data.Repositories.Security
{

    /// <summary>
    /// The repository for user module.
    /// </summary>
    internal sealed class UserRepository : TestProjectRepository<User>, IUserRepository
    {
        private readonly ITestProjectContext _context;

        /// <summary>
        /// The <see cref="UserRepository"/> constructor.
        /// </summary>
        /// <param name="context">The instance of DB Context <see cref="ITestProjectContext"/></param>
        public UserRepository(ITestProjectContext context) : base(context)
        {
            _context = context;
        }

        public usp_Security_Login_Result ValidateLogin(string userName, string userPassword, string IPAddress)
        {
            return _context.usp_Security_Login(userName, userPassword, IPAddress).FirstOrDefault();
        }
    }
}
