using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.Doc;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.EDI
{
    [IJobMetaData("EDI Conversion Job", nameof(SEIDR.FileSystem), "Convert Control Character Segment Delimiters to ~, and set extension of output based on Type of File.",
        ConfigurationTable: "SEIDR.EdiConversion",
        NotificationTime: 5, NeedsFilePath: true, AllowRetry: false)]
    public class EdiConversionJob : IJob
    {
        const string NO_CLAIM_SEGMENT = "NC";
        public int CheckThread(JobExecution jobCheck, int passedThreadID, IJobExecutor jobExecutor)
        {
            return passedThreadID;
        }
        public bool ProcessSettings(JobExecution execution, EdiConversion settings, ref ExecutionStatus status)
        {

            Encoding fileEncoding = Encoding.Default;
            if (settings.CodePage.HasValue)
                fileEncoding = Encoding.GetEncoding(settings.CodePage.Value);
            bool overwrite = false;
            string folder = Path.GetDirectoryName(execution.FilePath);
            if (!string.IsNullOrWhiteSpace(settings.OutputFolder))
            {
                overwrite = true;
                folder = FS.ApplyDateMask(settings.OutputFolder, execution.ProcessingDate);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            string outputFilePath = null;
            using (StreamReader sr = new StreamReader(execution.FilePath, fileEncoding))
            {
                const int MINIMUM_BLOCK_SIZE = 1000; //Minimum block size.
                int BLOCK_SIZE = settings.BlockSize;
                if (BLOCK_SIZE < MINIMUM_BLOCK_SIZE)
                    BLOCK_SIZE = MINIMUM_BLOCK_SIZE;

                char[] buffer = new char[BLOCK_SIZE];

                const int ISA_LENGTH = 106;
                const int ELEMENT_DELIMITER_POSITION = 3;
                const int SEGMENT_DELIMITER_POSITION = ISA_LENGTH - 1;
                const int GS_SEGMENT_POSITION = 1;
                const int VERSION_ELEMENT = 8;

                int block = 0;
                if ((block = sr.ReadBlock(buffer, 0, BLOCK_SIZE)) < ISA_LENGTH)
                {
                    status = new ExecutionStatus
                    {
                        ExecutionStatusCode = "IH",
                        Description = "Invalid header - less than 106 characters found in file.",
                        IsError = true,
                        IsComplete = false
                    };
                    return false;
                }
                string ISA = new string(buffer, 0, ISA_LENGTH);
                string Init = new string(buffer, 0, block);

                char _ELEMENT_DELIMITER = ISA[ELEMENT_DELIMITER_POSITION];
                char _SEGMENT_DELIMITER = ISA[SEGMENT_DELIMITER_POSITION];

                var initSegments = Init.Split(new char[] { _SEGMENT_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);
                string version = initSegments[GS_SEGMENT_POSITION].Split(_ELEMENT_DELIMITER)[VERSION_ELEMENT];
                
                EdiFileType myFileType = EdiFileType.UNKNOWN;

                if (version.Contains("X221"))
                    myFileType = EdiFileType.EDI835;
                else if (version.Contains("X222"))
                    myFileType = EdiFileType.EDI837p;
                else if (version.Contains("X223"))
                    myFileType = EdiFileType.EDI837i;
                else if (version.Contains("X224"))
                    myFileType = EdiFileType.EDI837d;

                string ext = "." + myFileType.ToString();
                if (myFileType != EdiFileType.UNKNOWN)
                    ext = ext.Replace("EDI", string.Empty);

                if (ext == Path.GetExtension(execution.FilePath) && settings.OutputFolder != null) //If we're going to a new folder, and we would be duplicating the extension, set this to empty string instead.
                    ext = string.Empty;
                /*
                string DatePrepend = execution.ProcessingDate.ToString("yyyyMMdd");
                if (execution.FileName.StartsWith(DatePrepend))
                    DatePrepend = string.Empty;
                */
                char[] CONTROL_SET = "\f\n\r\t\v".ToCharArray();
                switch (myFileType)
                {
                    case EdiFileType.EDI835:
                        {
                            if (initSegments.NotExists(segment =>
                            {
                                string temp = segment.TrimStart(CONTROL_SET);
                                return temp.StartsWith("CLP" + _ELEMENT_DELIMITER);
                            }))
                            {
                                status = new ExecutionStatus
                                {
                                    ExecutionStatusCode = NO_CLAIM_SEGMENT,
                                    NameSpace = nameof(EDI),
                                    IsComplete = true,
                                    IsError = false,
                                    Description = "Missing Claim/Patient Segments"
                                };
                                return true;
                            }
                            break;
                        }
                    case EdiFileType.EDI837d:
                    case EdiFileType.EDI837i:
                    case EdiFileType.EDI837p:
                        {
                            if (initSegments.NotExists(segment =>
                            {
                                string temp = segment.TrimStart(CONTROL_SET);
                                return temp.StartsWith("CLM" + _ELEMENT_DELIMITER);
                            }))
                            {
                                status = new ExecutionStatus
                                {
                                    ExecutionStatusCode = NO_CLAIM_SEGMENT,
                                    NameSpace = nameof(EDI),
                                    IsComplete = true,
                                    IsError = false,
                                    Description = "Missing Claim/Patient Segments"
                                };                                
                                return true;
                            }
                            break;
                        }                        
                }


                //ToDo: Parse and check that the file has at least 1 CLM or CLP segment if it's either an 835 or 837. E.g., if we have an 835 or 837, then we can probably just check that initSegments has a line that starts with CLM or CLP
                //Also note: we could probably just increase the block size and then only check the initial block. EDI files typically aren't very big, so by ~10,000 it should be reasonable to assume that we would have either finished the file or come across the CLM/CLP segment if it's there. (The example file that was missing a CLP segment had a TOTAL length of ~700)
                //Also - DocReader class defaults to 10 million as the block size (~10 MB) without performance issues, so we shouldn't have any issue with 10, 000 (10 KB, or .01 MB)

                outputFilePath = Path.Combine(folder, execution.FileName + ext);
               
                if (char.IsControl(_SEGMENT_DELIMITER))
                {                                     
                    using (var sw = new StreamWriter(outputFilePath, false, fileEncoding))
                    {
                        sw.Write(Init.Replace(_SEGMENT_DELIMITER.ToString(), "~" + _SEGMENT_DELIMITER));
                        while ((block = sr.ReadBlock(buffer, 0, BLOCK_SIZE)) != 0)
                        {
                            sw.Write(EdiClean(buffer, block, _SEGMENT_DELIMITER));
                        }
                    }
                }
                else /**************    Segment Delimiter is NOT a control character. No modification to file needed.       **************************/
                {
                    if (settings.KeepOriginal)
                        File.Copy(execution.FilePath, outputFilePath, overwrite);
                    else
                        File.Move(execution.FilePath, outputFilePath);

                    execution.FilePath = outputFilePath; //Hash & Length already accurate if just moving.
                    return true;
                }   
                /*****************************************************************************************************************************/

            }
            FileInfo output = new FileInfo(outputFilePath);
            if (!output.Exists)
            {
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "NO",
                    Description = "No Output File found.",
                    IsError = true
                };
                return false;
            }

            if (!settings.KeepOriginal)
            {
                File.Delete(execution.FilePath);
            }
            execution.SetFileInfo(output);
            return true;
        }
        string EdiClean(char[] input, int len, char _SEGMENT_DELIMITER)
        {
            string working = new string(input, 0, len);
            return working.Replace(_SEGMENT_DELIMITER.ToString(), "~" + _SEGMENT_DELIMITER);
        }


        public bool Execute(IJobExecutor jobExecutor, JobExecution execution, ref ExecutionStatus status)
        {
            if(string.IsNullOrWhiteSpace(execution.FilePath))
            {
                status = new ExecutionStatus
                {
                    ExecutionStatusCode = "NS",
                    Description = "No Source File",
                    IsError = true,
                    IsComplete = false
                };                
                return false;                
            }

            EdiConversion settings = jobExecutor.Manager.SelectSingle<EdiConversion>(new { JobProfile_JobID = execution.JobProfile_JobID }); //, RequireSingle: true, Schema: "SEIDR"); //SEIDR.usp_EdiConversion_ss

            bool result = ProcessSettings(execution, settings, ref status);
            if (status != null && status.ExecutionStatusCode == NO_CLAIM_SEGMENT)
                jobExecutor.LogInfo("Missing Segment for linking data to Accounts. Force completing Execution.");
            return result;

        }
    }
}
