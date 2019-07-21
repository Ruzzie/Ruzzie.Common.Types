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
        public void SelectLeftObeysFirstFunctorLaw(Either<string, int> e)
        {
            e.Should().Be(e.SelectLeft(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectLeftObeysFirstFunctorLaw(IEitherValueType<string, int> e)
        {
            e.Should().Be(e.SelectLeft(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectRightObeysFirstFunctorLaw(Either<string, int> e)
        {
            e.Should().Be(e.SelectRight(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectRightWithExtensionMethodObeysFirstFunctorLaw(IEitherValueType<string, int> e)
        {
            e.Should().Be(e.SelectRight(l => Identity(l)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothObeysFirstFunctorLaw(Either<string, int> e)
        {
            e.Should().Be(e.SelectBoth(l => Identity(l), r => Identity(r)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothOnExtensionMethodObeysFirstFunctorLaw(IEitherValueType<string, int> e)
        {
            e.Should().Be(e.SelectBoth(l => Identity(l), r => Identity(r)));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(Either<string, int> e)
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
        public void SecondFunctorLawHoldsForSelectLeft(Either<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
 
            Assert.AreEqual(e.SelectLeft(x => f(g(x))), e.SelectLeft(g).SelectLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectLeft(IEitherValueType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s.Length;
 
            Assert.AreEqual(e.SelectLeft(x => f(g(x))), e.SelectLeft(g).SelectLeft(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectRight(Either<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.SelectRight(x => f(g(x))), e.SelectRight(g).SelectRight(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectRight(IEitherValueType<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.SelectRight(x => f(g(x))), e.SelectRight(g).SelectRight(f));
        }

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothCompositionLawHolds(Either<string, int> e)
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