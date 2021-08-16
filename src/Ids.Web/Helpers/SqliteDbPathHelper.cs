using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EscapeDungeonIdentityWeb.Helpers
{
    public static class SqliteDbPathHelper
    {
        public static string GetConnectionString(string connectionString)
        {
            string executable = Assembly.GetExecutingAssembly().Location;
            string dbPath = Path.Combine(Path.GetDirectoryName(executable), "db_data");
            var dbFilePath = connectionString.Replace("{db_path}", dbPath);
            return dbFilePath;
        }
    }
}
