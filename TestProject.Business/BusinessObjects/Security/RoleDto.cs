using TestProject.Business.BusinessObjects.Common;
using System.Collections.Generic;

namespace TestProject.Business.BusinessObjects.Security
{
    public class RoleDto : TestProjectBaseDto
    {
        public short RoleID { get; set; }
        public string RoleName { get; set; }
        public int Status { get; set; }
        public UserDto User { get; set; }
        public UserDto User1 { get; set; }
        public List<UserRoleDto> UserRole { get; set; }

    }
}
