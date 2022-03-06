using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.DemoMap.CERNER;
using SEIDR.Doc;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class CernerTest : ContextJobTestBase<CernerBase, MappingContext>
    {
        private BasicContext _testContext;
        private DocRecord _testLine;
        private DemoMapJobConfiguration _testSettings;
        //File Mapping - need to identify a good way to test that. But for now, can test the transformation to a given record.

        #region Helpers
        protected override void Init()
        {
            base.Init();
            //Default preparations. In individual tests, can modify from default values.
            var dir = PrepSubfolder("OUTPUT");
            _testContext = new BasicContext();

            _TestExecution.SetOrganizationID(860);
            _testSettings = new DemoMapJobConfiguration
            {
                //OutputFolder = dir.FullName,
                PayerLookupDatabaseID = 1,
                //PayerFacilityID = 326, //Move to using Organization.
                FileMapDatabaseID = 1,
            };
            _testSettings.SetOutputFolder(dir.FullName);
            DocRecordColumnCollection colSet = new DocRecordColumnCollection();
            const string DEFAULT_FACILITY_CODE = "A";
            var required = new Dictionary<string, string>
            {
                {nameof(Account.AccountNumber), "1234A"},
                {nameof(Account.FacilityKey), DEFAULT_FACILITY_CODE },
                {"BillingStatusCode", "UNKNOWN" },
                {nameof(Account.OriginalBillDate), "02/25/2020" },
                {nameof(Account.VendorCode), "*UNKNOWN*" },
                {"PatientState", "CAV" },
                {"SendToAlternateAddress" , "Y"},
                { "PatientSSN",  "12345678910"},
                { "PatientLanguageCode", "SPANISH"},
                {nameof(Account.CurrentAccountBalance), "100.00" },
                {nameof(Account.CurrentPatientBalance), "90" },
                {nameof(Account.CurrentInsuranceBalance), "9.99" },
                {nameof(Account.NonBillableBalance), null },
                {nameof(Account.Inpatient), "O" },
                {nameof(Account._PatientBalanceUnavailable), "0" },
                {nameof(Account._InsuranceBalanceUnavailable), "0" },
                {nameof(Account._InsuranceDetailUnavailable), "0" },
                {nameof(Account._PartialDemographicLoad), "0" } //Loads account as Inferred = 1.
            };
            foreach (var k in required.Keys)
            {
                colSet.AddColumn(k);
            }

            string[] optional =
            {
                "LastReconciliationDate",
                "BillingStatusDate",
                "GuarantorLanguageCode",
                "GuarantorSSN",
                "PatientAddress1",
                "PatientAddress2",
                //"PatientState",
                "PatientZip",
                "PatientCity",
                "GuarantorAddress1",
                "GuarantorAddress2",
                "GuarantorState",
                "GuarantorZip",
                "GuarantorCity"
            };
            string[] insOptional =
            {
                "PayerCode",
                "Balance",
                "BillToExtension",
                "IsSelfPay",
                "BillToAddress1",
                "BillToAddress2",
                "BillToState",
                "BillToZip",
                "BillToCity"
            };
            foreach (var o in optional)
            {
                colSet.AddColumn(o);
            }

            for (int i = 1; i <= 8; i++)
            {
                foreach (var o in insOptional)
                {
                    colSet.AddColumn($"Ins{i}_{o}");
                }
            }

            var mock = (MockDatabaseManager)_Executor.GetManager(_testSettings.FileMapDatabaseID);
            mock.DefaultSchema = "STAGING";
            var mm = mock.NewMockModelQualified(MAPS_DELIMITED.GET_EXECUTION_INFO);
            List<MAPS_DELIMITED> prep = new List<MAPS_DELIMITED>();
            foreach (var col in colSet)
            {
                prep.Add(new MAPS_DELIMITED
                {
                    ClientFieldIndex = col.Position + 1, //1 based.
                    ClientFieldName = col.ColumnName,
                    CymetrixFieldName = col.ColumnName
                });
            }
            mm.MapToNewRows(prep);

            PayerMaster_MapInfo pm = new PayerMaster_MapInfo("PAYER", DEFAULT_FACILITY_CODE, false);
            mm = mock.NewMockModel<PayerMaster_MapInfo>();
            mm.MapToNewRow(pm);

            _testContext.Init(MyContext, 1, _testSettings, _Executor);

            _testLine = new DocRecord(colSet, true); //ToDo: populate with some mock data.
            foreach (var kv in required)
            {
                _testLine[kv.Key] = kv.Value;
            }

            
        }
        [TestMethod]
        public void BasicTest()
        {
            var acct = new Account(_testLine, _testContext);
            Address g = acct.GuarantorAddress;
            Assert.IsNull(g.State);
            g.State = "District of Columbia";
            acct["GuarantorSSN"] = "123456789";
            int expected = acct.BucketCount + 1; //OOO Bucket should be created.
            

            TransformCall(acct);

            Assert.AreEqual("CA", acct["PatientState"]);
            Assert.AreEqual("DC", acct["GuarantorState"]);
            Address pt = acct.PatientAddress;
            Assert.AreEqual("CA", pt.State);

            //Added nullable refresh in account refresh - should be refreshed when the transform refreshes the account
            //NOTE: if the state cleanup is put in the preFinancialTransform rather than the StartTransform, then the below would need to be uncommented again.
            
            //Assert.AreNotEqual("DC", g.State); //Should still need to be refreshed from transform. (Depends on which override the state cleanup is in)
            //g.Refresh();
            Assert.AreEqual("DC", g.State);


            Assert.AreEqual(expected, acct.BucketCount); //OOO
            Assert.AreEqual(expected, acct.MaxSequence);
            var b = acct[acct.MaxSequence];
            Assert.IsNotNull(b);
            Address a = b.GetAddressInfo();
            Assert.IsTrue(string.IsNullOrEmpty(a.Address1));
            a.Address1 = "Test";
            Assert.AreEqual(a.Address1, b["BillToAddress1"]);
            Assert.AreEqual(Bucket.OOO_BUCKET_PAYER_CODE, b.PayerCode);

            //If more buckets added in the init/prep, then may need to redo the balance assertion.
            Assert.AreEqual(1, expected); 
            Assert.AreEqual(acct.CurrentInsuranceBalance, b.Balance);

            Assert.IsTrue(string.IsNullOrEmpty(acct["PatientSSN"]));
            Assert.AreEqual("123456789", acct["GuarantorSSN"]);

            Assert.AreEqual("1", acct["SendToAlternateAddress"]);

            Assert.AreEqual("SP", acct["PatientLanguageCode"]);
        }
        [TestMethod]
        public void NoOOOTest()
        {
            //Force ins balance to match individual buckets (none), so no OOO is created.
            var acct = new Account(_testLine, _testContext){ CurrentInsuranceBalance = 0 };

            acct.CurrentAccountBalance = acct.CurrentPatientBalance;
            
            // Original bucket count should be maintained.
            int expectedBucketCount = acct.BucketCount;

            TransformCall(acct);

            Assert.AreEqual(expectedBucketCount, acct.BucketCount);
            Assert.AreEqual(expectedBucketCount, acct.MaxSequence);
        }

        public bool TransformCall(Account acct)
        {
            //Call with asserts
            return _JOB.Transform( _testLine, _testContext, acct);
        }
        #endregion
    }
}
