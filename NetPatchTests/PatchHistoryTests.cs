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
        public void Sample()
        {
            var patch = PatchHelper.GeneratePatch(
                        new { Unchanged = new[] { 1, 2, 3, 4, 5 }, Changed = "1", Removed = "1" },
                        new { Unchanged = new[] { 1, 2, 3, 4, 5 }, Changed = "2", Added = new { x = "1" } });

            Console.WriteLine(JsonConvert.SerializeObject(patch));
        }

        [Fact]
        public void BasicSystemTextJsonExample() {
            var patch = PatchHelper.GetPatchForObject("{}", "{}");
            Assert.Equal(1,1);
        }
    }
}
