using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using static Ruzzie.Common.Types.Result;
// ReSharper disable InconsistentNaming
namespace Ruzzie.Common.Types.UnitTests
{
    public class ResultTests
    {
        [Test]
        public void Cannot_InferTypesWhenTypesAreTheSame_Ok()
        {
            //so use named parameter to indicate result type
            var sutCtorOk = new Result<string, string>(ok: "OK!");
            var sutStaticOk = Result<string, string>.Ok("OK!");

            sutCtorOk.Should().BeEquivalentTo(sutStaticOk);
        }

        [Test]
        public void Cannot_InferTypesWhenTypesAreTheSame_Err()
        {
            //so use named parameter to indicate result type
            var sutCtorErr = new Result<string, string>(err: "ERR!");
            var sutStaticErr = Result<string, string>.Err("ERR!");

            sutCtorErr.Should().BeEquivalentTo(sutStaticErr);
        }

        [Test]
        public void MatchOk()
        {
            var sut = new Result<string, int>(42);
            var actual = sut.Match(s => s, i => i.ToString());
            Assert.AreEqual("42", actual);
        }

        [Test]
        public void MatchErr()
        {
            var sut = new Result<string, int>("foo");
            var actual = sut.Match(s => s, i => i.ToString());
            Assert.AreEqual("foo", actual);
        }

        [Test]
        public void DefaultIsErr()
        {
            Result<object, object> res = default;
            //Is is an error, however the Exception type is initialized to default
            //When we try to consume that err value it is null, this results in a runtime exception.
            Assert.Throws<PanicException<object>>(() => res.Err().Match(n => n.ToString(), s => s.ToString()));
        }

        [Test]
        public void MapErrExample()
        {
            string stringify(uint i) => $"error code: {i}";

            var x = Ok<uint,uint>(2);
            Assert.AreEqual(x.MapErr(stringify), Ok<string, uint>(2));
            
            x = Err<uint,uint>(13);
            Assert.AreEqual(x.MapErr(stringify), Err<string,uint>("error code: 13"));
        }

        [Test]
        public void AndExample_One()
        {
            var x = Ok<string, uint>(2);
            var y = Err<string, string>("late error");
            Assert.AreEqual(x.And(y), Err<string, string>("late error"));
        }

        [Test]
        public void AndExample_Two()
        {
            var x = Err<string, uint>("early error");
            var y = Ok<string, string>("foo");
            Assert.AreEqual(x.And(y), Err<string, string>("early error"));
        }

        [Test]
        public void AndExample_Three()
        {
            var x = Err<string, uint>("not a 2");
            var y = Err<string, string>("late error");
            Assert.AreEqual(x.And(y), Err<string, string>("not a 2"));
        }

        [Test]
        public void AndExample_Four()
        {
            var x = Ok<string, uint>(2);
            var y = Ok<string, string>("different result type");
            Assert.AreEqual(x.And(y), Ok<string, string>("different result type"));
        }

        [Test]
        public void AndThenExample()
        {
            Result<uint, uint> sq(uint x) => Ok<uint, uint>(x * x);
            Result<uint, uint> err(uint x) => Err<uint, uint>(x);

            Assert.AreEqual(Ok<uint, uint>(2).AndThen(sq).AndThen(sq), Ok<uint, uint>(16));
            Assert.AreEqual(Ok<uint, uint>(2).AndThen(sq).AndThen(err), Err<uint, uint>(4));
            Assert.AreEqual(Ok<uint, uint>(2).AndThen(err).AndThen(sq), Err<uint, uint>(2));
            Assert.AreEqual(Err<uint, uint>(3).AndThen(sq).AndThen(sq), Err<uint, uint>(3));
        }

