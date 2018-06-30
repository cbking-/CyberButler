using System;
using System.Collections.Generic;

namespace CyberButler.DatabaseRecords
{
    class UsernameHistoryRecord : BaseRecord
    { 
        public string Server { get; set; }
        public string UserID { get; set; }
        public string NameBefore { get; set; }
        public string NameAfter { get; set; }
        public string InsertDateTime { get; set; }

        public override void Insert()
        {
            var statement = $"insert into username_history (server, userid, name_before, name_after, insert_datetime) values (@server, @userid, @namebefore, @nameafter, @datetime)";
            var parameters = new Dictionary<String, String>
            {
                { "@server", Server },
                { "@userid", UserID },
                { "@namebefore", NameBefore },
                { "@nameafter", NameAfter },
                { "@datetime", InsertDateTime }
            };

            db.NonQuery(statement, parameters);
        }

        public IEnumerable<UsernameHistoryRecord> Select(String _server, String _userid)
        {
            var query = $"select name_before, name_after from username_history where server = @server and userid = @userid order by insert_datetime desc";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server },
                { "@userid", _userid }
            };

            var dt = db.Select<UsernameHistoryRecord>(query, parameters);

            var result = new Dictionary<String, String>();

            var records = db.Select<UsernameHistoryRecord>(query, parameters);

            return records;
        }

    }
}
