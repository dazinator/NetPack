namespace Microsoft.AspNetCore.Http
{
    public static class PathStringUtils
    {

        public static PathString ToPathString(this string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return new PathString();
            }

            if(path.StartsWith("/"))
            {
                return (PathString)path;
            }

            return new PathString($"/{path}");
        }

    }
}
