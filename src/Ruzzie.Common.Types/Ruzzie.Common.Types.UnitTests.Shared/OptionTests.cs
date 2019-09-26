using System;
using FluentAssertions;
using NUnit.Framework;

namespace Ruzzie.Common.Types.UnitTests
{
    [TestFixture]
    public class OptionTests
    {
        [Test]
        public void HasSomeSmokeTest()
        {
            //Arrange
            var option = Option<string>.Some("Hello!");
            //Act
            string value = option.Match(x => "No value!", val => val);
            //Assert
            value.Should().Be("Hello!");
        }

        [Test]
        public void HasNoneSmokeTest()
        {
            //Arrange
            var option = Option<string>.None;
            //Act
            string value = option.Match( x => "No value!",val => val);
            //Assert
            value.Should().Be("No value!");
        }

        [Test]
        public void ForExample()
        {
            var x = Option.Some("cars");
            x.For(() => Assert.Fail("should be some."), val => Assert.AreEqual("cars", val));
        }

        [Test]
        public void MapOrExample()
        {
            var x = Option.Some("foo");
            Assert.AreEqual(x.MapOr(42, v=> v.Length), 3);

            var y = Option.None<string>();
            Assert.AreEqual(y.MapOr(42, v=> v.Length), 42);
        }

        [Test]
        public void UnwrapOrExample()
        {
            Assert.AreEqual(Option.Some("car").UnwrapOr("bike"), "car");
            Assert.AreEqual(Option.None<string>().UnwrapOr("bike"), "bike");
        }

        [Test]
        public void UnwrapOrElseExample()
        {
            var k = 10;
            Assert.AreEqual(Option.Some(4).UnwrapOrElse(() => 2 * k), 4);
            Assert.AreEqual(Option.None<int>().UnwrapOrElse(() => 2 * k), 20);
        }

        [Test]
        public void NoneEqualsNone()
        {
            var optionA = Option<string>.None;
            var optionB = new Option<string>();

            //Assert
            optionA.Equals(optionB).Should().BeTrue();
        }

        [Test]
        public void EqualsNullAsOptionTypeTest()
        {
#if !CS8
            Option<int> optionA = null;
            #else
             Option<int> optionA = null!;
#endif
           
            Option<int> optionB = 23;

            optionA.Equals(optionB).Should().BeFalse();
            optionB.Equals(optionA).Should().BeFalse();

            optionA.Equals(optionA).Should().BeTrue();
        }

        [Test]
        public void OpEqualityTestSome()
        {
            var optionA = Option<string>.Some("A");
            var optionB = new Option<string>("A");

            (optionA == optionB).Should().BeTrue();
        }

        [Test]
        public void OpEqualityTestNone()
        {
            var optionA = Option<string>.None;
            var optionB = new Option<string>();

            (optionA == optionB).Should().BeTrue();
        }

        [Test]
        public void OpInEqualityTestSomeNone()
        {
            var optionA = Option<string>.None;
            var optionB = new Option<string>("A");

            (optionA != optionB).Should().BeTrue();
        }

        [Test]
        public void OpInEqualityTestSomeSome()
        {
            var optionA = Option<string>.Some("B");
            var optionB = new Option<string>("A");

            (optionA != optionB).Should().BeTrue();
        }

        [Test]
        public void GetHashCodeForNoneIsZero()
        {
            Option<string>.None.GetHashCode().Should().Be(0);
        }

        [Test]
        public void GetHashCodeForSomeIsEqualToSomeValue()
        {
            Option<string>.Some("A").GetHashCode().Should().Be("A".GetHashCode());
        }

        [Test]
        public void CheckIfValueReferencesAreOk()
        {
            var valueA = 42;
            ref int valRef = ref valueA;
            var someNumber = new Option<int>(valRef);
            valueA = 33;

            someNumber.Match(() => false, x => x == 42).Should().BeTrue();
        }

        [Test]
        public void SomeEqualsSome()
        {
            var optionA = "Hello!".AsSome();
            var optionB = Option<string>.Some("Hello!");

            //Assert
            optionA.Should().Be(optionB);
        }

        [Test]
        public void SomeNotEqualsSome()
        {
            Option<string> optionA = "Hello!";
            var optionB = Option<string>.Some("Helloooooo!");

            //Assert
            optionA.Should().NotBe(optionB);
        }

        [Test]
        public void SomeNotEqualsNone()
        {
            var optionA = Option<string>.Some("Hello!");
            var optionB = Option<string>.None;

            //Assert
            optionA.Should().NotBe(optionB);
        }


        [Theory]
        [TestCase("")]
        [TestCase("foo")]
        [TestCase("bar")]
        [TestCase("corge")]
        [TestCase("antidisestablishmentarianism")]
        public void PopulatedOptionObeysFirstFunctorLaw(string value)
        {
            //Source: https://blog.ploeh.dk/2018/03/26/the-maybe-functor/
            Func<string, string> id = x => x;
            var m = new Option<string>(value);
            
            Assert.AreEqual(m, m.Map(id));
        }

        [Test]
        public void EmptyOptionObeysFirstFunctorLaw()
        {
            Func<string, string> id = x => x;
            var m = new Option<string>();
 
            Assert.AreEqual(m, m.Map(id));
        }

        [Theory]
        [TestCase("", true)]
        [TestCase("foo", false)]
        [TestCase("bar", false)]
        [TestCase("corge", false)]
        [TestCase("antidisestablishmentarianism", true)]
        public void PopulatedOptionObeysSecondFunctorLaw(string value, bool expected)
        {
            //Source: https://blog.ploeh.dk/2018/03/26/the-maybe-functor/
            Func<string, int> g = s => s.Length;
            Func<int, bool>   f = i => i % 2 == 0;
            var m = new Option<string>(value);
 
            Assert.AreEqual(m.Map(g).Map(f), m.Map(s => f(g(s))));
        }

        [Test]            
        public void EmptyOptionObeysSecondFunctorLaw()
        {
            //Source: https://blog.ploeh.dk/2018/03/26/the-maybe-functor/
            Func<string, int> g = s => s.Length;
            Func<int, bool>   f = i => i % 2 == 0;
            var m = new Option<string>();
 
            Assert.AreEqual(m.Map(g).Map(f), m.Map(s => f(g(s))));
        }
    }
}