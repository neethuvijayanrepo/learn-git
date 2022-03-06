//#define TEST_PROCS

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using System.Data;
using System.Threading.Tasks;

namespace SEIDR.DataBase.Test
{

    public class Inheritor : DatabaseObject<Inheritor>
    {

    }

    [TestClass]
    public class DataBaseUnitTest
    {
        //static DatabaseConnection db = new DatabaseConnection(@"OWNER-PC\SQLEXPRESS", "MIMIR");        
        static DatabaseConnection db = new DatabaseConnection(@"metaldev.cymetrix.com\sql2014", "MIMIR") { ApplicationName = "'test;'" };
        DatabaseManager m = new DatabaseManager(db, "SEIDR");
        #region Command text to Validate existence/setup of procedures/test table
        const string CHECK_FIRST_PROC = @"
IF OBJECT_ID('SEIDR.usp_DatamanagerTest', 'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE SEIDR.usp_DataManagerTest as set nocount on;')
END
DECLARE @SQL varchar(max) = '
ALTER PROCEDURE SEIDR.usp_DataManagerTest
    @MapName varchar(50),
    @TestOther int,
    @DefaultTest bit = 1
AS
BEGIN
    SELECT @MapName [Map], @TestOther [TestOther], @DefaultTest [DefaultTest]
END'
exec (@SQL)
";
        const string CHECK_SECOND_PROC = @"
IF OBJECT_ID('SEIDR.DM_Test') IS NOT NULL 
    DELETE SEIDR.DM_Test --Truncate would also be fine, but removes ident
ELSE
BEGIN    
    EXEC('CREATE TABLE SEIDR.DM_Test(ID int identity(1,1) primary key , DC datetime)' )
END
IF OBJECT_ID('SEIDR.usp_DataManagerTest2', 'P') is null
    EXEC('CREATE PROCEDURE SEIDR.usp_DatamanagerTest2 AS SET NOCOUNT ON;')

    DECLARE @SQL varchar(max) = '
ALTER PROCEDURE SEIDR.usp_DataManagerTest2
    @ID int output
AS
BEGIN
	INSERT INTO SEIDR.DM_Test(DC)
	VALUES(GETDATE())
    SET @ID = SCOPE_IDENTITY()
	SELECT * FROM SEIDR.DM_TEST
    RETURN 1
END'

EXEC (@SQL) 

";
        #endregion
        public DataBaseUnitTest()
        {
#if TEST_PROCS
            m.ExecuteText(CHECK_FIRST_PROC);
            m.ExecuteTextNonQuery(CHECK_SECOND_PROC);
#endif
            Inheritor i = new Inheritor();
        }
        public class TestClass
        {
            public int ThreadID { get; set; }
            public int BatchID { get; set; }
            public int Test { get; set; }
            public string Files { get; set; }
            public string Name { get; set; }
        }
        [TestMethod]
        public void DatabaseConnectionFromString()
        {
            string conn = "SERVER=test;Database='Test;';Application Name={Hey My App}";
            DatabaseConnection db = DatabaseConnection.FromString(conn);
            string expected = "Server='test';Database='Test;';Application Name='{Hey My App}';Trusted_Connection=true;";
            Assert.AreEqual(expected, db.ConnectionString);
        }
        [TestMethod]
        public void DatabaseExtensionTest()
        {
            var b = new TestClass
            {
                ThreadID = 2,
                BatchID = 1,
                Test = 3,
                Files = "Test123",
                Name = "TEST"
            };
            DataTable dt = b.ToTable("udt_Batch", "FileXML", "Files");
            var r = dt.GetFirstRowOrNull();
            Assert.IsNotNull(r);            
            Assert.AreEqual(b.ThreadID, (int)r["ThreadID"]);
            Assert.IsFalse(r.Table.Columns.Contains("Files"));
            TestClass b2 = r.ToContentRecord<TestClass>();
            Assert.AreEqual(b.ThreadID, b2.ThreadID);
            Assert.AreEqual(b.BatchID, b2.BatchID);
            Assert.AreEqual(b.Name, b2.Name);
        }
        [TestMethod]
        public void ToContentTypeTest()
        {
            var b = new
            {
                ThreadID = (byte?)5,
                Test = 325,
                BatchID = 12057,
                Name = "Test"
            };
            var b2 = new
            {
                ThreadID = (byte?)null,
                Test = 327,
                BatchID = 12058,
                Name = "Test2"
            };
            DataTable dt = b.ToTable();
            dt.AddRow(b2);
            var test = dt.ToContentRecord<TestClass>(0);
            var test2 = dt.ToContentRecord<TestClass>(1);
            Assert.AreEqual(5, test.ThreadID);
            Assert.AreEqual(325, test.Test);
            Assert.AreEqual(0, test2.ThreadID); //Default            
        }

