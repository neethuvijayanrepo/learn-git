namespace TestProject.Business.BusinessInterfaces.Security
{
    using BusinessObjects;
    using BusinessObjects.Common;
    using BusinessObjects.Security;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for user module business
    /// </summary>
    public interface IUserManager
    {
        TestProjectResponse<LoginDto> Login(string UserName, string Password,string IPAddress);

        void Logout(Guid sessionGuid);
    }
}
