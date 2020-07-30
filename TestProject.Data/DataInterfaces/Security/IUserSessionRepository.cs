namespace TestProject.Data.DataInterfaces.Security
{
    using DataObjects;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;
    using Utilities.Common;

    /// <summary>
    /// Interface for user session repository.
    /// </summary>
    public interface IUserSessionRepository
    {
        /// <summary>
        /// Update the value of LogoutDateTime using sessionGuid
        /// </summary>
        /// <param name="sessionGuid"></param>
        void Logout(Guid sessionGuid);
    }
}
