using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class TenantRepository
    {
        private readonly DatabaseHelper _db;

        public TenantRepository(DatabaseHelper db) => _db = db;

        public async Task<List<Tenant>> GetAllAsync()
        {
            var list = new List<Tenant>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM Tenants", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(MapTenant(reader));
                }
            }
            return list;
        }

        public async Task<Tenant> GetByIdAsync(int id)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM Tenants WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapTenant(reader);
                    }
                }
            }
            return null;
        }

        public async Task UpdateKycStatusAsync(int tenantId, string status)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand(
                    "UPDATE Tenants SET VerificationStatus = ? WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", tenantId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> AddAsync(Tenant tenant)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                using (var checkCmd = new OleDbCommand(
                    "SELECT COUNT(*) FROM Tenants WHERE PassportIDNumber = ?", conn))
                {
                    checkCmd.Parameters.AddWithValue("@passport", tenant.PassportIDNumber);
                    if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                        throw new InvalidOperationException("A tenant with this passport/ID number already exists.");
                }

                var sql = @"INSERT INTO Tenants (FirstName, LastName, EmailAddress, PassportIDNumber, Nationality, DeclaredMonthlyIncome, VerificationStatus) 
                    VALUES (?, ?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@first", tenant.FirstName);
                    cmd.Parameters.AddWithValue("@last", tenant.LastName);
                    cmd.Parameters.AddWithValue("@email", tenant.EmailAddress);
                    cmd.Parameters.AddWithValue("@passport", tenant.PassportIDNumber);
                    cmd.Parameters.AddWithValue("@nat", tenant.Nationality);
                    cmd.Parameters.AddWithValue("@income", tenant.DeclaredMonthlyIncome);
                    cmd.Parameters.AddWithValue("@status", tenant.VerificationStatus ?? "Pending");
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"UPDATE Tenants SET EmailAddress = ?, DeclaredMonthlyIncome = ?, VerificationStatus = ? 
                            WHERE TenantID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@email", tenant.EmailAddress);
                    cmd.Parameters.AddWithValue("@income", tenant.DeclaredMonthlyIncome);
                    cmd.Parameters.AddWithValue("@status", tenant.VerificationStatus);
                    cmd.Parameters.AddWithValue("@id", tenant.TenantID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int tenantId)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("DELETE FROM Tenants WHERE TenantID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", tenantId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static Tenant MapTenant(DbDataReader reader) => new Tenant
        {
            TenantID = Convert.ToInt32(reader["TenantID"]),
            FirstName = Convert.ToString(reader["FirstName"]),
            LastName = Convert.ToString(reader["LastName"]),
            EmailAddress = Convert.ToString(reader["EmailAddress"]),
            PassportIDNumber = Convert.ToString(reader["PassportIDNumber"]),
            Nationality = Convert.ToString(reader["Nationality"]),
            DeclaredMonthlyIncome = Convert.ToDecimal(reader["DeclaredMonthlyIncome"]),
            VerificationStatus = Convert.ToString(reader["VerificationStatus"])
        };
    }
}
