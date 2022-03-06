using SEIDR.DataBase;
using SEIDR.JobBase;
using System;
using System.IO;

namespace SEIDR.FileSystem.FTP
{
    
    [IJobMetaData(nameof(FTPJob), nameof(FileSystem.FTP), "FTP File Operations", ConfigurationTable: "SEIDR.FTPJob",
        NeedsFilePath: false, AllowRetry: true)]
    public class FTPJob : ContextJobBase<FTPContext>
    {
        public const string WILDCARD = "*";
        public override void Process(FTPContext context)
        {
            DatabaseManager dm = context.Manager;
            FTPConfiguration ftpConfig =  FTPConfiguration.GetFTPConfiguration(dm, context.JobProfile_JobID);
            ftpConfig.ProcessingDate = context.ProcessingDate;

            //If remoteIsFolder and receiving with an empty local path - then we're making use of the JobExecution file path information to drive the receive.
            bool remoteIsFolder = ftpConfig.RemotePath != null && (ftpConfig.RemotePath.EndsWith("/") || ftpConfig.RemotePath.EndsWith("\\"));

            if (string.IsNullOrEmpty(ftpConfig.LocalPath) 
               && (ftpConfig.Operation == FTPOperation.SEND 
                   || ftpConfig.Operation == FTPOperation.RECEIVE && remoteIsFolder
                   )
               )
            {
                context.LogInfo("No LocalPath Configured - Using JobExecution Path");
                ftpConfig.LocalPath = context.CurrentFilePath; //If doing a send and local path is null, set it to the JobExecution path.
            } 
            else
                ftpConfig.LocalPath = FS.ApplyDateMask(ftpConfig.LocalPath, ftpConfig.ProcessingDate);

            ftpConfig.RemotePath = FS.ApplyDateMask(ftpConfig.RemotePath, ftpConfig.ProcessingDate);
            ftpConfig.RemoteTargetPath = FS.ApplyDateMask(ftpConfig.RemoteTargetPath, ftpConfig.ProcessingDate);

            if (ftpConfig.OperationName == nameof(FTPOperation.MOVE_REMOTE))
            {
                //MOVE_REMOTE does not need to check LocalPath
                if (string.IsNullOrWhiteSpace(ftpConfig.RemoteTargetPath))
                {
	                context.SetStatus(FTPResult.RT);
	                //MOVE_REMOTE does not need to check LocalPath, remote remote paths.
	                return;
				}
            }
            else if (string.IsNullOrWhiteSpace(ftpConfig.LocalPath) 
                || (!ftpConfig.LocalPath.Contains(WILDCARD) && !File.Exists(ftpConfig.LocalPath) && ftpConfig.Operation.Equals(FTPOperation.SEND)) 
                //ftpConfig.Operation '0' is SEND file operation and it must have file name in path. 
                )
            {
                context.SetStatus(FTPResult.FL);
                return;
            }
            else if (ftpConfig.Operation.Equals(FTPOperation.RECEIVE)
                && !string.IsNullOrEmpty(context.FileName))
            {
                if (remoteIsFolder)
                {
                    //End in slash - directory only. Attempt to use file name of context to search for a file.
                    //This allows us to create a job execution to look for a specific response file ahead of time.
                    //E.g. After generating an EDI276, we know that we'll get a 277 response, so we can create the job execution that will search for the 277 right away, and just have it check every couple hours for the file up to a limit.
                    ftpConfig.RemotePath += context.FileName;
                }
            }

            if (string.IsNullOrWhiteSpace(ftpConfig.RemotePath))
            {
                context.SetStatus(FTPResult.FR); //Always needed
                return;
            }

            try
            {
                FTP ftp = new FTP(ftpConfig, context.Executor);
                context.Success = ftp.Process();
                if (context.Success)
                {
                    if (!string.IsNullOrEmpty(ftp.FileName))
                    {
                        FileInfo fi = new FileInfo(ftp.FileName);
                        if (fi.Exists)
                        {
                            context.Execution.SetFileInfo(fi);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                context.LogError($"IO Error while completing operation {ftpConfig.OperationName}.", ex);
                context.SetStatus(FTPResult.LO);
                return;
            }
            catch (Exception ex)
            {
                context.LogError("FTP/SFTP job failed: ", ex);
                context.SetStatus(FTPResult.FT);
                return;
            }
            finally
            {
                if(ftpConfig.Operation == FTPOperation.SYNC_REGISTER)
                {
                    ftpConfig.DateFlag = false;
                    context.SetStatus(FTPResult.SR);
                };
            }
            
            // If date flagged, then the job is looking for data associated with a specific date, and we can rely on the schedule.
            // Otherwise, we're checking for whatever is available, and it may come in at any time, so we'll requeue until end of day.
            if (ftpConfig.DateFlag) 
                return;
            if (context.ProcessingDate >= DateTime.Today)
                context.Requeue(30);
        }

    }
}
