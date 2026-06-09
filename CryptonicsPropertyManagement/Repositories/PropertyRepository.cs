using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class PropertyRepository
    {
        private readonly DatabaseHelper _db;

        public PropertyRepository(DatabaseHelper db) => _db = db;

        public async Task<List<Property>> GetAllAsync()
        {
            var list = new List<Property>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"SELECT p.*, o.FirstName & ' ' & o.LastName AS OwnerName 
                            FROM Properties p 
                            LEFT JOIN Owners o ON p.OwnerID = o.OwnerID";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(MapProperty(reader));
                }
            }
            return list;
        }

        public async Task<Property> GetByIdAsync(int id)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM Properties WHERE PropertyID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapPropertyRow(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Property property)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                using (var checkCmd = new OleDbCommand(
                    "SELECT COUNT(*) FROM Properties WHERE PhysicalAddress = ?", conn))
                {
                    checkCmd.Parameters.AddWithValue("@addr", property.PhysicalAddress);
                    if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                        throw new InvalidOperationException("A property with this address already exists.");
                }

                var sql = @"INSERT INTO Properties (PropertyDescription, PhysicalAddress, OwnerID, MonthlyRent, VacancyStatus) 
                            VALUES (?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desc", property.PropertyDescription);
                    cmd.Parameters.AddWithValue("@addr", property.PhysicalAddress);
                    cmd.Parameters.AddWithValue("@ownerId", property.OwnerID);
                    cmd.Parameters.AddWithValue("@rent", property.MonthlyRent);
                    cmd.Parameters.AddWithValue("@vacant", property.VacancyStatus);
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Property property)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"UPDATE Properties SET PropertyDescription = ?, MonthlyRent = ?, VacancyStatus = ? 
                            WHERE PropertyID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desc", property.PropertyDescription);
                    cmd.Parameters.AddWithValue("@rent", property.MonthlyRent);
                    cmd.Parameters.AddWithValue("@vacant", property.VacancyStatus);
                    cmd.Parameters.AddWithValue("@id", property.PropertyID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int propertyId)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("DELETE FROM Properties WHERE PropertyID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", propertyId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static Property MapProperty(DbDataReader reader)
        {
            var property = MapPropertyRow(reader);
            if (reader.HasColumn("OwnerName") && reader["OwnerName"] != DBNull.Value)
                property.OwnerName = Convert.ToString(reader["OwnerName"]);
            return property;
        }

        private static Property MapPropertyRow(DbDataReader reader) => new Property
        {
            PropertyID = Convert.ToInt32(reader["PropertyID"]),
            PropertyDescription = Convert.ToString(reader["PropertyDescription"]),
            PhysicalAddress = Convert.ToString(reader["PhysicalAddress"]),
            OwnerID = Convert.ToInt32(reader["OwnerID"]),
            MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
            VacancyStatus = Convert.ToBoolean(reader["VacancyStatus"])
        };
    }
}
