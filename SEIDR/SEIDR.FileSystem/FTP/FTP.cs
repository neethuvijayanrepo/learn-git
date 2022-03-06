using WinSCP;
using System.IO;
using System.Configuration;
using System;
using SEIDR.JobBase;
using Cymetrix.Andromeda.Encryption;

namespace SEIDR.FileSystem.FTP
{
    public class FTP
    {
        public FTPOperation Operation { get; set; }
        private SessionOptions sessionOptions { get; set; }
        private FTPConfiguration _ftpConfig { get; set; }
        private delegate bool ProcessFTP_Operation();
#if DEBUG
        private const string FTPOperationLogFileName = "FTPOperationSession.log";
#else
        private const string FTPOperationLogFileName = null; //Session log only when debugging, because it grows really fast... Possible todo: Setting on FTPJob?
#endif
        IJobExecutor _caller;
        public string FileName { get; private set; } = null;
        public FTP(FTPConfiguration config, IJobExecutor caller)
        {
            _caller = caller;
            _ftpConfig = config;
            string _password = new Encryptor().Decrypt(config.Password);
            string PPK_FilePath = string.Empty;
            if (!string.IsNullOrEmpty(config.PpkFileName))
                PPK_FilePath = ConfigurationManager.AppSettings["SFTP_PPK_File_Path"].ToString() + config.PpkFileName;

            sessionOptions = new SessionOptions
            {
                Protocol = config.Protocol == "FTP" ? Protocol.Ftp : Protocol.Sftp,//ToDo: SSL
                HostName = config.Server,
                UserName = config.UserName,
                Password = _password,
                SshHostKeyFingerprint = config.Fingerprint,
                PortNumber = config.Port.HasValue ? (int)config.Port : 21,//used default port if null
                SshPrivateKeyPath = PPK_FilePath,
                PrivateKeyPassphrase = !string.IsNullOrEmpty(PPK_FilePath) ? _password : null
            };
            Operation = config.Operation;
        }

        public bool Process()
        {
            return ((ProcessFTP_Operation)Delegate.CreateDelegate(typeof(ProcessFTP_Operation), this, Operation.ToString())).Invoke(); 
        }

        private bool SEND()
        {
            using (Session session = new Session { SessionLogPath = FTPOperationLogFileName /*, ExecutablePath= @"C:\SEIDR.Jobs\WinSCP.exe" */})
            {
                // Connect
                session.Open(sessionOptions);
                //Transfer Resume Support is turned on or off depending on the bit column
                TransferResumeSupport resumeSupport = new TransferResumeSupport
                {
                    State = _ftpConfig.TransferResumeSupport 
                                ? TransferResumeSupportState.Default 
                                : TransferResumeSupportState.Off
                };
                // Upload files
                TransferOperationResult transferResult = session.PutFiles(_ftpConfig.LocalPath, _ftpConfig.RemotePath, _ftpConfig.Delete,
                                                        new TransferOptions() { TransferMode = TransferMode.Binary,ResumeSupport= resumeSupport });
                               
                // Throw on any error
                transferResult.Check();
                transferResult.Transfers.ForEach(t =>
                {
                    _caller.LogInfo("Transferred file: " + t.FileName);
                });
                session.Close();
                return transferResult.IsSuccess;
            }
        }

        private bool RECEIVE()
        {
            using (Session session = new Session { SessionLogPath = FTPOperationLogFileName })
            {
                // Connect
                session.Open(sessionOptions);

                // Download files                
                TransferOperationResult transferResult = session.GetFiles(_ftpConfig.RemotePath, _ftpConfig.LocalPath, _ftpConfig.Delete,
                                                        new TransferOptions() { TransferMode = TransferMode.Binary });

                if (transferResult.IsSuccess && transferResult.Transfers.Count == 1)
                    FileName = transferResult.Transfers[0].Destination;
                else if (transferResult.Transfers.Count == 0)
                {
                    _caller.LogInfo("No files found.");
                    session.Close(); //using statement probably actually covers this.
                    return false; //No files found is forced failure, just log no file found and return false.
                }

                transferResult.Transfers.ForEach(t =>
                {
                    if (string.IsNullOrEmpty(t.Destination))
                        _caller.LogError("Unable to transfer file '" + t.FileName + "'");
                    else
                        _caller.LogInfo("Transferred file: " + t.Destination);
                });
                if (!transferResult.IsSuccess)
                {
                    transferResult.Failures.ForEach(e =>
                    {
                        _caller.LogError(e.Message, e);
                    });
                }

                session.Close(); 
                return transferResult.IsSuccess;
            }
        }

        private bool MAKE_DIR_LOCAL()
        {
            DirectoryInfo di = new DirectoryInfo(_ftpConfig.LocalPath);
            if (!di.Exists)
            {
                Directory.CreateDirectory(_ftpConfig.LocalPath);
                _caller.LogInfo("Created Local Directory: " + _ftpConfig.LocalPath);
                return true;
            }
            else
                return false;
        }

