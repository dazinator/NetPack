using NetPack.Pipeline;

namespace NetPack.Requirements
{
    public interface IRequirement
    {
        void Check(IPipeLine pipeline);
    }
}