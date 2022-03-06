using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;
using SEIDR.METRIX_EXPORT.Utility;

namespace SEIDR.METRIX_EXPORT
{
    public class ExportContextHelper
    {
        /// <summary>
        /// Initialize a general job context.
        /// <para>Ex: if we want to call some FileSystem job as part of our export cleanup, then we need to provide a FileSystemContext object.
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T InitializeHelperContext<T>() where T: BaseContext, new()
        {
            T help = new T();
            help.Init(Executor, Execution);
            return help;
        }
        /// <summary>
        /// Can be set to non-null for returning to the SEIDR Service.
        /// </summary>
        public ExecutionStatus ReturnStatus;

        public bool? SkipSuccessNotification { get; set; } = null;

        public static implicit operator JobExecution(ExportContextHelper context) => context.Execution;
        public static implicit operator ExportSetting(ExportContextHelper context) => context.Settings;
        public ExportContextHelper(ExportJobBase job, IJobExecutor caller, JobExecution execution, ExportSetting settings)
        {
            Executor = caller;
            Execution = execution;
            Settings = settings;
            Owner = job;
        }

        private DataBase.DatabaseManager _readMgr;
        /// <summary>
        /// Readonly version of <see cref="MetrixManager"/> 
        /// </summary>
        public DataBase.DatabaseManager MetrixReadManager
        {
            get
            {
                if (_readMgr == null)
                {
                    if (Settings == null)
                        _readMgr = Owner.GetMetrixDatabaseManager(Executor, true);
                    else
                        _readMgr = Owner.GetMetrixDatabaseManager(Executor, Settings, true)
                                    ?? Owner.GetMetrixDatabaseManager(Executor, true);
                }
                return _readMgr;
            }
            set
            {
                _readMgr = value;
            }
            
        }
        private DataBase.DatabaseManager _mgr;
        /// <summary>
        /// Get or Set the Database Manager for Connecting to Metrix.
        /// <para>If it has not been set, it will attempt to use a ReadWrite version of the DatabaseManager specified by <see cref="Settings"/>. </para>
        /// <para>If the DatabaseManager specified by settings is unavailable, will attempt to get the Manager based on Description. ( <see cref="ExportJobBase.GetMetrixDatabaseManager(IJobExecutor, bool)"/> ) </para>
        /// <para>You can change the Manager being pointed to at any time, but it should point to a ReadWrite manager.</para>
        /// </summary>
        public DataBase.DatabaseManager MetrixManager 
        {
            get
            {
                if (_mgr == null)
                {
                    if (Settings == null) //Shouldn't really happen, but potentially could? Especially for unit testing.
                                          //Only checking if something hasn't been cached, so not really worried about this, though.
                        _mgr = Owner.GetMetrixDatabaseManager(Executor);
                    else
                        _mgr = Owner.GetMetrixDatabaseManager(Executor, Settings, false)
                               ?? Owner.GetMetrixDatabaseManager(Executor, false);
                }
                return _mgr;
            }
            set
            {
                _mgr = value;
            }
        }
        private ExportJobBase Owner { get; }
        public IJobExecutor Executor { get; }

        public void LogError(string message, Exception ex = null, int? extraID = null) =>
            Executor.LogError(message, ex, extraID);

