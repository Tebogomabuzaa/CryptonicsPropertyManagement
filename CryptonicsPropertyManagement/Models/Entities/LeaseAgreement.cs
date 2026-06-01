using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Models.Entities
{
    public class LeaseAgreement
    {
        public int LeaseID { get; set; }
        public int PropertyID { get; set; }
        public int TenantID { get; set; }
        public int ManagerID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public string LeaseStatus { get; set; }

        // Navigation properties for the UI displays
        public string PropertyAddress { get; set; }
        public string TenantName { get; set; }
        public string ManagerName { get; set; }
    }
}
