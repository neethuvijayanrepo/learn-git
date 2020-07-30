namespace Prism.Science.Business.BusinessObjects.Security
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Business object for user details.
    /// </summary>
    public class UserDto
    {
        public short UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public DateTime DC { get; set; }
        public DateTime LU { get; set; }
        public DateTime? DD { get; set; }
        public int? UID { get; set; }
        public int Active { get; set; }
        public string Domain { get; set; }

    }
}
