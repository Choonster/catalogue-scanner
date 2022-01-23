using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Serialisation
{
    public abstract class BaseJsonNullableStructConverterFactory<T> : JsonConverterFactory where T : struct
    {
        public override bool CanConvert(Type typeToConvert)
        {
            #region null checks
            if (typeToConvert is null)
            {
                throw new ArgumentNullException(nameof(typeToConvert));
            }
            #endregion

            return typeToConvert == typeof(T)
                || (typeToConvert.IsGenericType && IsNullableOfT(typeToConvert));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            #region null checks
            if (typeToConvert is null)
            {
                throw new ArgumentNullException(nameof(typeToConvert));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            #endregion

            var baseConverter = CreateBaseConverter(typeToConvert, options);

            return typeToConvert.IsGenericType
                ? new NullableConverter(baseConverter)
                : new StandardConverter(baseConverter);
        }

        protected abstract JsonConverter<T> CreateBaseConverter(Type typeToConvert, JsonSerializerOptions options);

        private static bool IsNullableOfT(Type typeToConvert)
        {
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert);

            return underlyingType != null && underlyingType == typeof(T);
        }

        private class StandardConverter : JsonConverter<T>
        {
            private readonly JsonConverter<T> converter;

            public StandardConverter(JsonConverter<T> converter) => this.converter = converter;

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                converter.Read(ref reader, typeToConvert, options);

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
                converter.Write(writer, value, options);
        }

        private class NullableConverter : JsonConverter<T?>
        {
            private readonly JsonConverter<T> converter;

            public NullableConverter(JsonConverter<T> converter) => this.converter = converter;

            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                converter.Read(ref reader, typeToConvert, options);

            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) =>
                converter.Write(writer, value!.Value, options);
        }
    }
}