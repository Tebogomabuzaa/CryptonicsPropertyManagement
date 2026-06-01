// Repositories/PropertyRepository.cs
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CryptonicsPropertyManagement.Helpers;
using CryptonicsPropertyManagement.Models.Entities;

namespace CryptonicsPropertyManagement.Repositories
{
    // Handles all database interactions related to the physical properties.
    public class PropertyRepository
    {
        private readonly DatabaseHelper _db;

        // Dependency injection automatically provides the database connection helper
        public PropertyRepository(DatabaseHelper db)
        {
            _db = db;
        }

        // Retrieves all properties and joins the Owner table to get the full name of the property owner.
        public async Task<List<Property>> GetAllAsync()
        {
            var list = new List<Property>();

            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                // Using a LEFT JOIN so we can display the Owner's actual name on the web page, instead of just displaying their meaningless integer ID.
                var sql = @"SELECT p.*, o.FirstName & ' ' & o.LastName AS OwnerName 
                            FROM Properties p 
                            LEFT JOIN Owners o ON p.OwnerID = o.OwnerID";

                using (var cmd = new OleDbCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Property
                        {
                            PropertyID = Convert.ToInt32(reader["PropertyID"]),
                            PropertyDescription = Convert.ToString(reader["PropertyDescription"]),
                            PhysicalAddress = Convert.ToString(reader["PhysicalAddress"]),
                            OwnerID = Convert.ToInt32(reader["OwnerID"]),
                            MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"]),
                            VacancyStatus = Convert.ToBoolean(reader["VacancyStatus"]),
                            // This comes from the JOIN, not the Properties table directly
                            OwnerName = Convert.ToString(reader["OwnerName"])
                        });
                    }
                }
            }
            return list;
        }

        // Safely inserts a new property into the database and returns its new unique ID.
        public async Task<int> AddAsync(Property property)
        {
            using (var conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                // Using '?' as parameters prevents SQL injection attacks. We never concatenate user input directly into SQL strings.
                var sql = @"INSERT INTO Properties (PropertyDescription, PhysicalAddress, OwnerID, MonthlyRent, VacancyStatus) 
                            VALUES (?, ?, ?, ?, ?)";

                using (var cmd = new OleDbCommand(sql, conn))
                {
                    // The order of these parameters MUST match the order of the '?' in the SQL string above.
                    cmd.Parameters.AddWithValue("@desc", property.PropertyDescription);
                    cmd.Parameters.AddWithValue("@addr", property.PhysicalAddress);
                    cmd.Parameters.AddWithValue("@ownerId", property.OwnerID);
                    cmd.Parameters.AddWithValue("@rent", property.MonthlyRent);
                    cmd.Parameters.AddWithValue("@vacant", property.VacancyStatus);
                    await cmd.ExecuteNonQueryAsync();
                }

                // @@IDENTITY is a special Access command that retrieves the AutoNumber ID of the row we literally just inserted.
                using (var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn))
                {
                    return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
                }
            }
        }
    }
}