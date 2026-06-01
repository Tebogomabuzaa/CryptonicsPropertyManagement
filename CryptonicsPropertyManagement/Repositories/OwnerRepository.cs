// Repositories/OwnerRepository.cs
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    // Manages data retrieval for Atlas Premier Properties investors (Owners).
    public class OwnerRepository
    {
        private readonly DatabaseHelper _db;

        public OwnerRepository(DatabaseHelper db) => _db = db;

        // Reads all owners from the database to populate selection dropdowns.
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
                    {
                        list.Add(new Owner
                        {
                            OwnerID = Convert.ToInt32(reader["OwnerID"]),
                            FirstName = Convert.ToString(reader["FirstName"]),
                            LastName = Convert.ToString(reader["LastName"]),
                            EmailAddress = Convert.ToString(reader["EmailAddress"]),
                            ContactNumber = Convert.ToString(reader["ContactNumber"]),
                            BankAccountDetails = Convert.ToString(reader["BankAccountDetails"])
                        });
                    }
                }
            }
            return list;
        }
    }
}