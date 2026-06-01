using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Models.Entities
{
    public class Property
    {
        public int PropertyID { get; set; }
        public string PropertyDescription { get; set; }
        public string PhysicalAddress { get; set; }
        public int OwnerID { get; set; }
        public decimal MonthlyRent { get; set; }
        public bool VacancyStatus { get; set; } = true;

        // Navigation property (populated via JOIN in repository)
        public string OwnerName { get; set; } = string.Empty;
    }
}
