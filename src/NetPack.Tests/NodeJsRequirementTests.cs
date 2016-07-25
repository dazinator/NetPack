using System.Collections.Generic;
using NetPack.Requirements;
using Xunit;

namespace NetPack.Tests
{
    public class NodeJsRequirementTests
    {
        [Fact]
        public void Can_Detect_Node_Js()
        {

            var requirementOne = new NodeJsRequirement();
            var sut = new NodeJsRequirement();
            sut.Check();
        }

        [Fact]
        public void Equality_Compare()
        {

            var requirementOne = new NodeJsRequirement();

            var list = new List<IRequirement>();
            list.Add(requirementOne);

            var requirementTow = new NodeJsRequirement();
            Assert.True(list.Contains(requirementTow));
        }
    }
}