using System.Collections.Generic;
using NetPack.Requirements;
using Xunit;

namespace NetPack.Tests.Requirements
{
    public class NpmDependencyTests
    {
        //[Fact]
        //public void Can_Ensure_Npm_Module()
        //{
        //    var sut = new NpmDependency("typescript");
        //    sut.Check();
        //}


        [Fact]
        public void Equality_Compare()
        {

            var sut = new NpmDependency("typescript");

            var list = new List<NpmDependency>();
            list.Add(sut);

            var requirementTow = new NpmDependency("typescript");
            Assert.Contains(requirementTow, list);

            var requirementThree = new NpmDependency("another");
            Assert.DoesNotContain(requirementThree, list);
        }
    }
}