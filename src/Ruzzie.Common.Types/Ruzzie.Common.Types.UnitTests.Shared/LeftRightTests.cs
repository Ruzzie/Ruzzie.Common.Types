using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class LeftRightTests
    {
        [Test]
        public void MatchRight()
        {
            IEitherReferenceType<string, int> sut = new Right<string, int>(42);
            var actual = sut.Match(s => s, i => i.ToString());
            Assert.AreEqual("42", actual);
        }

        [Test]
        public void MatchLeft()
        {
            IEitherReferenceType<string, int> sut = new Left<string, int>("foo");
            var actual = sut.Match(s => s, i => i.ToString());
            Assert.AreEqual("foo", actual);
        }

        [Test]
        public void OpInEquality()
        {
            object leftEither = LeftRightExtensions.AsLeft<int, string>(12);
            object rightEither = LeftRightExtensions.AsRight<int, string>("12");

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            (leftEither != rightEither).Should().BeTrue();
        }

        [Test]
        public void OpInEqualityReferenceType()
        {
            var a =  LeftRightExtensions.AsRight<int, List<string>>(new List<string>{"1","2"});
            var b =  LeftRightExtensions.AsRight<int, List<string>>(new List<string>{"1","2"});

            (a != b).Should().BeTrue();
        }

        [Test]
        public void OpEqualityReferenceType()
        {
            var list = new List<string>{"1","2"};
            var leftEither = LeftRightExtensions.AsRight<int, List<string>>(list);
            var rightEither = LeftRightExtensions.AsRight<int, List<string>>(list);

            (leftEither == rightEither).Should().BeTrue();
        }

        [Test]
        public void OpEquality()
        {
            var leftEither = LeftRightExtensions.AsRight<int, string>("12");
            var rightEither = LeftRightExtensions.AsRight<int, string>("12");

            (leftEither == rightEither).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfLeftValue()
        {
            var leftEither = LeftRightExtensions.AsLeft<int, string>(12);

            leftEither.GetHashCode().Should().Be(12.GetHashCode());
        }

        [Test]
        public void GetHashCodeReturnsHashCodeOfRightValue()
        {
            var rightEither = LeftRightExtensions.AsRight<int, string>("12");

            rightEither.GetHashCode().Should().Be("12".GetHashCode());
        }


        //From: https://blog.ploeh.dk/2019/01/07/either-bifunctor/
        private static T Identity<T>(T x)
        {
            return x;
        }
        public static IEnumerable<IEitherReferenceType<string, int>[]> BifunctorLawsData
        {
            get
            {
                yield return new[] {new Left<string, int>("foo")};
                yield return new[] {new Left<string, int>("bar")};
                yield return new[] {new Left<string, int>("baz")};
                yield return new[] {new Right<string, int>(42)};
                yield return new[] {new Right<string, int>(1337)};
                yield return new[] {new Right<string, int>(0)};
                #if CS8
                yield return new[] {new Left<string, int>(null!)};
                #else
                yield return new[] {new Left<string, int>(null)};
                #endif
            }
        }
            
        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void SelectLeftObeysFirstFunctorLaw(Either<string, int> e)
        //{
        //    e.Should().Be(e.SelectLeft(l => Identity(l)));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectLeftObeysFirstFunctorLaw(IEitherReferenceType<string, int> e)
        {
            e.Should().Be(e.MapLeft(l => Identity(l)));
        }

        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void SelectRightObeysFirstFunctorLaw(Either<string, int> e)
        //{
        //    e.Should().Be(e.SelectRight(l => Identity(l)));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectRightWithExtensionMethodObeysFirstFunctorLaw(IEitherReferenceType<string, int> e)
        {
            e.Should().Be(e.MapRight(l => Identity(l)));
        }

        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void SelectBothObeysFirstFunctorLaw(Either<string, int> e)
        //{
        //    e.Should().Be(e.SelectBoth(l => Identity(l), r => Identity(r)));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothOnExtensionMethodObeysFirstFunctorLaw(IEitherReferenceType<string, int> e)
        {
            e.Should().Be(e.Map(l => Identity(l), r => Identity(r)));
        }

        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void ConsistencyLawHolds(Either<string, int> e)
        //{
        //    bool f(string s) => string.IsNullOrWhiteSpace(s);
        //    DateTime g(int i) => new DateTime(i);
 
        //    Assert.AreEqual(e.SelectBoth(f, g), e.SelectRight(g).SelectLeft(f));
        //    Assert.AreEqual(
        //        e.SelectLeft(f).SelectRight(g),
        //        e.SelectRight(g).SelectLeft(f));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void ConsistencyLawHolds(IEitherReferenceType<string, int> e)
        {
            bool f(string s) => string.IsNullOrWhiteSpace(s);
            DateTime g(int i) => new DateTime(i);
 
            Assert.AreEqual(e.Map(f, g), e.MapRight(g).MapLeft(f));
            Assert.AreEqual(
                e.MapLeft(f).MapRight(g),
                e.MapRight(g).MapLeft(f));
        }

        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void SecondFunctorLawHoldsForSelectLeft(Either<string, int> e)
        //{
        //    bool f(int x) => x % 2 == 0;
        //    int g(string s) => s.Length;
 
        //    Assert.AreEqual(e.SelectLeft(x => f(g(x))), e.SelectLeft(g).SelectLeft(f));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectLeft(IEitherReferenceType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s?.Length ?? 0;
 
            Assert.AreEqual(e.MapLeft(x => f(g(x))), e.MapLeft(g).MapLeft(f));
        }

        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void SecondFunctorLawHoldsForSelectRight(Either<string, int> e)
        //{
        //    char f(bool b) => b ? 'T' : 'F';
        //    bool g(int i) => i % 2 == 0;
 
        //    Assert.AreEqual(e.SelectRight(x => f(g(x))), e.SelectRight(g).SelectRight(f));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SecondFunctorLawHoldsForSelectRight(IEitherReferenceType<string, int> e)
        {
            char f(bool b) => b ? 'T' : 'F';
            bool g(int i) => i % 2 == 0;
 
            Assert.AreEqual(e.MapRight(x => f(g(x))), e.MapRight(g).MapRight(f));
        }

        //[Theory, TestCaseSource(nameof(BifunctorLawsData))]
        //public void SelectBothCompositionLawHolds(Either<string, int> e)
        //{
        //    bool f(int x) => x % 2 == 0;
        //    int g(string s) => s.Length;
        //    char h(bool b) => b ? 'T' : 'F';
        //    bool i(int x) => x % 2 == 0;
 
        //    Assert.AreEqual(
        //        e.SelectBoth(x => f(g(x)), y => h(i(y))),
        //        e.SelectBoth(g, i).SelectBoth(f, h));
        //}

        [Theory, TestCaseSource(nameof(BifunctorLawsData))]
        public void SelectBothCompositionLawHolds(IEitherReferenceType<string, int> e)
        {
            bool f(int x) => x % 2 == 0;
            int g(string s) => s?.Length ?? 0;
            char h(bool b) => b ? 'T' : 'F';
            bool i(int x) => x % 2 == 0;
 
            Assert.AreEqual(
                e.Map(x => f(g(x)), y => h(i(y))),
                e.Map(g, i).Map(f, h));
        }
    }
}