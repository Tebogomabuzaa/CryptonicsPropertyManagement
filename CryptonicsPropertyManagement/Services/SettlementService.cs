// Services/SettlementService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace CryptonicsPropertyManagement.Services
{
    // A small helper object to hold the results of our math
    public class SettlementResult
    {
        public decimal GrossRent { get; set; }
        public decimal MaintenanceCosts { get; set; }
        public decimal NetAmount { get; set; }
        public decimal ManagementFee { get; set; }
        public decimal OwnerPayout { get; set; }
        public int DaysOccupied { get; set; }
        public bool IsProRata => DaysOccupied < 30;
    }

    // Handles all core financial calculations for the Atlas property management rules.
    public class SettlementService
    {
        // The fixed Atlas Management Fee (12%)
        private const decimal ManagementFeeRate = 0.12m;
        private const int DaysPerMonth = 30;

        public SettlementResult CalculatePayout(decimal monthlyRent, decimal maintenanceCosts, int daysOccupied = 30)
        {
            if (daysOccupied < 1 || daysOccupied > 31)
                throw new System.ArgumentOutOfRangeException(nameof(daysOccupied), "Days occupied must be between 1 and 31.");

            if (maintenanceCosts < 0)
                throw new System.ArgumentException("Maintenance costs cannot be negative.");

            var effectiveRent = Math.Round((monthlyRent / DaysPerMonth) * daysOccupied, 2);

            // 1. Subtract maintenance before taking the management fee
            var netAmount = effectiveRent - maintenanceCosts;

            if (netAmount < 0)
                throw new System.InvalidOperationException("Maintenance costs cannot exceed the effective earned rent.");

            // 2. Calculate the 12% cut on the Net Amount
            var managementFee = Math.Round(netAmount * ManagementFeeRate, 2);

            // 3. What the wealthy property investor actually receives
            var ownerPayout = Math.Round(netAmount - managementFee, 2);

            // Return the bundled result, rounded securely to 2 decimal places for ZAR currency
            return new SettlementResult
            {
                GrossRent = effectiveRent, // Uses the pro-rata rent calculation                MaintenanceCosts = Math.Round(maintenanceCosts, 2),
                MaintenanceCosts = maintenanceCosts,
                NetAmount = Math.Round(netAmount, 2),
                ManagementFee = managementFee,
                OwnerPayout = ownerPayout,
                DaysOccupied = daysOccupied // Maps the incoming integer value
            };
        }
    }
}