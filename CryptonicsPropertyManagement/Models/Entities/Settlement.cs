using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Models.Entities
{
    public class Settlement
    {
        public int SettlementID { get; set; }
        public int LeaseID { get; set; }
        public decimal GrossRentAmount { get; set; }
        public decimal MaintenanceCosts { get; set; }
        public DateTime SettlementDate { get; set; }
        public string SettlementStatus { get; set; }
        public string CryptoInvoiceLink { get; set; }

        // Calculated fields (handled by your business logic later)
        public decimal NetAmount => GrossRentAmount - MaintenanceCosts;
        public decimal ManagementFee => NetAmount * 0.12m; // The 12% Atlas fee
        public decimal OwnerPayout => NetAmount - ManagementFee;
    }
}
