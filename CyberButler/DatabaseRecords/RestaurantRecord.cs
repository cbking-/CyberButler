using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberButler.DatabaseRecords
{
    internal class RestaurantRecord : BaseRecord
    {
        public string Server { get; set; }
        public string Restaurant { get; set; }

        public override void Insert()
        {
            var statement = @"INSERT INTO restaurant
                                          (server,
                                           restaurant)
                              VALUES      (@server,
                                           @restaurant)";
            var parameters = new Dictionary<String, String>
            {
                { "@server", Server },
                { "@restaurant", Restaurant }
            };

            db.NonQuery(statement, parameters);
        }

        public RestaurantRecord SelectRandom(string _server)
        {
            var query = @"SELECT restaurant
                          FROM   restaurant
                          WHERE  server = @server
                          ORDER  BY Random()
                          LIMIT  1";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server }
            };

            var records = db.Select<RestaurantRecord>(query, parameters);

            var result = records.Cast<RestaurantRecord>().First();

            return result;
        }

        public IEnumerable<RestaurantRecord> SelectAll(String _server)
        {
            var query = @"SELECT restaurant
                          FROM   restaurant
                          WHERE  server = @server";

            var parameters = new Dictionary<String, String>
            {
                { "@server", _server }
            };

            var records = db.Select<RestaurantRecord>(query, parameters);

            return records;
        }
    }
}