using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEIDR.DataBase;
using SEIDR.FileSystem;
using SEIDR.FileSystem.FileValidation;
using SEIDR.JobBase;
using JobUnitTest;
using SEIDR.Doc;

namespace JobUnitTest
{
    [TestClass]
    public class FileValidationJobTest:JobExecution
    {
        public FileValidationJobTest()
        {
            c = new DatabaseConnection(@"NCIHCTSTSQL07\sql2014", "MIMIR");
            mgr = new DatabaseManager(c);
        }
        DatabaseConnection c;
        DatabaseManager mgr;


        JobExecution job = JobExecution.GetSample(1, 123, 1234, 1, 1, 0, null, null, null, null, "SC", "SEIDER", @"C:\SEIDR\Source\TextDocument.txt", null, null);
        //    = new JobExecution
        //{
        //    FilePath = @"C:\SEIDR\Source\TextDocument.txt",
        //     JobProfile_JobID = 123
        //};

        ExecutionStatus Status = new ExecutionStatus
        {
            
        };
        TestExecutor test = new TestExecutor
        {
          
        };
        [TestMethod]
        public void FileValidationJobMetadataTest()
        {
            var Manager = new DatabaseManager(this.c, DefaultSchema: "SEIDR");
            using (var h = Manager.GetBasicHelper())
            {
                FileValidationJobConfiguration validate = FileValidationJobConfiguration.GetFileValidationJobConfiguration(Manager, 1);

            }
        }

        [TestMethod]
        public void FileValidationCRTest()
        {
            FileValidationJob FV = new FileValidationJob
            {
            };
            Assert.IsTrue(FV.Execute(test, job, ref Status));
        }
    }
}

