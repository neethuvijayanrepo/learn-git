﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SEIDR.Doc
{
    /// <summary>
    /// Helper using DocMetaData to wrap a StreamWriter and write metaData to a file
    /// </summary>
    public class DocWriter: IDisposable
    {        
        #region operators   
        /// <summary>
        /// Allow treating the doc writer as a DocMetaData to help keep code more succinct
        /// </summary>
        /// <param name="writer"></param>
        public static implicit operator DocMetaData(DocWriter writer)
        {
            return writer.md;
        }
        /// <summary>
        /// Allow treating the writer as a column colleciton to help keep code more succinct
        /// </summary>
        /// <param name="writer"></param>
        public static implicit operator DocRecordColumnCollection(DocWriter writer)
        {
            return writer.md?.Columns;
        }
        #endregion
        StreamWriter sw;
        DocMetaData md;        
        /// <summary>
        /// True if the file being written to is being written with columns having fixed widths and positions.
        /// </summary>
        public bool FixedWidthMode => md.FixedWidthMode;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="AppendIfExists"></param>
        /// <param name="bufferSize">Initial buffer size for underlying stream in KB. 
        /// <para>Note: can be forced to grow, which can be expensive according to https://stackoverflow.com/questions/32346051/what-does-buffer-size-mean-when-streaming-text-to-a-file. </para>
        /// <para>Adding one line at a time, though, so should probably choose based on max size of a line </para>
        /// <para>May also need to consider whether you're writing locally or on a network.</para></param>
        public DocWriter(DocMetaData metaData, bool AppendIfExists = false, int bufferSize = 5000)
        {
            if (!metaData.Valid)
                throw new InvalidOperationException("MetaData is not in a valid state");
            if (!metaData.Columns.Valid)
                throw new InvalidOperationException("Column state Invalid");
            md = metaData;
            bool AddHeader = md.HasHeader && (!File.Exists(metaData.FilePath) || !AppendIfExists);
            sw = new StreamWriter(metaData.FilePath, AppendIfExists, metaData.FileEncoding, bufferSize);
            if (AddHeader)
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < md.SkipLines; i ++)
                {
                    sb.Append(md.LineEndDelimiter);
                }
                foreach(var col in Columns)
                {
                    if(FixedWidthMode)
                        sb.Append(col.ColumnName.PadRight(col.MaxLength.Value));
                    else
                    {
                        sb.Append(col.ColumnName);
                        if (col.Position < Columns.Count -1)
                            sb.Append(md.Delimiter);
                    }                    
                }
                if (!string.IsNullOrEmpty(md.LineEndDelimiter))
                    sb.Append(md.LineEndDelimiter);
                sw.Write(sb);
            }
            
        }
        /// <summary>
        /// Sets the textQualifier. Default is "
        /// </summary>
        public string TextQualifier
        {
            get { return Columns.TextQualifier; }
            set { Columns.TextQualifier = value; }
        }
        /// <summary>
        /// Sets whether or not qualify listed columns as text when writing. Default is false.<para>Note: Ignored in FixedWidth</para>
        /// </summary>
        /// <param name="qualifying"></param>
        /// <param name="columnsToQualify"></param>
        public void SetTextQualify(bool qualifying, params string[] columnsToQualify)
        {
            foreach(var col in columnsToQualify)
            {
                Columns[col].TextQualify = qualifying;
            }
        }
        /// <summary>
        /// Sets whether or not qualify listed columns as text when writing. Default is false.<para>Note: Ignored in FixedWidth</para>
        /// </summary>
        /// <param name="qualifying"></param>
        /// <param name="columnsToQualify"></param>
        public void SetTextQualify(bool qualifying, params int[] columnsToQualify)
        {
            foreach(var col in columnsToQualify)
            {
                Columns[col].TextQualify = qualifying;
            }
        }
        /// <summary>
        /// Changes the justification for listed columns. (Default is to leftJustify)
        /// </summary>
        /// <param name="leftJustify"></param>
        /// <param name="columnsToJustify"></param>
        public void SetJustification(bool leftJustify, params string[] columnsToJustify)
        {
            foreach(var col in columnsToJustify)
            {
                Columns[col].LeftJustify = leftJustify;
            }
        }
        /// <summary>
        /// Changes the justification for listed columns. (Default is to leftJustify)
        /// </summary>
        /// <param name="leftJustify"></param>
        /// <param name="columnsToJustify"></param>
        public void SetJustification(bool leftJustify, params int[] columnsToJustify)
        {
            foreach (var col in columnsToJustify)
            {
                Columns[col].LeftJustify = leftJustify;
            }
        }
        /// <summary>
        /// Column meta Data
        /// </summary>
        public DocRecordColumnCollection Columns => md.Columns;
        
        /// <summary>
        /// Calls <see cref="AddRecord{RecordType}(RecordType, IDictionary{int, DocRecordColumnInfo})"/> using the DocWriter's underlying dictionary.
        /// </summary>
        /// <typeparam name="RecordType"></typeparam>
        /// <param name="record"></param>
        /// <param name="columnMapping"></param>
        public void AddRecord<RecordType>(RecordType record, DocWriterMap columnMapping) 
            where RecordType : DocRecord
        {
            AddRecord(record, columnMapping.MapData);
        }
        /// <summary>
        /// Adds the record to the file via streamWriter
        /// </summary>
        /// <param name="record"></param>
        /// <param name="columnMapping">Optional mapping override. Positions can be set to null or ignored to use the default mapping. 
        /// <para>Key should be the target position in the output file, value should be the column information from the source.
        /// </para>
        /// </param>
        public void AddRecord<RecordType>(RecordType record, IDictionary<int, DocRecordColumnInfo> columnMapping = null) 
            where RecordType: DocRecord
        {
            if (!md.Columns.Valid)
                throw new InvalidOperationException("Column state Invalid");
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if(record.Columns == md.Columns 
                && (columnMapping == null || columnMapping.Count == 0))
            {
                sw.Write(record.ToString()); //same column collection, no mapping override, just write the toString
                return;
            }
            StringBuilder sb = new StringBuilder();
            Columns.ForEachIndex((col, idx) =>
            {                
                if (!md.FixedWidthMode && col.TextQualify)
                    sb.Append(Columns.TextQualifier);
                DocRecordColumnInfo map = col;
                if (columnMapping != null && columnMapping.ContainsKey(idx))
                    map = columnMapping[idx];
                string s = record.GetBestMatch(map.ColumnName, map.OwnerAlias) ?? string.Empty;
                if (FixedWidthMode)
                {
                    System.Diagnostics.Debug.Assert(col.MaxLength.HasValue);
                    if (col.LeftJustify)
                        sb.Append(s.PadRight(col.MaxLength.Value));
                    else
                        sb.Append(s.PadLeft(col.MaxLength.Value));
                }
                else
                {
                    System.Diagnostics.Debug.Assert(Columns.Delimiter.HasValue);
                    System.Diagnostics.Debug.Assert(md.Delimiter.HasValue); //Same as columns delimiter
                    if (s.Contains(Columns.Delimiter.Value) && !col.TextQualify)
                    {
                        sb.Append(Columns.TextQualifier);
                        col.TextQualify = true; //force text qualify in the column going forward.
                    }

                    sb.Append(s);
                    if (col.TextQualify)
                        sb.Append(Columns.TextQualifier);
                    if (idx < Columns.Count - 1)
                        sb.Append(md.Delimiter.Value);
                }
                
            });
            if (!string.IsNullOrEmpty(md.LineEndDelimiter))
                sb.Append(md.LineEndDelimiter);
            sw.Write(sb);            
        }
        /// <summary>
        /// Writes the records out using ToString without validating that they match the column meta data of the writer.
        /// <para>Null records will be ignored.</para>
        /// <para>NOTE: THIS IGNORES METADATA.</para>
        /// </summary>
        /// <param name="toWrite"></param>
        public void BulkWrite(IEnumerable<DocRecord> toWrite)
        {
            foreach (var rec in toWrite)
            {
                if (rec == null)
                    continue;
                sw.Write(rec.ToString());
            }
        }
        /// <summary>
        /// Writes the records out using ToString without validating that they match the column meta data of the writer.
        /// <para>NOTE: THIS IGNORES METADATA.</para>
        /// </summary>
        /// <param name="toWrite"></param>
        public void BulkWrite(params DocRecord[] toWrite) => BulkWrite((IEnumerable<DocRecord>)toWrite);

        /// <summary>
        /// Writes the strings out without validating that they match the column meta data of the writer. Will add the LineEndDelimiter of this metaData if specified, though.
        /// <para>NOTE: THIS IGNORES METADATA.</para>
        /// </summary>
        /// <param name="Lines"></param>
        public void BulkWrite(IEnumerable<string> Lines)
        {
            foreach (var line in Lines)
            {
                if (line == null)
                    continue;
                sw.Write(line + Columns.LineEndDelimiter ?? string.Empty);
        	}
        }

        /// <summary>
        /// Writes the strings out without validating that they match the column meta data of the writer. Will add the LineEndDelimiter of this metaData if specified, though.
        /// <para>NOTE: THIS IGNORES METADATA.</para>
        /// </summary>
        /// <param name="Lines"></param>
        public void BulkWrite(params string[] Lines)
        {
            foreach (var line in Lines)
            {
                if (line == null)
                    continue;
                sw.Write(line + Columns.LineEndDelimiter ?? string.Empty);
            }
        }
        /// <summary>
        /// Adds record to the file via underlying streamWriter
        /// </summary>
        /// <param name="record"></param>
        public void AddRecord(string record)
        {
            AddRecord(Columns.ParseRecord(false, record));
        }
        /// <summary>
        /// Parses the strings and maps them using this collection's MetaData. Will add the LineEndDelimiter of this metaData if specified, though.
        /// </summary>
        /// <param name="Lines"></param>
        public void BulkAdd(IEnumerable<string> Lines)
        {
            foreach (var line in Lines)
                sw.Write(Columns.ParseRecord(false, line));
        }

        /// <summary>
        /// Parses the strings and maps them using this collection's MetaData. Will add the LineEndDelimiter of this metaData if specified, though.
        /// </summary>
        /// <param name="Lines"></param>
        public void BulkAdd(params string[] Lines) => BulkAdd((IEnumerable<string>)Lines);
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if(sw != null) //StreamWriter has its own finalizer
                    {
                        sw.Flush();
                        sw.Dispose();
                        sw = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        /* //StreamWriter already has a finalizer and that's the only thing we're cleaning up, so we don't need this
        ~DocWriter()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }*/

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose underlying <see cref="StreamWriter"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}