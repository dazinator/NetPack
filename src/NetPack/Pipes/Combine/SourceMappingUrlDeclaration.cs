using System;

namespace NetPack.Pipes
{
    public class SourceMappingUrlDeclaration
    {

        public static int SourceMappingUrlDeclarationLength = "//# sourceMappingURL=".Length;

        public static bool TryParse(string line, out SourceMappingUrlDeclaration declaration)
        {
            if (line.StartsWith("//# sourceMappingURL=", StringComparison.OrdinalIgnoreCase))
            {
                declaration = new SourceMappingUrlDeclaration();
                declaration.SourceMappingUrl = line.Substring(SourceMappingUrlDeclarationLength);
                return true;
            }

            if (line.StartsWith("//@ sourceMappingURL=", StringComparison.OrdinalIgnoreCase))
            {
                declaration = new SourceMappingUrlDeclaration();
                declaration.SourceMappingUrl = line.Substring(SourceMappingUrlDeclarationLength);
                return true;
            }
            declaration = null;
            return false;

        }

        public string SourceMappingUrl { get; set; }

        public int LineNumber { get; set; }

        public long Position { get; set; }
    }
}