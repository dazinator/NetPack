using System;
using System.Reflection;

namespace NetPack.Utils
{
    public static class ReflectionUtils
    {
        public static Assembly GetAssemblyFromType(Type type)
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