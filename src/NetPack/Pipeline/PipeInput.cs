using System;
using System.Collections.Generic;
using System.Linq;

namespace NetPack.Pipeline
{
    public class PipeInput
    {
        public PipeInput()
        {
            IncludeList = new List<string>();
            ExcludeList = new List<string>();
        }

        protected List<string> IncludeList { get; }

        protected List<string> ExcludeList { get; }

        public void AddInclude(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if (!IncludeList.Contains(searchPattern))
            {
                IncludeList.Add(searchPattern);
                LastChanged = DateTime.UtcNow;
            }          
        }

        public void AddExclude(string pattern)
        {
            string searchPattern = pattern;
            if (!pattern.StartsWith("/"))
            {
                searchPattern = "/" + pattern;
            }
            if(!ExcludeList.Contains(searchPattern))
            {
                ExcludeList.Add(searchPattern);
                LastChanged = DateTime.UtcNow;
            }          
        }

        public DateTime LastChanged { get; set; }

        public IEnumerable<string> GetIncludes()
        {
            return IncludeList.AsEnumerable();
        }

        public IEnumerable<string> GetExcludes()
        {
            return ExcludeList.AsEnumerable();
        }       
       
    }


}