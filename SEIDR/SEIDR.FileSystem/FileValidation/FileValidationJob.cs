using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;
using SEIDR.FileSystem.FileValidation;
using SEIDR.DataBase;
using SEIDR;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;

namespace SEIDR.FileSystem
{
    [IJobMetaData(nameof(FileValidationJob), nameof(FileSystem), 
        "File Validation and cleaning",
        ConfigurationTable: "SEIDR.FileValidationJob", 
        NeedsFilePath:true, AllowRetry: false)]
    public partial class FileValidationJob : ContextJobBase<FileSystemContext>
    {
        public override void Process(FileSystemContext context)
        {
            var execution = context.Execution;
            if (string.IsNullOrWhiteSpace(execution.FilePath))
            {
                context.SetStatus(ResultStatusCode.NS);
                return;
            }
            ValidationError errCode = ValidationError.None;
            DatabaseManager mgr = context.Manager; // new DatabaseManager(jobExecutor.connection);
            //Get settings
            FileValidationJobConfiguration FV = FileValidationJobConfiguration.GetFileValidationJobConfiguration(mgr, execution.JobProfile_JobID);
            if (FV.Delimiter == null)
                FV.Delimiter = "|"; //Default output delimiter. NOTE: FileValidationJob does not work with Fix Width file at this time. Would need a more detailed configuration first to be able to do that.

            #region DocMetaDataSet, pull info
            DocMetaDataSet dms = null;
            //If DoConfiguration is true, will create a new DMS and insert after the DocReader infers metaData
            if (!FV.DoMetaDataConfiguration) 
            {
                //Store meta data over time, allow running for an older version of a file later on
                dms = new DocMetaDataSet(mgr, execution.JobProfile_JobID, execution.ProcessingDate); 
                if (dms.MetaData == null)
                {
                    errCode = ValidationError.UC;
                    context.SetStatus(errCode);
                    context.LogError(errCode.ToString() + ": not flagged to configure meta data, but no MetaData found valid for " + execution.ProcessingDate.ToShortDateString());
                    return;
                }
                //May want to eventually have something to override this and always use the current version? Can look at later on if needed
                else if (FV.CurrentMetaDataVersion != dms.MetaData.Version) 
                {
                    //Override current FV values with version values
                    FV.HasHeader = dms.MetaData.HasHeader;
                    FV.HasTrailer = dms.MetaData.HasTrailer;
                    FV.SkipLines = dms.MetaData.SkipLines;
                    FV.TextQualifier = dms.MetaData.TextQualifier;
                    FV.CurrentMetaDataVersion = dms.MetaData.Version; 
                }
            }
            #endregion

            var locFile = context.GetExecutionLocalFile(true);
            var outFile = context.ReserveBasicLocalFile("Output.CYM", true);
            outFile.SetExecutionFileInfo = true;
            outFile.OutputFilePath = execution.FilePath + (string.IsNullOrWhiteSpace(FV.OverrideExtension) ? ".CYM" : "." + FV.OverrideExtension);

            Doc.DocMetaData metaData = new Doc.DocMetaData(locFile, "r")
                .SetHasHeader(FV.HasHeader)
                .SetMultiLineEndDelimiters("\r", "\n", "\r\n") //Handle all three line endings...
                .SetSkipLines(FV.SkipLines);
            if (FV.MinimumColumnCountForMerge > 0)
                metaData.Columns.AllowMissingColumns = true;
            string emailMessage = string.Empty;
            using (var reader = new Doc.DocReader(metaData))
            {
                #region Meta Data, column validation
                string msg;
                if (dms != null)
                {
                    if (metaData.HasHeader) //If no header, then possible that first line needs cleaning
                    {
                        errCode = dms.CompareColumnData(reader.Columns, out msg);
                        if (errCode != ValidationError.None)
                        {
                            context.SetStatus(errCode);
                            context.LogError($"{errCode}: {msg}");
                            return;
                        }
                    }
                    else
                    {
                        if(metaData.Columns.Count < dms.MetaDataColumns.Count) //Use meta data columns from DB if this file has an erroneous newline in the first line
                        {
                            for (int i = metaData.Columns.Count; i < dms.MetaDataColumns.Count; i++)
                            {
                                metaData.AddColumn(dms.MetaDataColumns[i].ColumnName);
                            }
                        }
                    }
                }
                else
                {                    
                    List<DocMetaDataColumn> cols = new List<DocMetaDataColumn>();
                    metaData.Columns.ForEach(col =>
                    {
                        cols.Add(new DocMetaDataColumn
                        {
                            ColumnName = col.ColumnName,
                            Position = col.Position
                        });
                    });
                    var ColumnMetaData = new System.Data.DataTable("udt_DocMetaDataColumn");
                    ColumnMetaData.AddColumns<DocMetaDataColumn>();
                    cols.ForEach(c => ColumnMetaData.AddRow(c));
                    using (var helper = mgr.GetBasicHelper())
                    {
                        helper.ExpectedReturnValue = 0;
                        helper[nameof(FV.JobProfile_JobID)] = FV.JobProfile_JobID;
                        helper[nameof(ColumnMetaData)] = ColumnMetaData;
                        helper[nameof(DocMetaData.Delimiter)] = metaData.Delimiter;
                        helper[nameof(DocMetaData.HasHeader)] = FV.HasHeader;
                        helper[nameof(DocMetaData.HasTrailer)] = FV.HasTrailer;
                        helper[nameof(DocMetaData.SkipLines)] = FV.SkipLines;
                        helper[nameof(DocMetaData.TextQualifier)] = FV.TextQualifier;
                        helper[nameof(execution.ProcessingDate)] = execution.ProcessingDate;
                        helper.QualifiedProcedure = "[SEIDR].[usp_FileValidationDocMetaData_i]"; 
                        //Much of the same logic as usp_DocMetaData_i, but slightly different because of the extra config table.
                        mgr.ExecuteNonQuery(helper);
                        if(helper.ReturnValue != helper.ExpectedReturnValue)
                        {
                            context.SetStatus(ValidationError.MD);
                            return;
                        }
                    }
                }
                #endregion



                if (FV.TextQualifyColumnNumber.HasValue)
                {
                    FV.TextQualifyColumnNumber--; //Allow 1-based in DB, 0-based in C#

                    if (metaData.Columns.Count <= FV.TextQualifyColumnNumber || FV.TextQualifyColumnNumber < 0)
                    {
                        context.LogError("Configured TextQualifyColumnNumber is equal to/greater than total no. of columns. Column no.:" + metaData.Columns.Count + " Configured TextQualifyColumnNumber Column no.:" + FV.TextQualifyColumnNumber);
                        context.SetStatus(ValidationError.CT);
                        return;
                    }
                }
                
                Doc.DocMetaData output = new Doc.DocMetaData(outFile)
                     .SetHasHeader(FV.HasHeader)
                     .AddDetailedColumnCollection(reader.Columns);
                if (metaData.Delimiter != null)
                {
                    char? FVDelim = null;
                    if (!string.IsNullOrWhiteSpace(FV.Delimiter))
                        FVDelim = FV.Delimiter[0];
                    else
                    {
                        FVDelim = metaData.Delimiter;
                        FV.Delimiter = metaData.Delimiter.ToString();
                    }
                    output.SetDelimiter(FVDelim.Value);
                }
                output.Columns.AllowMissingColumns = true;
                output.Columns.TextQualifier = FV.TextQualifier;
                //Default: CRLF, CR and LF are both active. Don't need to do anything then
                if (!FV.LineEnd_CR)
                    output.SetLineEndDelimiter("\n"); //Just LF if not CRLF.
                else if (!FV.LineEnd_LF)
                    output.SetLineEndDelimiter("\r"); //Just CR if not CRLF. Available line endings: CR, LF, CRLF
                else
                    output.SetLineEndDelimiter("\r\n");
                //Note: at least one of the two above has to be flaggged. Constraint on table.

                using (var writer = new Doc.DocWriter(output))
                {
                    bool mergeNext = false;
                    int HeaderColumnCount = reader.Columns.Count;
                    int ColumnCount = 0;
                    string MergeRecords = string.Empty;
                    FV.CurrentDelimiter = metaData.Delimiter.Value;

                    for (int i = 0; i < reader.PageCount; i++)
                    {
                        var page = reader.GetPageLines(i);

                        //string[] hcol = page[0].Split((char)metaData.Delimiter);//considering first row is header. 
                        //HeadColumnNumbers = hcol.Length;
                        bool lastRecord = false;

                        for (int j = 0; j < page.Count; j++)
                        {
                            if (i == reader.PageCount - 1 && j == page.Count - 1)
                            {
                                lastRecord = true;
                            }                            

                            string record = page[j];
                            if (string.IsNullOrEmpty(record))
                                continue; //Skip lines that are totally empty. Maybe NullOrWhiteSpace?

                            string[] fields;

                            if (mergeNext)
                            {
                                record = MergeRecords + record;
                                MergeRecords = string.Empty;
                                mergeNext = false;
                            }

                            //Clean string logic
                            record = StringCleaningBeforeProcessing(record, FV, metaData);

                            fields = record.Split((char)metaData.Delimiter);
                            ColumnCount = fields.Length;
                            if (lastRecord)
                            {
                                if (ColumnCount != HeaderColumnCount && !FV.TextQualifyColumnNumber.HasValue || ColumnCount< HeaderColumnCount)
                                {
                                    if (FV.HasTrailer)
                                    {
                                        record = StringCleaningAfterProcessing(record, FV);                                        
                                        writer.BulkWrite(record);
                                        break; //Otherwise, would lose the trailer.
                                    }
                                    else if (ColumnCount >= FV.MinimumColumnCountForMerge) //Ignore records with fewer columns than minimum (default: 0).
                                    {
                                        context.LogError(" Last Record has the wrong no. of columns.");
                                        context.SetStatus(ValidationError.CC);
                                        return;
                                    }
                                }
                                else if (FV.TextQualifyColumnNumber.HasValue)
                                {
                                    int endIndex = ColumnCount - HeaderColumnCount + (int)FV.TextQualifyColumnNumber;
                                    fields[(int)FV.TextQualifyColumnNumber] = fields[(int)FV.TextQualifyColumnNumber].Insert(0, FV.TextQualifier);
                                    fields[endIndex] = fields[endIndex] + FV.TextQualifier;
                                    record = string.Join(FV.Delimiter, fields);
                                }
                            }
                            else if (ColumnCount < FV.MinimumColumnCountForMerge)
                            {
                                MergeRecords = string.Empty;
                                mergeNext = false;
                            }
                            else if (ColumnCount < HeaderColumnCount)
                            {
                                mergeNext = true;
                                MergeRecords = record;
                                continue;
                            }
                            else if (FV.TextQualifyColumnNumber.HasValue)
                            {
                                int endIndex = ColumnCount - HeaderColumnCount + (int)FV.TextQualifyColumnNumber;
                                fields[(int)FV.TextQualifyColumnNumber] = fields[(int)FV.TextQualifyColumnNumber].Insert(0, FV.TextQualifier);
                                fields[endIndex] = fields[endIndex] + FV.TextQualifier;
                                record = string.Join(FV.Delimiter, fields);                                
                            }
                            else if (ColumnCount > HeaderColumnCount)
                            {
                                long lineNumber = reader.CheckLineNumber(i, j);
                                context.LogError("Found too many columns at the line # " + lineNumber);
                                context.SetStatus(ValidationError.CC);
                                return;
                            }
                            else
                            {
                                MergeRecords = string.Empty;
                                mergeNext = false;
                            }

                            //Clean string.
                            record = StringCleaningAfterProcessing(record, FV);
                            if(ColumnCount < FV.MinimumColumnCountForMerge)
                            {
                                writer.BulkWrite(record);
                                continue;
                            }                            
                            writer.AddRecord(record);                           
                        }
                    }
                }
                
                #region File size validation check
                FileInfo cleanedFileInfo = new FileInfo(output.FilePath);
                cleanedFileInfo.CreationTime = execution.ProcessingDate;                
                bool isValidFileSize = ValidateFileSize(FV, cleanedFileInfo, execution, ref emailMessage);
                if (!isValidFileSize)
                {
                    if (!FV.SizeThresholdWarningMode)
                    {
                        emailMessage = $"File Cleaned successfully, but processing will halt due to file size.";
                        context.SetStatus(ValidationError.FS);
                        context.Executor.SendMail(new System.Net.Mail.MailMessage("SEIDR.JobExecutor@navigant.com", FV.NotificationList, "File Validation: File size check failed: JobProfileID :" + execution.JobProfileID, emailMessage));
                        return;
                    }
                    else//file processed but send warning email
                    {
                        emailMessage += $"File Cleaned successfully, but the file fails Size check. Processing will continue.";
                        context.Executor.SendMail(new System.Net.Mail.MailMessage("SEIDR.JobExecutor@navigant.com", FV.NotificationList, "File Validation: File size check failed: JobProfileID :" + execution.JobProfileID, emailMessage));
                    }
                }
                #endregion
            }

            locFile.Dispose();
                
            context.WorkingFile = outFile;
            if (!FV.KeepOriginal)
            {
                File.Delete(metaData.FilePath); //Delete original source file after successful cleaning, if KeepOriginal = 0
            }

            outFile.Finish();
        }

