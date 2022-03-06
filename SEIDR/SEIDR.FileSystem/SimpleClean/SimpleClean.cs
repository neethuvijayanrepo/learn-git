using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEIDR.JobBase;

namespace SEIDR.FileSystem.SimpleClean
{
    [IJobMetaData(nameof(SimpleCleanJob), nameof(FileSystem), "Simple and Clean file output.", 
        NeedsFilePath: true, AllowRetry:false, 
        NotificationTime: 5,
        ConfigurationTable: "SEIDR.SimpleCleanFileJob")]
    public class SimpleCleanJob : ContextJobBase<FileSystemContext>
    {
        public string DoClean(string inputFile, SimpleCleanConfiguration config, FileSystemContext context)
        {
            string outputFile = inputFile + "." + config.Extension;
            string lineEnd = "";
            if (config.LineEnd_CR)
                lineEnd = "\r";
            if (config.LineEnd_LF)
                lineEnd += "\n";

            int bufferLen = config.BlockSize ?? Doc.DocMetaData.DEFAULT_PAGE_SIZE; ;
            int lineCounter = 0;

            Encoding enc = Encoding.Default;
            if (config.CodePage != null)
                enc = Encoding.GetEncoding(config.CodePage.Value);
            using (StreamReader sr = new StreamReader(inputFile, enc))
            using (StreamWriter sw = new StreamWriter(outputFile, false, enc))
            {
                char[] block = new char[bufferLen];
                int x;
                StringBuilder hold = new StringBuilder();
                while ((x = sr.ReadBlock(block, 0, bufferLen)) > 0)
                {
                    bool last = x < bufferLen;
                    string content;
                    if (hold.Length == 0)
                    {
                        content = new string(block, 0, x);
                    }
                    else
                    {
                        content = hold + new string(block, 0, x);
                        hold.Clear();
                    }
                    string[] lineData = content.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (last)
                    {
                        foreach (var line in lineData)
                        {
                            lineCounter++;
                            if (config.Line_MinLength.HasValue && line.Length < config.Line_MinLength)
                            {
                                context.LogError("Line # " + lineCounter + $" - Length below minimum length({config.Line_MinLength}): " + line.Length);
                                context.SetStatus(ResultStatusCode.LL);
                                return null;
                            }
                            if (config.Line_MaxLength.HasValue && line.Length > config.Line_MaxLength)
                            {
                                context.LogError("Line # " + lineCounter + $" - Length above maximum length({config.Line_MaxLength}): " + line.Length);
                                context.SetStatus(ResultStatusCode.HL);
                                return null;
                            }
                            sw.Write(line.Replace('\0', ' ') + lineEnd);
                        }
                        break;
                    }
                    else if (lineData.Length > 0) //All empty entries? 
                    {
                        hold.Append(lineData[lineData.Length - 1]);
                        if (content.EndsWith("\r\n")
                            || content.EndsWith("\r")
                            || content.EndsWith("\n"))
                        {
                            hold.Append(lineEnd);
                        }
                        for (int i = 0; i < lineData.Length - 1; i++)
                        {
                            string line = lineData[i];
                            lineCounter++;
                            if (config.Line_MinLength.HasValue && line.Length < config.Line_MinLength)
                            {
                                context.LogError("Line # " + lineCounter + $" - Length below minimum length({config.Line_MinLength}): " + line.Length);
                                context.SetStatus(ResultStatusCode.LL);
                                return null;
                            }
                            if (config.Line_MaxLength.HasValue && line.Length > config.Line_MaxLength)
                            {
                                context.LogError("Line # " + lineCounter + $" - Length above maximum length({config.Line_MaxLength}): " + line.Length);
                                context.SetStatus(ResultStatusCode.HL);
                                return null;
                            }
                            sw.Write(line.Replace('\0', ' ') + lineEnd);
                        }
                    }
                }
                if (config.AddTrailer)
                    sw.Write("TRAILER:" + Path.GetFileName(outputFile) + "    LineCount:" + lineCounter);
            }
            return outputFile;
        }
        public override void Process(FileSystemContext context)
        {
            //Note: use currentFilePath so that we can optionally use a local file.
            if (string.IsNullOrWhiteSpace(context.CurrentFilePath) || !File.Exists(context.CurrentFilePath))
            {
                context.SetStatus(ResultStatusCode.NS);
                return;
            }
            SimpleCleanConfiguration config;
            using (var h = context.Manager.GetBasicHelper())
            {
                h.QualifiedProcedure = "SEIDR.usp_SimpleCleanFileJob_ss";
                h[nameof(context.JobProfile_JobID)] = context.JobProfile_JobID;
                config = context.Manager.SelectSingle<SimpleCleanConfiguration>(h) ?? new SimpleCleanConfiguration();
            }
            if (context.WorkingFile == null)
            {
                context.WorkingFile = context.GetExecutionLocalFile();
                context.WorkingFile.OutputFileName = context.FileName + "." + config.Extension;
            }
            else
                config.KeepOriginal = true; //force Keep Original if we're using 
            string outputPath = DoClean(context.CurrentFilePath, config,context);
            if (outputPath == null)
            {
                if (context.ResultStatus == null)
                    context.SetStatus(false);
                return;
            }
            FileInfo fi = new FileInfo(outputPath);
            fi.CreationTime = context;
            if (!config.KeepOriginal)
                File.Delete(context.FilePath);
            context.SetCurrentFilePath(outputPath); 
            // Note: localFile will have Finish called by the caller (other job, or ContextJobBase.Execute)
        }
    }
}