        [Test]
        public void OrExample_One()
        {
            var x = Ok<string, uint>(2);
            var y = Err<string, uint>("late error");
            Assert.AreEqual(x.Or(y), Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_Two()
        {
            var x = Err<string, uint>("early error");
            var y = Ok<string, uint>(2);
            Assert.AreEqual(x.Or(y), Ok<string, uint>(2));
        }

        [Test]
        public void OrExample_Three()
        {
            var x = Err<string, uint>("not a 2");
            var y = Err<string, uint>("late error");
            Assert.AreEqual(x.Or(y), Err<string, uint>("late error"));
        }

        [Test]
        public void OrExample_Four()
        {
            var x = Ok<string, uint>(2);
            var y = Ok<string, uint>(100);
            Assert.AreEqual(x.Or(y), Ok<string, uint>(2));
        }

        [Test]
        public void OrElseExample()
        {
            Result<uint, uint> sq(uint x) => Ok<uint, uint>(x * x);
            Result<uint, uint> err(uint x) => Err<uint, uint>(x);

            Assert.AreEqual(Ok<uint, uint>(2).OrElse(sq).OrElse(sq), Ok<uint, uint>(2));
            Assert.AreEqual(Ok<uint, uint>(2).OrElse(err).OrElse(sq), Ok<uint, uint>(2));
            Assert.AreEqual(Err<uint, uint>(3).OrElse(sq).OrElse(err), Ok<uint, uint>(9));
            Assert.AreEqual(Err<uint, uint>(3).OrElse(err).OrElse(err), Err<uint, uint>(3));
        }

        [Test]
        public void UnwrapOrExample()
        {
            uint optb = 2;
            var x = Ok<string, uint>(9);
            Assert.AreEqual(x.UnwrapOr(optb), 9);

            var y = Err<string, uint>("error");
            Assert.AreEqual(y.UnwrapOr(optb), optb);
        }

        [Test]
        public void UnwrapOrElseExample()
        {
            int Count(string x) => x.Length;

            Assert.AreEqual(Ok<string,int>(2).UnwrapOrElse(Count), 2);
            Assert.AreEqual(Err<string,int>("foo").UnwrapOrElse(Count), 3);
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
            Action act = ()=> res.Match(e => e, o => o.ToString());

            act.Should().Throw<PanicException<string>>();
        }

        [Test]
        public void DefaultStateIsErr()
        {
            Result<string, int> res = default;

            res.IsErr.Should().BeTrue();
        }

        [Test]
        public void OpInEquality()
        {
            var err = Err<int, string>(12);
            var ok = Ok<int, string>("12");

            (err != ok).Should().BeTrue();
        }

        [Test]
        public void OpInEqualityReferenceType()
        {
            var left = new List<string> { "1", "2" }.AsOk<int, List<string>>();
            var right = new List<string> { "1", "2" }.AsOk<int, List<string>>();

            (left != right).Should().BeTrue();
        }

        [Test]
        public void OpEquality()
        {
            var left = "12".AsOk<int, string>();
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
            #if CS8
            var right = default(Result<int, string>)!;
            #else
            var right = default(Result<int, string>);
            #endif

            right.GetHashCode().Should().Be(0);
        }

        [Test]
        public void GetHashCodeReturnsZeroWhenIsErrValueIsNull()
        {
            #if CS8
            var result = Result<string, string>.Err(null!);
            #else
            var result = Result<string, string>.Err(null);
            #endif

            result.GetHashCode().Should().Be(0);
        }

        [Test]
        public void GetHashCodeReturnsZeroWhenIsOkValueIsNull()
        {
            #if CS8
            var result = Result<string, string>.Ok(null!);
            #else
            var result = Result<string, string>.Ok(null);
            #endif

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
            var list = new List<string> { "1", "2" };
            var left = list.AsOk<int, List<string>>();
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
                yield return new[] {new Result<string, int>("foo")};
                yield return new[] {new Result<string, int>("bar")};
                yield return new[] {new Result<string, int>("baz")};
                yield return new[] {new Result<string, int>(42)};
                yield return new[] {new Result<string, int>(1337)};
                yield return new[] {new Result<string, int>(0)};
                //yield return new[] {default(Result<string, int>)};
            }
        }
            
        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapErrObeysFirstFunctorLaw(Result<string, int> e)
        {
            e.Should().Be(e.MapErr(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapObeysFirstFunctorLaw(Result<string, int> e)
        {
            e.Should().Be(e.Map(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectObeysFirstFunctorLaw(Result<string, int> e)
        {
            e.Should().Be(e.Select(l => Identity(l), r => Identity(r)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(Result<string, int> e)
        {
            bool f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int i) => new DateTime(i);
 
            Assert.AreEqual(e.SelectBoth(f, g), e.SelectRight(g).SelectLeft(f));
            Assert.AreEqual(
                e.SelectLeft(f).SelectRight(g),
                e.SelectRight(g).SelectLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(IEitherValueType<string, int> e)
        {
            bool f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int i) => new DateTime(i);
 
            Assert.AreEqual(e.SelectBoth(f, g), e.SelectRight(g).SelectLeft(f));
            Assert.AreEqual(
                e.SelectLeft(f).SelectRight(g),
                e.SelectRight(g).SelectLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapErr(Result<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
 
            Assert.AreEqual(e.MapErr(x => f(g(x))), e.MapErr(g).MapErr(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectLeft(IEitherValueType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
 
            Assert.AreEqual(e.SelectLeft(x => f(g(x))), e.SelectLeft(g).SelectLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMap(Result<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.Map(x => f(g(x))), e.Map(g).Map(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectRight(IEitherValueType<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.SelectRight(x => f(g(x))), e.SelectRight(g).SelectRight(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothCompositionLawHolds(Result<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
            char h(bool b) => b ? 'T' : 'F';
            bool i(int x) => x % 2 == 0;
 
            Assert.AreEqual(
                e.SelectBoth(x => f(g(x)), y => h(i(y))),
                e.SelectBoth(g, i).SelectBoth(f, h));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothCompositionLawHolds(IEitherValueType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
            char h(bool b) => b ? 'T' : 'F';
            bool i(int x) => x % 2 == 0;
 
            Assert.AreEqual(
                e.SelectBoth(x => f(g(x)), y => h(i(y))),
                e.SelectBoth(g, i).SelectBoth(f, h));
        }
    }
}