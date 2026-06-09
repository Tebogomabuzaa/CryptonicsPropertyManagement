using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class SettlementRepository
    {
        private readonly DatabaseHelper _db;

        public SettlementRepository(DatabaseHelper db) => _db = db;

        public async Task<List<Settlement>> GetAllAsync()
        {
            var list = new List<Settlement>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"SELECT s.*, p.PhysicalAddress AS LeaseDisplay
                            FROM (Settlements s
                            LEFT JOIN LeaseAgreements l ON s.LeaseID = l.LeaseID)
                            LEFT JOIN Properties p ON l.PropertyID = p.PropertyID";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(MapSettlement(reader));
                }
            }
            return list;
        }

        public async Task<int> AddAsync(Settlement settlement)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"INSERT INTO Settlements (LeaseID, GrossRent, MaintenanceCosts, NetAmount, ManagementFee, OwnerPayout, DaysOccupied, SettlementDate, CryptoInvoiceLink) 
                            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@lease", settlement.LeaseID);
                    cmd.Parameters.AddWithValue("@gross", settlement.GrossRent);
                    cmd.Parameters.AddWithValue("@maint", settlement.MaintenanceCosts);
                    cmd.Parameters.AddWithValue("@net", settlement.NetAmount);
                    cmd.Parameters.AddWithValue("@fee", settlement.ManagementFee);
                    cmd.Parameters.AddWithValue("@payout", settlement.OwnerPayout);
                    cmd.Parameters.AddWithValue("@days", settlement.DaysOccupied);
                    cmd.Parameters.AddWithValue("@date", settlement.SettlementDate);
                    cmd.Parameters.AddWithValue("@link", settlement.CryptoInvoiceLink);
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
            }
        }

        private static Settlement MapSettlement(DbDataReader reader)
        {
            var settlement = new Settlement
            {
                SettlementID = Convert.ToInt32(reader["SettlementID"]),
                LeaseID = Convert.ToInt32(reader["LeaseID"]),
                MaintenanceCosts = Convert.ToDecimal(reader["MaintenanceCosts"]),
                DaysOccupied = Convert.ToInt32(reader["DaysOccupied"]),
                SettlementDate = Convert.ToDateTime(reader["SettlementDate"]),
                CryptoInvoiceLink = Convert.ToString(reader["CryptoInvoiceLink"])
            };

            settlement.GrossRent = ReadDecimal(reader, "GrossRent", "GrossRentAmount");
            settlement.NetAmount = Convert.ToDecimal(reader["NetAmount"]);
            settlement.ManagementFee = Convert.ToDecimal(reader["ManagementFee"]);
            settlement.OwnerPayout = Convert.ToDecimal(reader["OwnerPayout"]);

            if (reader.HasColumn("LeaseDisplay") && reader["LeaseDisplay"] != DBNull.Value)
                settlement.LeaseDisplay = Convert.ToString(reader["LeaseDisplay"]);

            return settlement;
        }

        private static decimal ReadDecimal(DbDataReader reader, string primary, string fallback)
        {
            if (reader.HasColumn(primary))
                return Convert.ToDecimal(reader[primary]);
            if (reader.HasColumn(fallback))
                return Convert.ToDecimal(reader[fallback]);
            return 0;
        }
    }
}
