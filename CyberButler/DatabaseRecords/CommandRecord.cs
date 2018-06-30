using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberButler.DatabaseRecords
{
    class CommandRecord : BaseRecord
    {
        private readonly bool CaseSensitive = Boolean.Parse(Configuration.Config["CommandCaseSensitive"]);

        public string Server { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }

        public override void Insert()
        {
            if (CaseSensitive)
            {
                Command = Command.ToLower();
            }

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
            if (CaseSensitive)
            {
                _command = _command.ToLower();
            }

            var statement = $"update custom_command set text = @text where server = @server and command = @command";
            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@command", _command },
                { "@text", _text }
            };

            db.NonQuery(statement, parameters);
        }

        public CommandRecord SelectOne(String _server, String _command)
        {
            if (CaseSensitive)
            {
                _command = _command.ToLower();
            }

            var query = $"select text from custom_command where server = @server and command = @command";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@command", _command}
            };

            var records = db.Select<CommandRecord>(query, parameters);
            CommandRecord result = null;

            try
            {
                result = records.Cast<CommandRecord>().First();
            }
            catch
            {
                //no record to return
            }

            return result;
        }

        public IEnumerable<CommandRecord> SelectAll(String _server)
        {
            var query = $"select command, text from custom_command where server = @server";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server }
            };

            var records = db.Select<CommandRecord>(query, parameters);

            return records;
        }

        public void Delete(String _server, String _command)
        {
            if (CaseSensitive)
            {
                _command = _command.ToLower();
            }

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
