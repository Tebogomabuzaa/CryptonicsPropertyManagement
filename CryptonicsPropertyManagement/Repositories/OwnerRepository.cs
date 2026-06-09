using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class OwnerRepository
    {
        private readonly DatabaseHelper _db;

        public OwnerRepository(DatabaseHelper db) => _db = db;

        public async Task<List<Owner>> GetAllAsync()
        {
            var list = new List<Owner>();
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM Owners", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(MapOwner(reader));
                }
            }
            return list;
        }

        public async Task<Owner> GetByIdAsync(int id)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("SELECT * FROM Owners WHERE OwnerID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            return MapOwner(reader);
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Owner owner)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"INSERT INTO Owners (FirstName, LastName, EmailAddress, PhoneNumber) 
                            VALUES (?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@first", owner.FirstName);
                    cmd.Parameters.AddWithValue("@last", owner.LastName);
                    cmd.Parameters.AddWithValue("@email", owner.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", owner.ContactNumber ?? string.Empty);
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
            }
        }

        public async Task UpdateAsync(Owner owner)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                var sql = @"UPDATE Owners SET FirstName = ?, LastName = ?, EmailAddress = ?, PhoneNumber = ? 
                            WHERE OwnerID = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@first", owner.FirstName);
                    cmd.Parameters.AddWithValue("@last", owner.LastName);
                    cmd.Parameters.AddWithValue("@email", owner.EmailAddress);
                    cmd.Parameters.AddWithValue("@phone", owner.ContactNumber ?? string.Empty);
                    cmd.Parameters.AddWithValue("@id", owner.OwnerID);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int ownerId)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();
                using (var cmd = new OleDbCommand("DELETE FROM Owners WHERE OwnerID = ?", conn))
                {
                    cmd.Parameters.AddWithValue("@id", ownerId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static Owner MapOwner(DbDataReader reader)
        {
            var owner = new Owner
            {
                OwnerID = Convert.ToInt32(reader["OwnerID"]),
                FirstName = Convert.ToString(reader["FirstName"]),
                LastName = Convert.ToString(reader["LastName"]),
                EmailAddress = Convert.ToString(reader["EmailAddress"])
            };

            if (reader.HasColumn("PhoneNumber"))
                owner.ContactNumber = Convert.ToString(reader["PhoneNumber"]);
            else if (reader.HasColumn("ContactNumber"))
                owner.ContactNumber = Convert.ToString(reader["ContactNumber"]);

            if (reader.HasColumn("BankAccountDetails"))
                owner.BankAccountDetails = Convert.ToString(reader["BankAccountDetails"]);

            return owner;
        }
    }
}
