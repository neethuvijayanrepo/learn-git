using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dts.Runtime;
using SEIDR.JobBase;

namespace SEIDR.PreProcess
{
    public class SSISPackage
    {
        static readonly string AndromedaReportServer = ConfigurationManager.AppSettings["AndromedaReportServer"];
        readonly Dictionary<string, object> _optionalVariables = new Dictionary<string, object>();

        public object this[string Key]
        {
            get
            {
                if (!_optionalVariables.ContainsKey(Key))
                    return null;
                return _optionalVariables[Key];
            }
            set { _optionalVariables[Key] = value; }
        }

        public string PackagePath { get; private set; }
        public string InputFile { get; private set; }
        public int LoadBatchID { get; private set; }

        public int? DatabaseLookupID { get; private set; }
        public string DatabaseConnectionManager { get; private set; }


        public const string OUTPUT_FILE_NAME = "OutputFileName";
        public const string OUTPUT_FOLDER = "OutputFolder";
        public const string OUTPUT_FILEPATH = "OutputFilePath";
        public const string OUTPUT_PATH = "OutputPath";
        public const string OUTPUT_FILE = "OutputFile";
        
        const string SECONDARY_FILEPATH = "SecondaryFilePath";
        const string TERTIARY_FILEPATH = "TertiaryFilePath";

        private const string USER_PREFIX = "User::";

        /// <summary>
        /// Maps our variables here to the variables in the SSIS package.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>True if we successfully map our variables.<para>Return false if any variables have issue with being mapped. (E.g., case folding)</para></returns>
        public bool MapPackage(Package p, SSISContext context)
        {
            _p = p;
            vList = p.Variables;

            //Local working file if relevant.
            InputFile = context.CurrentFilePath;

            if (string.IsNullOrEmpty(InputFile) && vList.Contains("InputFile"))
                throw new Exception("Input file not set.");

            _optionalVariables.Add("InputFile", InputFile);
            _optionalVariables.Add("TestMode", false);
            _optionalVariables.Add("DebugMode", false);
            _optionalVariables.Add("LocalTestMode", false);
            _optionalVariables.Add("AndromedaReportServer", AndromedaReportServer);

            bool optionalMapFailure = false;
            

            if (DatabaseConnectionManager != null && DatabaseLookupID.HasValue)
            {
                var connection = context.Executor.GetConnection(DatabaseLookupID.Value);
                connection.ForceDataSource = true;
                Connections conns = p.Connections;
                bool found = false;
                foreach (ConnectionManager dbConnectionManager in conns)
                {

                    if (dbConnectionManager.Name == DatabaseConnectionManager)
                    {                        
                        found = true;
                        dbConnectionManager.ConnectionString = connection.ConnectionString;
                    }
                }
                if (!found)
                {
                    context.LogError("Matching ConnectionString cannot be found");
                    optionalMapFailure = true;
                }
            }

            foreach (var kv in _optionalVariables)
            {
                var variableName = USER_PREFIX + kv.Key;
                if (vList.Contains(variableName) && !vList[variableName].SystemVariable)
                {
                    _context.LogInfo($"{kv.Key} : {kv.Value ?? "(NULL)"} {Environment.NewLine}");
                    if (kv.Value != DBNull.Value && kv.Value != null)
                    {
                        if (vList[variableName].DataType == TypeCode.String)
                        {
                            if (kv.Value is DateTime)
                                vList[variableName].Value = ((DateTime)kv.Value).ToString("MM/dd/yyyy");
                            else
                                vList[variableName].Value = kv.Value.ToString();
                        }
                        else
                            vList[variableName].Value = kv.Value;
                    }
                    continue;
                }

                foreach (var v in vList)
                {
                    if (!v.SystemVariable && v.Name.Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        _context.SetStatus(ResultStatusCode.F);
                        _context.LogError($"Configuration Variable '{kv.Key}' - Case does not match package usage: '{v.Name}' (NameSpace: {v.Namespace})");
                        optionalMapFailure = true;
                    }
                }
            }
            //Data Source = NCIMTXIMPSQL06; User ID = idpnuser; Initial Catalog = NTIER_IDPN_Repl; Provider = SQLNCLI11.1;

            return !optionalMapFailure;
        }
        private SSISContext _context;
        private Package _p;
        private Variables vList;

