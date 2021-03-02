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

        PatchTestHelper _testHelper = new PatchTestHelper();

        [Fact]
        public void AppendixA01_AddingObjectMember()
        {
            string originalJson = "{\"foo\":\"bar\"}";
            string currentJson = "{\"baz\":\"qux\",\"foo\":\"bar\"}";
            string expectedPatch = "[{ \"op\": \"add\", \"path\": \"/baz\", \"value\": \"qux\" }]";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA02_AddingArrayElement()
        {
            string originalJson = "   { \"foo\": [ \"bar\", \"baz\" ] }";
            string currentJson = "{ \"foo\": [ \"bar\", \"qux\", \"baz\" ] }";
            string expectedPatch = "[{ \"op\": \"add\", \"path\": \"/foo/1\", \"value\": \"qux\" }]";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA03_RemovingObjectMember()
        {
            string originalJson = @"{
     ""baz"": ""qux"",
     ""foo"": ""bar""
   }";
            string currentJson = @"{ ""foo"": ""bar"" }";
            string expectedPatch = @"[
     { ""op"": ""remove"", ""path"": ""/baz"" }
   ]";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact]
        public void AppendixA04_RemovingArrayElement()
        {
            string originalJson = @"{ ""foo"": [ ""bar"", ""qux"", ""baz"" ] }";
            string currentJson = @"{ ""foo"": [ ""bar"", ""baz"" ] }";
            string expectedPatch = @"[
     { ""op"": ""remove"", ""path"": ""/foo/1"" }
   ]";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
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

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact (Skip = "Not implemented")]
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

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }

        [Fact (Skip = "Not implemented")]
        public void AppendixA07_MovingArrayElement()
        {
            string originalJson = @"{ ""foo"": [ ""all"", ""grass"", ""cows"", ""eat"" ] }";
            string currentJson = @"{ ""foo"": [ ""all"", ""cows"", ""eat"", ""grass"" ] }";
            string expectedPatch = @"[
     { ""op"": ""move"", ""from"": ""/foo/1"", ""path"": ""/foo/3"" }
   ]";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
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

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }


        [Fact]
        public void AppendixA16_AddingAnArrayValue()
        {
            string originalJson = @"{ ""foo"": [""bar""] }";
            string currentJson = @"{ ""foo"": [""bar"", [""abc"", ""def""]] }";
            string expectedPatch = @"[
     { ""op"": ""add"", ""path"": ""/foo/-"", ""value"": [""abc"", ""def""] }
   ]";

            Assert.True(_testHelper.PatchRoundTripMatches(originalJson, currentJson, expectedPatch));
        }
    }
}
