using SEIDR.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace SEIDR.METRIX_EXPORT.Statements
{
    public class PatientCoActivityFileUtil
    {
        private readonly string _connectionString;
        public PatientCoActivityFileUtil(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Bulk load a SQL Server table with data from another source.
        /// </summary>
        /// <param name="dt">The source data table.</param>
        /// <param name="exportBatchID">The export batch ID.</param>
        /// <returns></returns>
        public bool ActivityFileBulkCopy(DataTable dt, int exportBatchID)
        {
            bool success = true;

            if (dt.Rows.Count == 0)
                return success;
            
            using (SqlBulkCopy bulkCopier = new SqlBulkCopy(_connectionString, SqlBulkCopyOptions.FireTriggers))
            {

                bulkCopier.DestinationTableName = "IMPORT.PatientCoActivityFile";

                // Add extra columns to the source table
                dt.Columns.Add("ExportBatchID", typeof(int));
                dt.AcceptChanges();

                foreach (DataRow row in dt.Rows)
                {
                    row["ExportBatchID"] = exportBatchID;
                }

                SetDBNull(dt);

                bulkCopier.ColumnMappings.Add("Account Number", "AccountNumber");
                bulkCopier.ColumnMappings.Add("Account", "AccountNumber");
                

                // To Do ---> REmaining Column Mapping to be added

                try
                {
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

        
    }
}
