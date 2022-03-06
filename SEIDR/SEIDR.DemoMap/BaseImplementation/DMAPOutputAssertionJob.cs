using System;
using System.Linq;
using SEIDR.Doc;
using SEIDR.JobBase;
using SEIDR.JobBase.HelperClasses;
using System.Globalization;

namespace SEIDR.DemoMap.BaseImplementation
{
    [IJobMetaData(nameof(DMAPOutputAssertionJob), nameof(DemoMap), 
        "Test input file against expected output.", 
        NeedsFilePath:true, AllowRetry:false,
        ConfigurationTable: "SEIDR.FileAssertionTestJob")]
    public class DMAPOutputAssertionJob : ContextJobBase<MappingContext>
    {
        const string ACCOUNT_COL = "AccountNumber";
        const string KEY_COL = "FacilityKey";
        const string SEQ_COL = "SequenceNumber";



        public override void Process(MappingContext context)
        {
            var config = context.Manager.SelectSingle<FileAssertionTestConfiguration>(new {context.JobProfile_JobID});
            int lc = -1;
            bool finishedActual = false;
            try
            {
                DoTest(config, context, ref lc, out finishedActual);
            }
            catch (Exception ex)
            {
                string errFile = finishedActual ? "Expected" : "Actual";
                context.LogError("Uncaught exception at " + errFile + " file line # " + lc, ex);
                context.SetStatus(false);
            }
        }
        
