using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Business.BusinessObjects.Security
{
    public class LoginDto
    {
        public short UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public int?  LoginStatus { get; set; }
        public string IPAddress { get; set; }
        public  Guid SG { get; set; }
        public short RoleID { get; set; }
        public string RoleName { get; set; }
    }
}
