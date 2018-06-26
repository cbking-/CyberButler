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

                string createUsernameHistory = "create table username_history (server varchar, userid varchar, name_before varchar, name_after varchar, insert_datetime string)";
                //string createReactionCount = "";
                string createRestaurant = "create table restaurant (server varchar, restaurant varchar)";
                string createCustomCommands = "create table custom_command (server varchar, command varchar, text varchar)";

                DbConnection = new SQLiteConnection($"Data Source={databasePath};Version=3");

                DbConnection.Open();

                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandText = createUsernameHistory;
                    command.ExecuteNonQuery();

                    command.CommandText = createRestaurant;
                    command.ExecuteNonQuery();

                    command.CommandText = createCustomCommands;
                    command.ExecuteNonQuery();
                }

                DbConnection.Close();
            }
        }

        public void NonQuery(string _statement, Dictionary<String, String> _parameters = null)
        {
            DbConnection.Open();

            using (var command = DbConnection.CreateCommand())
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

                try
                {
                    var updated = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            DbConnection.Close();
        }

        public DataTable Select(string _statement, Dictionary<String, String> _parameters = null)
        {
            var result = new DataTable();

            DbConnection.Open();

            using (var command = DbConnection.CreateCommand())
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
