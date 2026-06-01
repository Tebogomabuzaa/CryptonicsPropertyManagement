using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Helper/DatabaseHelper.cs
using System.Data.OleDb;
using Microsoft.Extensions.Configuration;

namespace CryptonicsPropertyManagement.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        // The constructor pulls configuration settings automatically
        public DatabaseHelper (IConfiguration configuration)
        {
            // 1. Read the physical path to the .accdb file from appsettings.json
            var dbPath = configuration["DatabaseSettings:AccessDbPath"];

            // 2. Build the exact connection string required for MAccess x64 bit
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
        }

        // Every repository will call this method to open a secure channel to the database
        public OleDbConnection GetConnection()
        {
            return new OleDbConnection(_connectionString);
        }
    }
}
