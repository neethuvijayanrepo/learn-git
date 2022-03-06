using SEIDR.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SEIDR.FileSystem
{
    public class DocMetaDataSet
    {
        public DocMetaDataSet(DatabaseManager dm, int JobProfile_JobID, DateTime ProcessingDate)
        {
            using (var h = dm.GetBasicHelper())
            {
                h.QualifiedProcedure = "SEIDR.[usp_DocMetaDataColumn_sl]";
                h[nameof(JobProfile_JobID)] = JobProfile_JobID;
                h[nameof(ProcessingDate)] = ProcessingDate;
                var ds = dm.Execute(h);
                if (h.ReturnValue != 0)
                    return;
                MetaData = ds
                    .Tables[0]
                    .ToContentRecord<DocMetaData>(0);
                MetaDataColumns = ds
                    .Tables[1]
                    .ToContentList<DocMetaDataColumn>()
                    .OrderBy(a => a.Position)
                    .ToList();
            }
        }
        public ValidationError CompareColumnData(Doc.DocRecordColumnCollection parsedColumns, out string Message)
        {
            ValidationError ve = ValidationError.None;
            Message = null;
            if (parsedColumns.Count != MetaDataColumns.Count)
            {
                Message = "Expected " + MetaDataColumns.Count + " columns, found " + parsedColumns.Count;
                ve |= ValidationError.CC;
            }
            for (int i = 0; i < parsedColumns.Count; i++)
            {
                var col = parsedColumns[i]; //Grabs by position
                DocMetaDataColumn expected;
                if (i < MetaDataColumns.Count)
                    expected = MetaDataColumns[i]; //Ordered by position
                else
                    break;
                //if(expected.Max_Length != null) //later.

                if (MetaData.HasHeader && col.ColumnName != expected.ColumnName)
                {
                    Message = (Message == null ? Environment.NewLine : "") + "Position " + i + ". Expected '" + expected.ColumnName + "', found '" + col.ColumnName + "'";
                    ve |= ValidationError.CN; //Flag
                }
            }
            //Note: ValidationError.CC | ValidationError.CN = ValidationError.NC;
            return ve;
        }
        public readonly DocMetaData MetaData = null;
        public readonly List<DocMetaDataColumn> MetaDataColumns = null;
    }
}
