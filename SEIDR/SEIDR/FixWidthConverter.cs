using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SEIDR;
using SEIDR.Doc;

namespace SEIDR
{
    /// <summary>
    /// Class for converting a fixed width file to pipe delimited. Intended use is for FileAnalysis, not for loading.
    /// </summary>    
    public class FixWidthConverter
    {
        /// <summary>
        /// valid delimiters to pass to <see cref="SetDelimiter(DELIMITER)"/> 
        /// </summary>
        public enum DELIMITER
        {
            /// <summary>
            /// Tab Delimiter
            /// </summary>
            TAB,
            /// <summary>
            /// Comma Delimiter
            /// </summary>
            COMMA,
            /// <summary>
            /// Pipe Delimiter (|)
            /// </summary>
            PIPE
        }
        //Line +- Item1 named as Item2, pulling string from Item3 up to Item4 or end of line
        public List<Tuple<int, string, int, int?>> AnchorModDerivePulls { get; private set; } = new List<Tuple<int, string, int, int?>>();
        int CheckBufferQueueLength()
        {
            var p = (from t in AnchorModDerivePulls
                     where t.Item1 < 0
                     select -t.Item1);
            if (p.HasMinimumCount(1))
                return p.Max();
            return 0;
        }
        int CheckPostBufferWait()
        {
            var p = (from t in AnchorModDerivePulls
                     where t.Item1 > 0
                     select t.Item1);
            if (p.HasMinimumCount(1))
                return p.Max();
            return 0;
        }

        /// <summary>
        /// The LineEnding that will be used for the output file after running <see cref="ConvertFile"/> 
        /// </summary>
        public string LineEnding => _LineEnd;
        string _LineEnd = "\r\n";
        bool _CR = true;
        bool _LF = true;
        LikeExpressions LIKE = new LikeExpressions();
        /// <summary>
        /// Include carriage return in the output lines.
        /// </summary>
        public bool LineEnding_CR
        {
            get { return _CR; }
            set
            {
                _CR = value;
                _LineEnd = (_CR ? "\r" : "") + (_LF ? "\n" : "");
            }
        }
        /// <summary>
        /// Include LF in the output lines.
        /// </summary>
        public bool LineEnding_LF
        {
            get { return _LF; }
            set
            {
                _LF = value;
                _LineEnd = (_CR ? "\r" : "") + (_LF ? "\n" : "");
            }
        }
        string inputFile;
        string outputFile;
        /// <summary>
        /// Full path to Output File
        /// </summary>
        public string OutputFilePath => outputFile;
        string tempOut { get { return outputFile + ".work_temp"; } }
        Dictionary<string, string> DerivedCols = new Dictionary<string, string>();
        /// <summary>
        /// Regex based Derived columns.
        /// </summary>
        Dictionary<string, DerivedColumnInfo> Derived;// = new Dictionary<string, DerivedColumnInfo>(); //key = regex. If pass, call derived column info's update with string.
        
