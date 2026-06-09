using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;

namespace CryptonicsPropertyManagement.Services
{
    public class CoHostStats
    {
        public string ManagerName { get; set; }
        public int TotalDays { get; set; }
        public int DaysOccupied { get; set; }
        public decimal OccupancyRate =>
            TotalDays > 0 ? Math.Round((decimal)DaysOccupied / TotalDays * 100, 1) : 0;
    }

    public class ReportService
    {
        private readonly DatabaseHelper _db;

        public ReportService(DatabaseHelper db) => _db = db;

        public async Task<List<CoHostStats>> GetCoHostPerformanceAsync(DateTime start, DateTime end)
        {
            var stats = new List<CoHostStats>();
            int totalDays = (end - start).Days + 1;
            if (totalDays < 1) totalDays = 1;

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql =
                    "SELECT pm.FirstName & ' ' & pm.LastName AS ManagerName, " +
                    "COUNT(*) AS LeaseDays " +
                    "FROM LeaseAgreements la " +
                    "JOIN PropertyManagers pm ON la.ManagerID = pm.ManagerID " +
                    "WHERE la.LeaseStartDate <= ? AND la.LeaseEndDate >= ? " +
                    "GROUP BY pm.FirstName & ' ' & pm.LastName";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@start", start);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            stats.Add(new CoHostStats
                            {
                                ManagerName = Convert.ToString(reader["ManagerName"]),
                                TotalDays = totalDays,
                                DaysOccupied = Convert.ToInt32(reader["LeaseDays"])
                            });
                        }
                    }
                }
            }

            return stats;
        }
    }
}
