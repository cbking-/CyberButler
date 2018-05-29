using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberButler
{
    class DatabaseConnection
    {
        private SQLiteConnection DbConnection;

        public DatabaseConnection()
        {
            SetupDatabase();

            if (DbConnection == null)
            {
                DbConnection = new SQLiteConnection("Data Source=Database.sqlite;Version=3");
            }
        }

        private void SetupDatabase()
        {
            if (!System.IO.File.Exists("Database.sqlite"))
            {
                SQLiteConnection.CreateFile("Database.sqlite");

                string createUsernameHistory = "create table username_history (server varchar, userid varchar, name_before varchar, name_after varchar)";
                //string createReactionCount = "";
                string createRestaurant = "create table restaurant (server varchar, restaurant varchar)";

                DbConnection = new SQLiteConnection("Data Source=Database.sqlite;Version=3");

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
