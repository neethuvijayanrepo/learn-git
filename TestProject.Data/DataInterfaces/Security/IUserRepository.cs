using TestProject.Data.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TestProject.Utilities.Common;

namespace TestProject.Data.DataInterfaces.Security
{
    public interface IUserRepository
    {
        /// <summary>
        /// To create a new user.
        /// </summary>
        /// <param name="user"><see cref="User"/> entity object with details</param>
        /// <returns><see cref="User"/> with created user data.</returns>
        User Create(User user);

        IEnumerable<User> Get(Expression<Func<User, bool>> filter, string[] includeRefs,
            Func<User, object> orderBy, TestProjectSortOrder sortOrder = TestProjectSortOrder.ASC);

        usp_Security_Login_Result ValidateLogin(string userName, string userPassword, string IPAddress);
    }
}
