using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberButler.DatabaseRecords
{
    class RestaurantRecord : BaseRecord
    {
        public string Server { get; set; }
        public string Restaurant { get; set; }

        public override void Insert()
        {
            var statement = $"insert into restaurant (server, restaurant) values (@server, @restaurant)";
            var parameters = new Dictionary<String, String>
            {
                { "@server", Server },
                { "@restaurant", Restaurant }
            };

            db.NonQuery(statement, parameters);
        }

        public RestaurantRecord SelectRandom(string _server)
        {
            var query = $"select restaurant from restaurant where server = @server order by random() limit 1";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server }
            };

            var records = db.Select<RestaurantRecord>(query, parameters);

            var result = records.Cast<RestaurantRecord>().First();

            return result;
        }
    }
}
