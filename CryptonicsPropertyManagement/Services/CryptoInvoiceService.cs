using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Services
{
    public class CryptoInvoiceService
    {
        // Simulates calling the Coinbase Commerce API to generate a unique cryptocurrency payment link.
        public async Task<string> GenerateInvoiceLinkAsync(int leaseId, decimal rentAmount)
        {
            // 1. Simulate a 1.5 second delay connecting to the blockchain payment gateway
            await Task.Delay(1500);

            // 2. Generate a guaranteed unique ID for the invoice URL
            var uniqueId = Guid.NewGuid().ToString();

            // 3. Construct the fake URL to save to the database and display to the user
            var paymentUrl = $"https://pay.cryptonics.app/invoice/{uniqueId}?lease={leaseId}&zar={rentAmount}";

            return paymentUrl;
        }
    }
}
