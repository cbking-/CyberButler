using System;
using System.Collections.Generic;
using System.Data;

namespace CyberButler.DatabaseRecords
{
    class CommandRecord : BaseRecord
    {
        public string Server { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }

        public override void Insert()
        {
            var statement = $"insert into custom_command (server, command, text) values (@server, @command, @text)";
            var parameters = new Dictionary<String, String>
            {
                { "@server", Server },
                { "@command", Command },
                { "@text", Text }
            };

            db.NonQuery(statement, parameters);
        }

        public void Update(String _server, String _command, String _text)
        {
            var statement = $"update custom_command set text = @text where server = @server and command = @command";
            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@command", _command },
                { "@text", _text }
            };

            db.NonQuery(statement, parameters);
        }

        public String SelectOne(string _server, string _command)
        {
            var query = $"select text from custom_command where server = @server and command = @command";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@command", _command}
            };

            var dt = db.Select(query, parameters);

            var result = "";

            try
            {
                result = dt.Rows[0]["text"].ToString();
            }
            catch
            {
            }
            
            return result;
        }

        public Dictionary<String, String> SelectAll(string _server)
        {
            var query = $"select command, text from custom_command where server = @server";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server }
            };

            var dt = db.Select(query, parameters);

            var result = new Dictionary<String, String>();

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    result.Add(row["command"].ToString(), row["text"].ToString());
                }
            }
            catch
            {
            }

            return result;
        }

        public void Delete(String _server, String _command)
        {
            var statement = $"delete from custom_command where server = @server and command = @command";
            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@command", _command }
            };

            db.NonQuery(statement, parameters);
        }
    }
}
