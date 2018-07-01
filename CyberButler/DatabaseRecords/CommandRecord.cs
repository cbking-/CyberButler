using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberButler.DatabaseRecords
{
    internal class CommandRecord : BaseRecord
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

            var statement = @"INSERT INTO custom_command
                                          (server,
                                           command,
                                           text)
                              VALUES      (@server,
                                           @command,
                                           @text)";

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

            var statement = @"UPDATE custom_command
                              SET text = @text
                              WHERE server = @server
                                    AND command = @command  ";

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

            var query = @"SELECT text
                          FROM custom_command
                          WHERE server = @server
                               AND command = @command  ";

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
            var query = @"SELECT command,
                                  text
                          FROM   custom_command
                          WHERE  server = @server";

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

            var statement = @"DELETE FROM custom_command
                              WHERE server = @server
                                   AND command = @command  ";
            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@command", _command }
            };

            db.NonQuery(statement, parameters);
        }
    }
}