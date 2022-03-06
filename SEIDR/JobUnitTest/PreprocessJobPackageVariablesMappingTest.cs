using System;
using System.IO;
using System.Data;
using JobUnitTest.MockData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.JobBase;
using SEIDR.PreProcess;

namespace JobUnitTest
{
    [TestClass]
    public class PreprocessJobPackageVariablesMappingTest : JobTestBase<PreProcessJob>
    {
        //ToDo: Rewrite tests with the refactored version ( was tested from Service calls rather than unit tests )...
        DatabaseManager mgr => MockManager;
        ExecutionStatus Status = new ExecutionStatus();
        private TestExecutor test => _Executor;
        PreProcessJob ppj => _JOB;

        // need to set absolute file path to the package file
        //readonly string InitPackageFullFileName = @"D:\Navigant\SEIDR_ROOT\SEIDR_VS2017_3\JobUnitTest\FileResource folder\TEST_PACK_VariableOnly.dtsx";
        readonly string InitPackageFullFileName = @"...\JobUnitTest\FileResource folder\TEST_PACK_VariableOnly.dtsx";

        JobExecution job;
        DataRow _preProcessConfig;
        //PreProcessJobService _preProcessJobService;
        /*
        public PreprocessJobPackageVariablesMappingTest()
        {

            //job = JobExecution.GetSample(2351, 19, 44, 6, 1, 0, null, null, null, null, "SC", "SEIDR", @"c:\SEIDR\Demo\any.txt", null, null);
            job = JobExecution.GetSample(21320501, 19, 44, 6, 1, 0, null, null, null, null, "SC", "SEIDR", @"\ncihctstsql07.nciwin.local\SEIDR_QA\RegistrationGM\_Registered\TestFile (2).txt", null, null);
            _preProcessConfig = PreProcessConfiguration.GetPreProcessConfiguration(mgr, job.JobProfile_JobID);
            //_preProcessJobService = new PreProcessJobService(_preProcessConfig, job);
        }

        private string Init(string VariableNameToReplace, string VariableNameReplaceWith)
        {
            var TestedPackageFullFileName = Path.GetDirectoryName(InitPackageFullFileName) + @"\VarTested_" + Path.GetFileName(InitPackageFullFileName);

            if (File.Exists(TestedPackageFullFileName))
                File.Delete(TestedPackageFullFileName);

            string contents_replaced;
            using (StreamReader reader = new StreamReader(InitPackageFullFileName))               
                contents_replaced = reader.ReadToEnd().Replace(VariableNameToReplace, VariableNameReplaceWith);

            File.WriteAllText(TestedPackageFullFileName, contents_replaced);
            _preProcessConfig["PackagePath"] = TestedPackageFullFileName;
          
            return TestedPackageFullFileName;
        }

        [TestMethod]
        public void PackagePath_IsNull()
        {
            JobExecution job = JobExecution.GetSample(2351, 19, 44, 6, 1, 0, null, null, null, null, "SC", "SEIDR", null, null, null);
            var _preProcessConfig = PreProcessConfiguration.GetPreProcessConfiguration(mgr, job.JobProfile_JobID);
            _preProcessConfig["PackagePath"] = DBNull.Value;

            PreProcessJobService _preProcessJobService = new PreProcessJobService(_preProcessConfig, job);
            ExecutionStatus status = new ExecutionStatus();
            _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(status.IsError);
            Assert.IsTrue(status.Description == "PreProcess job failed. Package path not set.");
        }

        // -------MET-12971------- Mandatory Variables: only mandatory variable should be the InputFile at the moment ---------------------------- 
        // "InputFile" --------------------------------------------------------------------
        [TestMethod]
        [ExpectedException(typeof(Exception), "Package does not contain variable 'InputFile'")]
        public void InputFile_Package_Mandatory_Variable_Is_Absent_In_The_Package()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("InputFile", "AbsentField");

            //var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);
        }       

        // --- OPTIONAL VARIABLES: When preparing to execute the package, the following variables should be overridden if provided
        //--------------------------------------------------------------------------------
        //Optional variable is anything that doesn't throw an exception if it's not in the package.
        //Yes, they will only be checked if the configuration value from the database is not null. 
        //We'll still verify the variables to make sure it's not in the package with the wrong case.

        // If the variable isn't in the package, then the configuration doesn't matter.

        // "ReconciliationDate" --------------------------------------------------------------------
        [TestMethod]
        public void B_ReconciliationDate_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            var TestedPackageFullFileName = Init("ReconciliationDate", "AbsentField");

            ExecutionStatus status = new ExecutionStatus();
            //var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_ReconciliationDate_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Providedd()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ReconciliationDate", "Reconciliationdate");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        //ProcessingDate: sonner of all not needed----------------------------------------------------------
        [TestMethod]
        public void B_ProcessingDate_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            var TestedPackageFullFileName = Init("ProcessingDate", "AbsentField");

            ExecutionStatus status = new ExecutionStatus();
            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_ProcessingDate_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ProcessingDate", "Processingdate");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        //FileDate----------------------------------------------------------
        [TestMethod]
        public void B_FileDate_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            var TestedPackageFullFileName = Init("FileDate", "AbsentField");

            ExecutionStatus status = new ExecutionStatus();
            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_FileDate_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("FileDate", "Filedate");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        //InputFileDate----------------------------------------------------------
        [TestMethod]
        public void B_InputFileDate_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            var TestedPackageFullFileName = Init("InputFileDate", "AbsentField");

            ExecutionStatus status = new ExecutionStatus();
            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_InputFileDate_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("InputFileDate", "InputFiledate");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        // "JobExecutionID" --------------------------------------------------------------------
        [TestMethod]
        public void B_JobExecutionID_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("JobExecutionID", "AbsentField");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_JobExecutionID_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("JobExecutionID", "JobExecutionid");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        //JobProfile_JobID  ------------------------------------------------------------
        [TestMethod]
        public void B_JobProfile_JobID_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("JobProfile_JobID", "AbsentField");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_JobProfile_JobID_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("JobProfile_JobID", "JobProfile_jobID");

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        //ServerInstanceName  ------------------------------------------------------
        [TestMethod]
        public void C_ServerInstanceName_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("ServerInstanceName", "ServerInstanceName");
            _preProcessConfig["ServerInstanceName"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_ServerInstanceName_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("ServerInstanceName", "ServerInstanceName");
            _preProcessConfig["ServerInstanceName"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_ServerInstanceName_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ServerInstanceName", "AbsentField");

            _preProcessConfig["ServerInstanceName"] = "";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_ServerInstanceName_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ServerInstanceName", "AbsentField");

            _preProcessConfig["ServerInstanceName"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");
            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_ServerInstanceName_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ServerInstanceName", "ServerinstanceName");

            _preProcessConfig["ServerInstanceName"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_ServerInstanceName_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ServerInstanceName", "ServerinstanceName");

            _preProcessConfig["ServerInstanceName"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        // OutputFolder
        [TestMethod]
        public void C_OutputFolder_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("OutputFolder", "OutputFolder");
            _preProcessConfig["OutputFolder"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_OutputFolder_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("OutputFolder", "OutputFolder");
            _preProcessConfig["OutputFolder"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_OutputFolder_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("OutputFolder", "AbsentField");

            _preProcessConfig["OutputFolder"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_OutputFolder_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("OutputFolder", "AbsentField");

            _preProcessConfig["OutputFolder"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_OutputFolder_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("OutputFolder", "Outputfolder");

            _preProcessConfig["OutputFolder"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_OutputFolder_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("OutputFolder", "Outputfolder");

            _preProcessConfig["OutputFolder"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        // --- OPTIONAL VARIABLES: other columns from the configuration table should be treated as optional variables
        //--------------------------------------------------------------------------------
        //Optional variable is anything that doesn't throw an exception if it's not in the package.
        //Yes, they will only be checked if the configuration value from the database is not null. 
        //We'll still verify the variables to make sure it's not in the package with the wrong case

        // Category
        [TestMethod]
        public void C_Category_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("Category", "Category");
            _preProcessConfig["Category"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_Category_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("Category", "Category");
            _preProcessConfig["Category"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_Category_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("Category", "AbsentField");

            _preProcessConfig["Category"] = "";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_Category_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("Category", "AbsentField");

            _preProcessConfig["Category"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_Category_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("Category", "category");

            _preProcessConfig["Category"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_Category_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("Category", "category");

            _preProcessConfig["Category"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        // AndromedaServer
        [TestMethod]
        public void C_AndromedaServer_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("AndromedaServer", "AndromedaServer");
            _preProcessConfig["AndromedaServer"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_AndromedaServer_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("AndromedaServer", "AndromedaServer");
            _preProcessConfig["AndromedaServer"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_AndromedaServer_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("AndromedaServer", "AbsentField");

            _preProcessConfig["AndromedaServer"] = "";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_AndromedaServer_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("AndromedaServer", "AbsentField");

            _preProcessConfig["AndromedaServer"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_AndromedaServer_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("AndromedaServer", "Andromedaserver");

            _preProcessConfig["AndromedaServer"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_AndromedaServer_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("AndromedaServer", "Andromedaserver");

            _preProcessConfig["AndromedaServer"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        // PackageID
        [TestMethod]
        public void C_PackageID_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("PackageID", "PackageID");
            _preProcessConfig["PackageID"] = 33;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_PackageID_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("PackageID", "PackageID");
            _preProcessConfig["PackageID"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_PackageID_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("PackageID", "AbsentField");

            _preProcessConfig["PackageID"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_PackageID_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("PackageID", "AbsentField");

            _preProcessConfig["PackageID"] = "22";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_PackageID_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("PackageID", "PackageId");

            _preProcessConfig["PackageID"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_PackageID_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("PackageID", "PackageId");

            _preProcessConfig["PackageID"] = 22;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }


        // CB
        [TestMethod]
        public void C_CB_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("CB", "CB");
            _preProcessConfig["CB"] = "NCIWIN\testuser";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_CB_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("CB", "CB");
            _preProcessConfig["CB"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_CB_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("CB", "AbsentField");

            _preProcessConfig["CB"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_CB_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("CB", "AbsentField");

            _preProcessConfig["CB"] = "NCIWIN\testuser";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_CB_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("CB", "cb");

            _preProcessConfig["CB"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_CB_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("CB", "cb");

            _preProcessConfig["Category"] = "NCIWIN\testuser";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        //Name
        [TestMethod]
        public void C_Name_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("Name", "Name");
            _preProcessConfig["Name"] = "Test";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_Name_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("Name", "Name");
            _preProcessConfig["Name"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_Name_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init(">Name</DTS:Property>", ">AbsentField</DTS:Property>");

            _preProcessConfig["Name"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_Name_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init(">Name</DTS:Property>", ">AbsentField</DTS:Property>");

            _preProcessConfig["Name"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_Name_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init(">Name</DTS:Property>", ">name</DTS:Property>");

            _preProcessConfig["Name"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_Name_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init(">Name</DTS:Property>", ">name</DTS:Property>");

            _preProcessConfig["Name"] = "TEST";

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_ServerName_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("ServerName", "ServerName");
            _preProcessConfig["ServerName"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void D_ServerName_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ServerName</DTS:Property>", "AbsentField</DTS:Property>");

            _preProcessConfig["ServerName"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_ServerName_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("ServerName</DTS:Property>", "Servername</DTS:Property>");

            _preProcessConfig["ServerName"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }       

        // IsValid
        [TestMethod]
        public void C_IsValid_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("IsValid", "IsValid");
            _preProcessConfig["IsValid"] = true;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void G_IsValid_Package_Optional_Variable_Is_Present_In_The_Package_Value_Is_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();

            var TestedPackageFullFileName = Init("IsValid", "IsValid");
            _preProcessConfig["IsValid"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }


        [TestMethod]
        public void D_IsValid_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("IsValid", "AbsentField");

            _preProcessConfig["IsValid"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_IsValid_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("IsValid", "AbsentField");

            _preProcessConfig["IsValid"] = true;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void E_IsValid_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Not_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("IsValid", "Isvalid");

            _preProcessConfig["IsValid"] = DBNull.Value;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void A_IsValid_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Is_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("IsValid", "Isvalid");

            _preProcessConfig["IsValid"] = true;

            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        // PackagePath 
        [TestMethod]
        public void A_PackagePath_Package_Optional_Variable_Case_Not_Matched_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("PackagePath</DTS:Property>", "Packagepath</DTS:Property>");
            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(!result);
            Assert.IsTrue(status.Description == "FAILURE");

            File.Delete(TestedPackageFullFileName);
        }

        [TestMethod]
        public void B_PackagePath_Package_Optional_Variable_Is_Absent_In_The_Package_Value_Always_Provided()
        {
            ExecutionStatus status = new ExecutionStatus();
            var TestedPackageFullFileName = Init("PackagePath</DTS:Property>", "AbsentField</DTS:Property>");
            var result = _preProcessJobService.ExecutePackage(ppj, test, ref status);

            Assert.IsTrue(result);
            Assert.IsTrue(status.Description == "Success");

            File.Delete(TestedPackageFullFileName);
        }
        */

    }
}
