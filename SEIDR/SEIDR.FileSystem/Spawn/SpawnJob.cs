using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SEIDR.DataBase;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.SpawnJob
{ 
    [IJobMetaData(nameof(SpawnJob), 
        nameof(FileSystem.SpawnJob), "Spawn Job",
        ConfigurationTable: "SEIDR.SpawnJob",
        NeedsFilePath: false,
        AllowRetry: true)]
    public class SpawnJob : IJob
    {
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            bool jobStatus = false;
            DataTable spawnJobTable = null;
            try
            {
                DatabaseManager dm = jobExecutor.Manager;
                spawnJobTable = new DataTable("udt_SpawnJob");
                spawnJobTable.AddColumns<SpawnConfiguration>();
                
                spawnJobTable.Columns.Add("FileSize", typeof(int));
                spawnJobTable.Columns.Add("FileHash", typeof(string));


                List<SpawnConfiguration> configs = SpawnConfiguration.GetConfiguration(dm, execution.JobProfile_JobID);
                if (configs.Count == 0)
                {
                    throw new Exception(String.Format("Spawn Job for JobProfile_JobID={0} is not configured at SEIDR.SpawnJob table.", execution.JobProfile_JobID));
                }
                int spawnJobID = 1;
                foreach (SpawnConfiguration config in configs)
                {
                    if (string.IsNullOrEmpty(config.SourceFile)) //Single file is being used by multiple processes
                    {
                        DataRow row = spawnJobTable.NewRow();
                        row["SpawnJobID"] = spawnJobID;
                        row["JobProfile_JobID"] = config.JobProfile_JobID;
                        row["JobProfileID"] = config.JobProfileID;
                        row["SourceFile"] = execution.FilePath;
                        row["FileSize"] = execution.FileSize; 
                        row["FileHash"] = execution.FileHash;
                        spawnJobTable.Rows.Add(row);
                        spawnJobID++;
                        config.FileCounter = 1;
                        jobExecutor.LogInfo($"Spawn Job (ID: {config.SpawnJobID}) - File Info taken from Spawning JobExecution");
                        continue;
                        
                    }
                                        
                    config.SourceFile = FS.ApplyDateMask(config.SourceFile, execution.ProcessingDate);
                    string[] files = Utility.GetFiles(config.SourceFile, "*.*");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);                        
                        DataRow row = spawnJobTable.NewRow();
                        row["SpawnJobID"] = spawnJobID;
                        row["JobProfile_JobID"] = config.JobProfile_JobID;
                        row["JobProfileID"] = config.JobProfileID;
                        row["SourceFile"] = file;
                        row["FileSize"] = fi.Length;
                        row["FileHash"] = Doc.DocExtensions.GetFileHash(fi);
                        spawnJobTable.Rows.Add(row);
                        spawnJobID++;
                        config.FileCounter++;
                    }
                    if(config.FileCounter > 0)
                        jobExecutor.LogInfo($"Spawn Job (ID: {config.SpawnJobID}, FilePath: {config.SourceFile}) - {config.FileCounter} Files found.");
                    else
                        jobExecutor.LogError($"Spawn Job (ID: {config.SpawnJobID}, FilePath: {config.SourceFile}) - no Files found.");
                }
                if(configs.Exists(c => c.FileCounter == 0))
                {
                    status = new ExecutionStatus
                    {
                        ExecutionStatusCode = "MF",
                        Description = "Missing Files For SpawnJob Execution",
                        IsError = true
                    };
                    jobStatus = false;
                }
                else //Each config shoudl have at least one file.
                {
                    using (var helper = dm.GetBasicHelper())
                    {
                        helper.ExpectedReturnValue = 0;
                        helper["SpawningJobExecutionID"] = execution.JobExecutionID;
                        helper["ProcessingDate"] = execution.ProcessingDate;
                        helper["SpawnJobList"] = spawnJobTable;
                        helper.QualifiedProcedure = "SEIDR.usp_JobExecution_RegisterSpawnJob";
                        dm.ExecuteNonQuery(helper);
                        if (helper.ReturnValue != helper.ExpectedReturnValue)
                        {
                            status = new ExecutionStatus
                            {
                                ExecutionStatusCode = "SF",
                                Description = "Spawn Job - Unexpected Result from Register",
                                IsError = true
                            };
                        }
                        else
                        {
                            jobExecutor.LogInfo("Spawn Job has been added for JobProfile_JobID" + execution.JobProfile_JobID.ToString());
                            jobStatus = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                jobExecutor.LogError("Spawn job failed :", ex);
                //If the status doesn't indicate what type of failure, then it's fine to leave status null and let the JobExecutor set it to 'SEIDR.F'
                /* 
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "SF",
                    Description = ValidationError.SF.GetDescription(),
                    IsError = true
                };*/
                return false;
            }
            finally
            {
                if (spawnJobTable != null)
                {
                    spawnJobTable.Dispose();
                }
            }

            return jobStatus;
        }

    }
}
