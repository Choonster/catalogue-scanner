using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Serialisation;

public abstract class BaseJsonNullableStructConverterFactory<T> : JsonConverterFactory where T : struct
{
    public override bool CanConvert(Type typeToConvert)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(typeToConvert);
        #endregion

        return typeToConvert == typeof(T)
            || (typeToConvert.IsGenericType && IsNullableOfT(typeToConvert));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        #region null checks
        ArgumentNullException.ThrowIfNull(typeToConvert);

        ArgumentNullException.ThrowIfNull(options);
        #endregion

        var baseConverter = CreateBaseConverter(typeToConvert, options);

        return typeToConvert.IsGenericType
            ? new NullableConverter(baseConverter)
            : new StandardConverter(baseConverter);
    }

    protected abstract IBaseConverter CreateBaseConverter(Type typeToConvert, JsonSerializerOptions options);

    private static bool IsNullableOfT(Type typeToConvert)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);

        return underlyingType != null && underlyingType == typeof(T);
    }

    protected interface IBaseConverter
    {
        /// <summary>
        /// Equivalent to <see cref="JsonConverter{T}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>.
        /// </summary>
        T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        /// <summary>
        /// Equivalent to <see cref="JsonConverter{T}.Write(Utf8JsonWriter, T, JsonSerializerOptions)"/>
        /// </summary>
        void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
    }

    private sealed class StandardConverter(IBaseConverter baseConverter) : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            baseConverter.Read(ref reader, typeToConvert, options) ?? default;

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            baseConverter.Write(writer, value, options);
    }

    private sealed class NullableConverter(IBaseConverter baseConverter) : JsonConverter<T?>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            baseConverter.Read(ref reader, typeToConvert, options);

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options) =>
            baseConverter.Write(writer, value!.Value, options);
    }
}