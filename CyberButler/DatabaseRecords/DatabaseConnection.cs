using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace CyberButler
{
    class DatabaseConnection
    {
        private SQLiteConnection DbConnection;
        private readonly string databasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Database.sqlite3");

        public DatabaseConnection()
        {
            SetupDatabase();

            if (DbConnection == null)
            {
                DbConnection = new SQLiteConnection($"Data Source={databasePath};Version=3");
            }
        }

        private void SetupDatabase()
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);

                string createUsernameHistory = "create table username_history (server varchar, userid varchar, name_before varchar, name_after varchar)";
                //string createReactionCount = "";
                string createRestaurant = "create table restaurant (server varchar, restaurant varchar)";

                DbConnection = new SQLiteConnection($"Data Source={databasePath};Version=3");

                DbConnection.Open();

                SQLiteCommand command = new SQLiteCommand(createUsernameHistory, DbConnection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createRestaurant, DbConnection);
                command.ExecuteNonQuery();

                DbConnection.Close();
            }
        }

        public void Insert(string statement)
        {
            DbConnection.Open();
            using (var command = new SQLiteCommand(statement, DbConnection))
            {
                command.ExecuteNonQuery();
            }
            DbConnection.Close();
        }

        public DataTable Select(string statement)
        {
            var result = new DataTable();

            DbConnection.Open();

            using (var adapter = new SQLiteDataAdapter(statement,DbConnection))
            {
                adapter.Fill(result);
            }

            DbConnection.Close();
            return result;
        }
    }
}
