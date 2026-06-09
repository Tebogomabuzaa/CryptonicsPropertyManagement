using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class ManagerRepository
    {
        private readonly DatabaseHelper _db;

        public ManagerRepository(DatabaseHelper db) => _db = db;

        public async Task<List<PropertyManager>> GetAllAsync()
        {
            var list = new List<PropertyManager>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM PropertyManagers", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(MapManager(reader));
                }
            }
            return list;
        }

        public async Task<PropertyManager> GetByIdAsync(int id)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM PropertyManagers WHERE ManagerID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapManager(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(PropertyManager manager)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"INSERT INTO PropertyManagers (FirstName, LastName, EmailAddress) 
                            VALUES (?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@first", manager.FirstName);
                    cmd.Parameters.AddWithValue("@last", manager.LastName);
                    cmd.Parameters.AddWithValue("@email", manager.EmailAddress);
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(PropertyManager manager)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"UPDATE PropertyManagers SET FirstName = ?, LastName = ?, EmailAddress = ? 
                            WHERE ManagerID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@first", manager.FirstName);
                    cmd.Parameters.AddWithValue("@last", manager.LastName);
                    cmd.Parameters.AddWithValue("@email", manager.EmailAddress);
                    cmd.Parameters.AddWithValue("@id", manager.ManagerID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int managerId)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("DELETE FROM PropertyManagers WHERE ManagerID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", managerId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static PropertyManager MapManager(DbDataReader reader)
        {
            var manager = new PropertyManager
            {
                ManagerID = Convert.ToInt32(reader["ManagerID"]),
                FirstName = Convert.ToString(reader["FirstName"]),
                LastName = Convert.ToString(reader["LastName"]),
                EmailAddress = Convert.ToString(reader["EmailAddress"])
            };

            if (reader.HasColumn("ContactNumber"))
                manager.ContactNumber = Convert.ToString(reader["ContactNumber"]);

            return manager;
        }
    }
}
