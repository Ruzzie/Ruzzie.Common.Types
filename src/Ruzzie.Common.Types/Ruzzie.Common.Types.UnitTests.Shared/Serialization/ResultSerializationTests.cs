using FluentAssertions;
using NUnit.Framework;

namespace Ruzzie.Common.Types.UnitTests.Serialization
{
    [TestFixture]
    public class ResultSerializationTests
    {
        [Test]
        public void SerializeToDefaultBinaryFormat_SimpleType_Ok()
        {
            var result = Result<string,string>.Ok("Simple string option with Ok value.");
            
            result.AssertDefaultBinarySerializationSuccessForValueType();
        }

        [Test]
        public void SerializeToDefaultBinaryFormat_SimpleType_Err()
        {
            var result = Result<string,string>.Err("Simple string option with Err value.");
            
            result.AssertDefaultBinarySerializationSuccessForValueType();
        }

        [Test]
        public void SerializeToDefaultBinaryFormat_SimpleType_Unknown()
        {
            var result = default(Result<string,string>);

            Assert.That(() => result.AssertDefaultBinarySerializationSuccessForValueType(),
                Throws.Exception.TypeOf<PanicException<string>>()
                    .With
                    .Message.StartWith("Result is uninitialized. You cannot obtain the Err value."));

        }

        [Test]
        public void SerializeWithDataContractSerializer_SimpleType_Ok()
        {
            var result = Result<string,string>.Ok("Simple string option with Ok value.");
            result.AssertDefaultDataContractSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithDataContractSerializer_SimpleType_Err()
        {
            var result = Result<string,string>.Err("Simple string option with Err value.");
            
            result.AssertDefaultDataContractSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithDataContractSerializer_SimpleType_Unknown()
        {
            var result = default(Result<string,string>);
            
            Assert.That(() => result.AssertDefaultBinarySerializationSuccessForValueType(),
                Throws.Exception.TypeOf<PanicException<string>>()
                    .With
                    .Message.StartWith("Result is uninitialized. You cannot obtain the Err value."));
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_Ok()
        {
            var result = Result.Ok<string, string>("Simple string option with some value.");

            result.AssertNewtonsoftJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_Err()
        {
            var result = Result<string,string>.Err("Simple string option with Err value.");

            result.AssertNewtonsoftJsonSerializationSuccessForValueType();
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_Unknown()
        {
            var result = default(Result<string,string>);

            Assert.That(() => result.AssertDefaultBinarySerializationSuccessForValueType(),
                Throws.Exception.TypeOf<PanicException<string>>()
                    .With
                    .Message.StartWith("Result is uninitialized. You cannot obtain the Err value."));
        }

        [Test]
        public void SerializeWithNewtonSoftJsonSerializer_SimpleType_Some_Nested()
        {
            var result = new Result<int, Result<string, string>>(Result<string, string>.Ok("Nested value!"));

            var deserializedOption = result.AssertNewtonsoftJsonSerializationSuccessForValueType();

            deserializedOption.UnwrapOr(Result<string, string>.Ok("not me!")).UnwrapOr(string.Empty).Should()
                .Be("Nested value!");
        }
    }
}