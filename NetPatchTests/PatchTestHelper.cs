using System;
using NetPatch;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;


namespace NetPatchTests {
    public class PatchTestHelper {
        public bool PatchRoundTripMatches(string originalJson, string currentJson, string expectedPatch = null)
        {
            JsonPatchDocument patch = JsonPatch.GetPatch(
                        originalJson,
                        currentJson);

            var originalObj = JsonConvert.DeserializeObject(originalJson);

            var currentObj = JsonConvert.DeserializeObject(originalJson);
            patch.ApplyTo(currentObj);

            if (expectedPatch != null)
            {
                bool matchesExpectedPatch = JToken.DeepEquals(JToken.Parse(expectedPatch), JToken.Parse(JsonConvert.SerializeObject(patch)));

                if (!matchesExpectedPatch)
                {
                    throw new Exception($"Expected Patch Does Not Match. Expected {expectedPatch}. Received {JsonConvert.SerializeObject(patch)}");
                }
            }

            JToken expected = JObject.Parse(JsonConvert.SerializeObject(JObject.Parse(currentJson)));
            JToken observed = JObject.Parse(JsonConvert.SerializeObject(currentObj));
            bool areEqual = JToken.DeepEquals(expected, observed);

            if (!areEqual) {
                JsonPatchDocument failPatch = JsonPatch.GetPatch(
                        JsonConvert.SerializeObject(expected),
                        JsonConvert.SerializeObject(observed));
                throw new Exception($"Round trip failed. Expected {expected}. Received {observed}. Failure patch is {JsonConvert.SerializeObject(failPatch)}");
            }

            return areEqual;
        }
    }
}