        private bool MAKE_DIR_REMOTE()
        {
            using (Session session = new Session { SessionLogPath = FTPOperationLogFileName })
            {
                session.Open(sessionOptions);
                if (!session.FileExists(_ftpConfig.RemotePath))
                {
                    session.CreateDirectory(_ftpConfig.RemotePath);
                    _caller.LogInfo("Created Remote Directory: " + _ftpConfig.RemotePath);
                    return true;
                }
                else
                    return false;
            }
        }

        private bool DELETE_LOCAL()
        {
            DirectoryInfo dInfo = new DirectoryInfo(_ftpConfig.LocalPath);
            if (dInfo.Exists)
            {
                Directory.Delete(_ftpConfig.LocalPath, true);
                _caller.LogInfo("Deleted local Directory: " + _ftpConfig.LocalPath);
                return true;
            }
            else
                return false;
        }
        private bool DELETE_REMOTE()
        {
            using (Session session = new Session { SessionLogPath = FTPOperationLogFileName })
            {
                // Connect
                session.Open(sessionOptions);

                // Download files                
                RemovalOperationResult removalResult = session.RemoveFiles(_ftpConfig.RemotePath);

                removalResult.Removals.ForEach(r =>
                {
                    _caller.LogInfo("Deleted Remote file: " + r.FileName);
                });
                session.Close();
                return removalResult.IsSuccess;
            }
        }
        private bool SYNC_LOCAL()
        {
            return SYNC(SynchronizationMode.Local, false);
        }
        private bool SYNC_REMOTE()
        {
            return SYNC(SynchronizationMode.Remote, false);
        }

        private bool MOVE_REMOTE()
        {
            using (Session session = new Session {SessionLogPath = FTPOperationLogFileName})
            {
                session.Open(sessionOptions);
                //move file.
                session.MoveFile(_ftpConfig.RemotePath, _ftpConfig.RemoteTargetPath); //No Result information from remote-only operation. Should throw an exception if fail.
                session.Close();
                return true;
            }
        }


        private bool SYNC_BOTH()
        {
            return SYNC(SynchronizationMode.Both, false);
        }
        private bool SYNC(SynchronizationMode syncMode, bool RegisterDownloads)
        {
            using (Session session = new Session { SessionLogPath = FTPOperationLogFileName })
            {
                // Connect
                session.Open(sessionOptions);

                // Download files                
                SynchronizationResult syncResult = session.SynchronizeDirectories(syncMode, _ftpConfig.LocalPath, _ftpConfig.RemotePath, _ftpConfig.Delete);

                syncResult.Removals.ForEach(r =>
                {
                    _caller.LogInfo("Removed file: " + r.FileName);
                });
                syncResult.Downloads.ForEach(d =>
                {
                    _caller.LogInfo("Downloaded file to Local Path: " + d.FileName + (RegisterDownloads ? " - Attempt Register." : string.Empty));
                    if (RegisterDownloads)
                    {
                        FileInfo f = new FileInfo(d.Destination);
                        RegistrationFile rf = new RegistrationFile(_caller.job, f);
                        int? RC;
                        JobExecution je;
                        try
                        {
                            je = rf.Register(_caller.Manager);
                            RC = rf.ReturnCode ?? 2;
                        }
                        catch(Exception ex)
                        {
                            _caller.LogError(f.Name + " - Registration Error", ex);
                            je = null;
                            RC = 99999999;
                        }
                        if (je == null && RC.Value > 0) // > 0 => Error or unable to insert. < 0 -> file already registered. Already registered can ignore...
                        { //need to deal with errors while registering? 
                            _caller.LogError("Unable to register file: " + f.FullName + ", moving to _Rejected.");
                            var dir = f.Directory;
                            try
                            {
                                if (!dir.EnumerateDirectories()
                                        .Exists(child => child.Name.Equals("_Rejected", StringComparison.OrdinalIgnoreCase)))
                                {
                                    dir.CreateSubdirectory("_Rejected");
                                }

                                string rejectPath = Path.Combine(dir.FullName, "_Rejected", f.Name);
                                File.Move(d.Destination, rejectPath);
                            }
                            catch (Exception ex)
                            {
                                _caller.LogError("Unable to move file to _Rejected.", ex);
                            }
                        }
                        else if(je != null)
                        {
                            
                        }
                    }
                });
                syncResult.Uploads.ForEach(u =>
                {
                    _caller.LogInfo("Uploaded file to Remote Server: " + u.FileName);                                        
                });
                session.Close();
                return syncResult.IsSuccess;
            }
        }
        private bool SYNC_REGISTER()
        {
            /*
             * Sync changes to remote server to our local server - for any files that were uploaded to the local path as a resul of sync operation, create a JobExecution with status of 'R' for the same JobProfile. (Alternate step 1 specify that it starts from status of 'R'?) 
             * Might need to have a PreviousStep requirement added to JobProfile_Job to avoid the SYNC_REGISTER going to a second step after completing...Or else, allow forcing a complete status?
             * 
             * Might need to check the local path for unregistered files.. 
             */
            return SYNC(SynchronizationMode.Local, true);     
        }
    }
}


