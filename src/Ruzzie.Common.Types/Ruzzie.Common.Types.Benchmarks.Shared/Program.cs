using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Ruzzie.Common.Types.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ResultCtorValueVsReference>();
        }
    }

    [ClrJob,  CoreJob, CoreRtJob]
    //[IterationTime(500)]
    //[MaxIterationCount(10000)]
    //[InvocationCount(1)]
    [RankColumn]
    public class ResultCtorValueVsReference
    {
        private int _randomInt;
        private readonly Random _random = new Random(14);
        private List<object> _randomObject;

        [GlobalSetup]
        public void Setup()
        {
            _randomInt = _random.Next(int.MinValue, int.MaxValue);
            _randomObject = new List<object>();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _randomInt = _random.Next(int.MinValue, int.MaxValue);
            _randomObject = new List<object>{new object()};
        }

        [Benchmark]
        public Result<string, int> ResultStruct_WithStringIntValue()
        {
            return new Result<string, int>(_randomInt).Select(Select, Select);
        }

        [Benchmark]
        public Result<string, List<object>> ResultStruct_WithStringObjectValue()
        {
            return new Result<string, List<object>>(_randomObject).Select(Select, Select);
        }

        [Benchmark]
        public IEither<string, int> Right_WithStringIntValue()
        {
            return new Right<string,int>(_randomInt).SelectBoth(Select, Select);
        }

        [Benchmark]
        public IEither<string, List<object>> Right_WithStringObjectValue()
        {
            return new Right<string, List<object>>(_randomObject).SelectBoth(Select, Select);
        }

        static T Select<T>(T value)
        {
            return value;
        }
    }
}
