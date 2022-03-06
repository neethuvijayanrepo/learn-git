using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DemoMap.BaseImplementation;
using SEIDR.DemoMap;
using SEIDR.Doc;

namespace JobUnitTest.DemoMap
{
    [TestClass]
    public class BucketTest
    {
        private DocRecord record;
        private DocRecord recordB;
        private BasicContext context;
        public BucketTest()
        {
            string[] fields = 
            {
                "PayerCode",
                "Balance",
                "IsSelfPay",
                "LastBillDate"
            };
            DocRecordColumnCollection columns = new DocRecordColumnCollection();
            columns.AddColumn("AccountNumber");
            columns.AddColumn("FacilityKey");

            columns.AddColumn("BillingStatusCode");
            columns.AddColumn("OriginalBillDate");
            columns.AddColumn("VendorCode");
            columns.AddColumn("Inpatient");

            columns.AddColumn("CurrentInsuranceBalance");
            columns.AddColumn("CurrentPatientBalance");
            columns.AddColumn("CurrentAccountBalance");
            columns.AddColumn("_InsuranceDetailUnavailable");
            columns.AddColumn("_InsuranceBalanceUnavailable");
            columns.AddColumn("_PatientBalanceUnavailable");
            for (int i = 1; i <= 8; i++)
            {
                foreach (var field in fields)
                {
                    columns.AddColumn("Ins" + i + "_" + field);
                }
            }

            record = new DocRecord(columns, true)
            {
                ["AccountNumber"] = "1234",
                ["FacilityKey"] = FACILITY_KEY
            };

            recordB = new DocRecord(columns, true)
            {
                ["AccountNumber"] = "1235",
                ["FacilityKey"] = BAD_SELFPAY_FACILITY_KEY
            };

            context = new BasicContext();
            context.AddPayerInfo(new PayerMaster_MapInfo(OOO, FACILITY_KEY, false));
            context.AddPayerInfo(new PayerMaster_MapInfo(M1, FACILITY_KEY, false));
            context.AddPayerInfo(new PayerMaster_MapInfo(SELF_PAY, FACILITY_KEY, true));
            context.AddPayerInfo(new PayerMaster_MapInfo(SELF_PAY, BAD_SELFPAY_FACILITY_KEY, false)); //Bad payer info, but should not affect main facility.
        }

        const string FACILITY_KEY = "A";
        private const string BAD_SELFPAY_FACILITY_KEY = "B";
        private const string OOO = "OOO";
        private const string M1 = "M1";
        private const string SELF_PAY = "*SELFPAY*";
        [TestMethod]
        public void TestAddNewBucket()
        {
            Account a = new Account(record, context);
            Bucket test = new Bucket(record, 1, a);
            Assert.IsFalse(test.Valid);
            test.SetPayerInfo(M1);
            Assert.IsTrue(test.Valid);
            Assert.IsFalse(test.IsSelfPay);

            test.Balance = 50;
            test["LastBillDate"] = DateTime.Today.ToString("MM/dd/yyyy");
            test.Apply();

            Assert.AreEqual(record["Ins1_Balance"], test.PrincipalBalance);
            Assert.AreEqual(record["Ins1_LastBillDate"], test["LastBillDate"]);
        }
        [TestMethod]
        public void TestMultipleBucketsSameKey()
        {
            var AccountA = new Account(record, context);
            var AccountB = new Account(recordB, context);
            Bucket test1 = new Bucket(record, 1, AccountA);
            Bucket bad = new Bucket(recordB, 1, AccountB);
            Assert.AreEqual(test1.Valid, bad.Valid);
            test1.SetPayerInfo(SELF_PAY);
            bad.SetPayerInfo(SELF_PAY);
            Assert.IsTrue(test1.Valid);
            Assert.IsTrue(bad.Valid);
            Assert.AreEqual(test1.PayerCode, bad.PayerCode);
            Assert.AreNotEqual(test1.IsSelfPay, bad.IsSelfPay);
        }
        [TestMethod]
        public void TestAccount()
        {
            Account a = new Account(record, context);
            a.PrepBuckets();
            Assert.AreEqual(0, a.BucketCount);
            var b = a.AddBucket(SELF_PAY, 50, false);
            Assert.IsTrue(b.IsSelfPay);
            Assert.AreEqual(1, b.SequenceNumber);
            Assert.AreNotEqual(record["Ins1_Balance"], b.PrincipalBalance);
            b.Apply();
            Assert.AreEqual(record["Ins1_Balance"], b.PrincipalBalance);
            Assert.AreEqual(record["Ins1_Balance"], a["Ins1_Balance"]);
            Assert.AreEqual(b.Balance, a.TotalSelfPayBucketBalance);

            var b2 = a.AddBucket("New Payer Code", 250, false);
            Assert.IsFalse(b2.IsSelfPay);
            b2.Apply();
            Assert.AreEqual(a["Ins2_PayerCode"], b2.PayerCode);
            Assert.AreEqual(b2.Balance, a.GetBucketInsuranceBalances());
        }
        [TestMethod]
        public void TestAccountBucketShift()
        {
            Account a = new Account(record, context);
            a.PrepBuckets();
            var b1 = a.AddBucket(OOO, 50, false);
            var b2 = a.AddBucket(M1, 100);
            b1.Apply();
            b2.Apply();
            Assert.AreEqual(2, b2.SequenceNumber);
            Assert.AreEqual(record["Ins1_PayerCode"], b1.PayerCode);
            Assert.AreEqual(record["Ins1_PayerCode"], a["Ins1_PayerCode"]);
            Assert.AreEqual(record["Ins1_Balance"], b1.PrincipalBalance);
            Assert.AreEqual(record["Ins1_Balance"], a["Ins1_Balance"]);
            Assert.AreEqual(b1.Balance + b2.Balance, a.GetBucketInsuranceBalances());

            b2.Shift(1);
            Assert.AreEqual(1, b2.SequenceNumber);
            Assert.AreNotEqual(record["Ins1_Balance"], b1.PrincipalBalance);
            Assert.AreNotEqual(record["Ins1_PayerCode"], b1.PayerCode);

            Assert.AreEqual(record["Ins1_Balance"], b2.PrincipalBalance);
            Assert.AreEqual(b2.Balance, a.GetBucketInsuranceBalances());

        }
    }
}