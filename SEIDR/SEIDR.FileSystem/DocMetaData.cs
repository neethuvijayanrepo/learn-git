namespace SEIDR.FileSystem
{
    public class DocMetaData
    {
        public int MetaDataID { get; set; }
        public int Version { get; set; }
        public char? Delimiter { get; set; } //can set to string if have issues with parameter mapping, but better to have as char to match usage..
        public bool HasHeader { get; set; }
        public int SkipLines { get; set; } = 0;
        public string TextQualifier { get; set; } = "\"";
        public bool HasTrailer { get; set; }
    }
}
