using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Newtonsoft.Json;

namespace Ruzzie.Common.Types.UnitTests.Serialization
{
    public static class SerializationTestsExtensions
    {
        public static void AssertDefaultBinarySerializationSuccessForValueType<T>(this T value) where T : struct
        {
            var binaryFormatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                binaryFormatter.Serialize(stream, value);

                stream.Seek(0, SeekOrigin.Begin);

                var deserializedValue = (T) binaryFormatter.Deserialize(stream);

                deserializedValue.Should().NotBeSameAs(value);
                value.Equals(deserializedValue).Should().BeTrue();
            }
        }

        public static void AssertDefaultDataContractSerializationSuccessForValueType<T>(this T value) where T : struct
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);

                stream.Seek(0, SeekOrigin.Begin);

                var deserializedValue = (T) serializer.ReadObject(stream);

                deserializedValue.Should().NotBeSameAs(value);
                value.Equals(deserializedValue).Should().BeTrue();
            }
        }

        public static T AssertNewtonsoftJsonSerializationSuccessForValueType<T>(this T value) where T : struct
        {
            var serializedOption = JsonConvert.SerializeObject(value);

            var deserializedOption = JsonConvert.DeserializeObject<T>(serializedOption);

            deserializedOption.Should().NotBeSameAs(value);
            value.Equals(deserializedOption).Should().BeTrue();
            return deserializedOption;
        }
    }
}