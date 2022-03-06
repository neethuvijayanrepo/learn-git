using SEIDR.Doc;

namespace SEIDR.FileSystem
{
    public class DocMetaDataColumn : IRecordColumnInfo
    {
        //public int MetaDataColumnID { get; set; } //Not really needed here.
        public string ColumnName { get; set; }
        public int Position { get; set; }
        public short? Max_Length { get; set; }
        public bool SortASC { get; set; }
        public short? SortPriority { get; set; }
    }
}
