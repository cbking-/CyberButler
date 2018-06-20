using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace CyberButler
{
    class DatabaseConnection
    {
        SQLiteConnection DbConnection;
        readonly string databasePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Database.sqlite3");

        public DatabaseConnection()
        {
            SetupDatabase();

            if (DbConnection == null)
            {
                DbConnection = new SQLiteConnection($"Data Source={databasePath};Version=3");
            }
        }

        void SetupDatabase()
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);

                string createUsernameHistory = "create table username_history (server varchar, userid varchar, name_before varchar, name_after varchar)";
                //string createReactionCount = "";
                string createRestaurant = "create table restaurant (server varchar, restaurant varchar)";

                DbConnection = new SQLiteConnection($"Data Source={databasePath};Version=3");

                DbConnection.Open();

                var command = new SQLiteCommand(createUsernameHistory, DbConnection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(createRestaurant, DbConnection);
                command.ExecuteNonQuery();

                DbConnection.Close();
            }
        }

        public void Insert(string _statement, Dictionary<String, String> _parameters = null)
        {
            DbConnection.Open();
            using (var command = new SQLiteCommand(_statement, DbConnection))
            {
                command.CommandType = CommandType.Text;
                command.CommandText = _statement;

                if (_parameters == null)
                {
                    _parameters = new Dictionary<string, string>();
                }
                else
                {
                    foreach (var parameter in _parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                command.ExecuteNonQuery();
            }

            DbConnection.Close();
        }

        public DataTable Select(string _statement, Dictionary<String, String> _parameters = null)
        {
            var result = new DataTable();

            DbConnection.Open();

            using (var command = new SQLiteCommand(_statement, DbConnection))
            {
                command.CommandType = CommandType.Text;
                command.CommandText = _statement;

                if (_parameters == null)
                {
                    _parameters = new Dictionary<string, string>();
                }
                else
                {
                    foreach (var parameter in _parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                using (var adapter = new SQLiteDataAdapter(command))
                {
                    adapter.Fill(result);
                }
            }
            DbConnection.Close();
            return result;
        }
    }
}
