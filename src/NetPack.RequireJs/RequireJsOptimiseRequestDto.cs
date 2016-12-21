using System.Collections.Generic;

namespace NetPack.Pipes
{
    public class RequireJsOptimiseRequestDto
    {

        public RequireJsOptimiseRequestDto()
        {
            Files = new List<NodeInMemoryFile>();
        }

        public RequireJsOptimisationPipeOptions Options { get; set; }

        /// <summary>
        /// Allows the script file paths and contents to be sourced from this array (in memory)
        /// rather than disk IO calls.
        /// </summary>
        public List<NodeInMemoryFile> Files { get; set; }




        //public string TypescriptCode { get; set; }
        //public TypeScriptPipeOptions Options { get; set; }
        //public string FilePath { get; set; }

    }
}