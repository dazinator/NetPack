using System;

namespace NetPack.Tests.Wip
{
    public class InvalidTypeScriptFileException : Exception
    {
        public InvalidTypeScriptFileException() : base()
        {

        }
        public InvalidTypeScriptFileException(string message) : base(message)
        {

        }
    }
}
