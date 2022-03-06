using SEIDR.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.METRIX_EXPORT.SkipTracing
{
    public class SkipTracingUtil
    {
        private readonly string _connectionString;
        public SkipTracingUtil(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Bulk load a SQL Server table with data from another source.
        /// </summary>
        /// <param name="dt">The source data table.</param>
        /// <param name="exportBatchID">The export batch ID.</param>
        /// <returns></returns>
        public bool SkipTraceResponseBulkCopy(DataTable dt, int exportBatchID)
        {
            bool success = true;

            if (dt.Rows.Count == 0)
                return success;
            
            using (SqlBulkCopy bulkCopier = new SqlBulkCopy(_connectionString, SqlBulkCopyOptions.FireTriggers))
            {

                bulkCopier.DestinationTableName = "EXPORT.SkipTraceResponse";

                // Add extra columns to the source table
                dt.Columns.Add("ExportBatchID", typeof(int));
                dt.AcceptChanges();

                foreach (DataRow row in dt.Rows)
                {
                    row["ExportBatchID"] = exportBatchID;
                }

                // Set the value to DB null if it is null or empty.
                SetDBNull(dt);

                bulkCopier.ColumnMappings.Add("Account", "AccountID");
                bulkCopier.ColumnMappings.Add("dup_flag", "dup_flag");
                bulkCopier.ColumnMappings.Add("prim_range", "prim_range");
                bulkCopier.ColumnMappings.Add("predir", "predir");
                bulkCopier.ColumnMappings.Add("prim_name", "prim_name");
                bulkCopier.ColumnMappings.Add("suffix", "suffix");
                bulkCopier.ColumnMappings.Add("ExportBatchID", "ExportBatchID");

                bulkCopier.ColumnMappings.Add("postdir", "postdir");
                bulkCopier.ColumnMappings.Add("unit_desig", "unit_desig");
                bulkCopier.ColumnMappings.Add("sec_range", "sec_range");
                bulkCopier.ColumnMappings.Add("z5", "z5");
                bulkCopier.ColumnMappings.Add("zip4", "zip4");
                bulkCopier.ColumnMappings.Add("p_city_name", "GuarantorCity");
                bulkCopier.ColumnMappings.Add("st", "GuarantorState");
                bulkCopier.ColumnMappings.Add("subj_phone_1", "GuarantorPhoneNumber1");
                bulkCopier.ColumnMappings.Add("subj_phone_2", "GuarantorPhoneNumber2");
                bulkCopier.ColumnMappings.Add("subj_phone_3", "GuarantorPhoneNumber3");

                try
                {
                    // Bulk load the data table to the EXPORT.SkipTracingResponse
                    bulkCopier.WriteToServer(dt);

                }
                catch (Exception ex)
                {
                    success = false;
                    throw;
                }
            }
            return success;
        }

        /// <summary>
		/// Set the value to DB null if it is null or empty.
		/// </summary>
		/// <param name="dt">The dt.</param>
		private void SetDBNull(DataTable dt)
        {
            // Default any empty cells to null value
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn c in dt.Columns)
                {
                    string s = row[c].ToString();

                    if (string.IsNullOrEmpty(s))
                    {
                        row[c] = DBNull.Value;
                    }
                }
            }
        }

        
        public bool ExportSkipTracingDataToCSVFile(string filename, DataSet pendingDs)
        {
            if (pendingDs.Tables.Count == 0)
                return false;

            var builder = new StringBuilder();
            builder.Append("Account,Facility ID,First Name,Middle Name,Last Name,Full Name,Address1,Address2,City,State,Zip,SSN,Phone,DOB");
            builder.AppendLine();

            var dtPatient = pendingDs.Tables[0];
            foreach (DataRow row in dtPatient.Rows)
            {
                string AccountID = string.Format("\"{0}\"", row["AccountID"].ToString());
                string FacilityID = string.Format("\"{0}\"", row["FacilityID"].ToString());
                string GuarantorFirstName = string.Format("\"{0}\"", row["GuarantorFirstName"].ToString().Replace("\"", "\"\""));
                string GuarantorMI = string.Format("\"{0}\"", row["GuarantorMI"].ToString().Replace("\"", "\"\""));
                string GuarantorLastName = string.Format("\"{0}\"", row["GuarantorLastName"].ToString().Replace("\"", "\"\""));
                string GuarantorFullName = string.Format("\"{0}\"", row["GuarantorFullName"].ToString().Replace("\"", "\"\""));
                string GuarantorAddress1 = string.Format("\"{0}\"", row["GuarantorAddress1"].ToString().Replace("\"", "\"\""));
                string GuarantorAddress2 = string.Format("\"{0}\"", row["GuarantorAddress2"].ToString().Replace("\"", "\"\""));
                string GuarantorCity = string.Format("\"{0}\"", row["GuarantorCity"].ToString().Replace("\"", "\"\""));
                string GuarantorState = string.Format("\"{0}\"", row["GuarantorState"].ToString().Replace("\"", "\"\""));
                string GuarantorZip = string.Format("\"{0}\"", row["GuarantorZip"].ToString().Replace("\"", "\"\""));
                string GuarantorSSN = string.Format("\"{0}\"", row["GuarantorSSN"].ToString().Replace("\"", "\"\""));
                string GuarantorPhoneNumber = string.Format("\"{0}\"", row["GuarantorPhoneNumber"].ToString().Replace("\"", "\"\""));
                string GuarantorBirthDate = string.Format("\"{0}\"", string.IsNullOrEmpty(row["GuarantorBirthDate"].ToString()) ? row["GuarantorBirthDate"].ToString() : ((DateTime)row["GuarantorBirthDate"]).ToString("MM/dd/yyyy"));

                string str = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                    AccountID,
                    FacilityID,
                    GuarantorFirstName,
                    GuarantorMI,
                    GuarantorLastName,
                    GuarantorFullName,
                    GuarantorAddress1,
                    GuarantorAddress2,
                    GuarantorCity,
                    GuarantorState,
                    GuarantorZip,
                    GuarantorSSN,
                    GuarantorPhoneNumber,
                    GuarantorBirthDate);

                builder.Append(str);

                // Next line
                builder.AppendLine();
            }

            File.WriteAllText(filename, builder.ToString());
            return true;
        }
    }
}
