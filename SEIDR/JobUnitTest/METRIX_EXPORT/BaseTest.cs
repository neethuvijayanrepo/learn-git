using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.JobBase;
using SEIDR.METRIX_EXPORT;
using SEIDR.DataBase;

namespace JobUnitTest.METRIX_EXPORT
{
    [IJobMetaData(nameof(TestExport), nameof(METRIX_EXPORT), "Test.",
        NeedsFilePath: false, AllowRetry:false, ConfigurationTable:null)]
    public class TestExport : SEIDR.METRIX_EXPORT.ExportJobBase
    {
        public override ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile)
        {
            throw new NotImplementedException();
        }
    }
    [TestClass]
    public class BaseTest : JobTestBase<TestExport>
    {
        ExportSetting settings = new ExportSetting();
        private ExportContextHelper _context;
        [TestMethod]
        public void ReaderTest()
        {
            _TestExecution.SetFileInfo(GetTestFile("TestRead.txt", "METRIX_EXPORT"));
            using (var r = _JOB.GetReader(_TestExecution.FilePath))
            {
                Assert.AreEqual(3, r.Columns.Count);
                Assert.AreEqual(9, r.RecordCount);
                Assert.AreEqual("Row", r[0, 1][0]);
            }
        }
        [TestMethod]
        public void CsvReaderTest()
        {
            _TestExecution.SetFileInfo(GetTestFile("TestRead.csv", "METRIX_EXPORT"));
            using (var r = _JOB.GetReader(_TestExecution.FilePath))
            {
                Assert.AreEqual(3, r.Columns.Count);
                Assert.AreEqual(9, r.RecordCount);
                Assert.AreEqual("Row", r[0, 1][0]);
            }
        }
        [TestMethod]
        public void ExportBatchCreationTest()
        {
            var batch = _JOB.BeginExportBatch(_context, "Balance Transfers");
            Assert.AreNotEqual(0, batch.ExportTypeID);
            Assert.AreEqual(_TestExecution.ProcessingDate, batch.SubmissionDate);
            Assert.AreEqual(ExportStatusCode.SR, batch.ExportBatchStatusCode);

            _context.Execution.METRIX_ExportBatchID = null; //Prevent the BeginExportBatch from interfering with the logic below.
            var batch2 = _JOB.GetExportBatch(_context, "Balance Transfers");
            Assert.AreEqual(batch.ExportBatchID, batch2.ExportBatchID); //Should have grabbed the same batch.

            batch.Active = false;
            batch.DateFrom = DateTime.Today;
            batch.RecordCount = 50;
            batch.SetExportStatus(ExportStatusCode.SI);


            _JOB.UpdateExportBatch(_context, batch);
            var db = _context.MetrixManager;  //_JOB.GetMetrixDatabaseManager(_Executor);
            string command = "SELECT ExportBatchID, ExportProfileID, Active, DateFrom, RecordCount, ExportBatchStatusCode FROM EXPORT.ExportBatch WHERE ExportBatchID = " +
                             batch.ExportBatchID;
            var row = db.ExecuteText(command).Tables[0].Rows[0].ToContentRecord<ExportBatchModel>();
            Assert.IsFalse(row.Active);
            Assert.AreEqual(DateTime.Today, row.DateFrom);
            Assert.AreEqual(batch.ExportBatchID, row.ExportBatchID);
            Assert.AreEqual(batch.ExportProfileID, row.ExportProfileID);
            Assert.AreEqual(batch.RecordCount, row.RecordCount);
            Assert.AreEqual(batch.ExportBatchStatusCode, row.ExportBatchStatusCode);
        }

        [TestMethod]
        public void CheckResultTest()
        {
            var valSet = (IList)Enum.GetValues(typeof(ResultStatusCode));
            ExecutionStatus status;
            foreach (object t in valSet)
            {
                ResultStatusCode val = (ResultStatusCode)t;
                status = ExportJobBase.GetStatus(val);
                Assert.AreEqual(val < ResultStatusCode.SC, status.IsError);
                Assert.AreEqual(val >= ResultStatusCode.C, status.IsComplete);
                Assert.AreEqual(val.ToString(), status.ExecutionStatusCode);
            }
            //Hard coded checks below to ensure defaults are maintained.
            status = ExportJobBase.GetStatus(ResultStatusCode.IE);
            Assert.IsTrue(status.IsError);
            Assert.IsFalse(status.IsComplete);
            Assert.AreEqual("IE", status.ExecutionStatusCode);

            status = ExportJobBase.GetStatus(ResultStatusCode.SC);
            Assert.IsFalse(status.IsError);
            Assert.IsFalse(status.IsComplete);
            Assert.AreEqual("SC", status.ExecutionStatusCode);

            status = ExportJobBase.GetStatus(ResultStatusCode.C);
            Assert.IsTrue(status.IsComplete);
            Assert.IsFalse(status.IsError);
            Assert.AreEqual("C", status.ExecutionStatusCode);
        }

        protected override void Init()
        {
            base.Init();
            _context = new ExportContextHelper(_JOB, _Executor, _TestExecution, settings);
            
        }

    }
}
