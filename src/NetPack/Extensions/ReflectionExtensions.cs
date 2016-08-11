using System;
using System.Reflection;

namespace NetPack.Extensions
{
    public static class ReflectionExtensions
    {
        public static Assembly GetAssemblyFromType(this Type type)
        {

#if NETSTANDARD
            var assy = type.GetTypeInfo().Assembly;
#else
            var assy = type.Assembly;
#endif
            return assy;
        }
    }
}