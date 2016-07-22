using NetPack.Requirements;
using Xunit;

namespace NetPack.Tests
{
    public class NodeJsRequirementTests
    {
        [Fact]
        public void Can_Detect_Node_Js()
        {
            var sut = new NodeJsRequirement();
            sut.Check();
        }
    }
}