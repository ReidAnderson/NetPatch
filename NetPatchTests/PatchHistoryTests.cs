using System;
using Xunit;
using NetPatch;
using System.Text.Json;
using Newtonsoft.Json;

namespace NetPatchTests
{
    public class NetPatchTests
    {
        [Fact]
        public void BasicSystemTextJsonExample() {
            var patch = PatchHelper.GetPatchForObject("{}", "{}");
            Assert.Equal(1,1);
        }
    }
}
