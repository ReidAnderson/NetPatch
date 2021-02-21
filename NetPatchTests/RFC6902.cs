using System;
using Xunit;
using NetPatch;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace NetPatchTests
{
    public class RFC6902Tests
    {

        private bool PatchRoundTripMatches(string originalJson, string currentJson, string expectedPatch = null)
        {
            JsonPatchDocument patch = PatchHelper.GeneratePatch(
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
                    return false;
                }
            }

            return JToken.DeepEquals(JObject.Parse(currentJson), JObject.Parse(JsonConvert.SerializeObject(currentObj)));
        }

        [Fact]
        public void AppendixA01_AddingObjectMember()
        {
            string originalJson = "{\"foo\":\"bar\"}";
            string currentJson = "{\"baz\":\"qux\",\"foo\":\"bar\"}";
            string expectedPatch = "[{ \"op\": \"add\", \"path\": \"/baz\", \"value\": \"qux\" }]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA02_AddingArrayElement() {
            string originalJson = "   { \"foo\": [ \"bar\", \"baz\" ] }";
            string currentJson = "{ \"foo\": [ \"bar\", \"qux\", \"baz\" ] }";
            string expectedPatch = "[{ \"op\": \"add\", \"path\": \"/foo/1\", \"value\": \"qux\" }]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA03_RemovingObjectMember() {
            string originalJson = @"{
     ""baz"": ""qux"",
     ""foo"": ""bar""
   }";
            string currentJson = @"{ ""foo"": ""bar"" }";
            string expectedPatch = @"[
     { ""op"": ""remove"", ""path"": ""/baz"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA04_RemovingArrayElement() {
            string originalJson = @"{ ""foo"": [ ""bar"", ""qux"", ""baz"" ] }";
            string currentJson = @"{ ""foo"": [ ""bar"", ""baz"" ] }";
            string expectedPatch = @"[
     { ""op"": ""remove"", ""path"": ""/foo/1"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA05_ReplacingValue()
        {
            string originalJson = @"{
     ""baz"": ""qux"",
     ""foo"": ""bar""
   }";
            string currentJson = @"{
     ""baz"": ""boo"",
     ""foo"": ""bar""
   }";
            string expectedPatch = @"[
     { ""op"": ""replace"", ""path"": ""/baz"", ""value"": ""boo"" }
   ]
";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA06_MovingValue()
        {
            string originalJson = @"{
     ""foo"": {
       ""bar"": ""baz"",
       ""waldo"": ""fred""
     },
     ""qux"": {
       ""corge"": ""grault""
     }
   }";
            string currentJson = @"{
     ""foo"": {
       ""bar"": ""baz""
     },
     ""qux"": {
       ""corge"": ""grault"",
       ""thud"": ""fred""
     }
   }";
            string expectedPatch = @"[
     { ""op"": ""move"", ""from"": ""/foo/waldo"", ""path"": ""/qux/thud"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA07_MovingArrayElement()
        {
            string originalJson = @"{ ""foo"": [ ""all"", ""grass"", ""cows"", ""eat"" ] }";
            string currentJson = @"{ ""foo"": [ ""all"", ""cows"", ""eat"", ""grass"" ] }";
            string expectedPatch = @"[
     { ""op"": ""move"", ""from"": ""/foo/1"", ""path"": ""/foo/3"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact (Skip = "Not implemented")]
        public void AppendixA08_TestingValueSuccess()
        {
            string originalJson = @"{
     ""baz"": ""qux"",
     ""foo"": [ ""a"", 2, ""c"" ]
   }";
            string currentJson = @"";
            string expectedPatch = @"[
     { ""op"": ""test"", ""path"": ""/baz"", ""value"": ""qux"" },
     { ""op"": ""test"", ""path"": ""/foo/1"", ""value"": 2 }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact(Skip = "Not implemented")]
        public void AppendixA09_TestingValueError()
        {
            string originalJson = @"{ ""baz"": ""qux"" }";
            string currentJson = @"";
            string expectedPatch = @"[
     { ""op"": ""test"", ""path"": ""/baz"", ""value"": ""bar"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA10_AddingNestedMemberObject()
        {
            string originalJson = @"{ ""foo"": ""bar"" }";
            string currentJson = @"{
     ""foo"": ""bar"",
     ""child"": {
       ""grandchild"": {
       }
     }
   }";
            string expectedPatch = @"[
     { ""op"": ""add"", ""path"": ""/child"", ""value"": { ""grandchild"": { } } }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA11_IgnoringUnrecognizedElements()
        {
            string originalJson = @"{ ""foo"": ""bar"" }";
            string currentJson = @"{
     ""foo"": ""bar"",
     ""baz"": ""qux""
   }";
            string expectedPatch = @"[
     { ""op"": ""add"", ""path"": ""/baz"", ""value"": ""qux"", ""xyz"": 123 }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact(Skip = "Not implemented")]
        public void AppendixA12_AddingToNonexistentTarget()
        {
            string originalJson = @"{ ""foo"": ""bar"" }";
            string currentJson = @"";
            string expectedPatch = @"[
     { ""op"": ""add"", ""path"": ""/baz/bat"", ""value"": ""qux"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact(Skip = "Not implemented")]
        public void AppendixA13_InvalidJsonPatchDocument()
        {
            string originalJson = @"";
            string currentJson = @"";
            string expectedPatch = @"[
     { ""op"": ""add"", ""path"": ""/baz"", ""value"": ""qux"", ""op"": ""remove"" }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact(Skip = "Not implemented")]
        public void AppendixA14_TildeEscapeOrdering()
        {
            string originalJson = @"{
     ""/"": 9,
     ""~1"": 10
   }";
            string currentJson = @"{
     ""/"": 9,
     ""~1"": 10
   }";
            string expectedPatch = @"[
     {""op"": ""test"", ""path"": ""/~01"", ""value"": 10}
   ]
";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact(Skip = "Not implemented")]
        public void AppendixA15_ComparingStringsAndNumbers()
        {
            string originalJson = @"{
     ""/"": 9,
     ""~1"": 10
   }";
            string currentJson = @"";
            string expectedPatch = @"[
     {""op"": ""test"", ""path"": ""/~01"", ""value"": ""10""}
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA16_AddingAnArrayValue()
        {
            string originalJson = @"{ ""foo"": [""bar""] }";
            string currentJson = @"{ ""foo"": [""bar"", [""abc"", ""def""]] }";
            string expectedPatch = @"[
     { ""op"": ""add"", ""path"": ""/foo/-"", ""value"": [""abc"", ""def""] }
   ]";

            Assert.True(PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }
    }
}
