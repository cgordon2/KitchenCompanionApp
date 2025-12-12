using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePOC.DB
{
    public class DBConstants
    {
        public const string DATABASE_NAME = "recipe.db3";

        public const SQLite.SQLiteOpenFlags Flags = 
            SQLite.SQLiteOpenFlags.ReadWrite | 
            SQLite.SQLiteOpenFlags.Create | 
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DATABASE_NAME);
            }
        }  

        public static async Task<bool> TableExistsAsync<T>(SQLiteAsyncConnection connection)
        {
            var tableName = typeof(T).Name;
            var result = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?;", tableName);

            return result > 0;
        }
    }
}
