// Repositories/TenantRepository.cs
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    // Manages the data layer for Tenant profiles and their verification statuses.
    public class TenantRepository
    {
        private readonly DatabaseHelper _db;

        public TenantRepository(DatabaseHelper db)
        {
            _db = db;
        }

        // Retrieves the master list of all registered tenants.
        public async Task<List<Tenant>> GetAllAsync()
        {
            var list = new List<Tenant>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = "SELECT * FROM Tenants";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Tenant
                        {
                            TenantID = Convert.ToInt32(reader["TenantID"]),
                            FirstName = Convert.ToString(reader["FirstName"]),
                            LastName = Convert.ToString(reader["LastName"]),
                            EmailAddress = Convert.ToString(reader["EmailAddress"]),
                            PassportIDNumber = Convert.ToString(reader["PassportIDNumber"]),
                            Nationality = Convert.ToString(reader["Nationality"]),
                            DeclaredMonthlyIncome = Convert.ToDecimal(reader["DeclaredMonthlyIncome"]),
                            VerificationStatus = Convert.ToString(reader["VerificationStatus"])
                        });
                    }
                }
            }
            return list;
        }

        // Specifically updates a tenant's KYC status without touching the rest of their profile. This is called after our internal simulated KYC check is complete.
        public async Task UpdateKycStatusAsync(int tenantId, string status)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = "UPDATE Tenants SET VerificationStatus = ? WHERE TenantID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    // Order matters: @status replaces the first '?', @id replaces the second '?'.
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", tenantId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}