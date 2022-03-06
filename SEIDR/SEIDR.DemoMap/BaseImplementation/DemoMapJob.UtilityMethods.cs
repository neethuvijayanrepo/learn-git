using System;
using System.Collections.Generic;
using System.Linq;
using SEIDR.Doc;

namespace SEIDR.DemoMap.BaseImplementation
{
    public partial class DemoMapJob<T>
    {
        /// <summary>
        /// Flips any negative sign in the field value.
        /// <para>If the field value is null/empty/white space, will return either null or '0.00', depending on <paramref name="coalesceToZero"/></para>
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="coalesceToZero"></param>
        /// <returns></returns>
        public string FlipMoneySign(string fieldValue, bool coalesceToZero= true)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return coalesceToZero ? "0.00" : null;
            if (fieldValue.IndexOf('-') >= 0)
                return fieldValue.Replace("-", "");
            return '-' + fieldValue;
        }

        /// <summary>
        /// Returns an enumerable of column information which should be for money values, based on the column names.
        /// <para>Ignore the non mapped columns (start with _N_), as well as settings columns. (<see cref="Account.SETTINGS_COLUMNS"/> ) </para>
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public IEnumerable<DocRecordColumnInfo> GetMoneyColumns(Account record)
        {
            return record.GetColumns(col =>
            {
                if (col.ColumnName.StartsWith("_N_", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (col.ColumnName.In(Account.SETTINGS_COLUMNS))
                    return false;
                return col.ColumnName.IndexOf("Charges", StringComparison.OrdinalIgnoreCase) >= 0
                       || col.ColumnName.IndexOf("Payments", StringComparison.OrdinalIgnoreCase) >= 0
                       || col.ColumnName.IndexOf("Balance", StringComparison.OrdinalIgnoreCase) >= 0
                       || col.ColumnName.IndexOf("Adjustments", StringComparison.OrdinalIgnoreCase) >= 0
                       || col.ColumnName.IndexOf("EstimatedAmountDue", StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }
        /// <summary>
        /// Gets the column name for an insurance sequence. Probably not needed with the <see cref="Bucket"/> utility class.
        /// </summary>
        /// <param name="Sequence"></param>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public string GetInsuranceColumnName(int Sequence, string ColumnName)
        {
            if (ColumnName.StartsWith("_"))
                return "Ins" + Sequence + ColumnName;
            return $"Ins{Sequence}_{ColumnName}";
        }
    }
}
