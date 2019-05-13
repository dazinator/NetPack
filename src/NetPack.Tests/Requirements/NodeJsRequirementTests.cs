using System.Collections.Generic;
using NetPack.Requirements;
using Xunit;

namespace NetPack.Tests.Requirements
{
    public class NodeJsRequirementTests
    {
        [Fact]
        public void Can_Detect_Node_Js()
        {

            var requirementOne = new NodeJsIsInstalledRequirement();
            var sut = new NodeJsIsInstalledRequirement();
            sut.Check(null);
        }

        [Fact]
        public void Equality_Compare()
        {

            var requirementOne = new NodeJsIsInstalledRequirement();

            var list = new List<IRequirement>();
            list.Add(requirementOne);

            var requirementTow = new NodeJsIsInstalledRequirement();
            Assert.True(list.Contains(requirementTow));
        }
    }
}