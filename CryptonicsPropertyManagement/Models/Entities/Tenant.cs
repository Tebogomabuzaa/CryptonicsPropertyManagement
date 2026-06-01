using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Models.Entities
{
    public class Tenant
    {
        public int TenantID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PassportIDNumber { get; set; }
        public string Nationality { get; set; }
        public decimal DeclaredMonthlyIncome { get; set; }
        public string VerificationStatus { get; set; } = "Pending";

        // A helpful extra property that doesn't exist in the database,
        // but makes displaying the name on your UI much easier:
        public string FullName => $"{FirstName} {LastName}";
    }
}
