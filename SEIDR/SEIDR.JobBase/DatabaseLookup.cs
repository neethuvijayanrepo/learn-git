using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.JobBase
{
    public class DatabaseLookup: DataBase.DatabaseConnection
    {
        public int DatabaseLookupID { get; set; }
        public string Description { get; set; }
        public string EncryptedPassword { get; set; }
    }
}
