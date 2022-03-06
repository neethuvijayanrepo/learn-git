using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.METRIX_EXPORT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.METRIX_EXPORT.LiveVoxExport;

namespace JobUnitTest
{
    [TestClass]
    public class LiveVoxExportTest : JobTestBase<LiveVoxFileGenerationJob>
    {
        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        [TestMethod]
        public void LiveVoxExportFileGenerationJob_Test()
        {
            //DevSE01 export batchID
            const int EXPORT_BATCH_ID = 40205;
            _TestExecution.SetJobProfile_JobID(583);
            _TestExecution.SetUserKey(136);
          //  _TestExecution.SetJobProfileID(106);
            _TestExecution.METRIX_ExportBatchID = EXPORT_BATCH_ID;
            TestPath = Path.Combine(JobFolder, "LV_FileGenerationJob_Test.csv");

            Assert.IsTrue(ExecuteTest());

            /*Note: could POTENTIALLY do a test file, but would need to make sure that none of the data on the accounts included is going to change.
             That is, no data updating allowed on those accounts.
             */
            var db = _JOB.GetMetrixDatabaseManager(_Executor);
            var rc = (int)db.ExecuteText("SELECT COUNT(*) FROM EXPORT.Campaign_Account WHERE ExportBatchID = " + EXPORT_BATCH_ID).Tables[0].Rows[0][0];
            //Check number of rows exported versus EXPORT.campaign_Account for the batchID
            using (var r = _JOB.GetReader(_TestExecution.FilePath))
            {
                Assert.AreEqual(rc, r.RecordCount);
            }

            /*

DECLARE @ExportBatchID int = -35493
IF NOT EXISTS(SELECT null FROM EXPORT.ExportBatch WHERE ExportBatchID = @ExportBatchID)
BEGIN
	
	declare @ExportBatchStatusCode varchar (2) = 'SR'
	DECLARE @ExportType varchar(60) = 'LiveVox Campaign Export'

	DECLARE @ExportTypeID int, @ExportProfileID int
	SELECT @ExportTypeID = t.ExportTypeID,
		@ExportProfileID = ep.ExportProfileID
	FROM EXPORT.ExportType t
	LEFT JOIN EXPORT.ExportProfile ep
		ON t.ExportTypeID = ep.ExportTypeID
		AND ep.FacilityID is null
		AND ep.ProjectID is null
		AND ep.Active = 1
	WHERE t.Description = @ExportType

	IF @ExportProfileID IS NULL
	BEGIN
		INSERT INTO EXPORT.ExportProfile(ExportTypeID, Description, UID, RV, DC, LU)
		VALUES(@ExportTypeID, 'SEIDR - ' + @ExportType, 1, 0, GETDATE(), GETDATE())
		
		SELECT @ExportProfileID = SCOPE_IDENTITY()
	END

	SET IDENTITY_INSERT EXPORT.ExportBatch ON;

	INSERT INTO EXPORT.ExportBatch(ExportBatchID, ExportProfileID, ExportTypeID, ExportBatchStatusCode, SubmissionDate)
	VALUES(@ExportBatchID, @ExportProfileID, @ExportTypeID, @ExportBatchStatusCode,  GETDATE())	
	
	SET IDENTITY_INSERT EXPORT.ExportBatch OFF;
    
	INSERT INTO EXPORT.Campaign_Account(ExportBatchID, Campaign_AccountID)
	SELECT TOP 1000 @ExportBatchID, Campaign_AccountID
	FROM APP.Campaign_Account ca
	JOIN app.account_project ap WITH(NOLOCK) 
		on ca.accountid =ap.accountid 
		and ap.IsCurrent = 1 
		and ap.active =1 
		and ap.IsCancelled = 0  
	WHERE ca.Active = 1
	ORDER BY DC DESC
END 
             
             */

        }

    }
}
