using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Models.Entities
{
    public class Owner
    {
        public int OwnerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public string BankAccountDetails { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
