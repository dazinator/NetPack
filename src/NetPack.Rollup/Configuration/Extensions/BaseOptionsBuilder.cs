namespace NetPack
{
    public static partial class RollupConfigExtensions
    {
        public abstract class BaseOptionsBuilder<T>
            where T : BaseOptionsBuilder<T>
        {
            private readonly dynamic _options;

            public BaseOptionsBuilder(dynamic options)
            {
                _options = options;
            }

            public T AddFunctionProperty(string name, string function)
            {
                string func = EncodeFunctionAsJsonPropertyValue(function);
                _options.Add(name, func);
                return (T)this;
            }

            private string EncodeFunctionAsJsonPropertyValue(string function)
            {
                // Bit of a hack.
                // We are building up a JSON object which doesn't allow javascript functions.
                // So we store the function as a json property (string) but later, when serialising, we can detect any string property
                // wrapped in FUNC keyword, and serialise it as a js function instead.
                return $"FUNC{function}FUNC";
            }

            public dynamic Object => _options;
        }


    }
}
