using System;

using SEIDR.DataBase;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.PGP
{
    [IJobMetaData(nameof(PGPJob), nameof(FileSystem.PGP), "PGP Operations", ConfigurationTable: "SEIDR.PGPJob",
        NeedsFilePath: false, AllowRetry: true)]
    public class PGPJob : IJob
    {
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }


        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            bool jobStatus = false;
            ValidationError error = ValidationError.None;
            try
            {
                DatabaseManager dm = jobExecutor.Manager;

                PGPConfiguration config = PGPConfiguration.GetConfiguration(dm, execution.JobProfile_JobID);

                //check for override Source and Output using profile settings
                if (string.IsNullOrEmpty(config.SourcePath) && !string.IsNullOrEmpty(execution.FilePath))
                {
                    config.SourcePath = execution.FilePath;
                    config.SourcePath = Utility.RemoveTailSlash(config.SourcePath);
                }

                if (string.IsNullOrEmpty(config.OutputPath) && !string.IsNullOrEmpty(config.SourcePath))
                {
                    config.OutputPath = config.SourcePath;
                }            

                config.SourcePath = FS.ApplyDateMask(config.SourcePath, execution.ProcessingDate);
                config.OutputPath = FS.ApplyDateMask(config.OutputPath, execution.ProcessingDate);

                string outFile = PGP.GetOutputFile(config);

                if (config.PGPOperationID == (int)PGPOperation.GenerateKey)
                {
                    jobExecutor.LogInfo($"PGP Job: {((PGPOperation)config.PGPOperationID).GetDescription()}, Private Key: {config.PrivateKeyFile}, Public Key: {config.PublicKeyFile}");
                }
                else
                {
                    jobExecutor.LogInfo($"PGP Job: {((PGPOperation)config.PGPOperationID).GetDescription()}, Source: {config.SourcePath}, Output: {(string.IsNullOrEmpty(outFile) ? config.OutputPath : outFile)}");
                }                

                PGP pgpInstance = new PGP(config);
                jobStatus = pgpInstance.Process(ref error);
                if (!jobStatus)
                {
                    status = new ExecutionStatus
                    {
                        ExecutionStatusCode = error.ToString(),
                        Description = error.GetDescription(),
                        IsError = true
                    };
                    return false;
                }
                else if (! string.IsNullOrEmpty(outFile))
                {
                    //change file path only for existing file
                    if (System.IO.File.Exists(outFile))
                    {
                        execution.FilePath = outFile;
                    }                    
                }
            }
            catch (Exception ex)
            {
                jobExecutor.LogError("PGP job failed :", ex);
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = ValidationError.PJ.ToString(),
                    Description = ValidationError.PJ.GetDescription(),
                    IsError = true
                };                
                return false;
            }

            return jobStatus;            
        }
    }
}
