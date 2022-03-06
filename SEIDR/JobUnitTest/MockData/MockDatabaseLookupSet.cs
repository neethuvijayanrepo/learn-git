using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DataBase;

namespace JobUnitTest.MockData
{
    public class MockDatabaseLookupSet : DatabaseLookupSet
    {
        private readonly MockDatabaseManager _default;
        public MockDatabaseLookupSet(DatabaseManager source) : base(source)
        {
            _default = new MockDatabaseManager(source.CloneConnection());
        }
        public override DatabaseManager GetManager(int LookupID, bool ReadOnly = false)
        {
            var key = new Tuple<int, bool>(LookupID, ReadOnly); // Note: readonly and readwrite servers should be expected to have different procedure calls being used,
            // so parameter caching won't overlap anyway.

            lock (syncLock)
            {
                if (Managers.ContainsKey(key))
                {
                    return Managers[key];
                }
                DatabaseConnection db = GetConnection(LookupID);
                if (db != null)
                {
                    db.ReadOnlyIntent = ReadOnly;
                    var manager = new MockDatabaseManager(db);
                    Managers.Add(key, manager);
                    return manager;
                }
            }
            return _default; 
        }
    }
}
