using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberButler.DatabaseRecords
{
    class UsernameHistoryRecord : BaseRecord
    { 
        public string Server { get; set; }
        public string UserID { get; set; }
        public string NameBefore { get; set; }
        public string NameAfter { get; set; }

        public override void Insert()
        {
            db.Insert($"insert into username_history (server, userid, name_before, name_after) values ('{Server}', '{UserID}', '{NameBefore}', '{NameAfter}')");
        }

    }
}
