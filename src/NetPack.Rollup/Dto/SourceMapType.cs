using System.Runtime.Serialization;

namespace NetPack.Rollup
{
    public enum SourceMapType
    {
        [EnumMember(Value = "file")]
        File,
        [EnumMember(Value = "inline")]
        Inline,       
    }

}