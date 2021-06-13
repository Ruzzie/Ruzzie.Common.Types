using FluentAssertions;
using NUnit.Framework;

namespace Ruzzie.Common.Types.UnitTests.Serialization
{
    [TestFixture, Ignore("Proper serialization feature is not implemented yes")]
    public class OptionSerializationTests
    {
        [Test]
        public void SerializeToDefaultBinaryFormat_SimpleType_Some()
        {
            var option = Option<string>.Some("Simple string option with some value.");

            option.AssertDefaultBinarySerializationSuccessForValueType();
        }

        [Test]
        public void SerializeToDefaultBinaryFormat_SimpleType_None()
        {
            var option = Option<string>.None;
            option.AssertDefaultBinarySerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithDataContractSerializer_SimpleType_Some()
        {
            var option = Option<string>.Some("Simple string option with some value.");
            option.AssertDefaultDataContractSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithDataContractSerializer_SimpleType_None()
        {
            var option = Option<string>.None;

            option.AssertDefaultDataContractSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_Some()
        {
            var option = Option<string>.Some("Simple string option with some value.");

            option.AssertNewtonsoftJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_None()
        {
            var option = Option<string>.None;

            option.AssertNewtonsoftJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_Some_Nested()
        {
            var option = Option<Option<string>>.Some(Option<string>.Some("Nested value!"));

            var deserializedOption = option.AssertNewtonsoftJsonSerializationSuccessForValueType();

            deserializedOption.UnwrapOr(Option<string>.None).UnwrapOr(string.Empty).Should().Be("Nested value!");
        }


        [Test]
        public void SerializeWithSystemTextJsonSerializer_SimpleType_Some()
        {
            var option = Option<string>.Some("Simple string option with some value.");

            option.AssertSystemTextJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithSystemTextJsonSerializer_SimpleType_None()
        {
            var option = Option<string>.None;

            option.AssertSystemTextJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithSystemTextJsonSerializer_SimpleType_Some_Nested()
        {
            var option = Option<Option<string>>.Some(Option<string>.Some("Nested value!"));

            var deserializedOption = option.AssertSystemTextJsonSerializationSuccessForValueType();

            deserializedOption.UnwrapOr(Option<string>.None).UnwrapOr(string.Empty).Should().Be("Nested value!");
        }

        [Test]
        public void SerializeWithJilJsonSerializer_SimpleType_Some()
        {
            var option = Option<string>.Some("Simple string option with some value.");

            option.AssertJilJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithJilJsonSerializer_SimpleType_None()
        {
            var option = Option<string>.None;

            option.AssertJilJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithJilJsonSerializer_SimpleType_Some_Nested()
        {
            var option = Option<Option<string>>.Some(Option<string>.Some("Nested value!"));

            var deserializedOption = option.AssertJilJsonSerializationSuccessForValueType();

            deserializedOption.UnwrapOr(Option<string>.None).UnwrapOr(string.Empty).Should().Be("Nested value!");
        }
    }
}
