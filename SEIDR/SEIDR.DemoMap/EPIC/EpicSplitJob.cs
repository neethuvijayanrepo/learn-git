using System;
using System.Collections.Generic;
using SEIDR.JobBase;


namespace SEIDR.FileSystem.FileSplitting
{
    [IJobMetaData(nameof(EpicSplitJob),
        nameof(FileSystem), "Split Files from EPIC into a file per record type",
        NeedsFilePath: true, AllowRetry: false, ConfigurationTable: null)]
    public class EpicSplitJob : IJob
    {

        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {

            Doc.DocMetaData EpicPartialMetaData = new Doc.DocMetaData(execution.FilePath)
            {
                HasHeader = false
            };
            if (!EpicPartialMetaData.CheckExists())
            {
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "NS",
                    NameSpace = nameof(FileSystem),
                    IsError = true
                };
                return false;
            }

            var outDir = new System.IO.DirectoryInfo(System.IO.Path.Combine(EpicPartialMetaData.Directory, "_SplitDemo"));
            if (!outDir.Exists)
                outDir.Create();
 			jobExecutor.LogInfo($"Preparing to split EPIC file: {execution.FilePath}");
            int numParts = 0;

            using (System.IO.StreamReader file = new System.IO.StreamReader(execution.FilePath))
            {
                Dictionary<string, System.IO.StreamWriter> outs = new Dictionary<string, System.IO.StreamWriter>();

                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string fileType = line.Substring(0, 2);
                    fileType = fileType == "05" ? "04" : fileType;
                    if (!outs.ContainsKey(fileType))
                    {
                        outs.Add(fileType, new System.IO.StreamWriter(outDir.FullName + $"\\{fileType}_" + execution.FileName));
                    }
                    outs[fileType].WriteLine(line);
                }

                numParts = outs.Count;

                // close all our split file outputs
                foreach (var op in outs.Keys)
                {
                    if (outs[op] != null)
                    {
                        outs[op].Dispose();
                        //   outs[op] = null;   // taking this out as maybe not needed ?
                    }
                }
            }

            jobExecutor.LogInfo($"EPIC file has been split into {numParts} pieces.");

            // set the output file as the 01 file so the DMAP process will use that as the input.
            execution.SetFileInfo(outDir.FullName + "\\01_" + execution.FileName);

            return true;
		}
    }
}
