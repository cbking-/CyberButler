using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberButler.DatabaseRecords
{
    abstract class BaseRecord
    {
        protected DatabaseConnection db = new DatabaseConnection();

        public abstract void Insert();
    }
}
