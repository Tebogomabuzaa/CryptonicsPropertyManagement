// Repositories/ManagerRepository.cs
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class ManagerRepository
    {
        // This private variable holds our connection to the database
        private readonly DatabaseHelper _db;

        // Constructor: Dependency Injection passes the DatabaseHelper in automatically
        public ManagerRepository(DatabaseHelper db)
        {
            _db = db;
        }

        // Retrieves a list of all Property Managers from the Access Database.
        public async Task<List<PropertyManager>> GetAllAsync()
        {
            var list = new List<PropertyManager>();

            // 1. Get the connection string and wrap it in a 'using' statement to ensure it safely closes even if the database crashes.
            using (var conn = _db.GetConnection())
            {
                // 2. Open the connection to the .accdb file asynchronously
                await conn.OpenAsync();

                // 3. Write the raw SQL query
                var sql = "SELECT * FROM PropertyManagers";

                // 4. Create the command and execute the reader to read rows one by one
                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // 5. Loop through every row the database returns
                    while (await reader.ReadAsync())
                    {
                        // Map the raw database columns into our clean C# object
                        list.Add(new PropertyManager
                        {
                            ManagerID = Convert.ToInt32(reader["ManagerID"]),
                            FirstName = Convert.ToString(reader["FirstName"]),
                            LastName = Convert.ToString(reader["LastName"]),
                            EmailAddress = Convert.ToString(reader["EmailAddress"]),
                            ContactNumber = Convert.ToString(reader["ContactNumber"])
                        });
                    }
                }
            }

            // 6. Return the fully populated list to the web page
            return list;
        }
    }
}