        public void LogInfo(string message) => Executor.LogInfo(message);
        public JobExecution Execution { get; }
        public long JobExecutionID => Execution.JobExecutionID ?? 0; //In practice, this should never be null...
                                                                     //Return 0 if it hasn't been initialized for a unit test or something, though
        /// <summary>
        /// Sets the <see cref="JobExecution.METRIX_ExportBatchID"/> 
        /// </summary>
        /// <param name="MetrixExportBatchID"></param>
        public void SetExportBatchID(int MetrixExportBatchID)
        {
            Execution.METRIX_ExportBatchID = MetrixExportBatchID;
        }
        /// <summary>
        /// Returns the JobExecution's METRIX_ExportBatchID, or 0 if the value is not set.
        /// </summary>
        public int ExportBatchID => Execution.METRIX_ExportBatchID ?? 0;
        /// <summary>
        /// JobExecution FileName
        /// </summary>
        public string CurrentFileName => Execution.FileName;
        /// <summary>
        /// <see cref="Execution"/> FilePath
        /// </summary>
        public string CurrentFilePath => Execution.FilePath;
        /// <summary>
        /// <see cref="Execution"/> File Hash
        /// </summary>
        public string CurrentFileHash => Execution.FileHash;
        /// <summary>
        /// <see cref="Execution"/> processing Date
        /// </summary>
        public DateTime ProcessingDate => Execution.ProcessingDate;
        /// <summary>
        /// <see cref="Execution"/> Processing Date
        /// </summary>
        public DateTime SubmissionDate => Execution.ProcessingDate;
        /// <summary>
        /// Returns whether or not <see cref="Execution"/>'s initial status is an error status or not.
        /// </summary>
        public bool IsError => Execution.IsError;
        public void UpdateFilePath(string FilePath)
        {
            Execution.SetFileInfo(FilePath);
        }
        /// <summary>
        /// Returns <see cref="IJobExecutor.GetLastCheckPoint"/> from <see cref="Executor"/> 
        /// </summary>
        /// <returns></returns>
        public JobExecutionCheckPoint GetLastCheckPoint()
        {
            return Executor.GetLastCheckPoint();
        }
        /// <summary>
        /// Logs a checkpoint using the Integer value of the ExportBatchStage as the CheckPoint's ID
        /// </summary>
        /// <param name="Stage"></param>
        /// <param name="Message"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public JobExecutionCheckPoint LogCheckPoint(ExportJobBase.ExportBatchStage Stage, string Message = null,
            string Key = null)
            => LogCheckPoint((int) Stage, Message, Key);
        /// <summary>
        /// Calls the Checkpoint logic on the underlying JobExecutor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Message"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public JobExecutionCheckPoint LogCheckPoint(int ID, string Message = null, string Key = null)
        {
            return Executor.LogCheckPoint(ID, Message, Key);
        }
        public ExportSetting Settings { get; }

        private void checkProcedureNameQualified(string procName, DataBase.DatabaseManagerHelperModel model)
        {
            if (procName != null && procName.Contains('.'))
                model.QualifiedProcedure = procName;
            else
            {
                model.Schema = "EXPORT";
                model.Procedure = procName;
            }
        }
        /// <summary>
        /// Gets a Basic Helper without an open connection, but with the ExportBatchID, SubmissionDate, and JobExecutionID values already set in the parameter dictionary.
        /// </summary>
        /// <param name="ProcedureName">The ProcedureName for executing. If null is passed, it will need to be set in the returned value afterward.</param>
        /// <returns></returns>
        public DataBase.DatabaseManagerHelperModel GetExportBatchHelperModel(string ProcedureName)
        {
            DataBase.DatabaseManagerHelperModel h = MetrixManager.GetBasicHelper();
            h[ExportJobBase.EXPORT_BATCH_ID_PARAMETER] = (object) Execution.METRIX_ExportBatchID ?? DBNull.Value;
            h[nameof(JobExecutionID)] = JobExecutionID;
            h[nameof(SubmissionDate)] = SubmissionDate; //ProcessingDate alias
            h[nameof(ProcessingDate)] = ProcessingDate; // In case the procedure has ProcessingDate instead
            h[nameof(Execution.ProjectID)] = (object)Execution.ProjectID ?? DBNull.Value;
            h["UID"] = 1;
            checkProcedureNameQualified(ProcedureName, h);
            return h;
        }

