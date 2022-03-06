using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SEIDR.METRIX_EXPORT
{
    /// <summary>
    /// Helper class to allow working with a local file path for writing to FileStream
    /// <para>(Writing across the network can be expensive, while a FileCopy/Move of the finished file is relatively cheap)</para>
    /// <para>NOTE: This class allows implicitly casting to a string by using the overridden <see cref="ToString"/> method.</para>
    /// </summary>
    public class LocalFileHelper
    {
        /// <summary>
        /// Indicates whether or not <see cref="Finish"/> has been called.
        /// </summary>
        public bool Finished { get; private set; } = false;
        /// <summary>
        /// Indicates that the Working File has been started and is not finished.
        /// </summary>
        public bool Working => !Finished && File.Exists(WorkingFilePath);
        /// <summary>
        /// The path for the file being processed.
        /// </summary>
        public readonly string WorkingFilePath;

        /// <summary>
        /// Implicitly treats the Object as it's ToString representation, since the LocalFileHelper is about managing the File's Path, as well as its state.
        /// </summary>
        /// <param name="file"></param>
        public static implicit operator string(LocalFileHelper file) => file.ToString();

        /// <summary>
        /// Depending on the state of the LocalFileHelper object, returns either <see cref="WorkingFilePath"/> (<see cref="Finished"/> = False), or <see cref="OutputFilePath"/> (<see cref="Finished"/> = True)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Finished ? OutputFilePath : WorkingFilePath;
        }
        /// <summary>
        /// Initialize to <see cref="ExportSetting.ArchiveLocation"/> 
        /// </summary>
        public string OutputDirectory { get; set; }
        /// <summary>
        /// Final file path
        /// </summary>
        public string OutputFileName { get; set; }

        /// <summary>
        /// Combines OutputDirectory/OutputFileName.
        /// </summary>
        public string OutputFilePath
        {
            get { return Path.Combine(OutputDirectory, OutputFileName); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Attempted to pass empty or null string to OutputFilePath.");
                OutputDirectory = Path.GetDirectoryName(value);
                OutputFileName = Path.GetFileName(value);
            }
        }
        /// <summary>
        /// The directory of the Working File Path. Value that was passed to the constructor.
        /// </summary>
        public string WorkDirectory { get; }
        /// <summary>
        /// Default directory to use for working.
        /// </summary>
        public static readonly string DefaultWorkingDirectory;

        static LocalFileHelper()
        {
            //Should never be null, because this is the same key used by the service as the location to pick up the DLL that we're running from.
            string jobFolder = ConfigurationManager.AppSettings["JobLibrary"]; 
            DefaultWorkingDirectory = Path.Combine(jobFolder, nameof(METRIX_EXPORT));
        }

        private readonly ExportJobBase _job;
        private readonly ExportContextHelper _context;
        private JobBase.JobExecution _execution => _context.Execution;

        private JobBase.IJobExecutor _caller => _context.Executor;


        public LocalFileHelper(ExportJobBase job, 
            ExportContextHelper context,
            string WorkingDirectory)
        {
            _job = job;
            _context = context;
            if (string.IsNullOrWhiteSpace(WorkingDirectory))
                throw new ArgumentException("WorkingDirectory must be provided a non empty string", nameof(WorkingDirectory));

            WorkDirectory = WorkingDirectory;
            WorkingFilePath = Path.Combine(WorkingDirectory, Math.Abs(context.JobExecutionID) + ".TEMP");
            if(!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);
            if (File.Exists(WorkingFilePath))
                File.Delete(WorkingFilePath);

            OutputDirectory = context.Settings?.ArchiveLocation; //Some export jobs may not require ExportSettings to be configured. (no Archive location, or okay with using default METRIX database lookup by description)
        }

        public void InitializeFromFile(string sourceFilePath)
        {
            if (Working)
                throw new InvalidOperationException("Working file has already been started.");
            File.Copy(sourceFilePath, WorkingFilePath, true);
        }

        /// <summary>
        /// Initializes and returns a <see cref="Doc.DocMetaData"/> instance that can be used with a <see cref="Doc.DocWriter"/> to create content in the working file.
        /// </summary>
        /// <returns></returns>
        public Doc.DocMetaData GetDocMetaData()
        {
            return new Doc.DocMetaData(WorkingFilePath);
        }
        /// <summary>
        /// Initializes and returns a <see cref="Doc.DocMetaData"/> instance that can be used with a <see cref="Doc.DocWriter"/> to create content in the working file.
        /// <para>Specifies the delimiter and any columns.</para>
        /// </summary>
        /// <returns></returns>
        public Doc.DocMetaData GetDocMetaData(char delimiter, params string[] columns)
        {
            var md = new Doc.DocMetaData(WorkingFilePath);
            return md.SetDelimiter(delimiter)
                    .AddDelimitedColumns(columns);
        }
        

        /// <summary>
        /// Moves the working file from the temporary location to the Output location, and sets information in the job execution originally passed.
        /// </summary>
        public void Finish()
        {
            if (Finished)
                throw new InvalidOperationException($"{nameof(Finish)}() cannot be called. ('{nameof(Finished)}' = True)");
            if (string.IsNullOrWhiteSpace(OutputFileName))
                throw new InvalidOperationException($"{nameof(OutputFileName)} has not been set.");

            DirectoryInfo di = new DirectoryInfo(OutputDirectory);
            if (!di.Exists)
                di.Create();
            File.Move(WorkingFilePath, OutputFilePath);
            _execution.SetFileInfo(OutputFilePath);
            _job.SetCheckPoint_Finalize(_context);
            Finished = true;
        }

        public void ClearWork()
        {
            if (Finished)
                throw new InvalidOperationException($"{nameof(ClearWork)} cannot be called. ('{nameof(Finished)}' = True)");
            if (File.Exists(WorkingFilePath))
            {
                try
                {
                    File.Delete(WorkingFilePath);
                    Finished = true;
                }
                catch(Exception ex)
                {
                    _context.LogError("Unable to clear working file", ex);
                }
            }
        }
    }
}
