// Repositories/LeaseRepository.cs
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    // The central hub data layer connecting Properties, Tenants, and Managers together.
    public class LeaseRepository
    {
        private readonly DatabaseHelper _db;

        public LeaseRepository(DatabaseHelper db) => _db = db;

        // Retrieves all leases and executes a complex 4-way join to grab the human-readable names for the Property, Tenant, and Manager associated with each lease.
        public async Task<List<LeaseAgreement>> GetAllAsync()
        {
            var list = new List<LeaseAgreement>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                // MS Access requires parentheses around multiple JOIN clauses. We alias the tables (e.g., 'LeaseAgreements l') to keep the code readable.
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
                    {
                        list.Add(new LeaseAgreement
                        {
                            LeaseID = Convert.ToInt32(reader["LeaseID"]),
                            PropertyID = Convert.ToInt32(reader["PropertyID"]),
                            TenantID = Convert.ToInt32(reader["TenantID"]),
                            ManagerID = Convert.ToInt32(reader["ManagerID"]),
                            StartDate = Convert.ToDateTime(reader["StartDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"]),
                            RentAmount = Convert.ToDecimal(reader["RentAmount"]),
                            DepositAmount = Convert.ToDecimal(reader["DepositAmount"]),
                            LeaseStatus = Convert.ToString(reader["LeaseStatus"]),

                            // These properties are populated strictly for the UI to consume
                            PropertyAddress = Convert.ToString(reader["PropertyAddress"]),
                            TenantName = Convert.ToString(reader["TenantName"]),
                            ManagerName = Convert.ToString(reader["ManagerName"])
                        });
                    }
                }
            }
            return list;
        }
    }
}