        public void DoTest(FileAssertionTestConfiguration config, MappingContext context, ref int lineCounter, out bool finishedActual)
        {
            finishedActual = false;
            string expectedPath = FileSystem.FS.ApplyDateMask(config.ExpectedOutputFile, context);


            var actual = context.GetExecutionLocalFile();
            var expected = context.GetLocalFile(expectedPath);
            var left = actual.GetDocMetaData();
            if (left == null)
            {
                context.SetStatus(ResultCode.NA);
                context.LogError("Could not find file '" + context.FilePath + "'");
                return;
            }

            var right = expected.GetDocMetaData();
            if (right == null)
            {
                context.SetStatus(ResultCode.NE);
                context.LogError("Could not find file '" + expectedPath + "'");
                return;
            }

            string[] skipColumns = config.SkipColumns?.Split(';', ',') ?? new string[0];
            

            using (var ar = new DocReader(actual.GetDocMetaData().SetPageSize(DocMetaData.DEFAULT_PAGE_SIZE * 4)))
            using (var ex = new DocReader(expected.GetDocMetaData().SetPageSize(DocMetaData.DEFAULT_PAGE_SIZE * 4)))
            {
                if (config.CheckColumnNameMatch)
                {
                    foreach (var col in ex.Columns)
                    {
                        if (ar.Columns.NotExists(a => a.ColumnName == col.ColumnName))
                        {
                            context.LogError($"Expected column name '{col.ColumnName}' - not found in Actual file.");
                            context.SetStatus(ResultCode.MC);
                        }
                    }

                    foreach (var col in ar.Columns)
                    {
                        if (ex.Columns.NotExists(e => e.ColumnName == col.ColumnName))
                        {
                            context.LogError($"Actual column name '{col.ColumnName}' - not found in the Expected file.");
                            context.SetStatus(ResultCode.MC);
                        }
                    }
                    if (context.CurrentStatus == ResultCode.MC)
                        return;

                    /*
                    var q = (from eCol in ex.Columns
                            select eCol.ColumnName).ToList();
                    var q2 = (from aCol in ar.Columns
                             select aCol.ColumnName).ToList();
                    if (q.Except(q2).HasMinimumCount(1)
                        || q2.Except(q).HasMinimumCount(1))
                    {
                        context.LogError("One of the files has columns that are not in the other (By Name)");
                        context.SetStatus(ResultCode.MC);
                        return;
                    }
                    */
                    if (config.CheckColumnOrderMatch)
                    {
                        foreach (var col in ex.Columns)
                        {
                            var aCol = ar.Columns[col.Position].ColumnName;
                            if (aCol != col.ColumnName)
                            {
                                context.LogError($"Expected {col.ColumnName} at position {col.Position} - found {aCol} instead.");
                                context.SetStatus(ResultCode.CO);
                            }
                        }
                        if (context.CurrentStatus == ResultCode.CO)
                            return;
                    }
                }

                lineCounter = 0;

                bool checkSequenceNumber = context.Branch.Equals("APB", StringComparison.OrdinalIgnoreCase);

                string[] BOOL_VALUES = new[]
                {
                    "TRUE", "FALSE"
                };
                int counter = 0;
                const int MAX_COUNTER = 50;
                CultureInfo decParse = new CultureInfo("EN-US");
                foreach (var record in ar)
                {
                    lineCounter++;
                    string accountNumber = record[ACCOUNT_COL].Trim();
                    string facilityKey = record[KEY_COL].Trim();
                    DocRecord compare;
                    if (checkSequenceNumber)
                    {
                        string sequence = record[SEQ_COL].Trim();
                        compare = ex.FirstOrDefault(r => MatchTest(accountNumber, facilityKey, sequence, r));
                    }
                    else
                        compare = ex.FirstOrDefault(r => MatchTest(accountNumber, facilityKey, r));

                    if (compare == null)
                    {
                        context.LogError("Actual File, Line Number " + lineCounter + " - No Match found in expected output file.");
                        context.SetStatus(ResultCode.DD);

                        if (++counter >= MAX_COUNTER)
                        {
                            context.LogError("MAX ERROR LOG COUNTER REACHED.");
                            return;
                        }
                        continue;
                    }

                    bool[] hasBucket = new bool[8];
                    for (int i = 0; i < hasBucket.Length; i++)
                    {
                        string prefix = "Ins" + (i + 1) + "_";
                        //If column doesn't exist, leave hasBucket as false.
                        if (!ar.Columns.HasColumn(null, prefix + "PayerCode"))
                            continue;
                        if (!string.IsNullOrEmpty(record[prefix + "PayerCode"]))
                            hasBucket[i] = true;
                        else if (!string.IsNullOrEmpty(record[prefix + "Balance"]))
                        {
                            decimal ret;
                            if (decimal.TryParse(record[prefix + "Balance"], out ret))
                            {
                                //If payerCode is null but there's a balance, only compare result when balance != 0.
                                hasBucket[i] = ret != 0;
                            }
                        }
                        else if (!string.IsNullOrEmpty(compare[prefix + "PayerCode"]))
                            hasBucket[i] = true;
                        else if (!string.IsNullOrEmpty(compare[prefix + "Balance"]))
                        {
                            decimal ret;
                            if (decimal.TryParse(compare[prefix + "Balance"], out ret))
                            {
                                //If payerCode is null but there's a balance, only compare result when balance != 0.
                                hasBucket[i] = ret != 0; 
                            }
                        }
                    }
                    foreach (var col in ar.Columns)
                    {
                        if (col.ColumnName.In(ACCOUNT_COL, KEY_COL))
                            continue;
                        if (checkSequenceNumber && col.ColumnName == SEQ_COL)
                            continue;
                        if (skipColumns.Contains(col.ColumnName))
                            continue;
                        if (!config.CheckColumnNameMatch && !ex.Columns.HasColumn(null, col.ColumnName))
                            continue;

                        if (col.ColumnName.Like("Ins[1-8]_%", false))
                        {
                            var seq = int.Parse(col.ColumnName[3].ToString()); //4th letter
                            if (!hasBucket[seq - 1])
                                continue; //Skip InsX if do not have bucket in either file.
                        }

                        const string ERROR_FORMAT = @"Actual File, Line Number {0} - Data mismatch for column '{1}'.
AccountNumber {2}, Facility {3} - Expected '{4}'; Found '{5}'.";
                        if (col.ColumnName[0] != '_' && col.ColumnName.ContainsAnySubstring(StringComparison.OrdinalIgnoreCase, "Payments", "Adjustments", "Charges", "Balance", "EstimatedAmountDue"))
                        {
                            decimal decActual, decExpected;
                            if (decimal.TryParse(record[col], NumberStyles.Currency, decParse, out decActual))
                            {
                                if (decimal.TryParse(compare[col.ColumnName], NumberStyles.Currency, decParse, out decExpected))
                                {
                                    if (decActual != decExpected)
                                    {
                                        context.LogError(string.Format(ERROR_FORMAT, lineCounter, col.ColumnName, accountNumber, facilityKey, decExpected, decActual));
                                        context.SetStatus(ResultCode.DD);
                                        counter++;
                                    }
                                }
                                else
                                {
                                    context.LogError(string.Format(ERROR_FORMAT, lineCounter, col.ColumnName, accountNumber, facilityKey, "(PARSE FAILURE)", decActual));
                                    context.SetStatus(ResultCode.DD);
                                    counter++;
                                }
                            }
                            else if (decimal.TryParse(compare[col], NumberStyles.Currency, decParse, out decExpected))
                            {
                                context.LogError(string.Format(ERROR_FORMAT, lineCounter, col.ColumnName, accountNumber, facilityKey, decExpected, "(PARSE FAILURE)"));
                                context.SetStatus(ResultCode.DD);
                                counter++;
                            }
                            continue;
                        }

                        string actualVal = record[col]?.Trim();
                        string expectedVal = compare[col.ColumnName]?.Trim(); //LoaderTransform will trim, so don't worry about missing trim calls here for now.
                        if (actualVal == expectedVal)
                            continue; //Regardless of case, or if both values are null.
                        
                        if (actualVal == null //If both were null, then would be caught by above == check.
                            || !actualVal.Equals(expectedVal, StringComparison.OrdinalIgnoreCase)) //Ignore case because it doesn't matter in SQL either.
                        {
                            if (string.IsNullOrEmpty(actualVal) && string.IsNullOrEmpty(expectedVal))
                                continue;
                            if (BOOL_VALUES.Any(b => b.Equals(expectedVal, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (actualVal == "0" && expectedVal.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
                                    continue;
                                if (actualVal == "1" && expectedVal.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                                    continue;
                            }

                            context.LogError("Actual File, Line Number " + lineCounter + " - Data mismatch for column '" + col.ColumnName + "'."
                                             + Environment.NewLine
                                             + $"AccountNumber {accountNumber}, Facility {facilityKey} - Expected '{expectedVal}'; found '{actualVal}'"
                                            );
                            context.SetStatus(ResultCode.DD);
                            counter++;
                        }
                    }

                    if (counter >= MAX_COUNTER) //Allow finishing all columns for a given line, but do not continue to another line once we reach the max counter for error logging.
                    {
                        context.LogError("MAX ERROR LOG COUNTER REACHED.");
                        return;
                    }
                }

                finishedActual = true;
                lineCounter = 0;
                foreach (var record in ex)
                {
                    lineCounter++;
                    string accountNumber = record[ACCOUNT_COL].Trim();
                    string facilityKey = record[KEY_COL].Trim();
                    if (checkSequenceNumber)
                    {
                        string sequence = record[SEQ_COL];
                        if (ar.NotExists(r => MatchTest(accountNumber, facilityKey, sequence, r)))
                        {
                            context.LogError("Expected file, line number " + lineCounter + " - No Match found in actual file");
                            context.SetStatus(ResultCode.DD);

                            if (++counter >= MAX_COUNTER)
                            {
                                context.LogError("MAX ERROR LOG COUNTER REACHED.");
                                return;
                            }
                        }
                        
                    }
                    else if (ar.NotExists(r => MatchTest(accountNumber, facilityKey, r)))
                    {
                        context.LogError("Expected file, line number " + lineCounter + " - No Match found in actual file");
                        context.SetStatus(ResultCode.DD);

                        if (++counter >= MAX_COUNTER)
                        {
                            context.LogError("MAX ERROR LOG COUNTER REACHED.");
                            return;
                        }
                    }
                }
            }

        }

        bool MatchTest(string AccountNumber, string facilityKey, string SequenceNumber, DocRecord right)
        {
            return right[ACCOUNT_COL].Trim().Equals(AccountNumber, StringComparison.OrdinalIgnoreCase)
                   && right[KEY_COL].Trim().Equals(facilityKey, StringComparison.OrdinalIgnoreCase)
                   && right[SEQ_COL].Trim().Equals(SequenceNumber);
        }

        bool MatchTest(string AccountNumber, string FacilityKey, DocRecord right)
        {
            return right[ACCOUNT_COL].Trim().Equals(AccountNumber, StringComparison.OrdinalIgnoreCase)
                   && right[KEY_COL].Trim().Equals(FacilityKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
