using Microsoft.Extensions.DependencyInjection;
using Moq;
using OSK.Extensions.Serialization.SystemTextJson.Polymorphism;
using OSK.Extensions.Serialization.SysTextJson.Polymorphism.UnitTests.Helpers;
using OSK.Serialization.Abstractions.Json;
using OSK.Serialization.Json.SystemTextJson;
using OSK.Serialization.Polymorphism;
using OSK.Serialization.Polymorphism.Discriminators;
using OSK.Serialization.Polymorphism.Models;
using OSK.Serialization.Polymorphism.Ports;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace OSK.Extensions.Serialization.SysTextJson.Polymorphism.UnitTests
{
    public class PolymorphismJsonConverterTests
    {
        #region Variables

        private readonly Mock<IPolymorphismContextProvider> _mockContextProvider;
        private readonly PolymorphismJsonConverter _converter;

        #endregion

        #region Constructors

        public PolymorphismJsonConverterTests()
        {
            _mockContextProvider = new Mock<IPolymorphismContextProvider>();

            _converter = new PolymorphismJsonConverter(_mockContextProvider.Object);
        }

        #endregion

        #region CanConvert

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanConvert_CallsPolymorphismContextProvider_ReturnsProvidersResult(bool hasStrategy)
        {
            // Arrange
            _mockContextProvider.Setup(m => m.HasPolymorphismStrategy(It.IsAny<Type>()))
                .Returns(hasStrategy);

            // Act
            var result = _converter.CanConvert(typeof(TestAbstract));

            // Assert
            Assert.Equal(hasStrategy, result);
        }

        #endregion

        #region End to End

        [Fact]
        public async void Validate_ServiceCollectionExtensions()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSystemTextJsonSerialization();
            serviceCollection.AddPolymorphismEnumDiscriminatorStrategy();
            serviceCollection.AddSystemTextJsonPolymorphism();

            var provider = serviceCollection.BuildServiceProvider();

            var items = new TestAbstract[]
{
                new TestChildA()
                {
                    A = 1,
                    B = [ 1, 2, 3 ],
                    Today = DateTime.Now,
                    AbstractType = AbstractType.ChildA
                },
                new TestChildB()
                {
                    A = 1,
                    B = [ 1, 2, 3 ],
                    C = new TestChildB()
                    {
                        A = 1,
                        B = new List<int> { 1, 2, 3 }
                    },
                    AbstractType = AbstractType.ChildB
                }
            };

            // Act
            var serializer = provider.GetRequiredService<IJsonSerializer>();
            var data = await serializer.SerializeAsync(items);
            _ = await serializer.DeserializeAsync(data, items.GetType());
        }

        [Fact]
        public void EndToEnd_HasDiscriminator_ReturnsExpectedObject()
        {
            // Arrange
            var mockEnumStrategy = new Mock<IPolymorphismEnumDiscriminatorStrategy>();
            mockEnumStrategy.Setup(m => m.GetConcreteType(It.IsAny<PolymorphismAttribute>(),
                It.IsAny<Type>(), It.IsAny<object>()))
                .Returns((PolymorphismAttribute attribute, Type typeToConvert, object currentValue) =>
                {
                    switch (currentValue)
                    {
                        case 0:
                            return typeof(TestChildA);
                        case 1:
                            return typeof(TestChildB);
                        default:
                            throw new InvalidCastException("Current value not an expected abstract type");
                    }
                });

            var context = new PolymorphismContext(
                PolymorphismAttribute.GetPolymorphismAttribute(typeof(TestAbstract)),
                typeof(TestAbstract),
                mockEnumStrategy.Object
                );

            _mockContextProvider.Setup(m => m.HasPolymorphismStrategy(It.Is<Type>(t => t == typeof(TestAbstract))))
                .Returns(true);
            _mockContextProvider.Setup(m => m.GetPolymorphismContext(It.Is<Type>(t => t == typeof(TestAbstract))))
                .Returns(context);

            var items = new TestAbstract[]
            {
                new TestChildA()
                {
                    A = 1,
                    B = [ 1, 2, 3 ],
                    Today = DateTime.Now,
                    AbstractType = AbstractType.ChildA
                },
                new TestChildB()
                {
                    A = 1,
                    B = [ 1, 2, 3 ],
                    C = new TestChildB()
                    {
                        A = 1,
                        B = new List<int> { 1, 2, 3 }
                    },
                    AbstractType = AbstractType.ChildB
                }
            };

            var serializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            serializerOptions.Converters.Add(_converter);
            serializerOptions.MakeReadOnly();
            using var memoryStream = new MemoryStream();
            using var writer = new Utf8JsonWriter(memoryStream);

            // Act
            JsonSerializer.Serialize(memoryStream, items, serializerOptions);

            memoryStream.Position = 0;
            var b = JsonSerializer.Deserialize(memoryStream, items.GetType(), serializerOptions);
        }

        #endregion
    }
}