        /// <summary>
        /// Loop through derived columns for UI purposes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DerivedColumnInfo> CheckDerivedColumns()
        {
            foreach (var k in Derived.Keys)
                yield return Derived[k];
        }
        /// <summary>
        /// Check line to see if it can update any of the derived column values
        /// </summary>
        /// <param name="line"></param>
        public void Derive(string line)
        {
            LIKE.AllowRegex = true;            
            foreach (var p in Derived)
            {
                if (LIKE.Compare(line, p.Value.expression))
                {
                    Derived[p.Key].Set(line);
                }
            }

        }
        /// <summary>
        /// Add a derived column info to the dictionary for Comparing
        /// </summary>
        /// <param name="dci"></param>
        public void InsertDerived(DerivedColumnInfo dci){
            Derived.Add(dci.columnName, dci);
        }
        /// <summary>
        /// Adds a set of derived column info to the dictionary for comparing.
        /// </summary>
        /// <param name="dci"></param>
        public void InsertDerived(params DerivedColumnInfo[] dci)
        {
            foreach (var col in dci)
                InsertDerived(col);
        }
        string _Extension = ".dat";
        /// <summary>
        /// Delimiter for output file.
        /// </summary>
        public char Delimiter => _DELIM;
        char _DELIM = '|';
        /// <summary>
        /// Sets the delimiter after validating that it is a covered value 
        /// </summary>
        /// <param name="outDelimiter"></param>
        public void SetDelimiter(char outDelimiter)
        {
            DELIMITER d;
            if (outDelimiter == ',')
                d = DELIMITER.COMMA;
            else if (outDelimiter == '\t')
                d = DELIMITER.TAB;
            else if(outDelimiter == '|')
                d = DELIMITER.PIPE;
            else
                throw new Exception("Invalid Delimiter setting");
            SetDelimiter(d);
        }
        /// <summary>
        /// Sets the output delimiter for
        /// </summary>
        /// <param name="outDelimiter"></param>
        public void SetDelimiter(DELIMITER outDelimiter)
        {
            switch (outDelimiter)
            {
                case DELIMITER.PIPE:
                    _DELIM = '|';
                    _Extension = ".dat";
                    break;
                case DELIMITER.COMMA:
                    _DELIM = ',';
                    _Extension = ".csv";
                    break;
                case DELIMITER.TAB:
                    _DELIM = '\t';
                    _Extension = ".tab";
                    break;
                default:
                    _Extension = ".dat";
                    break;
            }
            if (!explicitOutput)
                outputFile = inputFile + _Extension;
        }
        /// <summary>
        /// Add derived columns and their values to the line. Column name or value is chosen based on header
        /// </summary>
        /// <param name="line"></param>
        /// <param name="header">If true, add column name. If false, add derived column value</param>
        /// <returns></returns>
        public string AddDerived(string line, bool header)
        {
            DerivedColumnInfo[] dci = Derived.Values.OrderBy(i => i.columnName).ToArray();
            foreach (var d in dci)
            {
                if (header)
                {
                    line = line + _DELIM + d.columnName;
                }
                else
                    line = line + _DELIM + d.value;
            }
            return line;
        }
        Doc.FileReader qr;
        /// <summary>
        /// Contains filters for removing lines from the result file if true.
        /// </summary>
        public List<string> filterOut { get; private set; } = new List<string>();
        /// <summary>
        /// Contains filters for adding lines to the result file if true.
        /// </summary>
        public List<string> filterIn { get; private set; } = new List<string>();
        /// <summary>
        /// Contains length of each field.
        /// </summary>
        public List<int> fieldWidths { get; private set; } = new List<int>();
        /// <summary>
        /// Constructor. Takes an input file and desired output file.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="outputPath"></param>
        public FixWidthConverter(string inputPath, string outputPath)
        {
            inputFile = inputPath;
            outputFile = outputPath;
            explicitOutput = true;
            if (!File.Exists(inputFile))
                throw new Exception("Input File doesn't exist.");
            if (inputFile == outputFile)
                throw new Exception("Output File cannot be the same as input file.");
            Derived = new Dictionary<string, DerivedColumnInfo>();
        }
        bool explicitOutput = false;
        /// <summary>
        /// Constructor. Output file will be input file path + an extension dependent on the delimiter.
        /// </summary>
        /// <param name="inputPath"></param>
        public FixWidthConverter(string inputPath)
        {
            inputFile = inputPath;
            outputFile = inputPath + _Extension;
            if (!File.Exists(inputFile))
                throw new Exception("Input File doesn't exist.");

            Derived = new Dictionary<string, DerivedColumnInfo>();
        }
        /// <summary>
        /// Indicates whether or not this instance can be used to create an output file.
        /// </summary>
        public bool CanCreateOutput { get; private set; } = true;
        /// <summary>
        /// Version just for creating settings
        /// </summary>
        public FixWidthConverter()
        {
            CanCreateOutput = false;
            Derived = new Dictionary<string, DerivedColumnInfo>();
        }        
        /// <summary>
        /// Returns a fixedWidthConverter setup based on the setupfile. output will be input + .csv
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="SetUpFile"></param>
        public static FixWidthConverter construct(string inputFile, string SetUpFile)
        {
            FixWidthConverter fwc;
            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                fwc = new FixWidthConverter();
            else
                fwc = new FixWidthConverter(inputFile);
            fwc.Derived = new Dictionary<string, DerivedColumnInfo>();
            fwc.hasHeader = false;
            fwc.HeaderDifferentIndexes = true;

            string text = File.ReadAllText(SetUpFile);
            string[] sections = text.Split(SETTINGS_MAJOR_DELIM);
            int i = 0;
            //Header replacement.
            if (sections[i] != null && sections[i] != "")
                fwc.NewHeader = sections[0];
            i++;
            //Length of each field, pipe indexes
            if (sections[i] != null && sections[i] != "")
            {
                string[] indexes = sections[1].Split(SETTINGS_MINOR_DELIM);
                foreach (string s in indexes)
                {
                    fwc.fieldWidths.Add(Convert.ToInt32(s));
                }
            }
            i++;
            //Filter- Lines to keep
            if(sections[i]!= null && sections[i] != "")
                fwc.filterIn = new List<string>(sections[2].Split((char)12));
            i++;
            //Filter- lines to ignore
            if (sections[i] != null && sections[i] != "")
                fwc.filterOut = new List<string>(sections[3].Split((char)12));
            i++;
            //Derived columns- Section/subsection info from file
            if (i < sections.Length && sections[i] != null && sections[i] != "")
            {
                string[] tostrings = sections[i].Split(SETTINGS_MINOR_DELIM);
                foreach (string s in tostrings)
                {
                    DerivedColumnInfo di = new DerivedColumnInfo(s);
                    fwc.Derived.Add(di.columnName, di);
                }
            }
            i++;
            if(i < sections.Length && !string.IsNullOrEmpty(sections[i]))
            {
                fwc.SetDelimiter(sections[i][0]);
            }
            i++;
            if(i < sections.Length && !string.IsNullOrWhiteSpace(sections[i]))
            {
                string[] flags = sections[i].Split(SETTINGS_MINOR_DELIM);
                fwc.LineEnding_CR = flags[0][0] == '1';
                fwc.LineEnding_LF = flags[1][0] == '1';
            }
            i++;
            if(i < sections.Length && !string.IsNullOrWhiteSpace(sections[i]))
            {
                string[] tups = sections[i].Split(SETTINGS_MINOR_DELIM);
                foreach(var tuple in tups)
                {
                    var t = tuple.Split(SETTINGS_MINOR_SUBDELIM);
                    int? end = null;
                    int temp;
                    if (int.TryParse(t[3], out temp))
                        end = temp;
                    Tuple<int, string, int, int?> tvalue = new Tuple<int, string, int, int?>(int.Parse(t[0]), t[1], int.Parse(t[2]), end);
                    fwc.AnchorModDerivePulls.Add(tvalue);
                }
            }
            return fwc;
        }
        const char SETTINGS_MAJOR_DELIM = '\0';
        const char SETTINGS_MINOR_DELIM = (char)12;
        const char SETTINGS_MINOR_SUBDELIM = (char)14;
        /// <summary>
        /// ToString method. Can be written to a file for use by the static construct method in order to use the same settings on another file.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(this.NewHeader + SETTINGS_MAJOR_DELIM);
            for(int i = 0; i < fieldWidths.Count; i++)
            {
                result.Append(fieldWidths[i]);
                if(i < fieldWidths.Count -1)
                    result.Append(SETTINGS_MINOR_DELIM);
            }            
            result .Append( SETTINGS_MAJOR_DELIM);
            for(int i= 0; i < filterIn.Count; i++)
            {
                result.Append(filterIn[i]);
                if(i < filterIn.Count - 1)
                    result.Append(SETTINGS_MINOR_DELIM);
            }
            result.Append(SETTINGS_MAJOR_DELIM);
            for(int i= 0; i < filterOut.Count; i++)
            //foreach (var fo in filterOut)
            {
                result.Append(filterOut[i]);
                if(i < filterOut.Count - 1)
                    result.Append(SETTINGS_MINOR_DELIM);
                //result = result + fo + SETTINGS_MINOR_DELIM;
            }
            result.Append(SETTINGS_MAJOR_DELIM);
            var dv = Derived.Values.ToList();
            for(int i = 0; i < dv.Count; i++)            
            {
                result.Append(dv[i].FileString());
                if(i < dv.Count - 1)
                    result.Append(SETTINGS_MINOR_DELIM);                
            }
            result.Append( SETTINGS_MAJOR_DELIM);
            result.Append(_DELIM);
            result.Append(SETTINGS_MAJOR_DELIM);
            result.Append(LineEnding_CR ? '1' : '0');
            result.Append(SETTINGS_MINOR_DELIM);
            result.Append(LineEnding_LF ? '1' : '0');
            result.Append(SETTINGS_MAJOR_DELIM);
            for(int i = 0; i < AnchorModDerivePulls.Count; i++)
            {
                var tup = AnchorModDerivePulls[i];
                result.Append(tup.Item1);
                result.Append(SETTINGS_MINOR_SUBDELIM);
                result.Append(tup.Item2);
                result.Append(SETTINGS_MINOR_SUBDELIM);
                result.Append(tup.Item3);
                result.Append(SETTINGS_MINOR_SUBDELIM);
                result.Append(tup.Item4 ?? -1);
                if(i < AnchorModDerivePulls.Count - 1)
                    result.Append(SETTINGS_MINOR_DELIM); //don't add the minor delim for the end.
            }
            result.Append(SETTINGS_MAJOR_DELIM);
            return result.ToString();
        }
        /// <summary>
        /// Set to true if the file already contains a header.
        /// </summary>
        public bool hasHeader = true;
        bool customHeader { get { return NewHeader.Trim() != ""; } }
        /// <summary>
        /// String to Replacement for existing header. should be pipe delimited.
        /// </summary>
        public string NewHeader= "";
        /// <summary>
        /// Create the new delimited file using fieldwidths, filters, and header settings.
        /// </summary>
        public void ConvertFile()
        {
            if (!CanCreateOutput)
                throw new InvalidOperationException("Instance cannot be used to create an output file.");
            if (File.Exists(outputFile))
            {
                File.Move(outputFile, outputFile + System.DateTime.Now.ToString("MMddyyyyhhmmss"));
            }
            if(File.Exists(tempOut)){
                File.Move(tempOut, tempOut + System.DateTime.Now.ToString("MMddyyyyhhmmss"));
            }
            if(!Directory.Exists(Path.GetDirectoryName(outputFile))){
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            }
            if (!LineEnding_CR && !LineEnding_LF)
                throw new Exception("Invalid line ending settings for output - CR and LF are both false.");
            PreAnchorBuffer.Clear();
            var bufferLength = CheckBufferQueueLength();
            var postAnchor = CheckPostBufferWait();
            AnchorModDerivePulls.Sort((t, t2) =>
            {
                int c = t.Item1.CompareTo(t2.Item1);
                if (c == 0)
                {
                    c = t.Item3.CompareTo(t2.Item3); //Same line, tie break by start position.
                    if(c == 0)
                    {
                        if (t.Item4 == null && t2.Item4 != null)
                            return 1;
                        if (t.Item4 == null)
                            return t.Item2.CompareTo(t2.Item2);
                        if (t2.Item4 == null)
                            return -1;
                        c = t.Item4.Value.CompareTo(t2.Item4.Value);
                    }
                }                
                return c;
            });
            using(var sr = new StreamReader(inputFile))
            //using (qr = new FileReader(inputFile))
            {
                bool hasWork;
                string[] lines;
                //lines = qr.Read(out work);
                lines = GetWorkSet(sr, out hasWork);


                if (customHeader)
                {
                    if (hasHeader)
                    {
                        lines[0] = NewHeader;
                    }
                    else
                    {
                        string temp = string.Join("\n", lines);
                        temp = NewHeader + "\n" + temp;
                        lines = temp.Split('\n');
                        hasHeader = true;
                    }
                }
                if (hasHeader && !HeaderDifferentIndexes)
                {
                    lines[0] = ConvertIndexes(lines[0]);
                }
                FilterWrite(lines, hasHeader, bufferLength, postAnchor);
                while (hasWork)
                {
                    //lines = qr.Read(out work);
                    lines = GetWorkSet(sr, out hasWork);
                    FilterWrite(lines, false, bufferLength, postAnchor);
                }
            }
            File.Move(tempOut, outputFile);
        }

