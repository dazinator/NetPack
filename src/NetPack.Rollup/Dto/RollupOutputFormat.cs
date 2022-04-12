using System.Runtime.Serialization;

namespace NetPack.Rollup
{
    public enum RollupOutputFormat
    {
        [EnumMember(Value = "system")]
        System,
        [EnumMember(Value = "amd")]
        Amd,
        [EnumMember(Value = "cjs")]
        Cjs,
        [EnumMember(Value = "esm")]
        Esm,
        [EnumMember(Value = "iife")]
        Iife,
        [EnumMember(Value = "umd")]
        Umd
    }

}