        [TestMethod]
        public void BasicExecution()
        {   
            var map = new
            {
                MapName="Teeeeeest",
                TestOther = 22
            };
            var r = m.Execute("SEIDR.usp_DatamanagerTest", map).GetFirstRowOrNull();
            Assert.IsNotNull(r);
            Assert.AreEqual(r["Map"], map.MapName);
            Assert.AreEqual(r["TestOther"], map.TestOther);
            Assert.AreEqual(r["DefaultTest"], true);
            map = new
            {
                MapName = "Test 2!", TestOther = 21
            };
            r = m.Execute("SEIDR.usp_DatamanagerTest", map).GetFirstRowOrNull();
            Assert.IsNotNull(r);
            Assert.AreEqual(r["Map"], map.MapName);
            Assert.AreEqual(r["TestOther"], map.TestOther);
            Assert.AreEqual(r["DefaultTest"], true);
            DatabaseManagerHelperModel h = new DatabaseManagerHelperModel(map, "SEIDR.usp_DatamanagerTest");
            Assert.AreEqual("[usp_DatamanagerTest]", h.Procedure);
            Assert.AreEqual("[SEIDR].[usp_DatamanagerTest]", h.QualifiedProcedure);
            h.SetKey("DefaultTest", false);
            r = m.Execute(h).GetFirstRowOrNull();
            Assert.IsNotNull(r);
            Assert.AreEqual(r["Map"], map.MapName);
            Assert.AreEqual(r["TestOther"], map.TestOther);
            Assert.AreEqual(r["DefaultTest"], false);
            using (h = m.GetBasicHelper(true))
            {
                h.ParameterMap = map;
                h.Procedure = "usp_DatamanagerTest";
                h.ExpectedReturnValue = 0;
                h.SetKey("DefaultTest", false);
                m.Execute(h);
                Assert.AreEqual(0, h.ReturnValue);
                h.SetKey("DefaultTest", true);
                r = m.Execute(h).GetFirstRowOrNull();
                Assert.IsNotNull(r);
                Assert.AreEqual(r["DefaultTest"], true);
            }
        }

        [TestMethod]
        public void TestTran()
        {
           
            using (var h = m.GetBasicHelper(true))
            {
                h.BeginTran();
                h.AddKey("ID", 0);
                h.Procedure = "usp_DataManagerTest2";
                h.ExpectedReturnValue = 1;
                m.Execute(h);
                int id = (int)h["ID"];
                Assert.AreNotEqual(0, id);
                Assert.AreNotEqual(0, h.ReturnValue);
                m.Execute(h);
                Assert.AreNotEqual(id, h["ID"]); //Should be greater.
                var ds = m.Execute(h).GetFirstTableOrNull();
                Assert.IsNotNull(ds);
                Assert.AreNotEqual(0, ds.Rows.Count);
                h.RollbackTran();
                h.BeginTran();
                ds = m.Execute(h, true).GetFirstTableOrNull();
                Assert.AreEqual(1, ds.Rows.Count);
                Assert.IsFalse(h.HasOpenTran);                
                ds = m.Execute(h).GetFirstTableOrNull();
                Assert.AreEqual(2, ds.Rows.Count);
            }
        }
        [TestMethod]
        public void TestUnexpectedReturnValue_Rollback()
        {
            using (var h = m.GetBasicHelper(true))
            {
                h.BeginTran();
                h.AddKey("ID", 0);                
                h.QualifiedProcedure = "SEIDR.usp_DataManagerTest2";
                Assert.AreEqual("[usp_DataManagerTest2]", h.Procedure);
                Assert.AreEqual(m.DefaultSchema, h.Schema);
                h.ExpectedReturnValue = 0;
                m.Execute(h);
                Assert.IsTrue(h.IsRolledBack);
            }
        }

