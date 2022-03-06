using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.JobBase;

namespace JobUnitTest
{
    [TestClass]
    public class DatabaseLookupSetTest : TestBase
    {
        [TestMethod]
        public void LookupTest()
        {
            DatabaseLookupSet dls = new DatabaseLookupSet(base._Manager);
            int count = dls.ConnectionCount;
            Assert.AreNotEqual(0, dls.ConnectionCount);
            Assert.AreEqual(0, dls.ManagerCount);
            var mgr = dls.GetManager(1);
            Assert.IsNotNull(mgr);
            Assert.AreEqual(1, dls.ManagerCount);
            var db = mgr.CloneConnection();
            Assert.IsFalse(db.ReadOnlyIntent);

            mgr = dls.GetManager(1, true);
            Assert.IsNotNull(mgr);
            Assert.AreEqual(2, dls.ManagerCount);
            var db2 = mgr.CloneConnection();
            Assert.IsTrue(db2.ReadOnlyIntent);
            Assert.AreEqual(db.Server, db2.Server);
            Assert.AreEqual(db.useTrustedConnection, db2.useTrustedConnection);
            Assert.AreEqual(db.DefaultCatalog, db2.DefaultCatalog);

            var mgr2 = dls.GetManager(1, true);
            Assert.AreEqual(mgr, mgr2);  //should be the exact same reference

            dls.Clear();
            Assert.AreNotEqual(count, dls.ConnectionCount);
            dls.Refresh(_Manager);
            Assert.AreEqual(count, dls.ConnectionCount);
            

        }
    }
}