        private string[] GetWorkSet(StreamReader input, out bool moreWork)
        {
            moreWork = true;
            List<string> lines = new List<string>();
            const int LINE_COUNT = 5000;
            for (int i = 0; i < LINE_COUNT; i++)
            {
                string temp = input.ReadLine();
                if (temp == null)
                {
                    moreWork = false;
                    return lines.ToArray();
                }
                lines.Add(temp.Replace(((char)12).ToString(), ""));
            }

            return lines.ToArray();
        }
        /// <summary>
        /// Convert a fixed width line to pipe delimited. Public for use on the header if needed
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string ConvertIndexes(string line) { return ConvertIndexes(line, fieldWidths); }
        /// <summary>
        /// Set to true if the header's indexes do not match the normal records' indexes. May be useful if you want to use a custom header...
        /// <para>If true, you should call on ConvertIndexes on the header line yourself.</para>
        /// </summary>
        public bool HeaderDifferentIndexes = false;

        /// <summary>
        /// Convert to spaces for the conversion filter
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string makeSpaces(string input)
        {
            return @"\s{" + input.Length + "}?";
        }
        /// <summary>
        /// Convert to digit expression for conversion filter
        /// </summary>
        /// <param name="input"></param>
        /// <param name="includeSpace"></param>
        /// <returns></returns>
        public static string makeDigits(string input, bool includeSpace)
        {
            return (includeSpace ? @"[\s" : "[") + "0-9]{" + input.Length + "}?";
        }
        /// <summary>
        /// convert to letters for conversion filters
        /// </summary>
        /// <param name="input"></param>
        /// <param name="includeSpace"></param>
        /// <param name="includeNumeric"></param>
        /// <returns></returns>
        public static string makeLetters(string input, bool includeSpace, bool includeNumeric = false)
        {
            return (includeSpace ? @"[\s" : "[") 
                    + (includeNumeric? "0-9": string.Empty) 
                    + "a-zA-Z]{" + input.Length + "}?";
        }
        /// <summary>
        /// convert to anything for conversion filters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string makeAnything(string input)
        {
            return ".{" + input.Length + "}?";
        }
        
