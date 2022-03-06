using SEIDR.JobBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT
{
    public abstract class ExportJobBase : IJob
    {
        public const string DEFAULT_CONFIGURATION_TABLE = "METRIX.ExportSettings";
        /// <summary>
        /// Working directory passed to <see cref="LocalFileHelper"/>. Default is <see cref="LocalFileHelper.DefaultWorkingDirectory"/> 
        /// </summary>
        public virtual string WorkingDirectory { get; } = LocalFileHelper.DefaultWorkingDirectory;

        //public abstract ResultStatusCode ProcessJobExecution(JobExecution execution, IJobExecutor caller, ExportSetting settings, ref ExecutionStatus status);
        public abstract ResultStatusCode ProcessJobExecution(ExportContextHelper context, LocalFileHelper workingFile);

        public const string EXPORT_BATCH_ID_PARAMETER = "ExportBatchID";
        protected const string METRIX_DATABASE = "METRIX";
        public const string EXPORT_SCHEMA = "EXPORT";

        #region Execution Status
        /// <summary>
        /// Translates the ResultStatusCode to an Execution StatusCode.
        /// <para>Used in the base class <see cref="ExportJobBase.Execute(IJobExecutor, JobExecution, ref ExecutionStatus)"/> method,</para>
        /// <para>based on the result of <see cref="ExportJobBase.ProcessJobExecution(JobExecution, IJobExecutor, ExportSetting, ref ExecutionStatus)"/>.
        /// </para>
        /// <para>Will *NOT* be called if the status parameter is populated, though</para>
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ExecutionStatus GetStatus(ResultStatusCode code)
        {
            return new ExecutionStatus
            {
                ExecutionStatusCode = code.ToString(),
                Description = code.GetDescription(),
                IsError = code < SUCCESS_BOUNDARY,
                IsComplete = code >= COMPLETION_BOUNDARY,
                NameSpace = nameof(METRIX_EXPORT),
                //SkipSuccessNotification = true 
                //METRIX_EXPORT statuses should never be able to send Job completion notifications.
            };
        }
        /// <summary>
        /// Minimum ResultStatusCode value for a successful Execution status (IsError = 0)
        /// </summary>
        public const ResultStatusCode SUCCESS_BOUNDARY = ResultStatusCode.SC;
        /// <summary>
        ///  Error result status for No Data (IsError = 1)
        /// </summary>
        public const ResultStatusCode NO_DATA_FAILURE_CODE = ResultStatusCode.ND;
        /// <summary>
        /// Minimum ResultStatusCode for a complete Execution status (IsError = 0, IsComplete = 1)
        /// </summary>
        public const ResultStatusCode COMPLETION_BOUNDARY = ResultStatusCode.C;
        /// <summary>
        /// Default status for a complete execution. Matches the <see cref="COMPLETION_BOUNDARY"/> 
        /// </summary>
        public const ResultStatusCode DEFAULT_COMPLETE = COMPLETION_BOUNDARY;
        /// <summary>
        /// Default status code for a successful execution.  Matches the <see cref="SUCCESS_BOUNDARY"/> 
        /// </summary>
        public const ResultStatusCode DEFAULT_RESULT = SUCCESS_BOUNDARY;
        /// <summary>
        /// Default result status for a failed execution. Preferred to return a status with details, though.
        /// </summary>
        public const ResultStatusCode DEFAULT_FAILURE_CODE = ResultStatusCode.IE;
        #endregion

        #region IJob method implementations
        public virtual int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }
        
        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            var set = GetSettings(execution, jobExecutor);
            ExportContextHelper myContext = new ExportContextHelper(this, jobExecutor, execution, set);
            var local = new LocalFileHelper(this, myContext, WorkingDirectory);
            ResultStatusCode ret;
            try
            {
                ret = ProcessJobExecution(myContext, local);
            }
            catch
            {
                if (local.Working)
                    local.ClearWork();
                throw;
            }

            if (myContext.ReturnStatus != null)
            {
                status = myContext.ReturnStatus;
                if (local.Working)
                {
                    if (status.IsError)
                        local.ClearWork();
                    else
                        local.Finish();
                }
                if (myContext.SkipSuccessNotification.HasValue)
                    status.SkipSuccessNotification = myContext.SkipSuccessNotification.Value;
                return !status.IsError && ret >= SUCCESS_BOUNDARY;
            }
            status = GetStatus(ret);
            if (myContext.SkipSuccessNotification.HasValue)
                status.SkipSuccessNotification = myContext.SkipSuccessNotification.Value;
            if (local.Working)
            {
                if (status.IsError)
                    local.ClearWork();
                else
                    local.Finish();
            }
            return !status.IsError;
        }
        #endregion

        #region progress checkpointing
        /// <summary>
        /// Stage of progress for the job
        /// </summary>
        public enum ExportBatchStage
        {
            /// <summary>
            /// ExportBatch has not been started.
            /// </summary>
            NOT_STARTED = 0,
            /// <summary>
            /// Setting up the ExportBatch record, getting export batch model.
            /// </summary>
            SETUP = 1,
            /// <summary>
            /// Populating any working tables in the Metrix Database
            /// </summary>
            DATA_PREP = 2,
            /// <summary>
            /// Pulling data from database to write to a file
            /// </summary>
            DATA_PULL = 3,
            /// <summary>
            /// Data has been pulled into the end file.
            /// </summary>
            FINALIZED
        }

        public JobExecutionCheckPoint GetCheckPoint(ExportContextHelper context)
        {
            return context.Executor.GetLastCheckPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="FileTable">If populating multiple tables, can specify which table is being populated.</param>
        /// <returns></returns>
        public JobExecutionCheckPoint SetCheckPoint_DataPrep(ExportContextHelper context, string FileTable = null)
        {
            return context.LogCheckPoint(ExportBatchStage.DATA_PREP, Key: FileTable);
        }
        /// <summary>
        /// Checks the current stage of progress for the job.
        /// </summary>
        /// <param name="chk"></param>
        /// <returns></returns>
        public ExportBatchStage CheckStage(JobExecutionCheckPoint chk)
        {
            if (chk == null)
                return ExportBatchStage.NOT_STARTED;
            return (ExportBatchStage) chk.CheckPointNumber;
        }
        
        public ExportBatchStage CheckStage(ExportContextHelper context)
        {
            return CheckStage(context.Executor.GetLastCheckPoint());
        }

        /// <summary>
        /// Logs a check point indicating that we are doing file creation ( <see cref="ExportBatchStage.DATA_PULL"/> )
        /// </summary>
        /// <param name="context"></param>
        /// <param name="outputFileName"></param>
        /// <returns></returns>
        public JobExecutionCheckPoint SetCheckPoint_FileCreation(ExportContextHelper context,
            string outputFileName = null)
        {
            return context.LogCheckPoint(ExportBatchStage.DATA_PULL, Key: outputFileName);
        }


        /// <summary>
        /// Log a checkpoint indicating that ExportBatch set up is done.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public JobExecutionCheckPoint SetCheckPoint_ExportBatchCreation(ExportContextHelper context)
        {
            return context.LogCheckPoint(ExportBatchStage.SETUP);

        }

        public JobExecutionCheckPoint SetCheckPoint_Finalize(ExportContextHelper context)
        {
            return context.LogCheckPoint(ExportBatchStage.FINALIZED);
        }
        
        #endregion

        #region Export Batch utility methods

        /// <summary>
        /// Allow updating the RecordCount, DateFrom, DateThrough, OutputFilePath, ExportBatchStatusCode, and/or Active (DD) on the ExportBatch
        /// <para>Note: if the export batch is deactivated, the checkpoint will revert to stage 0</para>
        /// </summary>
        /// <param name="context">ExportContext for the JobExecutor, JobExecution, Export Settings, and Metrix Database Connection that we want to use.</param>
        /// <param name="batch"></param>
        public void UpdateExportBatch(ExportContextHelper context, ExportBatchModel batch)
        {
            var manager = context.MetrixManager;
            using (var help = manager.GetBasicHelper())
            {
                help.QualifiedProcedure = "EXPORT.usp_ExportBatch_u_SEIDR";
                help.ParameterMap = batch;
                manager.Execute(help);
            }

            if (!batch.Active)
                context.LogCheckPoint(ExportBatchStage.NOT_STARTED, "Deactivated Export batch");
            else if (batch.ExportBatchStatusCode == ExportStatusCode.SI)
                context.LogCheckPoint( ExportBatchStage.DATA_PULL,
                    "ExportBatchStatusCode set to SI - SEIDR Intermediate completion.");
            else if(batch.ExportBatchStatusCode == ExportStatusCode.SQ)
                context.LogCheckPoint(ExportBatchStage.DATA_PREP, "ExportBatchStatusCode set to 'Queued' - Working from SEIDR Queue");

        }

        /// <summary>
        /// Uses export settings within the export context, and the JobExecution's <see cref="JobExecution.METRIX_ExportBatchID"/> to get an ExportBatchModel instance
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ExportType"></param>
        /// <param name="ignoreCurrentExportBatchID">Indicate that we should ignore the value of <see cref="ExportContextHelper.ExportBatchID"/>
        /// from <paramref name="context"/>. <para>
        ///Example usage: if our job might run as a follow-up to a previous ExportBatchID, or if we might need to run a second ExportType in the same profile.
        /// </para> </param>
        /// <returns></returns>
        public ExportBatchModel GetExportBatch(ExportContextHelper context, string ExportType, bool ignoreCurrentExportBatchID = false)
        {
            if(!ignoreCurrentExportBatchID && context.Execution.METRIX_ExportBatchID.HasValue)
                return GetExportBatch(context.MetrixManager, context.ExportBatchID);
            //return GetExportBatch(context.Execution, context.Executor, ExportType, context.MetrixReadManager);

            if (CheckStage(context) < ExportBatchStage.SETUP)
                throw new InvalidOperationException("Setup stage has not been completed yet.");
            var manager = context.MetrixManager;
            using (var help = manager.GetBasicHelper())
            {
                help.QualifiedProcedure = "EXPORT.usp_ExportBatch_ss_SEIDR";
                help[nameof(ExportType)] = ExportType;
                help.ParameterMap = context.Execution;
                help.DoMapUpdate = true;
                return manager.SelectSingle<ExportBatchModel>(help);
            }
        }
        /// <summary>
        /// Gets the ExportBatchModel for a specific ExportBatchID, regardless of checkpointed stage/progress (<see cref="CheckStage(JobExecutionCheckPoint)"/> )
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="ExportBatchID"></param>
        /// <returns></returns>
        public ExportBatchModel GetExportBatch(DataBase.DatabaseManager manager, int ExportBatchID)
        {
            using (var help = manager.GetBasicHelper())
            {
                help.QualifiedProcedure = "EXPORT.usp_ExportBatch_ss_ExportBatchModel";
                help[nameof(ExportBatchID)] = ExportBatchID;
                return manager.SelectSingle<ExportBatchModel>(help);
            }
        }
        /// <summary>
        /// Uses ExportContext to begin an export batch in Metrix. Sets the parameter map to <paramref name="execution"/>
        /// <para>If the helper has a connection already, then that will be used. Also - if it does not have an open transaction on this connection, a new transaction will open</para>
        /// <para>If we have already gone through setup, an InvalidOperationException will be thrown.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ExportType"></param>
        /// <param name="helper">Allow passing an existing helper so that the transaction can be committed outside of this method call, if needed.</param>
        /// <returns></returns>
        public ExportBatchModel BeginExportBatch(ExportContextHelper context, string ExportType,
            DataBase.DatabaseManagerHelperModel helper)
        {
            //return BeginExportBatch(context.Execution, context.Executor, ExportType, helper, context.MetrixManager);
            if(CheckStage(context) >= ExportBatchStage.SETUP)
                throw new InvalidOperationException("Export Batch has already been created for this JobProfile_JobID during a previous call.");
            if (helper.Connection != null && !helper.HasOpenTran)
                helper.BeginTran();

            helper.QualifiedProcedure = Utility.JobExecutionHelper.EXPORT_BATCH_I_SEIDR;
            helper[nameof(ExportType)] = ExportType;
            helper.ParameterMap = context.Execution;
            helper["IsImport"] = false;
            helper.DoMapUpdate = true;
            //var db = GetMetrixDatabaseManager(caller);
            var batch = context.MetrixManager.SelectSingle<ExportBatchModel>(helper);
            context.LogCheckPoint(ExportBatchStage.SETUP, "ExportBatchID = " + context.ExportBatchID);
            return batch;
        }

        public Tuple<JobExecution, ExportBatchModel> CreateImportJobExecutionBatch(ExportContextHelper context,
            Utility.ExportChildExecutionInfo child, int? FacilityID = null)
        {
            var seidrDB = context.Executor.Manager;
            using (var executionHelper = seidrDB.GetBasicHelper(true))
            using(var metHelper = context.MetrixManager.GetBasicHelper())
            {
                executionHelper.BeginTran();
                executionHelper.QualifiedProcedure = Utility.JobExecutionHelper.JOB_EXECUTION_I_SS;
                executionHelper.ParameterMap = child;
                var je = seidrDB.SelectSingle<JobExecution>(executionHelper);


                metHelper.DoMapUpdate = true;
                metHelper[nameof(FacilityID)] = (object)FacilityID ?? DBNull.Value;
                var import = CreateImportBatch(context, child.FilePath, metHelper, je);
                if (import == null)
                {
                    executionHelper.RollbackTran();
                    throw new Exception("Unable to register import batch");
                }

                je.METRIX_ExportBatchID = import.ExportBatchID;

                executionHelper.QualifiedProcedure = Utility.JobExecutionHelper.JOB_EXECUTION_U_BATCHID;
                executionHelper.ParameterMap = je;
                seidrDB.Execute(executionHelper);

                if(metHelper.HasOpenTran)
                    metHelper.CommitTran();
                executionHelper.CommitTran();
                return new Tuple<JobExecution, ExportBatchModel>(je, import);
            }

            
        }


        public ExportBatchModel CreateImportBatch(ExportContextHelper context, 
            string FilePath,
            DataBase.DatabaseManagerHelperModel helper, JobExecution execution)
        {
            if (helper.Connection != null && !helper.HasOpenTran)
                helper.BeginTran();
            string ExportType = context.ImportType;
            if (string.IsNullOrWhiteSpace(ExportType))
                throw new InvalidOperationException("Import Type is not specified.");

            helper.QualifiedProcedure = Utility.JobExecutionHelper.EXPORT_BATCH_I_SEIDR;
            helper.ParameterMap = execution; //Set parameter map first so that the following adds to the property ignore
            helper[nameof(ExportType)] = ExportType;
            helper[nameof(FilePath)] = FilePath;
            helper["IsImport"] = true;
            //var db = GetMetrixDatabaseManager(caller);
            var batch = context.MetrixManager.SelectSingle<ExportBatchModel>(helper);
            return batch;
        }


        /// <summary>
        /// Use export context to create an import batch in Metrix.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ImportFilePath">File path for expected import.</param>
        /// <param name="doMapUpdate"></param>
        /// <param name="FacilityID">Optional facility - if the export batch is dealing with data at individual facility level.</param>
        /// <param name="execution"></param>
        /// <returns></returns>
        public ExportBatchModel CreateImportBatch(ExportContextHelper context, string ImportFilePath, 
                bool doMapUpdate = false, int? FacilityID = null, JobExecution execution = null)
        {
            using (var help = context.MetrixManager.GetBasicHelper())
            {
                help.DoMapUpdate = doMapUpdate;
                help[nameof(FacilityID)] = (object)FacilityID ?? DBNull.Value;
                return CreateImportBatch(context, ImportFilePath,  help, execution ?? context.Execution);
            }
        }
        /// <summary>
        /// Uses ExportContext to begin an export batch in Metrix. Sets the parameter map to <paramref name="execution"/>
        /// <para>If the helper has a connection already, then that will be used. Also - if it does not have an open transaction on this connection, a new transaction will open</para>
        /// <para>If we have already gone through setup, an InvalidOperationException will be thrown.</para>
        /// </summary>
        public ExportBatchModel BeginExportBatch(ExportContextHelper context, string ExportType)
        {
            using (var help = context.MetrixManager.GetBasicHelper())
            {
                return BeginExportBatch(context, ExportType, help);
            }
        }

        #endregion

        /// <summary>
        /// Given a <see cref="LocalFileHelper"/>, a Delimiter, and a set of columns, return a default <see cref="Doc.DocWriter"/> for writing a delimited file to the working path.
        /// <para>NOTE: <see cref="LocalFileHelper.Finish"/> should not be called until the <see cref="Doc.DocWriter"/>  is disposed.</para>
        /// </summary>
        /// <param name="localFile"></param>
        /// <param name="Delimiter"></param>
        /// <param name="Columns"></param>
        /// <returns></returns>
        public Doc.DocWriter GetWriter(LocalFileHelper localFile, char Delimiter, params string[] Columns)
        {
            return new Doc.DocWriter(localFile.GetDocMetaData(Delimiter, Columns));
        }
        /// <summary>
        /// Gets a basic DocReader object for reading delimited file.
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        public Doc.DocReader GetReader(string FilePath)
        {
            return new Doc.DocReader("EX", FilePath);
        }

        ExportSetting GetSettings(JobExecution execution, IJobExecutor caller)
        {
            using (var help = caller.Manager.GetBasicHelper())
            {
                help.QualifiedProcedure = "[METRIX].[usp_ExportSetting_ss]";
                help[nameof(execution.JobProfile_JobID)] = execution.JobProfile_JobID;
                return caller.Manager.SelectSingle<ExportSetting>(help);
            }
        }
        /// <summary>
        /// Gets a DatabaseManager for calls to the Metrix Database. Uses the Export Settings, if available.
        /// <para>Also changes the Database Manager pointed to by <see cref="ExportContextHelper.MetrixManager"/> or <see cref="ExportContextHelper.MetrixReadManager"/> (depending on <paramref name="ReadOnly"/> value)  </para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ReadOnly"></param>
        /// <returns></returns>
        public DataBase.DatabaseManager GetMetrixDatabaseManager(ExportContextHelper context, bool ReadOnly = false)
        {
            DataBase.DatabaseManager db;
            if (context.Settings != null)
            {
                db = GetMetrixDatabaseManager(context.Executor, context.Settings, ReadOnly);
                if (db != null)
                {
                    if (!ReadOnly)
                        context.MetrixManager = db;
                    else
                        context.MetrixReadManager = db;
                    return db;
                }
            }
            db = GetMetrixDatabaseManager(context.Executor, ReadOnly);
            if(!ReadOnly)
                context.MetrixManager = db;
            else
                context.MetrixReadManager = db;
            return db;
        }

        /// <summary>
        /// Gets a database manager for calls to the METRIX database, by using the LookupID from the Export Settings
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="settings"></param>
        /// <param name="ReadOnly"></param>
        /// <returns></returns>
        public DataBase.DatabaseManager GetMetrixDatabaseManager(IJobExecutor caller, ExportSetting settings, bool ReadOnly = false)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            return caller.GetManager(settings.MetrixDatabaseLookupID, ReadOnly);
        }
        /// <summary>
        /// Gets a database manager for calls to the METRIX database, by doing a lookup comparison of the description from <see cref="METRIX_DATABASE"/> 
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="ReadOnly"></param>
        /// <returns></returns>
        public DataBase.DatabaseManager GetMetrixDatabaseManager(IJobExecutor caller, bool ReadOnly = false)
        {
            return caller.GetManager(METRIX_DATABASE, ReadOnly);
        }

        public static string PrepForFileName(string input)
        {
            if (input == null)
                return null;

            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = $@"[{invalidChars},\s]+";
            return Regex.Replace(input.Replace(".", ""), 
                                 invalidReStr, "_")
                        .TrimEnd('_');
        }
    }
}
