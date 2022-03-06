using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SEIDR.DataBase;
using SEIDR.Doc;

namespace JobUnitTest.MockData
{
    public class MockDatabaseManager : DatabaseManager
    {
        public List<MockQueryModel> MockModels { get; } = new List<MockQueryModel>();

        public MockDatabaseManager AddMockModel(params MockQueryModel[] newModelList)
        {
            MockModels.AddRange(newModelList);
            return this;
        }

        /// <summary>
        /// Adds a new Mock Query model to <see cref="MockModels"/> and then returns it for modification.
        /// </summary>
        /// <param name="ProcedureName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public MockQueryModel NewMockModel(string ProcedureName, string schema = null)
        {
            var m = new MockQueryModel(schema ?? DefaultSchema, ProcedureName);
            MockModels.Add(m);
            return m;
        }
        /// <summary>
        /// Creates a MockModel with a stored procedure name such as usp_{typeof(<typeparamref name="Rt"/>)}_{suffix}
        /// <para>E.g., Rt: PayerMaster_MapInfo, suffix: sl -> usp_PayerMaster_MapInfo_sl</para>
        /// </summary>
        /// <typeparam name="Rt"></typeparam>
        /// <param name="suffix">Suffix for procedure - default is sl for select list.</param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public MockQueryModel NewMockModel<Rt>(string suffix = "sl", string schema = null)
        {
            var t = typeof(Rt);
            var m = new MockQueryModel(schema ?? DefaultSchema, $"usp_{t.Name}_{suffix}");
            MockModels.Add(m);
            return m;
        }

        public MockQueryModel NewMockModelQualified(string QualifiedProcedureName)
        {
            var m = new MockQueryModel {QualifiedStoredProcedure = QualifiedProcedureName};
            MockModels.Add(m);
            return m;
        }
        public MockQueryModel LoadFromFile(string FilePath, string ProcedureName = null, string Schema = null)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(FilePath);
            var mock = new MockQueryModel(Schema ?? nameof(SEIDR), ProcedureName ?? fileName);
            using (DocReader r = new DocReader("q", FilePath))
            {
                DataTable dt = new DataTable();
                foreach (var col in r.Columns)
                {
                    dt.Columns.Add(col.ColumnName);
                }
                foreach (var line in r)
                {
                    object[] objList = new object[r.Columns.Count];
                    for (int i = 0; i < objList.Length; i++)
                    {
                        objList[i] = line[i];
                    }
                    dt.Rows.Add(objList);
                }
                mock.Result.Tables.Add(dt);
            }
            return mock;
        }

        public MockQueryModel GetBestMatch(string QualifiedProcedure, object mapObj)
        {
            var q = from mock in MockModels
                    let match = mock.MatchMapObject(QualifiedProcedure, mapObj)
                    where match.Item1
                    orderby match.Item2 descending
                    select mock;
            return q.FirstOrDefault();
        }
        public MockQueryModel GetBestMatch(DatabaseManagerHelperModel model)
        {
            var q = from mock in MockModels
                    let match = mock.MatchQueryModel(model)
                    where match.Item1
                    orderby match.Item2 descending
                    select mock;
            return q.FirstOrDefault();
        }

        public MockDatabaseManager(DatabaseConnection Connection, string DefaultSchema = "dbo", string SaveFormat = "usp_{0}_iu", string UpdateFormat = "usp_{0}_u", string InsertFormat = "usp_{0}_i", string DeleteFormat = "usp_{0}_d", string SelectRowFormat = "usp_{0}_ss", string SelectListFormat = "usp_{0}_sl") 
            : base(Connection, DefaultSchema, SaveFormat, UpdateFormat, InsertFormat, DeleteFormat, SelectRowFormat, SelectListFormat)
        {
        }
        public override DataSet Execute(DatabaseManagerHelperModel i, bool CommitSuccess = false)
        {
            var qm = GetBestMatch(i);
            if (qm != null)
            {
                i.ReturnValue = qm.ReturnValue;
                foreach (var outP in qm.OutParameters)
                {
                    i[outP.Key] = outP.Value;
                }

                if (CommitSuccess && i.HasOpenTran)
                    i.CommitTran();
                return qm.Result;
            }
            return base.Execute(i, CommitSuccess);
        }
        public override DataSet Execute(string QualifiedProcedureName, object mapObj = null, bool updateMap = true)
        {
            var qm = GetBestMatch(QualifiedProcedureName, mapObj);
            if (qm != null)
                return qm.Result;
            return base.Execute(QualifiedProcedureName, mapObj, updateMap);
        }
        public override int ExecuteNonQuery(DatabaseManagerHelperModel i, bool CommitSuccess = false)
        {
            var qm = GetBestMatch(i);
            if (qm != null)
            {
                i.ReturnValue = qm.ReturnValue;
                foreach (var outP in qm.OutParameters)
                {
                    i[outP.Key] = outP.Value;
                }
                if (CommitSuccess && i.HasOpenTran)
                    i.CommitTran();
                return qm.ReturnValue;
            }
            return base.ExecuteNonQuery(i, CommitSuccess);
        }
        public override int ExecuteNonQuery(string QualifiedProcedureName, out int ReturnCode, object mapObj = null, bool? RetryDeadlock = default(bool?), bool updateMap = true)
        {
            return base.ExecuteNonQuery(QualifiedProcedureName, out ReturnCode, mapObj, RetryDeadlock, updateMap);
        }
        public override DataSet ExecuteText(DatabaseManagerHelperModel i, string CommandText, bool Commit = false)
        {
            return base.ExecuteText(i, CommandText, Commit);
        }
        public override DataSet ExecuteText(string CommandText, bool? RetryDeadlock = default(bool?))
        {
            return base.ExecuteText(CommandText, RetryDeadlock);
        }
        public override int ExecuteTextNonQuery(string CommandText, bool? RetryDeadlock = default(bool?))
        {
            return base.ExecuteTextNonQuery(CommandText, RetryDeadlock);
        }
        public override DataTable SelectWithKey(string Key, object value, string TableOrView, int Page = -1, int PageSize = -1)
        {
            return base.SelectWithKey(Key, value, TableOrView, Page, PageSize);
        }
    }
}
