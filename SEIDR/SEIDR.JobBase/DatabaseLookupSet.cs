using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DataBase;
using Cymetrix.Andromeda.Encryption;

namespace SEIDR.JobBase
{
    public class DatabaseLookupSet
    {
        public const string DEFAULT_APPLICATION_NAME = nameof(SEIDR);
        protected object syncLock = new object();
        private Encryptor pwDecryptor = new Encryptor();
        protected readonly Dictionary<int, DatabaseLookup> Connections;

        protected readonly Dictionary<Tuple<int, bool>, DatabaseManager> Managers = new Dictionary<Tuple<int, bool>, DatabaseManager>();

        public DatabaseLookupSet(DatabaseManager source)
        {
            Connections = source.SelectList<DatabaseLookup>().ToDictionary(db => db.DatabaseLookupID, db => db);
        }

        public DatabaseConnection GetConnection(int DatabaseLookupID)
        {
            lock (syncLock)
            {
                if (Connections.ContainsKey(DatabaseLookupID))
                {
                    var orig = Connections[DatabaseLookupID];
                    var ret = DatabaseConnection.FromString(orig.ConnectionString);
                    if (orig.UserName != null && orig.EncryptedPassword != null)
                        ret.Password = pwDecryptor.Decrypt(orig.EncryptedPassword);
                    return ret;
                }
            }
            return null;
        }

        public int ConnectionCount => Connections.Count;
        public int ManagerCount => Managers.Count;

        public DatabaseConnection GetConnection(string LookupDescription)
        {
            return GetConnection(Connections.FirstOrDefault(c => c.Value.Description.Equals(LookupDescription, StringComparison.OrdinalIgnoreCase)).Key); //Keep logic for password in one place.
        }
        public DatabaseManager GetManager(string LookupDescription, bool ReadOnly = false)
        {
            DatabaseLookup con = null;
            lock (syncLock)
                con = Connections.Values.FirstOrDefault(c => c.Description.Equals(LookupDescription, StringComparison.OrdinalIgnoreCase));
            if (con != null)
                return GetManager(con.DatabaseLookupID, ReadOnly);
            return null;
        }

        public virtual DatabaseManager GetManager(int LookupID, bool ReadOnly = false)
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
                    var manager = new DatabaseManager(db);
                    Managers.Add(key, manager);
                    return manager;
                }
            }
            return null;
        }

        public void Clear()
        {
            lock (syncLock)
            {
                Managers.Clear();
                Connections.Clear();
            }
        }
        public void Refresh(DatabaseManager source)
        {
            var check = source.SelectList<DatabaseLookup>();
            var q = from db in check
                    join conn in Connections on db.DatabaseLookupID equals conn.Key into dbGroup
                	from item in dbGroup.DefaultIfEmpty(new KeyValuePair<int, DatabaseLookup>(db.DatabaseLookupID, db))
                    select new {ItemKey = item.Key, NewValue = db};
            lock (syncLock)
            {
                foreach (var result in q)
                {
                    result.NewValue.ApplicationName = DEFAULT_APPLICATION_NAME;
                    if (Connections.ContainsKey(result.ItemKey))
                    {
                        //same connection could be in the manager dictionary twice - once for readonly and again for readwrite.
                        var k = new Tuple<int, bool>(result.ItemKey, true); 
                        if (Managers.ContainsKey(k))
                            Managers[k].ChangeConnection(result.NewValue);

                        k = new Tuple<int, bool>(result.ItemKey, false);
                        if (Managers.ContainsKey(k))
                            Managers[k].ChangeConnection(result.NewValue);
                    }

                    Connections[result.ItemKey] = result.NewValue;
                }
            }
        }
    }
}