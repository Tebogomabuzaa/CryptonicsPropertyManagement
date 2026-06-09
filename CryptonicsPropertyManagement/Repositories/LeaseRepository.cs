using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class LeaseRepository
    {
        private readonly DatabaseHelper _db;

        public LeaseRepository(DatabaseHelper db) => _db = db;

        public async Task<List<LeaseAgreement>> GetAllAsync()
        {
            var list = new List<LeaseAgreement>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"SELECT l.*, p.PhysicalAddress AS PropertyAddress, 
                            t.FirstName & ' ' & t.LastName AS TenantName,
                            m.FirstName & ' ' & m.LastName AS ManagerName
                            FROM ((LeaseAgreements l
                            LEFT JOIN Properties p ON l.PropertyID = p.PropertyID)
                            LEFT JOIN Tenants t ON l.TenantID = t.TenantID)
                            LEFT JOIN PropertyManagers m ON l.ManagerID = m.ManagerID";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(MapLease(reader, includeJoins: true));
                }
            }
            return list;
        }

        public async Task<LeaseAgreement> GetByIdAsync(int id)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM LeaseAgreements WHERE LeaseID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapLease(reader, includeJoins: false);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(LeaseAgreement lease)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"INSERT INTO LeaseAgreements (PropertyID, TenantID, ManagerID, LeaseStartDate, LeaseEndDate, MonthlyRent, LeaseStatus) 
                            VALUES (?, ?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@prop", lease.PropertyID);
                    cmd.Parameters.AddWithValue("@tenant", lease.TenantID);
                    cmd.Parameters.AddWithValue("@mgr", lease.ManagerID);
                    cmd.Parameters.AddWithValue("@start", lease.StartDate);
                    cmd.Parameters.AddWithValue("@end", lease.EndDate);
                    cmd.Parameters.AddWithValue("@rent", lease.RentAmount);
                    cmd.Parameters.AddWithValue("@status", lease.LeaseStatus ?? "Active");
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(LeaseAgreement lease)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"UPDATE LeaseAgreements SET PropertyID = ?, TenantID = ?, ManagerID = ?, 
                            LeaseStartDate = ?, LeaseEndDate = ?, MonthlyRent = ?, LeaseStatus = ? 
                            WHERE LeaseID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@prop", lease.PropertyID);
                    cmd.Parameters.AddWithValue("@tenant", lease.TenantID);
                    cmd.Parameters.AddWithValue("@mgr", lease.ManagerID);
                    cmd.Parameters.AddWithValue("@start", lease.StartDate);
                    cmd.Parameters.AddWithValue("@end", lease.EndDate);
                    cmd.Parameters.AddWithValue("@rent", lease.RentAmount);
                    cmd.Parameters.AddWithValue("@status", lease.LeaseStatus);
                    cmd.Parameters.AddWithValue("@id", lease.LeaseID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int leaseId)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("DELETE FROM LeaseAgreements WHERE LeaseID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", leaseId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static LeaseAgreement MapLease(DbDataReader reader, bool includeJoins)
        {
            var lease = new LeaseAgreement
            {
                LeaseID = Convert.ToInt32(reader["LeaseID"]),
                PropertyID = Convert.ToInt32(reader["PropertyID"]),
                TenantID = Convert.ToInt32(reader["TenantID"]),
                ManagerID = Convert.ToInt32(reader["ManagerID"]),
                StartDate = ReadDate(reader, "LeaseStartDate", "StartDate"),
                EndDate = ReadDate(reader, "LeaseEndDate", "EndDate"),
                RentAmount = ReadDecimal(reader, "MonthlyRent", "RentAmount"),
                LeaseStatus = Convert.ToString(reader["LeaseStatus"])
            };

            if (includeJoins)
            {
                if (reader.HasColumn("PropertyAddress"))
                    lease.PropertyAddress = Convert.ToString(reader["PropertyAddress"]);
                if (reader.HasColumn("TenantName"))
                    lease.TenantName = Convert.ToString(reader["TenantName"]);
                if (reader.HasColumn("ManagerName"))
                    lease.ManagerName = Convert.ToString(reader["ManagerName"]);
            }

            return lease;
        }

        private static DateTime ReadDate(DbDataReader reader, string primary, string fallback)
        {
            if (reader.HasColumn(primary))
                return Convert.ToDateTime(reader[primary]);
            return Convert.ToDateTime(reader[fallback]);
        }

        private static decimal ReadDecimal(DbDataReader reader, string primary, string fallback)
        {
            if (reader.HasColumn(primary))
                return Convert.ToDecimal(reader[primary]);
            return Convert.ToDecimal(reader[fallback]);
        }
    }
}
