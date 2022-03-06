using System.Data;
using System.IO;
using Microsoft.SqlServer.Dts.Runtime;
using SEIDR.DataBase;

namespace SEIDR.PreProcess
{
    public class SSISExecutor
    {
        /// <summary>
        /// Procedure for getting Pre process record.
        /// </summary>
        
        readonly SSISContext _context;
        public SSISExecutor(SSISContext context)
        {
            _context = context;
            Package = new SSISPackage();
        }
        public string ServerName { get; private set; }

        public static DataRow GetConfigurationDataRow(string configurationProcedure, DatabaseManager dm, int JobProfile_JobID)
        {
            using (var helper = dm.GetBasicHelper())
            {
                helper.QualifiedProcedure = configurationProcedure;
                helper[nameof(JobProfile_JobID)] = JobProfile_JobID;
                return dm.Execute(helper).GetFirstRowOrNull();
            }

        }
        public void SetUp(string configurationProcedure, DatabaseManager dm)
        {
            DataRow configuration = GetConfigurationDataRow(configurationProcedure, dm, _context.JobProfile_JobID);
            if (configuration == null)
            {
                _context.SetStatus(ResultStatusCode.CD);
                return;
            }
            ServerName = configuration[nameof(ServerName)].ToString();
            Package.Setup(configuration, _context);
        }
        public readonly SSISPackage Package;
        /// <summary>
        /// Loads the package and prepares for execution. 
        /// </summary>
        /// <returns></returns>
        public bool LoadPackage()
        {
            Application app = new Application();

            _context.LogInfo(string.Format("Loading Package : {0} \nPackagePath : {1} \nLoading from : {2} \nJobExecutionID : {3}"
                                         , Path.GetFileNameWithoutExtension(Package.PackagePath)
                                         , Package.PackagePath
                                         , !string.IsNullOrEmpty(ServerName) ? ServerName : "(File System)"
                                         , _context.JobExecutionID));
            Package p;
            if (!string.IsNullOrEmpty(ServerName))
                p = app.LoadFromSqlServer(Package.PackagePath, ServerName, null, null, null);
            else
                p = app.LoadPackage(Package.PackagePath, null);
            return Package.MapPackage(p, _context);
        }
        public void Execute() => Package.Execute();
    }
}
