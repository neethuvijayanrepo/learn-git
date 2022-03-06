using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;


namespace SEIDR.FileSystem.FileSplitting
{
    [IJobMetaData(nameof(NordisSplitJob), 
        nameof(FileSystem), "Split Files from Nordis into a file per Project", 
        NeedsFilePath: true, AllowRetry: false, ConfigurationTable:null)]
    public class NordisSplitJob : IJob
    {

        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }
        public string MapSourceFileToOutput(string sourceFile, DateTime ProcessingDate)
        {
            string ProjectInfo = sourceFile.Substring(0, sourceFile.IndexOf('_'));
            //return "NORDIS_" + ProjectInfo + "_" + ProcessingDate.ToString("yyyy_MM_dd") + ".txt";

            return $"Nav_Confirmation_{ProcessingDate.ToString("yyyyMMdd")}_1.txt.{ProjectInfo}.CYM";
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            
            Doc.DocMetaData NordisMetaData = new Doc.DocMetaData(execution.FilePath)
            {
                HasHeader = false
            };
            if (!NordisMetaData.CheckExists())
            {
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "NS",
                    NameSpace = nameof(FileSystem),
                    IsError = true
                };
                return false;
            }
            NordisMetaData.AddDelimitedColumns("FileSource", "AccountID", "FieldType", "Status", "FileType", 
                "COL 6", "COL 7", "COL 8", "COL 9", "COL 10", "COL 11", "COL 12", "COL 13", "COL 14", "COL 15", 
                "COL 16", "COL 17", "COL 18", "COL 19", "COL 20");
            NordisMetaData.Columns.AllowMissingColumns = true;
            NordisMetaData
                .SetDelimiter('|')
                .SetMultiLineEndDelimiters(@"\r", "\\n", "\\r\\n");

            var outDir = new System.IO.DirectoryInfo(System.IO.Path.Combine(NordisMetaData.Directory, "_Parsed"));
            if (!outDir.Exists)
                outDir.Create();

            Dictionary<string, string> fileOutputMappings = new Dictionary<string, string>();
            List<string> fileNames = new List<string>();
            int outputStartIndex = 0;
            using (Doc.DocReader r = new Doc.DocReader(NordisMetaData))
            {
                var p = r.GetPageInfo(0);
                jobExecutor.LogInfo($"Finished loading meta data for file {NordisMetaData.FileName}. Page 0 Fullness: {p.Fullness}, Record Count: {p.RecordCount} vs Full file Record Count: {r.RecordCount}");

                foreach(var record in r)
                {
                    if (record[0] == execution.FileName)
                    {
                        string sourceFile = record[1];
                        string fullOutputPath = System.IO.Path.Combine(outDir.FullName, MapSourceFileToOutput(sourceFile, execution.ProcessingDate));

                        fileOutputMappings.Add(sourceFile, fullOutputPath);
                        fileNames.Add(sourceFile);
                        outputStartIndex++;
                    }
                    else
                        break;
                }/*
                //File Names is sorted, so we can create DocWriter with Append = false instead of doing cleanup here.
                foreach(var path in fileOutputMappings.Values)
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }*/
                fileNames.Sort(); // Default sort should be good enough - File source names begin with ProjectID
                Doc.DocWriter dw = null;
                Doc.DocMetaData NordisOutput = null;
                foreach (string file in fileNames)
                {
                    string output = fileOutputMappings[file];
                    if(dw == null || output != NordisOutput.FilePath)
                    {
                        if(dw != null)
                        {
                            dw.Dispose();
                            dw = null;
                        }
                        NordisOutput = new Doc.DocMetaData(output);
                        NordisOutput
                            .SetLineEndDelimiter(Environment.NewLine)
                            .SetDelimiter('|')
                            .SetHasHeader(false)
                            .AddDetailedColumnCollection(NordisMetaData.Columns);                   
                        dw = new Doc.DocWriter(NordisOutput); //Create once per OutputFile.
                    }
                    for (long idx = outputStartIndex; idx < r.RecordCount; idx++)
                    {
                        if(r[idx]["FileSource"] == file)
                        {
                            dw.AddRecord(r[idx]);
                            if (idx == outputStartIndex)
                                outputStartIndex++;
                        }
                    }
                }
                if (dw != null)
                {
                    dw.Dispose();
                    dw = null;
                }
            }



            return true;
        }
    }
}
