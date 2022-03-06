using System;
using System.Net.Mail;
using SEIDR.DataBase;
using SEIDR.JobBase;

namespace JobUnitTest.MockData
{
    public class TestExecutor : IJobExecutor
    {
        public DatabaseConnection connection { get; }
        MockDatabaseManager _mgr;
        public DatabaseManager Manager => _mgr;
        public MockDatabaseManager MockManager => _mgr;

        public TestExecutor (DatabaseConnection conn)
        {
            this.connection = conn;
            _mgr = new MockDatabaseManager(conn, DefaultSchema:nameof(SEIDR));
            _lookup = new MockDatabaseLookupSet(_mgr);
        }        
        JobExecution exec;
        public void SetExecution(JobExecution detail) => exec = detail;
        public JobProfile job => new JobProfile();

        public int ThreadID => 1;

        public string ThreadName => "Test";
        

        public bool checkAcknowledgeCancel()
        {
            return false;
        }
        JobExecutionCheckPoint chkpoint = null;
        DateTime now = DateTime.Now;
        public JobExecutionCheckPoint GetLastCheckPoint()
        {
            return chkpoint;
        }

        public JobExecutionCheckPoint LogCheckPoint(int CheckPointNumber, string Message = null, string Key = null)
        {
            DateTime n = DateTime.Now;
            int duration = (int)n.Subtract(now).TotalSeconds;
                        
            chkpoint = new JobExecutionCheckPoint(exec)
            {                
                CheckPointDuration = duration,
                CheckPointKey = Key,
                CheckPointNumber = CheckPointNumber,
                Message = Message,                
                ThreadID = ThreadID                
            };
            now = n;
            return chkpoint;
        }
        public string LogFilePath { get; set; } = null;
        public void LogError(string message, Exception ex, int? ExtraID)
        {
            writeToLogFilePath(message);
            if (ex != null)
                writeToLogFilePath(ex.ToString());
        }
        void writeToLogFilePath(string message)
        {
            System.Diagnostics.Debug.WriteLine("******* TEST EXECUTOR *****" + Environment.NewLine + message);
            if (string.IsNullOrWhiteSpace(LogFilePath))
                return;
            System.IO.File.AppendAllText(LogFilePath, DateTime.Now.ToString("yyyy/MM/dd HH:mm") + message);
        }

        public void LogInfo(string message)
        {
            writeToLogFilePath(message);
        }

        public void Requeue(int delayMinutes)
        {
            writeToLogFilePath("TEST EXECUTOR REQUEUE REQUEST: " + delayMinutes + " MINUTES.");
        }

        public void SendMail(MailMessage message)
        {
            System.Diagnostics.Debug.WriteLine("DUMMY MAIL...." + DateTime.Now.ToString());
        }
        

        public void Wait(int sleepSeconds, string logReason)
        {
            System.Threading.Thread.Sleep(sleepSeconds * 1000);
            System.Diagnostics.Debug.Write(logReason);
        }

        private readonly MockDatabaseLookupSet _lookup;
        public DatabaseConnection GetConnection(string description) => _lookup.GetConnection(description);


        public DatabaseConnection GetConnection(int LookupID) => _lookup.GetConnection(LookupID);

        public DatabaseManager GetManager(string description, bool ReadOnly = false) =>
            _lookup.GetManager(description, ReadOnly);

        public DatabaseManager GetManager(int LookupID, bool ReadOnly = false) =>
            _lookup.GetManager(LookupID, ReadOnly);
        
    }
}
