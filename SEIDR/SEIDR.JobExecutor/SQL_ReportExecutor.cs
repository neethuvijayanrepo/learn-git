using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.DataBase;
using SEIDR;
using System.IO;

namespace SEIDR.JobExecutor
{
    class JobReport
    {
        public int JobReportID { get; private set; }
        public string Recipient { get; set; }
        public string ReportName { get; set; }
        public string SQLProcedure { get; set; }
        public string ArchiveFolder { get; set; }
        public byte Mode { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        
    }
    public class SQL_ReportExecutor : Executor
    {
        public SQL_ReportExecutor(DatabaseManager manager, JobExecutorService caller, ExecutorType type) : base(manager, caller, ExecutorType.Reporting)
        {
            reportManager = _Manager.Clone(programName: "SEIDR.ReportExecutor");
        }
        List<JobReport> SQLReportList = new List<JobReport>();

        public override int Workload => SQLReportList.Count;
            
        protected override void CheckWorkLoad()
        {
            var map = new
            {
                ProcessingDate = DateTime.Today
            };
            SQLReportList = _Manager.SelectList<JobReport>(map);
        }
        DatabaseManager reportManager;
        protected override void Work()
        {
            JobReport report = SQLReportList[0];
            SQLReportList.RemoveAt(0);
            if (string.IsNullOrWhiteSpace(report.ArchiveFolder))
                report.ArchiveFolder = $@"C:\SEIDR\Reports\{report.JobReportID}\";
                        
            Directory.CreateDirectory(report.ArchiveFolder);
            DatabaseConnection db = new DatabaseConnection(report.ServerName, report.DatabaseName);
            reportManager.ChangeConnection(db);
            string procName = report.SQLProcedure;

            var ds = reportManager.Execute(procName, mapObj: report);
            if(ds == null)
            {
                return;
            }            
            //create file from data set to be stored at ReportArchivePath, send email to list indicated by report.
            
        }
    }
}
