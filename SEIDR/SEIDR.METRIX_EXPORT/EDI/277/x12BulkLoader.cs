using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cymetrix.Andromeda.ClaimStatus;

namespace SEIDR.METRIX_EXPORT.EDI._277
{
    //Taken from ~/ClaimStatus/BulkLoader/x12BulkLoader.cs
    //ToDo: fix naming and member access (the logic in this class is extremely specific to EDI277, so there's really no reason to inherit or have protected)
    public class x12BulkLoader : IDisposable
    {
        private int _LoadBatchID = -1;
        private bool _isTest = false;
        public EDI_277 _SegmentReader;
        protected int _BatchSize = -1;
        protected int _BulkCopyTimeout = -1;
        protected int _SkippedSegmentCount = -1;

        protected bool _CanRun = true;

        //protected bool _InsertIntoTables = false;
        protected int _RowsCommitted = 0;
        protected string _connectionString;
        protected FileStream fs = null;
        public string message = String.Empty;
        private Edi997Tables _997Tables;

        public Edi997Tables NineNineSevenTables
        {
            get { return _997Tables; }
            set { _997Tables = value; }
        }

        public x12BulkLoader(string fileName, int bufferSize, int batchSize, string connectionString, int loadBatchID, int bulkCopyTimeout, bool isTest)
        {
            _isTest = isTest;
            Init(fileName, bufferSize, batchSize, connectionString, loadBatchID, bulkCopyTimeout);
        }

        public x12BulkLoader(string fileName, int bufferSize, int batchSize, string connectionString, int loadBatchID, int bulkCopyTimeout)
        {
            Init(fileName, bufferSize, batchSize, connectionString, loadBatchID, bulkCopyTimeout);
        }

        public x12BulkLoader(int batchSize, string connectionString, int bulkCopyTimeout)
        {
            this._BatchSize = batchSize;
            this._BulkCopyTimeout = bulkCopyTimeout;
            this._connectionString = connectionString;
        }

        private void Init(string fileName, int bufferSize, int batchSize, string connectionString, int loadBatchID, int bulkCopyTimeout)
        {
            this._BatchSize = batchSize;
            this._LoadBatchID = loadBatchID;
            this._BulkCopyTimeout = bulkCopyTimeout;
            this._connectionString = connectionString;
            using (fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {

                _SegmentReader = new EDI_277(fs, bufferSize, this._LoadBatchID);

                NineNineSevenTables = _SegmentReader.Get997Tables();

                CloseFile();
            }

            CreateTestTablesIfNecessary(_isTest);
            bulkCopy(_SegmentReader.Dt277Table);
            message += String.Format("Saved {0} row(s) to the {1} table.{2}", _SegmentReader.Dt277Table.Rows.Count.ToString()
                                   , _SegmentReader.Dt277Table.TableName, Environment.NewLine);
            bulkCopy(_SegmentReader.ServiceLineDataTable);
            message += String.Format("Saved {0} rows to the {1} table.{2}", _SegmentReader.ServiceLineDataTable.Rows.Count.ToString()
                                   , _SegmentReader.ServiceLineDataTable.TableName, Environment.NewLine);
            bulkCopy(_SegmentReader.StatusInfoTable);
            message += String.Format("Saved {0} rows to the {1} table.{2}", _SegmentReader.StatusInfoTable.Rows.Count.ToString()
                                   , _SegmentReader.StatusInfoTable.TableName, Environment.NewLine);
            if (NineNineSevenTables != null)
            {
                bulkCopy(NineNineSevenTables.DT997DataTable);
                message += String.Format("Saved {0} rows to the {1} table.{2}", NineNineSevenTables.DT997DataTable.Rows.Count.ToString()
                                       , NineNineSevenTables.DT997DataTable.TableName, Environment.NewLine);
                bulkCopy(NineNineSevenTables.DTAK2DataTable);
                message += String.Format("Saved {0} rows to the {1} table.{2}", NineNineSevenTables.DTAK2DataTable.Rows.Count.ToString()
                                       , NineNineSevenTables.DTAK2DataTable.TableName, Environment.NewLine);
                bulkCopy(NineNineSevenTables.DTAK3DataTable);
                message += String.Format("Saved {0} rows to the {1} table.{2}", NineNineSevenTables.DTAK3DataTable.Rows.Count.ToString()
                                       , NineNineSevenTables.DTAK3DataTable.TableName, Environment.NewLine);
                bulkCopy(NineNineSevenTables.DTAK4DataTable);
                message += String.Format("Saved {0} rows to the {1} table.{2}", NineNineSevenTables.DTAK4DataTable.Rows.Count.ToString()
                                       , NineNineSevenTables.DTAK4DataTable.TableName, Environment.NewLine);

            }

        }

        private void CreateTestTablesIfNecessary(bool isTest)
        {
            if (_isTest)
            {
                SqlConnection cn = new SqlConnection(_connectionString);
                cn.Open();
                SqlCommand cmd = new SqlCommand("STAGING.usp_Edi277_Load_CreateTestTables", cn);
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Dispose();

            }
        }

        private void ModifyTableNames(DataTable dt)
        {
            string strTest = "_Test";
            if (_isTest)
            {
                dt.TableName += strTest;
            }
        }


        /// <summary>
        /// Copies from DataTable object to the database.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool bulkCopy(DataTable dt)
        {
            bool rtn = false;
            if (dt.Rows.Count > 0)
            {
                //if (_isTest)
                //{
                //    ModifyTableNames(dt);
                //}

                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    using (SqlBulkCopy bulkCopier = new SqlBulkCopy(cn))
                    {
                        try
                        {

                            bulkCopier.DestinationTableName = dt.TableName;
                            bulkCopier.BatchSize = this._BatchSize;
                            bulkCopier.BulkCopyTimeout = this._BulkCopyTimeout;


                            try
                            {
                                bulkCopier.WriteToServer(dt);

                            }
                            catch (System.Exception ex)
                            {
                                if (ex.Message.Contains("bcp client"))
                                {
                                    int? rowsInDb = 0;
                                    string sqlRowCount = "SELECT Count(*) FROM " + dt.TableName;
                                    SqlCommand cmd = new SqlCommand(sqlRowCount, cn);
                                    cmd.CommandType = CommandType.Text;
                                    rowsInDb = (int?) cmd.ExecuteScalar();
                                    if (rowsInDb != dt.Rows.Count)
                                        throw;


                                }
                                else
                                    throw;
                            }

                            rtn = true;
                        }

                        catch
                        {
                            throw;

                        }
                        finally
                        {
                            if (bulkCopier != null) bulkCopier.Close();
                            cn.Close();
                        }
                    }
                }
            }

            //}
            return rtn;
        }

        private void CloseFile()
        {
            try
            {
                if (fs != null)
                {
                    if (fs.CanRead)
                    {
                        fs.Close();
                    }

                    fs.Dispose();
                    fs = null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            CloseFile();
        }

        #endregion
    }
}

