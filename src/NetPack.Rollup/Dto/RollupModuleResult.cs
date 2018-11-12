namespace NetPack.Rollup
{
    public class RollupModuleResult
    {
        public int OriginalLength { get; set; }

        public string[] RemovedExports { get; set; }

        public int Length { get; set; }

        public string[] Exports { get; set; }
    }
}