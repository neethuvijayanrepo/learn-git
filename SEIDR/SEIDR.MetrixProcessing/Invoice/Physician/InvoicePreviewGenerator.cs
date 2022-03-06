using SEIDR.DataBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.MetrixProcessing.Invoice.Physician 
{
    public class InvoicePreviewGenerator : IDisposable
    {
        /// <summary>
        /// Return value from the SQL when the Project has already invoiced for the Date.
        /// </summary>
        private const int ALREADY_INVOICED_ERROR_CODE = -20;
        public BlockingCollection<EncounterContainer> Encounters { get; private set; } = new BlockingCollection<EncounterContainer>();
        
        #region settings/constructor
        private readonly InvoicingContext _context;
        public int ProjectID { get; }
        public readonly Project_InvoiceSettings ProjectSettings;
        private static readonly string[] ColumnList;

        static InvoicePreviewGenerator()
        {
            //Properties should match target column for loading. - Include all public get properties that do not have an IgnoreBulkCopy attribute.
            ColumnList = typeof(InvoiceableTransaction_LineItem)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead && p.GetCustomAttribute(typeof(IgnoreBulkCopyAttribute), false) == null)
                    .Select(p => p.Name)
                    .ToArray();
        }
        public InvoicePreviewGenerator(InvoicingContext context)
        {
            _context = context;
            ProjectID = _context.ProjectID;
            ProjectSettings = _context.GetProjectSettings();
            _conn = _context.Metrix.GetConnection();
        }
        #endregion
        
            


        

        public int BulkInsertBatchSize { get; set; } = 5000;
        private DataTable _bulkCopyData;
        private static readonly object BulkSync = new object();
        private readonly object tabLock = new object();
        /// <summary>
        /// Populate the AMB.Transaction_LineItem_InvoicePreview_Staging table.
        /// </summary>
        public bool CheckBulkCopy(InvoiceableTransaction_LineItem tran = null)
        {
            lock (tabLock)
            {
                if (tran != null)
                {
                    
                    _bulkCopyData.AddRow(tran);
                    if (_bulkCopyData.Rows.Count < BulkInsertBatchSize)
                        return true;
                }

                //var rowCount = _bulkCopyData.Rows;
                lock (BulkSync) //synchronize the bulk insert to avoid locking issues
                {
                    try
                    {
                        _bulkCopier.WriteToServer(_bulkCopyData);
                    }
                    catch (SqlException ex)
                    {
                        //https://stackoverflow.com/questions/10442686/received-an-invalid-column-length-from-the-bcp-client-for-colid-6
                        if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                        {
                            string pattern = @"\d+";
                            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(ex.Message, pattern);
                            var index = Convert.ToInt32(match.Value) - 1;

                            FieldInfo fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings", BindingFlags.NonPublic | BindingFlags.Instance);
                            var sortedColumns = fi.GetValue(_bulkCopier);
                            var items = (object[]) sortedColumns.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sortedColumns);

                            FieldInfo itemdata = items[index].GetType().GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
                            var metadata = itemdata.GetValue(items[index]);

                            var column = metadata.GetType().GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);
                            var length = metadata.GetType().GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(metadata);

                            lock (_context.GetSyncObject())
                            {
                                _context.LogError($"Column '{column}' contains data with a length greater than {length}", ex);
                            }
                        }

                        lock (_context.GetSyncObject())
                        {
                            _context.LogError("Issue writing to " + _bulkCopier.DestinationTableName, ex);
                            _context.SetStatus(InvoiceResultCode.BC);
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {

                        lock (_context.GetSyncObject())
                        {
                            _context.LogError("Issue writing to " + _bulkCopier.DestinationTableName, ex);
                            _context.SetStatus(InvoiceResultCode.BC);
                        }

                        return false;
                    }
                }
                //TabLock covers this one, doesn't need bulk sync lock still.
                _bulkCopyData.Clear();
                
            }
            return true;
        }

        private readonly SqlConnection _conn;
        private SqlBulkCopy _bulkCopier;
        const string INVOICE_PREVIEW_STAGING = "AMB.Transaction_LineItem_InvoicePreview_Staging";
        /// <summary>
        /// Check if the process needs to stop because the context has failed.
        /// </summary>
        /// <returns></returns>
        bool checkContextBreak()
        {
            lock (_context.GetSyncObject())
            {
                if (_context.Failure)
                {
                    _context.LogInfo("Job Execution status set to failure. Breaking out preview generation data reader...");
                }
                return _context.Failure;
            }
        }

        public void Init()
        {
            _bulkCopyData = new DataTable();
            _bulkCopyData.AddColumns<InvoiceableTransaction_LineItem>();
            _conn.Open();

            _bulkCopier = new SqlBulkCopy(_conn, 
                                          SqlBulkCopyOptions.KeepNulls 
                                          | SqlBulkCopyOptions.CheckConstraints, 
                                          null)
            {
                DestinationTableName = INVOICE_PREVIEW_STAGING,
                BulkCopyTimeout = 300,
                EnableStreaming = false,
                BatchSize = 0
            };
            foreach (string column in ColumnList)
                _bulkCopier.ColumnMappings.Add(column, column);
        }
        //Context synchronization is only for checking if one of the other threads has failed the context. 
        //Otherwise, it's readonly
        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        public void Process()
        {
            var metrix = _context.Metrix;
            try
            {
                using (var help = metrix.GetBasicHelper())
                {
                    help.QualifiedProcedure = "[AMB].[usp_Transaction_Invoice_Preview2]";
                    help[nameof(ProjectID)] = ProjectID;
                    help["InvoiceDate"] = _context.ProcessingDate;
                    metrix.CommandTimeOut = 1200;
                    //Consideration: a second data reader for going through encounters.
                    using (var cmd = metrix.GetSqlCommand(help))
                    using (var reader = cmd.ExecuteReader(CommandBehavior.Default))
                    {
                        int lastEncounterID = 0;
                        EncounterContainer work = null;
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var tran = new InvoiceableTransaction_LineItem(reader, _context.ProcessingDate);
                                if (lastEncounterID != tran.EncounterID)
                                {
                                    if (work != null)
                                    {
                                        Encounters.Add(work);
                                        _context.ClearResetEvent();
                                    }

                                    work = new EncounterContainer(this, tran.EncounterID, _context);
                                    lastEncounterID = tran.EncounterID;

                                    work.LoadEncounterInformation(reader); //Additional encounter information from the select.
                                    if (checkContextBreak())
                                        break;
                                }

                                work.AddInvoiceItem(tran);
                            }

                            if (work != null)
                            {
                                Encounters.Add(work);
                            }
                            Encounters.CompleteAdding();
                        }
                        
                        reader.Close();
                        //Note: this should only happen if the reader didn't have rows
                        //However, the return value won't be available until the reader is closed, so it can't be checked in an else if block
                        if (cmd.GetReturnValue() == ALREADY_INVOICED_ERROR_CODE)
                        {
                            _context.SetStatus(InvoiceResultCode.AI);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _context.LogError("Invoice Preview Generator", ex);
            }
            finally
            {
                _context.ClearResetEvent();

            }

            //Enumerate transaction_LineItem records. Process. 
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Encounters != null)
                    {
                        Encounters.Dispose();
                        Encounters = null;
                    }
                    //Bulk copier probably has unmanaged resources, but is itself managed and should have a finalizer.
                    ((IDisposable) _bulkCopier)?.Dispose();

                    if (_bulkCopyData != null)
                    {
                        _bulkCopyData.Dispose();
                        _bulkCopyData = null;
                    }

                    if (_conn != null)
                    {
                        if (_conn.State == ConnectionState.Open)
                            _conn.Close();
                        _conn.Dispose();
                    }
                }


                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _bulkCopier = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~InvoicePreviewGenerator()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
