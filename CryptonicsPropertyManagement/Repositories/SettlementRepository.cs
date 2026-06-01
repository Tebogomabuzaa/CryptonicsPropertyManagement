// Repositories/SettlementRepository.cs
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    // Acts as the financial ledger for the system, storing split-payment transactions. Designed as an append-only structure for audit integrity.
    public class SettlementRepository
    {
        private readonly DatabaseHelper _db;

        public SettlementRepository(DatabaseHelper db) => _db = db;

        // Reads the full ledger history of all settlements processed by the system.
        public async Task<List<Settlement>> GetAllAsync()
        {
            var list = new List<Settlement>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM Settlements", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Settlement
                        {
                            SettlementID = Convert.ToInt32(reader["SettlementID"]),
                            LeaseID = Convert.ToInt32(reader["LeaseID"]),
                            GrossRentAmount = Convert.ToDecimal(reader["GrossRentAmount"]),
                            MaintenanceCosts = Convert.ToDecimal(reader["MaintenanceCosts"]),
                            SettlementDate = Convert.ToDateTime(reader["SettlementDate"]),
                            SettlementStatus = Convert.ToString(reader["SettlementStatus"]),

                            // Stores the generated fake crypto link so we can review it later
                            CryptoInvoiceLink = Convert.ToString(reader["CryptoInvoiceLink"])
                        });
                    }
                }
            }
            return list;
        }

        // Archives a new financial settlement into the database after the business logic layer calculates the Atlas management fee and owner payouts.
        public async Task SaveSettlementAsync(Settlement settlement)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                var sql = @"INSERT INTO Settlements (LeaseID, GrossRentAmount, MaintenanceCosts, SettlementDate, SettlementStatus, CryptoInvoiceLink) 
                            VALUES (?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@lease", settlement.LeaseID);
                    cmd.Parameters.AddWithValue("@gross", settlement.GrossRentAmount);
                    cmd.Parameters.AddWithValue("@maint", settlement.MaintenanceCosts);
                    cmd.Parameters.AddWithValue("@date", settlement.SettlementDate);
                    cmd.Parameters.AddWithValue("@status", settlement.SettlementStatus);

                    // Passing the uniquely generated simulated invoice link to the database
                    cmd.Parameters.AddWithValue("@link", settlement.CryptoInvoiceLink);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}