        [TestMethod]
        public void TestDeadlock()
        {            
            m.DefaultRetryOnDeadlock = true;
            db.CommandTimeout = 0;
            Task t1 = new Task(() =>
            {
                string cmd = @"
SET DEADLOCK_PRIORITY LOW
BEGIN TRAN


UPDATE SEIDR.JobProfile WITH (HOLDLOCK, ROWLOCK)
SET Description = '1' + Description
WHERE JobProfileID = 1

WAITFOR DELAY '00:00:15';


UPDATE SEIDR.JobProfile WITH (HOLDLOCK, ROWLOCK)
SET Description = '1' + Description
WHERE JobProfileID = 2

COMMIT
";
                m.ExecuteTextNonQuery(cmd, true); //Set Breakpoint in catch for ex.Number == 1205
            });
            t1.Start();
            System.Threading.Thread.Sleep(2000);
            Task t2 = new Task(() =>
            {
                string cmd = @"
SET DEADLOCK_PRIORITY HIGH
BEGIN TRAN


UPDATE SEIDR.JobProfile WITH (HOLDLOCK, ROWLOCK)
SET Description = '2' + RIGHT(Description, 98)
WHERE JobProfileID = 2

UPDATE SEIDR.JobProfile WITH (HOLDLOCK, ROWLOCK)
SET Description = '2' + RIGHT(Description, 98)
WHERE JobProfileID = 1



COMMIT
";
                m.ExecuteTextNonQuery(cmd, true);
            });
            t2.Start();
            try
            {
                t1.Wait(); //Note: Changing UPDATE SEIDR.JobProfile to UPDATE SEIDR.JobProfileID causes exception as expected for non deadlock(Invalid object)
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            t2.Wait();
            var desc = m.ExecuteText("SELECT Description FROM SEIDR.JobProfile WHERE JobProfileID = 1").GetFirstRowOrNull()[0].ToString();
            Assert.IsTrue(desc.StartsWith("12")); //2 should commit first even though 1 starts first.
            //If there's no deadlock + auto retry, then Task # 1 would either not have a '1' at the beginning, or the 1 would be to the right of the 2 since it should have supdated profileID 1 before Task # 2 starts.
        }
        [TestMethod]
        public void ParameterChangeTest()
        {
            m.ExecuteTextNonQuery(@"IF OBJECT_ID('temp.ParameterTest', 'P') IS NOT NULL
DROP PROCEDURE temp.ParameterTest;");

            m.ExecuteTextNonQuery(@"
CREATE PROCEDURE temp.ParameterTest 
@Val int
AS 
BEGIN
    SELECT @Val
END
");
            var mapObj = new { Val = 5, BoolBit = false };
            int sel1;
            int sel2;
           
            sel1 = Convert.ToInt32(m.Execute("temp.ParameterTest", mapObj).GetFirstRowOrNull()[0]);
           
           
            m.ExecuteTextNonQuery(@"ALTER PROCEDURE temp.ParameterTest 
@Val int, @BoolBit bit
AS
BEGIN
    SELECT @Val
END
");
        
            try
            {
                sel2 = Convert.ToInt32(m.Execute("temp.ParameterTest", mapObj).GetFirstRowOrNull()[0]);
                Assert.Fail(); //Parameters have changed, formal parameter list provided does not match. Should go into catch. and succeed there. 

                //Note that it does not auto-retry, and should not - changed parameters is an indication of a deployment, and anything in-progress during the change should probably be discarded.
            }
            catch
            {
                sel2 = Convert.ToInt32(m.Execute("temp.ParameterTest", mapObj).GetFirstRowOrNull()[0]);
            }            
            m.ExecuteTextNonQuery("DROP PROCEDURE temp.ParameterTest");
            Assert.AreEqual(sel1, sel2);
        }
    }
}
