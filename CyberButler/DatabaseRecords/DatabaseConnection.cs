using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace CyberButler.DatabaseRecords
{
    internal class DatabaseConnection
    {
        private SqliteConnectionStringBuilder ConString;
        private SqliteConnection DbConnection;
        private readonly string databasePath = 
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Database.sqlite3");

        public DatabaseConnection()
        {
            SetupDatabase();

            if (DbConnection == null)
            {
                ConString = new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate
                };

                DbConnection = new SqliteConnection(ConString.ConnectionString);
            }
        }

        private void SetupDatabase()
        {
            if (!File.Exists(databasePath))
            {
                string createTables = @"CREATE TABLE username_history
                                          (
                                             server          VARCHAR,
                                             userid          VARCHAR,
                                             name_before     VARCHAR,
                                             name_after      VARCHAR,
                                             insert_datetime VARCHAR
                                          );

                                        CREATE TABLE restaurant
                                          (
                                             server     VARCHAR,
                                             restaurant VARCHAR
                                          );

                                        CREATE TABLE custom_command
                                          (
                                             server  VARCHAR,
                                             command VARCHAR,
                                             text    VARCHAR
                                          );";

                ConString = new SqliteConnectionStringBuilder
                {
                    DataSource = databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate
                };

                DbConnection = new SqliteConnection(ConString.ConnectionString);

                DbConnection.Open();

                using (var command = DbConnection.CreateCommand())
                {
                    command.CommandText = createTables;
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

        public IEnumerable<T> Select<T>(string _statement, 
            Dictionary<String, String> _parameters = null) where T : class, new()
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

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, 
                                Convert.ChangeType(reader[prop.Name.ToLower()], 
                                propertyInfo.PropertyType), 
                                null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    yield return obj;
                }
            }

            DbConnection.Close();
        }
    }
}