using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;
using System.IO;

namespace SEIDR.FileSystem.FileSplitting
{
    [IJobMetaData(nameof(LexisNexisParseJob), 
        nameof(FileSystem), "Lexis Nexis File Parse - Split by facility", 
        true, false, null)]
    public class LexisNexisParseJob: IJob
    {
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }

        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            string input = execution.FilePath;
            if (!File.Exists(input))
            {
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "NS", IsError = true, NameSpace = nameof(FileSystem),
                    Description = "No Source File."
                };
                return false;
            }

            List<string> facilityList = new List<string>();
            var md = new Doc.DocMetaData(input)
            {
                HasHeader = true,
                SkipLines = 0
            };
            md.SetDelimiter(',').SetFileAccess(FileAccess.Read).SetMultiLineEndDelimiters("\r", "\n", "\r\n");
            const string FACILITY_ID = "\"Facility_ID\"";
            using (var read = new Doc.DocReader(md))
            {
                foreach (var record in read)
                {

                    var f = record[FACILITY_ID];
                    if (!facilityList.Contains(f))
                    {
                        facilityList.Add(f);
                    }
                }
                jobExecutor.LogInfo("Record Count identified: " + read.RecordCount + "; Facility Count: " + facilityList.Count);
                foreach (var fac in facilityList)
                {
                    int fID = int.Parse(fac.Replace("\"", ""));
                    var facOut = new Doc.DocMetaData($"{input}.{fID}.CYM");
                    facOut.CopyDetailedColumnCollection(md); //ToDo: Link column set instead of copy? Needs latest version of SEIDR library code.
                    facOut.SetHasHeader(true)
                        .SetDelimiter(',')
                        .SetFileAccess(FileAccess.Write);

                    using (var write = new Doc.DocWriter(facOut))
                    {
                        write.BulkWrite(read.Where(r => r[FACILITY_ID] == fac));
                    }

                    jobExecutor.LogInfo("Finished writing file " + facOut.FileName);
                }
            }

            return true;
        }
    }
}
