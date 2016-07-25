using System.Collections.Generic;
using NetPack.Requirements;
using Xunit;

namespace NetPack.Tests
{
    public class NpmModuleRequirementTests
    {
        [Fact]
        public void Can_Ensure_Npm_Module()
        {
            var sut = new NpmModuleRequirement("typescript", true);
            sut.Check();
        }


        [Fact]
        public void Equality_Compare()
        {

            var sut = new NpmModuleRequirement("typescript", false);

            var list = new List<IRequirement>();
            list.Add(sut);

            var requirementTow = new NpmModuleRequirement("typescript", false);
            Assert.True(list.Contains(requirementTow));
            
            var requirementThree = new NpmModuleRequirement("another", false);
            Assert.False(list.Contains(requirementThree));
        }
    }
}