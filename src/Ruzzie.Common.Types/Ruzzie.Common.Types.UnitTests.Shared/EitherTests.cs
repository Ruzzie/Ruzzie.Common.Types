using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class EitherTests
    {
        [Test]
        public void ThrowsEitherIsDefaultValueWhenMatchIsCalledOnUninitializedEither()
        {
            Either<string, int> either = default;

            Action act = () => either.Match(l => l, r => r.ToString());

            act.Should().Throw<EitherIsDefaultValueException>();
        }

        [Test]
        public void MatchRight()
        {
            IEitherValueType<string, int> sut = new Either<string, int>(42);
            var actual = sut.Match(s => s, i => i.ToString());
            Assert.AreEqual("42", actual);
        }

        [Test]
        public void MatchLeft()
        {
            IEitherValueType<string, int> sut = new Either<string, int>("foo");
            var actual = sut.Match(s => s, i => i.ToString());
            Assert.AreEqual("foo", actual);
        }

        [Test]
        public void MatchOptionDefaultReturnsNone()
        {
            var either = default(Either<string, int>);

            Option<string> option =
                either.Match(l => Option<string>.Some(l), r => Option<string>.Some(r.ToString()));

            option.IsNone().Should().BeTrue();
        }

        [Test]
        public void OpInEquality()
        {
            var leftEither = EitherExtensions.AsLeft<int, string>(12);
            var rightEither = EitherExtensions.AsRight<int, string>("12");

            (leftEither != rightEither).Should().BeTrue();
        }

        [Test]
        public void OpInEqualityReferenceType()
        {
            var leftEither =  EitherExtensions.AsRight<int, List<string>>(new List<string>{"1","2"});
            var rightEither =  EitherExtensions.AsRight<int, List<string>>(new List<string>{"1","2"});

            (leftEither != rightEither).Should().BeTrue();
        }

        [Test]
        public void OpEquality()
        {
            var leftEither = EitherExtensions.AsRight<int, string>("12");
            var rightEither = EitherExtensions.AsRight<int, string>("12");

            (leftEither == rightEither).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfLeftValue()
        {
            var leftEither = EitherExtensions.AsLeft<int, string>(12);

            leftEither.GetHashCode().Should().Be(12.GetHashCode());
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfRightValue()
        {
            var rightEither = EitherExtensions.AsRight<int, string>("12");

            rightEither.GetHashCode().Should().Be("12".GetHashCode());
        }

        [Test]
        public void OpEqualityReferenceType()
        {
            var list = new List<string>{"1","2"};
            var leftEither = EitherExtensions.AsRight<int, List<string>>(list);
            var rightEither = EitherExtensions.AsRight<int, List<string>>(list);

            (leftEither == rightEither).Should().BeTrue();
        }

        //From: https://blog.ploeh.dk/2019/01/07/either-bifunctor/
        private static T Identity<T>(T x)
        {
            return x;
        }
        public static IEnumerable<Either<string, int>[]> BifunctorLawsData
        {
            get
            {
                yield return new[] {new Either<string, int>("foo")};
                yield return new[] {new Either<string, int>("bar")};
                yield return new[] {new Either<string, int>("baz")};
                yield return new[] {new Either<string, int>(42)};
                yield return new[] {new Either<string, int>(1337)};
                yield return new[] {new Either<string, int>(0)};
            }
        }
            
        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapLeftObeysFirstFunctorLaw(Either<string, int> e)
        {
            e.Should().Be(e.MapLeft(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapLeftObeysFirstFunctorLaw(IEitherValueType<string, int> e)
        {
            e.Should().Be(e.MapLeft(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapRightObeysFirstFunctorLaw(Either<string, int> e)
        {
            e.Should().Be(e.MapRight(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapRightWithExtensionMethodObeysFirstFunctorLaw(IEitherValueType<string, int> e)
        {
            e.Should().Be(e.MapRight(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapObeysFirstFunctorLaw(Either<string, int> e)
        {
            e.Should().Be(e.Map(l => Identity(l), r => Identity(r)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapOnExtensionMethodObeysFirstFunctorLaw(IEitherValueType<string, int> e)
        {
            e.Should().Be(e.Map(l => Identity(l), r => Identity(r)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(Either<string, int> e)
        {
            bool f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int i) => new DateTime(i);
 
            Assert.AreEqual(e.Map(f, g), e.MapRight(g).MapLeft(f));
            Assert.AreEqual(
                e.MapLeft(f).MapRight(g),
                e.MapRight(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(IEitherValueType<string, int> e)
        {
            bool f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int i) => new DateTime(i);
 
            Assert.AreEqual(e.Map(f, g), e.MapRight(g).MapLeft(f));
            Assert.AreEqual(
                e.MapLeft(f).MapRight(g),
                e.MapRight(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapLeft(Either<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
 
            Assert.AreEqual(e.MapLeft(x => f(g(x))), e.MapLeft(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapLeft(IEitherValueType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
 
            Assert.AreEqual(e.MapLeft(x => f(g(x))), e.MapLeft(g).MapLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapRight(Either<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.MapRight(x => f(g(x))), e.MapRight(g).MapRight(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForMapRight(IEitherValueType<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.MapRight(x => f(g(x))), e.MapRight(g).MapRight(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapCompositionLawHolds(Either<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
            char h(bool b) => b ? 'T' : 'F';
            bool i(int x) => x % 2 == 0;
 
            Assert.AreEqual(
                e.Map(x => f(g(x)), y => h(i(y))),
                e.Map(g, i).Map(f, h));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void MapCompositionLawHolds(IEitherValueType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
            char h(bool b) => b ? 'T' : 'F';
            bool i(int x) => x % 2 == 0;
 
            Assert.AreEqual(
                e.Map(x => f(g(x)), y => h(i(y))),
                e.Map(g, i).Map(f, h));
        }
    }
}