        public void Execute()
        {
            _context.LogInfo($"Starting up execution of package : {Path.GetFileNameWithoutExtension(PackagePath)} \tJobExecutionID : {_context.JobExecutionID}");
            var executionResult = _p.Execute(null, vList, null, null, null);

            if (executionResult == DTSExecResult.Success || executionResult == DTSExecResult.Completion)
            {
                _context.SetStatus(ResultStatusCode.SS);
            }
            else if (executionResult == DTSExecResult.Canceled)
            {
                //Canceled
                _context.SetStatus(ResultStatusCode.CX);
            }
            else
            {
                _context.SetStatus(ResultStatusCode.F);
                //failure [executionResult == DTSExecResult.Failure]

                //Logging all Package errors.
                foreach (var e in _p.Errors)
                {
                    _context.LogError(string.Format("Error Source : {0} \n Error SubComponent : {1} \n ErrorCode : {2} \n Error Description : {3}", e.Source, e.SubComponent, e.ErrorCode, e.Description));
                }
            }
        }
        public void Setup(DataRow configuration, SSISContext context)
        {
            _context = context;
            Debug.Assert(configuration != null);
            PackagePath = configuration[nameof(PackagePath)].ToString();
            foreach (DataColumn optional in configuration.Table.Columns)
            {
                if (!_optionalVariables.ContainsKey(optional.ColumnName))
                {
                    if (optional.DataType == typeof(string) 
                        && (
                               optional.ColumnName.Like("Output%")
                               || optional.ColumnName.In(SECONDARY_FILEPATH, TERTIARY_FILEPATH)
                            )
                        )
                    {
                        string var = configuration[optional].ToString();
                        if (string.IsNullOrWhiteSpace(var)
                            && optional.ColumnName.In(OUTPUT_FILE_NAME, //Don't set these if empty.
                                                      SECONDARY_FILEPATH, 
                                                      TERTIARY_FILEPATH))
                            continue;

                        var = FileSystem.FS.ApplyDateMask(var, context.ProcessingDate);
                        _optionalVariables.Add(optional.ColumnName, var);
                    }
                    else
                    {
                        _optionalVariables.Add(optional.ColumnName, configuration[optional]);
                    }
                }
            }
            DatabaseConnectionManager = configuration[nameof(PreProcessConfiguration.DatabaseConnectionManager)].ToString();
            var dbLookupID = configuration[nameof(PreProcessConfiguration.DatabaseConnection_DatabaseLookupID)];
            if (!(dbLookupID is DBNull))
                DatabaseLookupID = Convert.ToInt32(dbLookupID);

            _optionalVariables.Add("JobExecutionID", context.JobExecutionID);

            LoadBatchID = int.Parse(context.JobExecutionID.ToString() + context.Execution.StepNumber.ToString());
            _optionalVariables.Add("LoadBatchID", LoadBatchID);

            _optionalVariables.Add("ProcessingDate", context.ProcessingDate);
            _optionalVariables.Add("InputFileDate", context.ProcessingDate);
            _optionalVariables.Add("ReconciliationDate", context.ProcessingDate.AddDays(-1));
            _optionalVariables.Add("FileDate", context.ProcessingDate);


            if (_optionalVariables.ContainsKey(OUTPUT_FOLDER) && _optionalVariables.ContainsKey(OUTPUT_FILE_NAME)
                                                             && !_optionalVariables.ContainsKey(OUTPUT_FILEPATH))
            {
                string filePath = Path.Combine(
                                               _optionalVariables[OUTPUT_FOLDER].ToString(),
                                               _optionalVariables[OUTPUT_FILE_NAME].ToString()
                                              );
                _optionalVariables.Add(OUTPUT_FILEPATH, filePath);
            }

            if (_optionalVariables.ContainsKey(OUTPUT_FOLDER) && _optionalVariables.ContainsKey(SECONDARY_FILEPATH))
            {
                string filePath = _optionalVariables[SECONDARY_FILEPATH].ToString();
                if (Path.GetDirectoryName(filePath) == string.Empty)
                    _optionalVariables[SECONDARY_FILEPATH] = Path.Combine(_optionalVariables[OUTPUT_FOLDER].ToString(), filePath);
            }

            if (_optionalVariables.ContainsKey(OUTPUT_FOLDER) && _optionalVariables.ContainsKey(TERTIARY_FILEPATH))
            {

                string filePath = _optionalVariables[TERTIARY_FILEPATH].ToString();
                if (Path.GetDirectoryName(filePath) == string.Empty)
                    _optionalVariables[TERTIARY_FILEPATH] = Path.Combine(_optionalVariables[OUTPUT_FOLDER].ToString(), filePath);
            }

        }

        public FileInfo CheckOutputFile(string Variable)
        {
            if (_p.Variables.Contains(Variable))
            {
                _context.LogInfo("Checking for OutputFilePath..." + _p.Variables[Variable].Value.ToString());
                FileInfo f = new FileInfo(_p.Variables[Variable].Value.ToString());
                return f; 
            }
            return null;
        }
    }
}
