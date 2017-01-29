using System;
using NetPack.RequireJs;

namespace NetPack.Pipeline
{

    public class PipeConfiguration
    {

        public IPipe Pipe { get; set; }
        public PipelineInput Input { get; set; }
        public DateTime LastProcessedEndTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public DateTime LastProcessStartTime { get; set; } = DateTime.MinValue.ToUniversalTime();
        public bool IsProcessing { get; set; }

        /// <summary>
        /// Returns true if the pipe has updated inputs that need to be processed.
        /// </summary>
        /// <returns></returns>
        internal bool IsDirty()
        {
            var isDirty = (Input.LastChanged > LastProcessStartTime);
            isDirty = isDirty && Input.LastChanged <= LastProcessedEndTime;
            isDirty = isDirty || Input.LastChanged > LastProcessedEndTime;

            return isDirty;
        }
    }
}