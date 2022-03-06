using SEIDR;
using SEIDR.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobUnitTest.MockData
{
    public class MockQueryModel
    {
        public Tuple<bool, int> MatchQueryModel(SEIDR.DataBase.DatabaseManagerHelperModel Query)
        {
            int matchLevel = 0;
            if (!Query.QualifiedProcedure.Equals(this.QualifiedStoredProcedure, StringComparison.OrdinalIgnoreCase))
            {
                return new Tuple<bool, int>(false, 0);
            }

            foreach (var k in FilterParameters)
            {
                if (!Query.Parameters.ContainsKey(k.Key))
                    return new Tuple<bool, int>(false, matchLevel);
                if (Query[k.Key] != k.Value)
                    return new Tuple<bool, int>(false, matchLevel);
                matchLevel++;
            }
            return new Tuple<bool, int>(true, matchLevel);
        }

        public Tuple<bool, int> MatchMapObject(string QualifiedProcedure, object map)
        {
            int matchLevel = 0;
            if (QualifiedProcedure != this.QualifiedStoredProcedure)
                return new Tuple<bool, int>(false, matchLevel);
            if (map == null)
                throw new ArgumentNullException(nameof(map));
            var props = map.GetType().GetProperties();
            foreach (var k in FilterParameters)
            {
                var prop = props.FirstOrDefault(p => p.Name == k.Key);
                if (prop == null)
                    return new Tuple<bool, int>(false, matchLevel);
                if (prop.GetValue(map) != k.Value)
                    return new Tuple<bool, int>(false, matchLevel);
                matchLevel++;
            }
            return new Tuple<bool, int>(true, matchLevel);
        }
        public MockQueryModel(string Schema, string StoredProcedure)
            :this(StoredProcedure)
        {
            this.Schema = Schema;
        }

        public MockQueryModel(string StoredProcedure)
        {
            this.StoredProcedure = StoredProcedure;
            FilterParameters = new Dictionary<string, object>();
            OutParameters = new Dictionary<string, object>();
        }

        public MockQueryModel()
        {
            FilterParameters = new Dictionary<string, object>();
            OutParameters = new Dictionary<string, object>();
        }


        public MockQueryModel AddFilter(string Key, object val)
        {
            FilterParameters.Add(Key, val);
            return this;
        }

        public MockQueryModel SetFilter(string key, object val)
        {
            FilterParameters[key] = val;
            return this;
        }

        public MockQueryModel AddOutParameter(string key, object val)
        {
            OutParameters.Add(key, val);
            return this;
        }
        public MockQueryModel SetOutParameter(string key, object val)
        {
            OutParameters[key] = val;
            return this;
        }
        public Dictionary<string, object> FilterParameters { get; }
        public Dictionary<string, object> OutParameters { get; }
        public int ReturnValue { get; set; } = 0;
        /// <summary>
        /// For use with comparison against actual data.
        /// </summary>
        public int ExpectedResultCount { get; set; } = -1; 
        public DataSet Result { get; set; } = new DataSet();
        private string _schema = "[" + nameof(SEIDR) + "]";

        private string Qualify(string value)
        {
            string result = value;
            if (value[0] != '[')
            {
                if (value[value.Length - 1] != ']')
                {
                    result = '[' + value + ']';
                }
                else
                    result = '[' + value;
            }
            else if (value[value.Length - 1] != ']')
            {
                result = value + ']';
            }
            return result;
        }
        public string Schema
        {
            get { return _schema; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _schema = Qualify(value);
            }
        }

        private string _procedure;

        public string StoredProcedure
        {
            get { return _procedure;}
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _procedure = Qualify(value);
            }
        }

        public MockQueryModel MapToNewRow<Rt>(Rt ro, int TableID = 0)
        {
            while (Result.Tables.Count <= TableID)
            {
                Result.Tables.Add();
            }

            var dt = Result.Tables[TableID];
            dt.AddRow(ro);
            return this;
        }

        public MockQueryModel MapToNewRows<Rt>(List<Rt> rl, int TableID = 0)
        {
            while (Result.Tables.Count <= TableID)
            {
                Result.Tables.Add();
            }

            var dt = Result.Tables[TableID];
            dt.AddRowRange(rl);
            return this;
        }
        public string QualifiedStoredProcedure 
        {
            get
            {
                return Schema + "." + StoredProcedure;
            }
            set
            {
                var s = value.Split('.');
                if (s.Length != 2)
                {
                    throw new ArgumentException($"Qualified Procedure must be {(s.Length < 2? "both": "only")} Schema and Procedure.");
                }
                Schema = s[0];
                StoredProcedure = s[1];
            }
        }
    }
}