        /// <summary>
        /// Convert a line to pipe delimited. Public for use on header in case its indexes don't match for some reason...
        /// </summary>
        /// <param name="line"></param>
        /// <param name="FieldLength"></param>
        /// <returns></returns>
        public string ConvertIndexes(string line, List<int> FieldLength)
        {
            int t = FieldLength.Count > 0 ? FieldLength[0] : line.Length;
            string temp = line.Substring(0, t).Trim();
            line = line.Substring(t);
            for(int i = 1; i < FieldLength.Count; i++)
            {
                
                try{
                    t= FieldLength[i];
                    temp = temp + _DELIM + line.Substring(0, t).Trim();
                    line = line.Substring(t);
                }
                catch{
                    temp = temp + _DELIM + line.Trim();
                    line = string.Empty;
                }
            }
            if(line != "")
                temp = temp + _DELIM + line;
            return AddDerived(temp, false);
        }
        Queue<string> PreAnchorBuffer = new Queue<string>();
        string[] PostAnchor = null;
        string work = null;
        int postAnchorCounter = 0;
        private void AddPreAnchors(ref string tempWork)
        {
            var buffer = PreAnchorBuffer.ToArray();
            foreach (var mod in AnchorModDerivePulls)
            {
                if (mod.Item1 > 0)
                    return;
                string l = buffer[buffer.Length - mod.Item1]; //If Item1 = 1, then previous Line = Last record of Buffer.
                if(mod.Item3 > l.Length)
                {
                    tempWork += Delimiter;                    
                }
                else if (mod.Item4.HasValue && l.Length > mod.Item3 + mod.Item4.Value)
                {
                    tempWork += Delimiter + l.Substring(mod.Item3, mod.Item4.Value);
                }
                else
                {
                    tempWork += Delimiter + l.Substring(mod.Item3);
                }
            }
        }
        private void FilterWrite(string[] data, bool includesHeader, int PreAnchorBufferLength, int PostAnchorWait)
        {
            
            using (StreamWriter sw = new StreamWriter(tempOut, true))
            {
                LIKE.AllowRegex = true;
                for (int i = 0; i < data.Length; i++)
                {
                    if (i == 0 && includesHeader)
                    {
                        if (data[i].Trim() != "")
                        {
                            var header = AddDerived(data[i], true);
                            foreach(var t in AnchorModDerivePulls)
                            {
                                header += Delimiter + t.Item2;
                            }
                            sw.Write(header + _LineEnd);
                        }
                        continue;
                    }                    
                    Derive(data[i]);//attempt to derive info from the line
                    bool write = true;
                    if (work == null)
                    {
                        foreach (string o_filter in filterOut)
                        {
                            if (o_filter != string.Empty && LIKE.Compare(data[i], o_filter))
                            {
                                write = false;
                                break;
                            }
                        }
                        foreach (string i_filter in filterIn)
                        {
                            if (i_filter != string.Empty && !LIKE.Compare(data[i], i_filter))
                            {
                                write = false;
                                break;
                            }
                        }
                        if (write)
                        {
                            postAnchorCounter = 0;
                            string output = ConvertIndexes(data[i]);
                            if (PostAnchorWait > 0)
                            {
                                postAnchorCounter = PostAnchorWait;
                                work = output;
                                var preAnchor = PreAnchorBuffer.ToList();
                                foreach(var mod in AnchorModDerivePulls.Where(a => a.Item1 < 0))
                                {
                                    string line = preAnchor[preAnchor.Count + mod.Item1]; //E.g., count = 2, if we're looking for -2 lines then 2 + -2  = 0. Offset will not be 0 here.
                                    if (mod.Item3 > line.Length)
                                        work += Delimiter;
                                    else if(mod.Item4.HasValue && (mod.Item3 + mod.Item4.Value) < line.Length)
                                    {
                                        work += Delimiter + line.Substring(mod.Item3, mod.Item4.Value);
                                    }
                                    else
                                    {
                                        work += Delimiter + line.Substring(mod.Item3);
                                    }
                                }
                            }
                            else
                            {
                                sw.Write(output + _LineEnd);
                                work = null;
                            }
                        }
                    }
                    else
                    {
                        postAnchorCounter--;
                        if (postAnchorCounter < 0)
                        {
                            sw.Write(work + _LineEnd);
                            work = null;
                        }
                        else
                        {
                            int anchorLine = PostAnchorWait - postAnchorCounter; //X lines after we started post anchor logic.
                            foreach (var mod in AnchorModDerivePulls)
                            {
                                if (mod.Item1 < anchorLine)
                                    continue;
                                else if (mod.Item1 > anchorLine)
                                    break;
                                string line = data[i];
                                if (mod.Item3 > line.Length)
                                    work += Delimiter;
                                else if (mod.Item4.HasValue && (mod.Item3 + mod.Item4.Value) < line.Length)
                                {
                                    work += Delimiter + line.Substring(mod.Item3, mod.Item4.Value);
                                }
                                else
                                {
                                    work += Delimiter + line.Substring(mod.Item3);
                                }
                            }
                        } //PostAnchor Counter >= 0     (has something in the mod to write.                                     
                    }

                    if (PreAnchorBufferLength > 0)
                    {
                        PreAnchorBuffer.Enqueue(data[i]);
                        if (PreAnchorBuffer.Count > PreAnchorBufferLength)
                            PreAnchorBuffer.Dequeue();
                    }
                }
                sw.Close();
            }
        }
        /// <summary>
        /// Filter records using the regex conditions. returns an array of included records.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hasHeader">If true, will not check the first record for conditions because it's a header.</param>
        /// <returns></returns>
        public string[] FilterRecords(string[] data, bool hasHeader)
        {
            List<string> temp = new List<string>();
            LIKE.AllowRegex = true;
            for(int i = 0; i < data.Length; i++){
                if (hasHeader && i == 0)
                {
                    temp.Add(AddDerived(data[i], true));
                    continue;
                }
                Derive(data[i]);//attempt to derive info from the line
                bool write = true;
                foreach (string o_filter in filterOut)
                {
                    if (LIKE.Compare(data[i], o_filter))
                    {
                        write = false;
                        break;
                    }
                }
                if (!write)
                    continue;
                foreach (string i_filter in filterIn)
                {
                    if (!LIKE.Compare(data[i], i_filter))
                    {
                        write = false;
                        break;
                    }
                }                
                if (write)
                {
                    temp.Add(data[i]);
                }
            }
            return temp.ToArray();
        }
    }
    /// <summary>
    /// For adding extra columns to a file
    /// </summary>
    public class DerivedColumnInfo
    {
        /// <summary>
        /// Regex for checking against each line.
        /// </summary>
        public string expression;
        private const char delim = (char) 1;
        /// <summary>
        /// Name to go in the output file
        /// </summary>
        public string columnName;
        int start;
        int length;
        bool endLine { get { return length == 0; } }
        /// <summary>
        /// Max Length of a line that passes. Or just the start position for a derived column that goes to the end of the line
        /// </summary>
        public int maxLength { get { return start + length; } }
        /// <summary>
        /// Value to place in column
        /// </summary>
        public string value;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="express"></param>
        /// <param name="name"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public DerivedColumnInfo(string express, string name, int start, int length)
        {
            expression = express;
            columnName = name;
            this.start = start;
            this.length = length;
            value = "";
        }
        /// <summary>
        /// Update value based on passed line and the start/length given when this was created
        /// </summary>
        /// <param name="line"></param>
        public void Set(string line)
        {
            if (endLine || line.Length < start + length) //variable length under max
                value = line.Substring(start).Trim();
            else
                value = line.Substring(start, length).Trim();
        }
        /// <summary>
        /// ToString for saving to file
        /// </summary>
        /// <returns></returns>
        public string FileString()
        {
            return columnName + delim + start + delim + length + delim + expression;
        }
        /// <summary>
        /// To String override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return columnName;
        }
        /// <summary>
        /// For constructing form file
        /// </summary>
        /// <param name="ToStringValue"></param>
        public DerivedColumnInfo(string ToStringValue)
        {
            string[] values = ToStringValue.Split(delim);
            columnName = values[0];
            start = Convert.ToInt32(values[1]);
            length = Convert.ToInt32(values[2]);
            if (values.Length > 4)
            {
                string s = string.Join("" + delim, values, 3, values.Length - 4);
                expression = s;
            }
            else
            {
                expression = values[3];
            }

        }
    }
}
