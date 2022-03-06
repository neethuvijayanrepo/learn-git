using SEIDR.DataBase;
using SEIDR.JobBase;
using System;
using System.IO;
using Microsoft.SqlServer.Dts.Runtime;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;

namespace SEIDR.PreProcess
{
    
    [IJobMetaData(nameof(PreProcessJob), 
        nameof(PreProcess), "PreProcess Loader", 
        NotificationTime: 240, ConfigurationTable: "SEIDR.LoaderJob",
        AllowRetry:true, NeedsFilePath: false)]
    public class PreProcessJob : ContextJobBase<SSISContext>
    {
        const string GET_EXECUTION_INFO = "SEIDR.usp_LoaderJob_ss";

        //ToDo: Rename project to DatabaseLoading or something like that. Could also do Metrix loading from here potentially? SEI-338/SEI-341
        public override int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            if (jobCheck.RequiredThreadID.HasValue)
                return jobCheck.RequiredThreadID.Value; // Configuration of thread needs to override hash map.
            //Possible ToDo: update the JobProfile_Job to have a required ThreadID so that we only need to do this once?
            // Potential problem: if we change the number of threads and then another profile is created for the package, it could end up pointing the other JobProfile_Job record to a different threadID for the same package.
            var configRow = SSISExecutor.GetConfigurationDataRow(GET_EXECUTION_INFO, jobExecutor.Manager, jobCheck.JobProfile_JobID);
            var packagePath = configRow[nameof(SSISPackage.PackagePath)].ToString();

            return !string.IsNullOrEmpty(packagePath) ? Math.Abs(packagePath.GetHashCode()) : passedThreadID;
        }
        
        public override void Process(SSISContext context)
        {
            SSISExecutor ex = new SSISExecutor(context);
            
            //If doing metrix loading, could potentially pass a different procedure/Database Manager to be able to reuse the SSIS Executor/SSIS Package.
            ex.SetUp(GET_EXECUTION_INFO, context.Manager); 
            if (context.ResultStatus != null && context.ResultStatus.IsError) 
                return; //Setup failure
            if (ex.LoadPackage())
            {
                /*
                ToDo: this would need to affect the variable PRIOR to loading package. Also, need to account for secondary/tertiary file path. SEI-213
                if (ex.Package[SSISPackage.OUTPUT_FOLDER] != null
                    && ex.Package[SSISPackage.OUTPUT_FILE_NAME] != null)
                {
                    BasicLocalFileHelper local = context.ReserveBasicLocalFile(ex.Package[SSISPackage.OUTPUT_FILE_NAME].ToString());
                    local.OutputDirectory = ex.Package[SSISPackage.OUTPUT_FOLDER].ToString();
                    local.SetExecutionFileInfo = true;
                    context.WorkingFile = local;
                }*/
                ex.Execute();
                if (context.Success)
                {
                    //If the working file was used, keep it set for caller to update JobExecution with.
                    //Else, set the WorkingFile information to null.
                    context.ClearUnusedWorkingFile();

                    var fi = ex.Package.CheckOutputFile(SSISPackage.OUTPUT_FILEPATH);
                    if (fi != null && fi.Exists)
                        context.SetCurrentFilePath(fi.FullName);
                    else
                    {
                        fi = ex.Package.CheckOutputFile(SSISPackage.OUTPUT_PATH);
                        if (fi != null && fi.Exists)
                            context.SetCurrentFilePath(fi.FullName);
                    }
                }
            }


        }

        
    }
}
