// Repositories/UserRepository.cs
using System;
using System.Linq;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseHelper _db;

        public UserRepository(DatabaseHelper db) => _db = db;

        // Searches the database for a specific user during the Login process.
        public async Task<SystemUser> GetUserByUsernameAsync(string username)
        {
            SystemUser user = null;

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                // We use parameterized queries (?) to prevent SQL Injection attacks!
                var sql = "SELECT * FROM SystemUsers WHERE Username = ?";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    // Bind the typed username safely to the query
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // If we find a match in the database, map it to the C# object
                        if (await reader.ReadAsync())
                        {
                            user = new SystemUser
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                Username = Convert.ToString(reader["Username"]),

                                // We pull the hashed password to compare it, NOT a plain text password
                                PasswordHash = Convert.ToString(reader["PasswordHash"]),
                                UserRole = Convert.ToString(reader["UserRole"])
                            };
                        }
                    }
                }
            }

            // Returns the user, or null if nobody was found
            return user;
        }
    }
}