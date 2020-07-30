using TestProject.Business.BusinessObjects.Common;

namespace TestProject.Business.BusinessObjects.Security
{
    public class UserRoleDto : TestProjectBaseDto
    {
        public short UserRoleID { get; set; }
        public short UserID { get; set; }
        public short RoleID { get; set; }
        public int Status { get; set; }

        public virtual RoleDto Role { get; set; }
        public virtual UserDto User { get; set; }
        public virtual UserDto User1 { get; set; }
        public virtual UserDto User2 { get; set; }
    }
}
