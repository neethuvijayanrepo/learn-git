using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SEIDR.OperationServiceModels;
using System.ComponentModel.Composition;
using System.IO.Compression;
using SEIDR.DataBase;
using System.IO;
using SEIDR.JobBase;
using SEIDR.Doc;

namespace SEIDR.FileSystem
{

    public partial class FS
    {

        public FileOperation Operation { get; set; }
        public string Source { get; set; }
        public string Filter { get; set; }
        public string OutputPath { get; set; }
        //ToAdd: Additional logic for various operations

        public bool Overwrite { get; set; }
        public bool UpdateExecutionPath { get; set; } = false;

        public int? LoadProfileID { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string InputFolder { get; set; }
        public string FileFilter { get; set; }
        public int DatabaseLookUpID { get; set; }

        /// <summary>
        /// Combines two file path components. Similar to Path.Combine, but does not validate the file path, so that it can be used for masked file paths.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileOrSubFolder"></param>
        /// <returns></returns>
        public static string Combine(string folder, string fileOrSubFolder)
        {
            if (folder.EndsWith("\\"))
                return folder + fileOrSubFolder;
            return folder + '\\' + fileOrSubFolder;
        }
        /// <summary>
        /// Combine arbitrary number of path components. The Path is not validated like in Path.Combine
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Combine(string[] args)
        {
            if (args.Length == 0)
                return null;
            StringBuilder ret = new StringBuilder(args[0]);
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i - 1].EndsWith("\\"))
                    ret.Append(args[i]);
                else
                    ret.Append("\\").Append(args[i]);
            }
            return ret.ToString();
        }
        /// <summary>
        /// Checks if a file path is likely to be a directory, as opposed to a file.
        /// </summary>
        /// <param name="pathMaybeDirectory"></param>
        /// <returns></returns>
        public static bool IsDirectory(string pathMaybeDirectory)
        {
            return pathMaybeDirectory.EndsWith(@"\") || Directory.Exists(pathMaybeDirectory);
        }
        /// <summary>
        /// Replace stars in a file path with the name or extension of the original file.<para>E.g., C:\Test\*.txt + File2.CYM -> C:\Test\File2.txt or C:\Test\NewName.* + File2.CYM -> C:\Test\NewName.CYM</para><para>Note: Does not expect more than 1 dot in the <paramref name="fullPath"/> parameter</para>
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="sourceFileName"></param>
        /// <returns></returns>
        public static string ReplaceStar(string fullPath, string sourceFileName)
        {
            if (IsDirectory(fullPath))
                return Path.Combine(fullPath, sourceFileName);
            if (!fullPath.Contains("*"))
                return fullPath;
            

            string name = Path.GetFileName(fullPath);
            if (name.Contains("."))
            {
                string[] nameParts = name.Split('.');
                nameParts[0] = nameParts[0].Replace("*", Path.GetFileNameWithoutExtension(sourceFileName));
                nameParts[1] = nameParts[1].Replace("*", Path.GetExtension(sourceFileName));
                fullPath = Path.Combine(Path.GetDirectoryName(fullPath), nameParts[0] + '.' + nameParts[1]);
            }
            else
            {
                fullPath = fullPath.Replace("*", sourceFileName);
            }
            return fullPath;
        }
        static readonly int CLEAN_SUBSTRING_IDX = "CLEAN_".Length;
        /// <summary>
        /// Call this in a try/catch.
        /// </summary>
        /// <param name="context">Context from caller.</param>
        /// <returns></returns>
        public void Process(FileSystemContext context)
        {
            JobProfile profile = context;
            JobExecution jobExecution = context;
            DateTime processingDate = jobExecution.ProcessingDate;
            ResultStatusCode ret = FileSystemContext.DEFAULT_RESULT;

            if (string.IsNullOrWhiteSpace(Filter))
                Filter = "*.*"; //If a filter is specified on the profile, but not on the operation, use that to maintain filtering to relevant files.
            else
                Filter = ApplyDateMask(Filter, processingDate); //E.g., <YYYY><MM><DD>*.csv -> 20180821*.csv to filter to files for a given day. Note that this wouldn't make sense to have at the profile level, because a ProcessingDate does not yet exist for its main usage


            bool forceUpdateIfMove = false;
            if (string.IsNullOrWhiteSpace(Source))
            {
                Source = jobExecution.FilePath;
                forceUpdateIfMove = true;
            }
            else
                Source = ApplyDateMask(Source, processingDate);


            /*
            //ToDo: validation of single file operations - more meaningful log message instead of a null exception.
           if (!Operation.ToString("F").EndsWith("_ANY", StringComparison.OrdinalIgnoreCase)
               && !Operation.ToString("F").EndsWith("_ALL", StringComparison.OrdinalIgnoreCase)
               && !Operation.ToString("F").EndsWith("DIR", StringComparison.OrdinalIgnoreCase)
               && Operation != FileOperation.ZIP)
           {
               string fileName = Path.GetFileName(Source);
               if (string.IsNullOrEmpty(fileName))
               {
                   context.LogError($"Operation '{Operation:F}' requires single file as source. Provided source: '{Source}'");
                   context.SetStatus(ResultStatusCode.NS);
                   return;
               }
           }*/
            
            OutputPath = ApplyDateMask(OutputPath, processingDate);

            if (Operation.ToString("F").StartsWith("CLEAN", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo fi = new FileInfo(Source);
                if (!fi.Exists)
                {
                    string fileName = Path.GetFileName(Source);
                    if (fileName.Exists(c => c == '*'))
                    {
                        DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Source));
                        if (di.Exists)
                        {
                            //Note: should use COPY_ALL or MOVE_ALL if want to do multiple files.
                            fi = di.GetFiles(fileName).FirstOrDefault();
                            if(fi != null)
                                Source = fi.FullName;
                            else 
                                context.LogError("Path checked for wildcard files: " + di.FullName);
                        }
                    }
                    if (fi == null)
                    {
                        context.SetStatus(ResultStatusCode.NS);
                        return;
                    }

                }

                context.WorkingFile = context.GetLocalFile(Source);
                //Switch to normal version of operation
                Operation = (FileOperation)Enum.Parse(typeof(FileOperation), 
                                                      Operation.ToString("F").Substring(CLEAN_SUBSTRING_IDX), 
                                                      true);

                var scj = new SimpleClean.SimpleCleanJob();
                scj.Process(context);
                if (context.Failure)
                    return;
                //Point to cleaned file after processing.
                context.WorkingFile.Rename(fi.Name, true); 
                //Maintain original name so behavior can be essentially the same as a COPY or COPY_METRIX.
                Source = context.CurrentFilePath;
                context.WorkingFile = null;
            }


            switch (Operation)
            {
                case FileOperation.CREATEDIR:
                    {
                        //Create directory specified by 'Source' path. Requires root directory to exist (e.g., "\\sdsrv031.cymetrix.com\IS\" or "C:\"
                        DirectoryInfo di = new DirectoryInfo(OutputPath);
                        if (!di.Root.Exists)
                        {
                            context.SetStatus(ResultStatusCode.NR);
                            return;
                        }
                        else if (!di.Exists)
                        {
                            Directory.CreateDirectory(OutputPath); //Created directory IS the output
                        }
                        break;
                    }
                case FileOperation.UNZIP:
                    {
                        FileInfo fi = new FileInfo(Source); //ToDo: Wildcard source for multiple files to unzip? UNZIP_ALL?
                        if (fi.Exists)
                        {
                            if (OutputPath != null)
                            {
                                //Temporary file because ExtractToDirectory throws an error if the directory already exists. Probably not what we want.
                                string temporaryPath = Path.Combine(fi.DirectoryName, jobExecution.JobExecutionID + "TEMP");
                                context.LogInfo("TEMP FOLDER: '" + temporaryPath + "'");
                                if (Directory.Exists(temporaryPath))
                                    Directory.Delete(temporaryPath, true); //Cleanup in case we had an error before and were not able to cleanup..Also prevent issue if multiple jobExecutions for the same profile are running at once.
                                Directory.CreateDirectory(temporaryPath);
                                
                                ZipFile.ExtractToDirectory(Source, temporaryPath);
                                string[] files = Directory.GetFiles(temporaryPath);
                                string dest = null;
                                foreach (string file in files)
                                {
                                    string name = Path.GetFileName(file);
                                    dest = Path.Combine(OutputPath, name);
                                    if (Overwrite)
                                    {
                                        File.Copy(file, dest, Overwrite);
                                    }
                                    else
                                        File.Copy(file, dest);
                                    context.LogInfo($"Extracted file '{name}' to path '{dest}' from ZIP archive");
                                    //ToDo: set execution path from the first file path if specified by settings?
                                }
                                Directory.Delete(temporaryPath, true);
                                if (UpdateExecutionPath)
                                {
                                    if (files.Length == 1)
                                    {
                                        context.UpdateFilePath(dest);
                                    }
                                    else 
                                    {
                                        if (files.Length > 1)
                                            context.LogInfo("Multiple files unzipped, UpdateExecutionPath = 1 - setting FilePath to null.");
                                        else
                                            context.LogInfo("No files unzipped. Setting FilePath to null");
                                        context.UpdateFilePath(string.Empty);
                                    }
                                }

                            }
                            else
                            {
                                context.SetStatus(ResultStatusCode.ND);
                                return;
                            }

                        }
                        else
                        {
                            context.SetStatus(ResultStatusCode.NS);
                            return;
                        }
                        break;
                    }

                case FileOperation.ZIP:
                    {
                        DirectoryInfo di = new DirectoryInfo(Source);
                        FileInfo fi = new FileInfo(Source);
                        if (di.Exists)
                        {
                            if (OutputPath != null)
                            {
                                if (Directory.Exists(OutputPath))
                                {
                                    if (string.IsNullOrEmpty(Filter))
                                        OutputPath = Path.Combine(OutputPath, fi.Name + ".zip");
                                    else
                                        OutputPath = Path.Combine(OutputPath, di.Name + ".zip");
                                }
                                                                
                                String temporaryPath = Directory.CreateDirectory(di + "\\" + profile.JobProfileID + "_" + jobExecution.JobExecutionID).FullName;
                                if (string.IsNullOrEmpty(Filter))
                                {
                                    File.Copy(Source, Path.Combine(temporaryPath, Path.GetFileName(Source)));
                                }
                                else
                                {
                                    foreach (var file in Directory.GetFiles(Source, Filter))
                                    {
                                        File.Copy(file, Path.Combine(temporaryPath, Path.GetFileName(file)));
                                    }
                                }
                                if (Overwrite)
                                {
                                    if (File.Exists(OutputPath))
                                    {
                                        File.Delete(OutputPath);
                                    }
                                    ZipFile.CreateFromDirectory(temporaryPath, OutputPath);
                                }
                                else
                                {                                    
                                    ZipFile.CreateFromDirectory(temporaryPath, OutputPath);
                                }

                                Directory.Delete(temporaryPath,true);
                            }
                            else
                            {
                                context.SetStatus(ResultStatusCode.ND);
                                return;
                            }
                        }
                        else if (fi.Exists)
                        {
                            if (OutputPath != null)
                            {
                                if (Overwrite)
                                {
                                    if (File.Exists(OutputPath))
                                    {
                                        File.Delete(OutputPath);
                                    }
                                    ZipFile.CreateFromDirectory(Source, OutputPath);
                                }
                                else
                                {
                                    ZipFile.CreateFromDirectory(fi.FullName, OutputPath);
                                }
                            }
                            else
                            {
                                context.SetStatus(ResultStatusCode.ND);
                                return;
                            }
                        }

                        if (UpdateExecutionPath)
                            context.UpdateFilePath(OutputPath);//info from the newly created zip file.
                        
                        break;
                    }

                case FileOperation.COPY_METRIX:
                case FileOperation.MOVE_METRIX:
                {
                    var stagingManager = context.Executor.GetManager(DatabaseLookUpID);
                    stagingManager.IncreaseCommandTimeOut(180);
                    using (var load = stagingManager.GetBasicHelper())
                    {
                        load.QualifiedProcedure = "STAGING.usp_LoadProfile_ss_load";
                        if (LoadProfileID == null)
                            LoadProfileID = jobExecution.LoadProfileID;

                        load[nameof(LoadProfileID)] = LoadProfileID;
                        load[nameof(jobExecution.OrganizationID)] = jobExecution.OrganizationID;
                        load[nameof(jobExecution.ProjectID)] = (object) jobExecution.ProjectID ?? DBNull.Value;
                        //Without object mapping, need to explicitly pass DBNull (or else it will use default parameter, which is an error if there is no default)
                        DataSet ds = stagingManager.Execute(load);
                        DataRow dr = ds.GetFirstRowOrNull();
                        if (dr == null)
                        {
                            context.LogError($"Could not find LoadProfileID {LoadProfileID} in {ServerName}.{DatabaseName}");
                            context.SetStatus(ResultStatusCode.IP);
                            return;
                        }

                        // NOTE: DataSet and DataTable CaseSensitive property defaults to false.
                        string loadBatchTypeCode = dr[nameof(loadBatchTypeCode)].ToString().Trim(); //Metrix Load types sometimes have unnecessary spaces at the end...
                        if (jobExecution.LoadProfileID == LoadProfileID &&
                            jobExecution.UserKey1 != loadBatchTypeCode)
                        {
                            context.LogError($"Expected LoadBatchTypeCode = '{jobExecution.UserKey1}', found '{loadBatchTypeCode}'.");
                            context.SetStatus(ResultStatusCode.TM);
                            return;
                        }

                        InputFolder = dr[nameof(InputFolder)].ToString();
                        FileFilter = dr["FileNameFilter"].ToString().Replace("*", "%");
                        FileInfo fi = new FileInfo(Source);
                        if (!fi.Exists)
                        {
                            context.LogError("Path checked: " + Source);
                            context.SetStatus(ResultStatusCode.NS);
                            return;
                        }

                        string sourceName = fi.Name;
                        if (string.IsNullOrEmpty(InputFolder))
                        {
                            context.LogError("Load Profile does not have an InputFolder specified.");
                            context.SetStatus(ResultStatusCode.ND);
                            return;
                        }

                        if (string.IsNullOrEmpty(FileFilter))
                        {
                            context.LogError("FileFilter Empty");
                            context.SetStatus(ResultStatusCode.FF);
                            return;
                        }

                        if (!sourceName.Like(FileFilter)
                            && FileFilter.NotIn("*", "*.*")) //By Windows pattern match, these two filters are always a match.
                        {
                            context.LogError("'" + sourceName + "' did not match Load Profile's filter: '" + FileFilter + "'.",
                                             ExtraID: LoadProfileID);
                            context.SetStatus(ResultStatusCode.FF);
                            return;
                        }

                        const string PROCESSED_FOLDER = "_Processed_";
                        Directory.CreateDirectory(Path.Combine(InputFolder, PROCESSED_FOLDER));
                        string dest = Path.Combine(InputFolder, PROCESSED_FOLDER, fi.Name); //Input folder should not be date masked, and should not have wild cards.
                        using (var checkSeq = stagingManager.GetBasicHelper())
                        {
                            const string CAN_LOAD = "CanLoad";
                            const string OUT_OF_SEQUENCE_OLD = "OutOfSequenceOld";
                            const string OUT_OF_SEQUENCE_REASON = "OutOfSequenceReason";
                            checkSeq.QualifiedProcedure = "STAGING.usp_LoadBatch_CheckSequence";
                            checkSeq[CAN_LOAD] = false;
                            checkSeq["FilePathName"] = dest;
                            checkSeq[OUT_OF_SEQUENCE_OLD] = false;
                            checkSeq[OUT_OF_SEQUENCE_REASON] = string.Empty;
                            checkSeq[nameof(ServerName)] = this.ServerName;
                            checkSeq[nameof(LoadProfileID)] = this.LoadProfileID;
                            checkSeq["InputFileDateTime"] = jobExecution.ProcessingDate;
                            stagingManager.Execute(checkSeq);
                            if ((bool) checkSeq[CAN_LOAD] == false)
                            {
                                string oos = checkSeq[OUT_OF_SEQUENCE_REASON].ToString();
                                if ((bool) checkSeq[OUT_OF_SEQUENCE_OLD])
                                {
                                    if (!string.IsNullOrEmpty(oos))
                                        context.LogError(oos);
                                    context.SetStatus(ResultStatusCode.SO);
                                    return;
                                }

                                if (!string.IsNullOrEmpty(oos))
                                    context.LogInfo(oos);
                                context.Requeue(8); //Note: the JobExecutor automatically logs that a Requeue is requested.
                                context.SetStatus(ret);
                                return;
                            }

                            if (!Overwrite && File.Exists(dest))
                            {
                                context.LogError(dest + ": File already exists, Overwrite = false");
                                context.SetStatus(ResultStatusCode.AE);
                                return;
                            }

                            using (var register = stagingManager.GetBasicHelper(this, includeConnection: true))
                            {
                                // ReSharper disable once InconsistentNaming
                                int LoadBatchID;

                                register.BeginTran();
                                register.QualifiedProcedure = "STAGING.usp_LoadBatch_Register";
                                register["LoadBatchID"] = DBNull.Value;
                                register[CAN_LOAD] = false;
                                register["FilePathName"] = dest;
                                register["InputFileHash"] = fi.GetFileHash();
                                register[nameof(ServerName)] = this.ServerName;
                                register[nameof(LoadProfileID)] = this.LoadProfileID;
                                register["InputFileDateTime"] = jobExecution.ProcessingDate;
                                register["InputFileSize"] = fi.Length;
                                stagingManager.Execute(register);
                                try
                                {
                                    //Note: DBNull has a singleton, but otherwise is a normal class. Can check if value matches DBNull singleton value by checking if value is DBNull
                                    if ((bool) register[CAN_LOAD] == false || register[nameof(LoadBatchID)] is DBNull)
                                    {
                                        context.LogError("Duplicate File - No LoadBatch created.");
                                        context.SetStatus(ResultStatusCode.DF);
                                        return;
                                    }

                                    if (Operation == FileOperation.MOVE_METRIX)
                                    {
                                        if (Overwrite && File.Exists(dest))
                                            File.Delete(dest);
                                        File.Move(Source, dest);
                                    }
                                    else
                                        File.Copy(Source, dest, Overwrite);

                                    context.LogInfo(
                                                    Source + Environment.NewLine + "--> " + Operation +
                                                    Environment.NewLine + "-->" + dest);

                                    LoadBatchID = Convert.ToInt32(register[nameof(LoadBatchID)]);
                                    //Only set LoadBatchID on the JobExecution if a LoadBatchID has not been set yet, or if the LoadProfile matches
                                    if (jobExecution.METRIX_LoadBatchID == null || jobExecution.LoadProfileID == LoadProfileID)
                                        jobExecution.METRIX_LoadBatchID = LoadBatchID;
                                    register.CommitTran();
                                }
                                catch (Exception ex)
                                {
                                    context.LogError("Unable to Register File", ex);
                                    register.RollbackTran();
                                    if (File.Exists(dest))
                                    {
                                        //File cleanup, if we managed to move the file before going into the catch..
                                        if (Operation == FileOperation.COPY_METRIX)
                                            File.Delete(dest);
                                        else if (Operation == FileOperation.MOVE_METRIX)
                                            File.Move(dest, Source);
                                    }

                                    context.Requeue(30); //Unknown error, auto try again in 30 minutes.
                                    return;
                                }

                                try
                                {
                                    // Don't return false if we somehow fail here, because the loadbatch is still registered (Can be corrected from Metrix - the Loader will eventually set it to IE)
                                    using (var loadStatus = stagingManager.GetBasicHelper())
                                    {
                                        loadStatus.QualifiedProcedure = "STAGING.usp_Loader_SetStatus";
                                        loadStatus[nameof(LoadBatchID)] = LoadBatchID;
                                        loadStatus["LoadBatchStatusCode"] = "T1";
                                        stagingManager.Execute(loadStatus);
                                    }

                                    return;
                                }
                                catch (Exception ex)
                                {
                                    context.LogError("Error setting LoadBatchStatus to 'T1' - treating Job Step as success anyway.", ex, LoadBatchID);
                                    return;
                                }
                            }
                        }
                    }
                }

                case FileOperation.CREATE_DUMMY:
                case FileOperation.CREATE_DUMMY_TAG:
                    {
                        //Creates a dummy file at the path specified. If it already exists, return false, because that may indicate an unexpected situation.
                        FileInfo fi = new FileInfo(OutputPath);
                        if (fi.Exists)
                        {
                            if (Overwrite)
                                File.Delete(OutputPath);
                            else
                            {
                                context.SetStatus(ResultStatusCode.AE);
                                return;
                            }
                        }
                        string content = string.Empty; //no content.
                        if(Operation == FileOperation.CREATE_DUMMY_TAG)
                            content = Path.GetFileName(OutputPath);

                        File.WriteAllText(OutputPath, content);
                        if (UpdateExecutionPath)
                            jobExecution.SetFileInfo(OutputPath);
                        break;
                    }
                case FileOperation.GRAB:
                case FileOperation.MOVE:
                case FileOperation.COPY:
                case FileOperation.TAG:
                case FileOperation.TAG_DEST:
                    {
                        FileInfo fi = null;
                        string fileName = Path.GetFileName(Source);
                        if (fileName == null)
                        {
                            context.SetStatus(ResultStatusCode.NS);
                            return;
                        }
                        if (fileName.Exists(c => c == '*'))
                        {
                            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Source));
                            if (di.Exists)
                            {
                                //Note: should use COPY_ALL or MOVE_ALL if want to do multiple files.
                                fi = di.GetFiles(fileName).FirstOrDefault(); 
                            }
                            if(fi == null)                 
                            {
                                context.LogError("Path checked for wildcard files: " + di.FullName);
                                context.SetStatus(ResultStatusCode.NS);
                                return;
                            }

                            Source = fi.FullName;
                        }
                        else
                        {
                            fi = new FileInfo(Source);
                        }

                        if (!fi.Exists)
                        {
                            context.SetStatus( ResultStatusCode.NS);
                            return;
                        }
                    
                        string dest = Source; //Not Necessarily source, depending on wild cards.
                        if (OutputPath != null)
                        {
                            dest = ReplaceStar(OutputPath, fi.Name);
                            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
                        }

                        if (Operation.In(FileOperation.GRAB, FileOperation.MOVE))
                        {
                            if (Overwrite && File.Exists(dest))
                                File.Delete(dest);

                            File.Move(Source, dest);
                            if (UpdateExecutionPath || forceUpdateIfMove) //Old path doesn't exist. Note: don't need to recalculate size or hash.
                                jobExecution.FilePath = dest;

                            context.LogInfo(Source + Environment.NewLine + "--> " + Operation + Environment.NewLine + "-->" + dest);
                        }
                        else if (Operation.In(FileOperation.TAG, FileOperation.TAG_DEST))
                        {
                            string tagContent = "FILENAME: " + fi.Name;
                            if (Operation == FileOperation.TAG_DEST)
                                tagContent = "FILENAME: " + Path.GetFileName(dest);

                            string tagTemp = Source + ".TAG_TEMP"; //Create a temporary file in the same place to append the tag to the file content, so that a loader does not pick it up between move and append
                            File.Copy(Source, tagTemp, true);
                            //Only put a newline in front if the file is too big to ready extremely quickly, or if it already ends with a newline.
                            if (fi.Length >= 100000 || !File.ReadAllText(tagTemp).EndsWith(Environment.NewLine))
                                tagContent = Environment.NewLine + tagContent;

                            File.AppendAllText(tagTemp, tagContent); //Perform tag by appending the file name.


                            if (File.Exists(dest) && Overwrite)
                                File.Delete(dest);

                            File.Move(tagTemp, dest);

                            if (UpdateExecutionPath)
                            {
                                context.UpdateFilePath(dest);
                            }
                            context.LogInfo(Source + Environment.NewLine + "--> " + Operation + Environment.NewLine + "-->" + dest);
                        }
                        else
                        {

                            File.Copy(Source, dest, Overwrite);
                            //if (forceUpdateIfMove) //this is not for copy.
                            //    context.UpdateFilePath(dest);//New path (JobExecution did not specify a FilePath originally), so set Hash and size information
                            if (UpdateExecutionPath)
                                jobExecution.FilePath = dest;
                            
                            context.LogInfo(Source + Environment.NewLine + "--> " + Operation + Environment.NewLine + "-->" + dest);
                        }
                        break;
                    }
                case FileOperation.GRAB_ALL:
                case FileOperation.MOVE_ALL:
                case FileOperation.COPY_ALL:
                case FileOperation.MOVE_ANY:
                case FileOperation.COPY_ANY:
                    {
                        //Possible ToDo: If ForceUpdateIfMove is true, set jobExecutionFilePath to the last file from the enumeration. Or the first.
                        DirectoryInfo di = new DirectoryInfo(Source);
                        if (!di.Exists)
                        {
                            context.SetStatus( ResultStatusCode.NS);
                            return;
                        }
                        var files = di.GetFiles(Filter);
                        if(!files.Any()) //If no files...Log and return status depending on operation.                     
                        {
                            context.LogInfo("No Files found.");

                            if (Operation.In(FileOperation.COPY_ANY, FileOperation.MOVE_ANY))
                                context.SetStatus(FileSystemContext.DEFAULT_RESULT); //Nothing to do, return true for %_ANY.s
                            else
                                context.SetStatus(ResultStatusCode.NF);
                            return;
                        }
                        foreach (var file in files)
                        {
                            string dest = Path.Combine(OutputPath, file.Name);
                            Directory.CreateDirectory(OutputPath);


                            if (Operation.In(FileOperation.COPY_ALL, FileOperation.COPY_ANY))
                            {
                                File.Copy(file.FullName, dest, Overwrite);
                            }
                            else
                            {
                                if (Overwrite && File.Exists(dest)) //If !Overwrite, just let the File.Move error if it's in use
                                {
                                    File.Delete(dest);
                                }
                                File.Move(file.FullName, dest);
                            }
                            context.LogInfo($"{Operation}:'{file.FullName}' -> '{dest}'");
                        }
                        break;
                    }
                case FileOperation.CHECK:
                case FileOperation.EXIST:
                case FileOperation.DELETE:
                case FileOperation.CHECK_FILTER:
                    {

                        FileInfo fi = null;
                        string fileName = Path.GetFileName(Source);
                        if (fileName.Exists(c => c == '*')) //Cannot Handle * for a directory.
                        {
                            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Source));
                            if(di.Exists)
                                fi = di.GetFiles(fileName).FirstOrDefault();

                            if (fi == null)
                            {
                                context.SetStatus(ResultStatusCode.NS);
                                return;
                            }
                        }
                        else
                        {
                            fi = new FileInfo(Source);
                        }
                        if (!fi.Exists)
                        {
                            context.SetStatus( ResultStatusCode.NF);
                            return;
                        }
                        else if (Operation == FileOperation.CHECK_FILTER)
                        {
                            FileFilter = Filter.Replace("*", "%");

                            string sourceName = fi.Name;
                            if (FileFilter.In("*", "*.*") || //By Windows pattern match, these two filters are always a match.
                                !string.IsNullOrEmpty(FileFilter) && sourceName.Like(FileFilter))
                            {
                                ret = ResultStatusCode.FM;
                            }
                            else
                            {
                                ret = ResultStatusCode.FD;
                            }

                            context.SetStatus(ret);
                        }

                        if (Operation == FileOperation.DELETE)
                        {
                            File.Delete(Source);
                            if (UpdateExecutionPath)
                            {
                                context.ClearFilePath();
                            }
                        }
                        else if (UpdateExecutionPath)
                        {
                            context.UpdateFilePath(fi);
                        }
                        break;
                    }
                case FileOperation.MOVEDIR:
                case FileOperation.COPYDIR:
                    {
                        DirectoryInfo di = new DirectoryInfo(Source);
                        if (!di.Exists)
                        {
                            //Source directory does not exist
                            context.SetStatus(ResultStatusCode.NS);
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(OutputPath))
                        {
                            //No destination --cannot move or copy files
                            context.SetStatus(ResultStatusCode.ND);
                            return;
                        }
                        DirectoryInfo dest = new DirectoryInfo(OutputPath);
                        if (dest.Exists)
                        {
                            var f = di.GetFiles();
                            foreach (var file in f)
                            {
                                string fileDest = Path.Combine(OutputPath, file.Name);
                                if (Operation == FileOperation.MOVEDIR)
                                {
                                    if (Overwrite && File.Exists(fileDest))
                                        File.Delete(fileDest);

                                    file.MoveTo(fileDest);
                                }
                                else
                                {
                                    file.CopyTo(fileDest, Overwrite);
                                }
                            }
                            break;
                        }
                        else if (Operation == FileOperation.MOVEDIR)
                        {
                            Directory.Move(Source, OutputPath);
                            break;
                        }
                        else
                        {
                            Directory.CreateDirectory(OutputPath);
                            var f = di.GetFiles();
                            foreach (var file in f)
                            {
                                string fileDest = Path.Combine(OutputPath, file.Name);
                                if (!Overwrite && File.Exists(fileDest))
                                    continue;
                                file.CopyTo(fileDest, true);
                            }
                            break;
                        }
                    }
                case FileOperation.SIZE_CHECK:
                    {
                        FileInfo f = new FileInfo(Source);
                        if (!f.Exists)
                        {
                            context.SetStatus(ResultStatusCode.NF);
                            return;
                        }                        
                        DirectoryInfo d = f.Directory;
                        string pattern = System.Text.RegularExpressions.Regex.Replace(f.Name, @"[\d.]+", "*");
                        if (!string.IsNullOrEmpty(f.Extension))
                            pattern += "." + f.Extension;
                        var fList = d.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly)
                            .Exclude(f2 => 
                                f2.Name == f.Name 
                               || Math.Abs(jobExecution.ProcessingDate.Subtract(f2.CreationTime).TotalDays) > 30).ToList(); 
                        if(fList.UnderMaximumCount(0))
                        {
                            context.SetStatus(FileSystemContext.DEFAULT_RESULT);
                            return;
                        }
                        var avg = fList.Average(i => i.Length);
                        context.LogInfo("Average file size: " + avg + ", across " + fList.Count + " files.");
                        if (f.Length < avg * 0.8)
                        {
                            context.LogError("Size Check Failed, length = " + f.Length + ". Minimum for pass: " + avg * .8 + ". If file is actually okay, set status to 'SC'.");
                            context.SetStatus(ResultStatusCode.SZ);
                            return;
                        }
                        context.SetStatus(FileSystemContext.DEFAULT_RESULT);
                        return;
                    }
            }

            return;
        }
        
    }
}