        public bool ValidateFileSize(FileValidationJobConfiguration fileConfig, FileInfo cleanedFileInfo, JobExecution execution, ref string emailMessage)
        {
            bool isValid = true;
            if (fileConfig.SizeThreshold.HasValue)
            {
                var directory = new DirectoryInfo(cleanedFileInfo.DirectoryName);    
                //fromDate, toDate calculate + and - SizeThresholdDayRange of current proccessing date
                DateTime fromDate = execution.ProcessingDate.AddDays(Convert.ToDouble(-fileConfig.SizeThresholdDayRange));
                DateTime toDate = execution.ProcessingDate.AddDays(Convert.ToDouble(fileConfig.SizeThresholdDayRange));
                var Oldfiles = directory.GetFiles().AsEnumerable()
                  .Where(file => file.CreationTime.Date >= fromDate.Date && file.CreationTime.Date <= toDate.Date
                  && file.Extension == ".CYM" && file.Name != cleanedFileInfo.Name).ToArray();
                var dayOfWeekFiles = Oldfiles.AsEnumerable().Where(file => file.CreationTime.DayOfWeek == cleanedFileInfo.CreationTime.DayOfWeek).ToArray();

                if (Oldfiles.Count() > 0)
                {
                    double avgFileSizeInBytes = Oldfiles.Average(file => file.Length);
                    double requiredThresholdFileSize = avgFileSizeInBytes * (1 - (double)fileConfig.SizeThreshold / 100);


                    double avgDayofWeekSizeInBytes = -1;
                    double ratioDay = -1;
                    if (dayOfWeekFiles.HasMinimumCount(1))
                    {
                        avgDayofWeekSizeInBytes = dayOfWeekFiles.Average(file => file.Length);
                        ratioDay = (cleanedFileInfo.Length / avgDayofWeekSizeInBytes);
                    }

                    if (cleanedFileInfo.Length < requiredThresholdFileSize)
                    {
                        isValid = false;
                        emailMessage = $"File: {cleanedFileInfo.FullName}{Environment.NewLine}"
                                        + $"Folder:{cleanedFileInfo.DirectoryName}{Environment.NewLine}{Environment.NewLine}"
                                        + $"Validation Error: Failed file size check. Size was {FormatSize(cleanedFileInfo.Length)}. {Environment.NewLine}"
                                        + $"Required threshold file size is:{FormatSize((long)requiredThresholdFileSize)} {Environment.NewLine}{Environment.NewLine}"
                                        + $"Ratio:{cleanedFileInfo.Length / avgFileSizeInBytes }. Average size: {FormatSize((long)avgFileSizeInBytes)}.{Environment.NewLine}";
                        if (avgDayofWeekSizeInBytes >= 0)
                        {
                            emailMessage += $"Ratio by Day of week:({cleanedFileInfo.CreationTime.DayOfWeek}):{Math.Round(ratioDay, 3)}{Environment.NewLine}"
                                        + $"Average size by Day of week:({cleanedFileInfo.CreationTime.DayOfWeek}):{FormatSize((long)avgDayofWeekSizeInBytes / dayOfWeekFiles.Count())}{Environment.NewLine}";
                        }
                        emailMessage += $"Date range for check was - {fileConfig.SizeThresholdDayRange} days through + {fileConfig.SizeThresholdDayRange} days. Doubled for check by day. (SDR:{fileConfig.SizeThresholdDayRange * 2}) {Environment.NewLine}"; //Multiply by two due to fromDate and toDate calculation
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Get file size converted based on it's size
        /// </summary>
        /// <param name="value">file size</param>
        /// <param name="decimalPlaces">level decimal places default is 2</param>
        /// <returns>formated string with appropriate size</returns>
        /// ToDo:This method can be shifted to Creating Static Helper calss
        public string FormatSize(long value, int decimalPlaces = 2)
        {
            string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB" }; //if required then add "PB", "EB", "ZB", "YB" in array            
            int mag = (int)Math.Log(value, 1024);            
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));            
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }
            return string.Format("{0:n" + decimalPlaces + "} {1}",adjustedSize,SizeSuffixes[mag]);
        }

        #region ValidateClean Logic

        /// <summary>
        /// String cleaning before processing.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="fileConfig"></param>
        /// <returns></returns>
        public string StringCleaningBeforeProcessing(string line, FileValidationJobConfiguration fileConfig, Doc.DocMetaData metaData)
        {
            string fastInvalids = @"[\u0000-\u0008\u000B-\u0019\u00A0]";
            

            line = Regex.Replace(line, fastInvalids, "");
            line = Regex.Replace(line, @"[\u0092]", "'");

            if (string.IsNullOrEmpty(fileConfig.TextQualifier)) { return line; }
            if (fileConfig.Delimiter == null)
                return line; //Fix width output doesn't make sense to do switches.

            if (!line.Contains(fileConfig.TextQualifier))
                return line;
            


            string[] switcher = line.SplitByKeyword(fileConfig.TextQualifier).ToArray();
            if (switcher.Length % 2 == 0)
            {
                string NonQuote_Quote = "([^" + fileConfig.CurrentDelimiter + @"]+?)";
                NonQuote_Quote = NonQuote_Quote + @"(\\?" + fileConfig.TextQualifier + "|\\\\\")" + "(?=[^" + fileConfig.CurrentDelimiter + "])"; //Quotes not next to a delimiter
                                                                                                                                                  //([^|]+?)(\"|\\\\\")(?=[^|])
                                                                                                                                                  //Replace quote \" not adjacent to | with QUOTE_REPLACEMENT
                                                                                                                                                  // Replace quote not adjacent to | with QUOTE_REPLACEMENT


                line = Regex.Replace(line, NonQuote_Quote, "$1" + QUOTE_REPLACEMENT.ToString());

                int colCount = line.Split(fileConfig.Delimiter[0]).Length;
                if (colCount < metaData.Columns.Count && colCount >= fileConfig.MinimumColumnCountForMerge)
                    return line; //If we're missing columns here, then we might need to merge with the next line, and that may have the missing qualifiers.

                switcher = line.SplitByKeyword(fileConfig.TextQualifier).ToArray();
                
                if (switcher.Length % 2 == 0)
                    throw new Exception("Text Qualifier incorrect - Odd number of quotes identified. Change the text qualifier - unmatched quotes will cause parsing issues.");
            }

            for (int i = 1; i < switcher.Length; i += 2)
            {
                switcher[i] = switcher[i].Replace("" + fileConfig.CurrentDelimiter, fileConfig.DesireSwitch);
            }
            return string.Join("" + fileConfig.TextQualifier, switcher);
        }

        const char QUOTE_REPLACEMENT = (char)146;

        /// <summary>
        /// String cleaning after processing
        /// </summary>
        /// <param name="line"></param>
        /// <param name="fileConfig"></param>
        /// <returns></returns>
        public string StringCleaningAfterProcessing(string line, FileValidationJobConfiguration fileConfig)
        {
            if (string.IsNullOrEmpty(fileConfig.TextQualifier)) { return line; }
            if (fileConfig.Delimiter == null)
                return line; //Fix width output doesn't make sense to do switches. NOTE: Do not even currently support FixWidth, but that would be indicated by having a null delimiter. (Then using the Max Length of each column in MetaData to determine padding)

            string[] switcher = line.SplitByKeyword(fileConfig.TextQualifier).ToArray();            
            for (int i = 0; i < switcher.Length; i += 2)
            {
                switcher[i] = switcher[i].Replace(fileConfig.CurrentDelimiter.ToString(), fileConfig.Delimiter);
            }
            
            var result = string.Join("" + fileConfig.TextQualifier, switcher);
          

            switcher = result.SplitByKeyword(fileConfig.TextQualifier).ToArray();
            for (int i = 1; i < switcher.Length; i += 2)
            {
                switcher[i] = switcher[i].Replace(fileConfig.DesireSwitch, "" + fileConfig.CurrentDelimiter);
            }
            
            result = string.Join("" + fileConfig.TextQualifier, switcher);
            if (fileConfig.TextQualifier == "\"")
                result = result.Replace(QUOTE_REPLACEMENT.ToString(), "''");
            else
                result = result.Replace(QUOTE_REPLACEMENT, '"');

            return result;
        }

        #endregion
    }
}