        /// <summary>
        /// Gets a Basic Helper without an open connection, but with the ExportBatchID, SubmissionDate, UID, and JobExecutionID values already set in the parameter dictionary.
        /// </summary>
        /// <returns></returns>
        public DataBase.DatabaseManagerHelperModel GetExportBatchHelperModel(int exportBatchID, string ProcedureName)
        {
            DataBase.DatabaseManagerHelperModel h = MetrixManager.GetBasicHelper();
            h[ExportJobBase.EXPORT_BATCH_ID_PARAMETER] = exportBatchID;
            h[nameof(JobExecutionID)] = JobExecutionID;
            h[nameof(SubmissionDate)] = SubmissionDate; //ProcessingDate alias
            h[nameof(ProcessingDate)] = ProcessingDate; // In case the procedure has ProcessingDate instead
            h["UID"] = 1;
            checkProcedureNameQualified(ProcedureName, h);
            return h;
        }
        /// <summary>
        /// Gets a basic Helper with an open connection (either to ReadWrite or to ReadOnly manager, depending on <paramref name="Readonly"/> ).
        /// <para>ExportBatchID and JobExecutionID values are already set in the parameter dictionary.</para>
        /// </summary>
        /// <param name="ProcedureName">The ProcedureName for executing. If null is passed, it will need to be set in the returned value afterward.</param>
        /// <param name="Readonly"></param>
        /// <returns></returns>
        public DataBase.DatabaseManagerHelperModel GetConnectedExportBatchHelperModel(string ProcedureName, bool Readonly = false)
        {
            DataBase.DatabaseManagerHelperModel h;
            if (Readonly)
                h = MetrixReadManager.GetBasicHelper(true);
            else
                h = MetrixManager.GetBasicHelper(true);

            h[ExportJobBase.EXPORT_BATCH_ID_PARAMETER] = (object)Execution.METRIX_ExportBatchID ?? DBNull.Value;
            h[nameof(JobExecutionID)] = JobExecutionID;
            h[nameof(SubmissionDate)] = SubmissionDate; //ProcessingDate alias
            h["UID"] = 1;
            checkProcedureNameQualified(ProcedureName, h);
            return h;
        }
        /// <summary>
        /// Gets a basic Helper with an open connection (either to ReadWrite or to ReadOnly manager, depending on <paramref name="Readonly"/> ).
        /// <para>ExportBatchID and JobExecutionID values are already set in the parameter dictionary.</para>
        /// </summary>
        /// <param name="exportBatchID"></param>
        /// <param name="ProcedureName">The ProcedureName for executing. If null is passed, it will need to be set in the returned value afterward.</param>
        /// <param name="Readonly"></param>
        /// <returns></returns>
        public DataBase.DatabaseManagerHelperModel GetConnectedExportBatchHelperModel(int exportBatchID, string ProcedureName, bool Readonly = false)
        {
            DataBase.DatabaseManagerHelperModel h;
            if (Readonly)
                h = MetrixReadManager.GetBasicHelper(true);
            else
                h = MetrixManager.GetBasicHelper(true);

            h[ExportJobBase.EXPORT_BATCH_ID_PARAMETER] = exportBatchID;
            h[nameof(JobExecutionID)] = JobExecutionID;
            h[nameof(SubmissionDate)] = SubmissionDate; //ProcessingDate alias
            h["UID"] = 1;
            checkProcedureNameQualified(ProcedureName, h);
            return h;
        }


        public Tuple<JobExecution, ExportBatchModel> RegisterChildExecutionWithExport(Utility.ExportChildExecutionInfo childExecutionInfo)
        {
            return this.Executor.RegisterChildExecution(childExecutionInfo, this.MetrixManager);
        }

        public JobExecution RegisterChildExecution(Utility.ExportChildExecutionInfo childExecutionInfo)
        {
            return this.Executor.RegisterChildExecution(childExecutionInfo);
        }


        /// <summary>
        /// Set during configuration of the job, pulled from table configuration.
        /// </summary>
        public string VendorName => Settings?.VendorName;

        public string ExportType => Settings?.ExportType;
        public string ImportType => Settings?.ImportType;
    }
}
