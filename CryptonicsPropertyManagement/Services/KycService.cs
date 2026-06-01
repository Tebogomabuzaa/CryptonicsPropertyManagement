using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptonicsPropertyManagement.Services
{
    public class KycService
    {
        // Simulates reaching out to a third-party Identity Verification API.
        public async Task<string> VerifyTenantAsync(string passportNumber, decimal declaredIncome)
        {
            // 1. Await a 2 second delay to simulate internet/API latency. Because this is async, the web server doesn't freeze while waiting!
            await Task.Delay(2000);

            // 2. Simulated Business Rule: If they typed "REJECT" as their passport, fail them.
            if (passportNumber.ToUpper() == "REJECT")
            {
                return "Declined - Fraudulent ID";
            }

            // 3. Simulated Business Rule: Minimum income threshold
            if (declaredIncome < 5000)
            {
                return "Declined - Insufficient Income";
            }

            // 4. If they pass the checks, simulate a successful API response
            return "Approved";
        }
    }
}
