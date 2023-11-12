using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Ruzzie.Common.Types.Benchmarks;

internal class Program
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}

[MemoryDiagnoser]
[RankColumn]
[IterationCount(5000)]
[InvocationCount(4)]
public class ResultCtorValueVsReference
{
    private readonly Random       _random = new Random(14);
    private          int          _randomInt;
    private          List<object> _randomObject;

    [GlobalSetup]
    public void Setup()
    {
        _randomInt    = _random.Next(int.MinValue, int.MaxValue);
        _randomObject = new List<object>();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _randomInt    = _random.Next(int.MinValue, int.MaxValue);
        _randomObject = new List<object> { new object() };
    }

    [Benchmark]
    public Result<string, int> ResultStruct_WithStringIntValue()
    {
        return new Result<string, int>(_randomInt).Map(Select, Select);
    }

    [Benchmark]
    public Result<string, List<object>> ResultStruct_WithStringObjectValue()
    {
        return new Result<string, List<object>>(_randomObject).Map(Select, Select);
    }

    private static T Select<T>(T value)
    {
        return value;
    }
}

[MemoryDiagnoser]
[RankColumn]
[IterationCount(2000)]
[InvocationCount(4)]
public class IsPatternMatchVsGetType
{
    private readonly Random               _random = new Random(14);
    private          Option<int>          _optionValType;
    private          Option<List<string>> _optionWithRefType;

    private object _optionWithRefTypeBoxed;
    private object _optionWithValTypeBoxed;
    private int    _randomIntA;
    private int    _randomIntB;


    [GlobalSetup]
    public void Setup()
    {
        _optionWithRefType = Option<List<string>>.None;
        _optionValType     = Option<int>.None;

        _randomIntA = _random.Next(int.MinValue, int.MaxValue);

        _optionWithRefTypeBoxed = _optionWithRefType;
        _optionWithValTypeBoxed = _optionValType;
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _optionWithRefType = Option<List<string>>.None;
        _optionValType     = Option<int>.None;

        _randomIntA = _random.Next(int.MinValue, int.MaxValue);
        _randomIntB = _random.Next(int.MinValue, int.MaxValue);

        _optionWithRefTypeBoxed = _optionWithRefType;
        _optionWithValTypeBoxed = _optionValType;
    }

    [Benchmark]
    public bool GetType_Cast_TypesAreNotEqual()
    {
        return Equals_GetType_Cast<Option<int>>(_optionWithRefTypeBoxed, _optionWithValTypeBoxed);
    }

    [Benchmark]
    public bool GetType_Cast_TypesAreEqual()
    {
        return Equals_GetType_Cast<Option<int>>(_optionWithValTypeBoxed, _optionWithValTypeBoxed);
    }

    [Benchmark]
    public bool Is_PatternMatch_TypesAreNotEqual()
    {
        return Equals_Is_PatternMatch<Option<int>>(_optionWithRefTypeBoxed, _optionWithValTypeBoxed);
    }

    [Benchmark]
    public bool Is_PatternMatch_TypesAreEqual()
    {
        return Equals_Is_PatternMatch<Option<int>>(_optionWithValTypeBoxed, _optionWithValTypeBoxed);
    }

    public bool EqualsStub<T>(T a, T b)
    {
        return _randomIntA == _randomIntB;
    }

    public bool Equals_GetType_Cast<T>(object a, object b)
    {
        if (a.GetType() == b.GetType())
        {
            return EqualsStub((T)a, (T)b);
        }

        return false;
    }

    public bool Equals_Is_PatternMatch<T>(object a, object b)
    {
        if (a is T aT && b is T bT)
        {
            return EqualsStub(aT, bT);
        }

        return false;
    }
}