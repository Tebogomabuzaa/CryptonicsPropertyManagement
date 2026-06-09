using System;
using System.Data.Common;

namespace CryptonicsPropertyManagement.Helpers
{
    public static class DataReaderExtensions
    {
        public static bool HasColumn(this DbDataReader reader, string name)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
