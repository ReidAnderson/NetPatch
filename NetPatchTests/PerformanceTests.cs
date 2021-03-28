using System;
using Xunit;
using NetPatch;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.JsonPatch;
using System.Dynamic;
using System.Collections.Generic;
using NetPatchTests.TestData;
using AutoFixture;

namespace NetPatchTests
{
    public class PerformanceTests
    {
        Fixture _fixture = new Fixture();

        public TimeSpan Time(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        [Fact]
        public void OneMillionSmallObjects()
        {

            List<string> sampleList = new List<string>();
            int itemCount = 10000;

            for (int i = 0; i < itemCount; i++)
            {
                sampleList.Add(JsonSerializer.Serialize(_fixture.Create<SampleSmallClass>()));
            }

            long msElapsed = Time(() =>
            {
                for (int i = 1; i < itemCount; i++)
                {
                    JsonPatchDocument patch = JsonPatch.GetPatch(sampleList[i - 1], sampleList[i]);
                }
            }).Milliseconds;

            // We should always, always be able to do 10,000 in under a second
            Assert.True(1000 > msElapsed);
        }
    }
}