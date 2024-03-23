using OSK.Extensions.SystemTextJson.Common;
using OSK.Serialization.Polymorphism.Ports;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSK.Extensions.Serialization.SystemTextJson.Polymorphism
{
    public class PolymorphismJsonConverter : JsonConverter<object>
    {
        #region Variables

        private readonly IPolymorphismContextProvider _polymorphismContextProvider;

        #endregion

        #region Constructors

        public PolymorphismJsonConverter(IPolymorphismContextProvider polymorphismContextProvider)
        {
            _polymorphismContextProvider = polymorphismContextProvider ?? throw new ArgumentNullException(nameof(polymorphismContextProvider));
        }

        #endregion

        #region JsonConverter Overrides

        public override bool CanConvert(Type typeToConvert)
        {
            return _polymorphismContextProvider.HasPolymorphismStrategy(typeToConvert);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var polymorphismContext = _polymorphismContextProvider.GetPolymorphismContext(typeToConvert);
            var polymorphicPropertyValue = GetPolymorphismPropertyValue(ref reader, polymorphismContext.PolymorphismPropertyName, typeToConvert);
            if (polymorphicPropertyValue == null)
            {
                throw new InvalidOperationException($"Failed to deserialize object of type {typeToConvert.FullName} because the expected polymorphic property, {polymorphismContext.PolymorphismPropertyName}, was not found in the JSON stream.");
            }

            var concreteType = polymorphismContext.GetConcreteType(polymorphicPropertyValue);
            if (concreteType == null)
            {
                throw new InvalidOperationException($"The polymorphism strategy failed to determine a concrete type for the conversion for {typeToConvert.FullName}.");
            }

            return JsonSerializer.Deserialize(ref reader, concreteType, options);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options.GetTypeInfo(value.GetType()));
        }

        #endregion

        #region Helpers

        private object GetPolymorphismPropertyValue(ref Utf8JsonReader reader, string propertyName, Type typeToConvert)
        {
            if (!reader.TryFindPropertyValue(propertyName, out var propertyReader))
            {
                throw new InvalidOperationException($"Failed to deserialize object of type {typeToConvert.FullName} because the expected polymorphic property, {propertyName}, was not found in the JSON stream.");
            }

            return propertyReader.TokenType == JsonTokenType.Number
                ? (object)propertyReader.GetInt32()
                : propertyReader.GetString();
        }

        #endregion
    }
}
