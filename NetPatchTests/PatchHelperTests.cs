using System;
using Xunit;
using NetPatch;
using netjson = System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;

namespace NetPatchTests
{
    public class NetPatchTests
    {
        PatchTestHelper _testHelper = new PatchTestHelper();

        [Fact]
        public void NestedArrayObjects()
        {
            string originalJson = "{\"someValue\": 3,\"arrayValue\": [{\"something\": 2, \"else\": 4}, {\"something\": 5, \"else\": 3}]}";
            string currentJson = "{\"someValue\": 3,\"arrayValue\": [{\"something\": 3, \"else\": 4}, {\"something\": 5, \"else\": 3}]}";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson));

            JsonPatchDocument patch = JsonPatch.GetPatch(originalJson, currentJson);

            string jsonPatch = netjson.JsonSerializer.Serialize(patch);

            Assert.Equal(patch.Operations.Count, 1);
            Assert.Equal(patch.Operations[0].OperationType, OperationType.Replace);
            Assert.Equal(patch.Operations[0].path, "/arrayValue/0/something");
            Assert.Equal((long) patch.Operations[0].value, (long) 3);
        }

        [Fact]
        public void ApplyingPatchToEmptyObject()
        {
            string originalJson = "{}";
            string currentJson = "{  \"_id\": \"603dd3be51eeffafa66eb3e8\",  \"index\": 0,  \"guid\": \"fc02c53c-333c-4619-ad27-e70355e27b25\",  \"isActive\": true,  \"balance\": \"$1,393.02\",  \"picture\": \"http://placehold.it/32x32\",  \"age\": 20,  \"eyeColor\": \"green\",  \"name\": \"Giles Roman\",  \"gender\": \"male\",  \"company\": \"CYTREK\",  \"email\": \"gilesroman@cytrek.com\",  \"phone\": \"+1 (997) 544-3890\",  \"address\": \"753 Randolph Street, Brooktrails, Florida, 4868\",  \"about\": \"Excepteur velit proident veniam officia magna veniam pariatur minim non incididunt adipisicing aute consequat magna. Qui et pariatur id Lorem. Ullamco sint labore non officia adipisicing sunt fugiat exercitation.\",  \"registered\": \"2017-05-09T12:56:37 +05:00\",  \"latitude\": -30.543994,  \"longitude\": -121.283579,  \"tags\": [    \"labore\",    \"deserunt\",    \"sunt\",    \"reprehenderit\",    \"nulla\",    \"dolore\",    \"veniam\"  ],  \"friends\": [    {      \"id\": 0,      \"name\": \"Holman Haney\"    },    {      \"id\": 1,      \"name\": \"Vega Savage\"    },    {      \"id\": 2,      \"name\": \"Kennedy Weber\"    }  ],  \"greeting\": \"Hello, Giles Roman! You have 9 unread messages.\",  \"favoriteFruit\": \"banana\"}";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson));
        }
    }
}
