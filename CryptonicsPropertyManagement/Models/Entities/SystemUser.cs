using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Models.Entities
{
    public class SystemUser
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string UserRole { get; set; }
    }
}
