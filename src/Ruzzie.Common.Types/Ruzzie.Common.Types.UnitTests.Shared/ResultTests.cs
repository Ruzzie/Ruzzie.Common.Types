using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class ResultTests
    {
        [Test]
        public void IsOk_True()
        {
            var result = new Result<string, int>(ok: 42);

            Assert.AreEqual(result.IsOk(out int okValue, out _, 0, ""), true);
            Assert.AreEqual(okValue,                                    42);
        }

        [Test]
        public void IsError_True()
        {
            var result = new Result<string, int>(err: "error");

            Assert.AreEqual(result.IsErr(out _, out string errValue, 0, ""), true);
            Assert.AreEqual(errValue,                                        "error");
        }

        [Test]
        public void IsOk_PanicWhenNotInitialized()
        {
            var result = default(Result<string, int>);

            Assert.Throws<PanicException<string>>(() => result.IsOk(out _, out _, 0, ""));
        }


        [Test]
        public void JoinOk_Ok()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Ok<string, uint>(3);
            Assert.AreEqual(x.JoinOk(y), Result.Ok<string, (uint, uint)>((2, 3)));
        }

        [Test]
        public void JoinOk_FirstIsErr()
        {
            var x = Result.Err<string, uint>("first");
            var y = Result.Ok<string, uint>(3);
            Assert.AreEqual(x.JoinOk(y), Result.Err<string, (uint, uint)>("first"));
        }

        [Test]
        public void JoinOk_SecondIsErr()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Err<string, uint>("second");
            Assert.AreEqual(x.JoinOk(y), Result.Err<string, (uint, uint)>("second"));
        }

        [Test]
        public void AndJoinOk_Ok()
        {
            var x = Result.Ok<string, uint>(2);
            Assert.AreEqual(x.AndJoinOk(() => Result.Ok<string, uint>(3)), Result.Ok<string, (uint, uint)>((2, 3)));
        }

        [Test]
        public void AndJoinOk_FirstIsErr()
        {
            var x = Result.Err<string, uint>("first");
            Assert.AreEqual(x.AndJoinOk(() => Result.Ok<string, uint>(3)), Result.Err<string, (uint, uint)>("first"));
        }

        [Test]
        public void AndJoinOk_SecondIsErr()
        {
            var x = Result.Ok<string, uint>(2);
            Assert.AreEqual(x.AndJoinOk(() => Result.Err<string, uint>("second"))
                          , Result.Err<string, (uint, uint)>("second"));
        }

        [Test]
        public void MapOk2_Ok()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Ok<string, uint>(3);
            Assert.AreEqual(x.MapOk2(y, (a, b) => (a, b)), Result.Ok<string, (uint, uint)>((2, 3)));
        }

        [Test]
        public void MapOk2_FirstIsErr()
        {
            var x = Result.Err<string, uint>("first");
            var y = Result.Ok<string, uint>(3);
            Assert.AreEqual(x.MapOk2(y, (a, b) => (a, b)), Result.Err<string, (uint, uint)>("first"));
        }

        [Test]
        public void MapOk2_SecondIsErr()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Err<string, uint>("second");
            Assert.AreEqual(x.MapOk2(y, (a, b) => (a, b)), Result.Err<string, (uint, uint)>("second"));
        }

        [Test]
        public void GetValue_CSharp_Style_Branched_Continuation()
        {
            var list   = new List<string>();
            var result = Result<string, List<string>>.Ok(list);

            var (error, ok) = result.GetValue();

            if (ok.TryGetValue(out var okValue))
            {
                okValue.Should().Equal(list);
            }
            else if (error.TryGetValue(out var errorValue, "no-error"))
            {
                Assert.Fail(errorValue);
            }
            else
            {
                Assert.Fail(error.UnwrapOr("Unknown error state."));
            }
        }

        [Test]
        public void Pass_InParamFunc_ToMatch()
        {
            var result = new Result<int, double>(2.2);

            result.Match((in int _) => true, (in double _) => false).Should().BeFalse();
        }

        [Test]
        public void Cannot_InferTypesWhenTypesAreTheSame_Ok()
        {
            //so use named parameter to indicate result type
            var sutCtorOk   = new Result<string, string>(ok: "OK!");
            var sutStaticOk = Result<string, string>.Ok("OK!");

            sutCtorOk.Should().BeEquivalentTo(sutStaticOk);
        }

        [Test]
        public void Cannot_InferTypesWhenTypesAreTheSame_Err()
        {
            //so use named parameter to indicate result type
            var sutCtorErr   = new Result<string, string>(err: "ERR!");
            var sutStaticErr = Result<string, string>.Err("ERR!");

            sutCtorErr.Should().BeEquivalentTo(sutStaticErr);
        }

        [Test]
        public void MatchOk()
        {
            var result = new Result<string, int>(ok: 42);
            var actual = result.Match(onErr: s => s, onOk: i => i.ToString());
            Assert.AreEqual("42", actual);
        }

        [Test]
        public void MatchErr()
        {
            var result = new Result<string, int>(err: "foo");
            var actual = result.Match(onErr: s => s, onOk: i => i.ToString());
            Assert.AreEqual("foo", actual);
        }

        [Test]
        public void ForOk()
        {
            var sut = new Result<string, int>(42);
            sut.For(s => Assert.Fail(s), i => Assert.AreEqual(42, i));
        }

        [Test]
        public void ForErr()
        {
            var sut = new Result<string, int>("foo");
            sut.For(s => Assert.AreEqual("foo", s), _ => Assert.Fail("Err expected"));
        }

        [Test]
        public void DefaultIsErr()
        {
            Result<object, object> res = default;
            //Is is an error, however the Exception type is initialized to default
            //When we try to consume that err value it is null, this results in a runtime exception.
            Assert.Throws<PanicException<object>>(() => res.Err().Match(() => "Err", s => s.ToString()));
        }

        [Test]
        public void MapErrExample()
        {
            string stringify(uint i) => $"error code: {i}";

            var x = Result.Ok<uint, uint>(2);
            Assert.AreEqual(x.MapErr(stringify), Result.Ok<string, uint>(2));

            x = Result.Err<uint, uint>(13);
            Assert.AreEqual(x.MapErr(stringify), Result.Err<string, uint>("error code: 13"));
        }

        [Test]
        public void AndExample_One()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Err<string, string>("late error");
            Assert.AreEqual(x.And(y), Result.Err<string, string>("late error"));
        }

        [Test]
        public void AndExample_Two()
        {
            var x = Result.Err<string, uint>("early error");
            var y = Result.Ok<string, string>("foo");
            Assert.AreEqual(x.And(y), Result.Err<string, string>("early error"));
        }

        [Test]
        public void AndExample_Three()
        {
            var x = Result.Err<string, uint>("not a 2");
            var y = Result.Err<string, string>("late error");
            Assert.AreEqual(x.And(y), Result.Err<string, string>("not a 2"));
        }

        [Test]
        public void AndExample_Four()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Ok<string, string>("different result type");
            Assert.AreEqual(x.And(y), Result.Ok<string, string>("different result type"));
        }

        [Test]
        public void AndThenExample()
        {
            Result<uint, uint> sq(uint  x) => Result.Ok<uint, uint>(x * x);
            Result<uint, uint> err(uint x) => Result.Err<uint, uint>(x);

            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(sq).AndThen(sq),  Result.Ok<uint, uint>(16));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(sq).AndThen(err), Result.Err<uint, uint>(4));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(err).AndThen(sq), Result.Err<uint, uint>(2));
            Assert.AreEqual(Result.Err<uint, uint>(3).AndThen(sq).AndThen(sq), Result.Err<uint, uint>(3));
        }


        [Test]
        public unsafe void AndThenPassDelegatePointerExample()
        {
            static Result<uint, uint> sq(uint  x) => Result.Ok<uint, uint>(x * x);
            static Result<uint, uint> err(uint x) => Result.Err<uint, uint>(x);

            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(&sq).AndThen(&sq),  Result.Ok<uint, uint>(16));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(&sq).AndThen(&err), Result.Err<uint, uint>(4));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(&err).AndThen(&sq), Result.Err<uint, uint>(2));
            Assert.AreEqual(Result.Err<uint, uint>(3).AndThen(&sq).AndThen(&sq), Result.Err<uint, uint>(3));
        }

        [Test]
        public void AndThen_In_Example()
        {
            Result<uint, uint> sq(in  uint x) => Result.Ok<uint, uint>(x * x);
            Result<uint, uint> err(in uint x) => Result.Err<uint, uint>(x);

            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(sq).AndThen(sq),  Result.Ok<uint, uint>(16));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(sq).AndThen(err), Result.Err<uint, uint>(4));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(err).AndThen(sq), Result.Err<uint, uint>(2));
            Assert.AreEqual(Result.Err<uint, uint>(3).AndThen(sq).AndThen(sq), Result.Err<uint, uint>(3));
        }

        [Test]
        public unsafe void AndThen_In_PassDelegatePointerExample()
        {
            static Result<uint, uint> sq(in  uint x) => Result.Ok<uint, uint>(x * x);
            static Result<uint, uint> err(in uint x) => Result.Err<uint, uint>(x);

            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(&sq).AndThen(&sq),  Result.Ok<uint, uint>(16));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(&sq).AndThen(&err), Result.Err<uint, uint>(4));
            Assert.AreEqual(Result.Ok<uint, uint>(2).AndThen(&err).AndThen(&sq), Result.Err<uint, uint>(2));
            Assert.AreEqual(Result.Err<uint, uint>(3).AndThen(&sq).AndThen(&sq), Result.Err<uint, uint>(3));
        }

        [Test]
        public void OrExample_One()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Err<string, uint>("late error");
            Assert.AreEqual(x.Or(y), Result.Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_In_One()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Err<string, uint>("late error");
            Assert.AreEqual(x.Or(in y), Result.Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_Two()
        {
            var x = Result.Err<string, uint>("early error");
            var y = Result.Ok<string, uint>(2);
            Assert.AreEqual(x.Or(y), Result.Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_In_Two()
        {
            var x = Result.Err<string, uint>("early error");
            var y = Result.Ok<string, uint>(2);
            Assert.AreEqual(x.Or(in y), Result.Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_Three()
        {
            var x = Result.Err<string, uint>("not a 2");
            var y = Result.Err<string, uint>("late error");
            Assert.AreEqual(x.Or(y), Result.Err<string, uint>("late error"));
        }

        [Test]
        public void OrExample_In_Three()
        {
            var x = Result.Err<string, uint>("not a 2");
            var y = Result.Err<string, uint>("late error");
            Assert.AreEqual(x.Or(in y), Result.Err<string, uint>("late error"));
        }

        [Test]
        public void OrExample_Four()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Ok<string, uint>(100);
            Assert.AreEqual(x.Or(y), Result.Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_In_Four()
        {
            var x = Result.Ok<string, uint>(2);
            var y = Result.Ok<string, uint>(100);
            Assert.AreEqual(x.Or(in y), Result.Ok<string, uint>(2));
        }

        [Test]
        public void OrElseExample()
        {
            Result<uint, uint> sq(uint  x) => Result.Ok<uint, uint>(x * x);
            Result<uint, uint> err(uint x) => Result.Err<uint, uint>(x);

            Assert.AreEqual(Result.Ok<uint, uint>(2).OrElse(sq).OrElse(sq),    Result.Ok<uint, uint>(2));
            Assert.AreEqual(Result.Ok<uint, uint>(2).OrElse(err).OrElse(sq),   Result.Ok<uint, uint>(2));
            Assert.AreEqual(Result.Err<uint, uint>(3).OrElse(sq).OrElse(err),  Result.Ok<uint, uint>(9));
            Assert.AreEqual(Result.Err<uint, uint>(3).OrElse(err).OrElse(err), Result.Err<uint, uint>(3));
        }

        [Test]
        public void UnwrapOrExample()
        {
            uint optb = 2;
            var  x    = Result.Ok<string, uint>(9);
            Assert.AreEqual(x.UnwrapOr(optb), 9);

            var y = Result.Err<string, uint>("error");
            Assert.AreEqual(y.UnwrapOr(optb), optb);
        }

        [Test]
        public void UnwrapOrElseExample()
        {
            int Count(string x) => x.Length;

            Assert.AreEqual(Result.Ok<string, int>(2).UnwrapOrElse(Count),      2);
            Assert.AreEqual(Result.Err<string, int>("foo").UnwrapOrElse(Count), 3);
        }

        [Test]
        public void ImplicitOperator_ToOptionTuple_Ok()
        {
            var res = new Result<string, int>(1337);

            //Act
            (Option<string> err, Option<int> ok) opts = res;

            //Assert
            opts.ok.IsSome().Should().BeTrue();
        }

        [Test]
        public void ImplicitOperator_ToOptionTuple_Err()
        {
            var res = new Result<string, int>("error");

            //Act
            (Option<string> err, Option<int> ok) opts = res;

            //Assert
            opts.err.IsSome().Should().BeTrue();
        }

        [Test]
        public void ImplicitOperator_ToOption_Ok()
        {
            var res = new Result<string, int>(1337);

            //Act
            Option<int> ok = res;

            //Assert
            ok.IsSome().Should().BeTrue();
        }

        [Test]
        public void ImplicitOperator_ToOption_Err()
        {
            var res = new Result<string, int>("error");

            //Act
            Option<string> err = res;

            //Assert
            err.IsSome().Should().BeTrue();
        }

        [Test]
        public void ThrowsPanicExceptionWhenTryingToObtainErrValueOnDefaultResult()
        {
            Result<string, int> res = default;
            Action              act = () => res.Match(e => e, o => o.ToString());

            act.Should().Throw<PanicException<string>>();
        }

        [Test]
        public void DefaultStateIsErr()
        {
            Result<string, int> res = default;

            res.IsErr().Should().BeTrue();
        }

        [Test]
        public void OpInEquality()
        {
            var err = Result.Err<int, string>(12);
            var ok  = Result.Ok<int, string>("12");

            (err != ok).Should().BeTrue();
        }

        [Test]
        public void OpInEqualityReferenceType()
        {
            var left  = new List<string> { "1", "2" }.AsOk<int, List<string>>();
            var right = new List<string> { "1", "2" }.AsOk<int, List<string>>();

            (left != right).Should().BeTrue();
        }

        [Test]
        public void OpEquality()
        {
            var left  = "12".AsOk<int, string>();
            var right = "12".AsOk<int, string>();

            (left == right).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfLeftValue()
        {
            var left = 12.AsErr<int, string>();

            left.GetHashCode().Should().Be(12.GetHashCode());
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfRightValue()
        {
            var right = "12".AsOk<int, string>();

            right.GetHashCode().Should().Be("12".GetHashCode());
        }

        [Test]
        public void GetHashCodeReturnsZeroWhenNotInitialized()
        {
            var result = default(Result<int, string>);

            result.GetHashCode().Should().Be(0);
        }

        [Test]
        public void GetHashCodeReturnsZeroWhenIsErrValueIsNull()
        {
            var result = Result<string, string>.Err(null!);

            result.GetHashCode().Should().Be(0);
        }

        [Test]
        public void GetHashCodeReturnsZeroWhenIsOkValueIsNull()
        {
            var result = Result<string, string>.Ok(null!);

            result.GetHashCode().Should().Be(0);
        }

        [Test]
        public void EqualsIsTrueWhenBothAreNotInitialized()
        {
            var a = default(Result<string, int>);
            var b = default(Result<string, int>);

            a.Equals(b).Should().BeTrue();
        }

        [Test]
        public void OpEqualityReferenceType()
        {
            var list  = new List<string> { "1", "2" };
            var left  = list.AsOk<int, List<string>>();
            var right = list.AsOk<int, List<string>>();

            (left == right).Should().BeTrue();
        }

        //From: https://blog.ploeh.dk/2019/01/07/either-bifunctor/
        private static T Identity<T>(T x)
        {
            return x;
        }

        public static IEnumerable<Result<string, int>[]> BifunctorLawsData
        {
            get
            {
                yield return new[] { new Result<string, int>("foo") };
                yield return new[] { new Result<string, int>("bar") };
                yield return new[] { new Result<string, int>("baz") };
                yield return new[] { new Result<string, int>(42) };
                yield return new[] { new Result<string, int>(1337) };
                yield return new[] { new Result<string, int>(0) };
                //yield return new[] {default(Result<string, int>)};
            }
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapErrObeysFirstFunctorLaw(Result<string, int> e)
        {
            e.Should().Be(e.MapErr(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapOkObeysFirstFunctorLaw(Result<string, int> e)
        {
            e.Should().Be(e.MapOk(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectObeysFirstFunctorLaw(Result<string, int> e)
        {
            e.Should().Be(e.Map(l => Identity(l), r => Identity(r)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(Result<string, int> e)
        {
            bool     f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int    i) => new DateTime(i);

            Assert.AreEqual(e.Map(f, g), e.MapOk(g).MapErr(f));
            Assert.AreEqual(
                            e.MapLeft(f).MapRight(g)
                          , e.MapRight(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(IEitherValueType<string, int> e)
        {
            bool     f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int    i) => new DateTime(i);

            Assert.AreEqual(e.Map(f, g), e.MapRight(g).MapLeft(f));
            Assert.AreEqual(
                            e.MapLeft(f).MapRight(g)
                          , e.MapRight(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapErr(Result<string, int> e)
        {
            bool f(int    x) => x % 2 == 0;
            int  g(string s) => s.Length;

            Assert.AreEqual(e.MapErr(x => f(g(x))), e.MapErr(g).MapErr(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapLeft(IEitherValueType<string, int> e)
        {
            bool f(int    x) => x % 2 == 0;
            int  g(string s) => s.Length;

            Assert.AreEqual(e.MapLeft(x => f(g(x))), e.MapLeft(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapOk(Result<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int  i) => i % 2 == 0;

            Assert.AreEqual(e.MapOk(x => f(g(x))), e.MapOk(g).MapOk(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapRight(IEitherValueType<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int  i) => i % 2 == 0;

            Assert.AreEqual(e.MapRight(x => f(g(x))), e.MapRight(g).MapRight(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapCompositionLawHolds(Result<string, int> e)
        {
            bool f(int    x) => x % 2 == 0;
            int  g(string s) => s.Length;
            char h(bool   b) => b ? 'T' : 'F';
            bool i(int    x) => x % 2 == 0;

            Assert.AreEqual(
                            e.Map(x => f(g(x)), y => h(i(y)))
                          , e.Map(g,            i).Map(f, h));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapCompositionLawHolds(IEitherValueType<string, int> e)
        {
            bool f(int    x) => x % 2 == 0;
            int  g(string s) => s.Length;
            char h(bool   b) => b ? 'T' : 'F';
            bool i(int    x) => x % 2 == 0;

            Assert.AreEqual(
                            e.Map(x => f(g(x)), y => h(i(y)))
                          , e.Map(g,            i).Map(f, h));
        }
    }
}