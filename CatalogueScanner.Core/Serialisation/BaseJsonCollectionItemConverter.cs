using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Serialisation
{
    /// <summary>
    /// <para>JSON collection converter.</para>
    /// <para>
    /// Adapted from code by Ramjot Singh:<br/>
    /// https://github.com/dotnet/runtime/issues/54189#issuecomment-861628532
    /// </para>
    /// </summary>
    /// <typeparam name="TData">Type of item to convert.</typeparam>
    /// <typeparam name="TCollection">Type of collection to convert. Must be <see cref="List{T}"/> or any of its implemented interfaces.</typeparam>
    /// <typeparam name="TConverter">Converter to use for individual items.</typeparam>
    public class BaseJsonCollectionItemConverter<TData, TCollection, TConverter> : JsonConverter<TCollection>
        where TCollection : IEnumerable<TData>
        where TConverter : JsonConverter, new()
    {
        protected virtual JsonSerializerContext? GetJsonSerializerContext(JsonSerializerOptions options) => null;

        public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(typeToConvert);

            ArgumentNullException.ThrowIfNull(options);
            #endregion

            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            var jsonSerializerOptions = new JsonSerializerOptions(options);
            jsonSerializerOptions.Converters.Add(new TConverter());

            var context = GetJsonSerializerContext(jsonSerializerOptions);

            var returnValue = new List<TData>();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    var item = (
                        context != null
                            ? (TData?)JsonSerializer.Deserialize(ref reader, typeof(TData), context)
                            : JsonSerializer.Deserialize<TData>(ref reader, jsonSerializerOptions)
                    ) ?? throw new JsonException("Encountered null item in JSON array");

                    returnValue.Add(item);
                }

                reader.Read();
            }

            return (TCollection)(IEnumerable<TData>)returnValue;
        }

        public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(writer);

            ArgumentNullException.ThrowIfNull(options);
            #endregion

            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var jsonSerializerOptions = new JsonSerializerOptions(options);
            jsonSerializerOptions.Converters.Add(Activator.CreateInstance<TConverter>());

            var context = GetJsonSerializerContext(jsonSerializerOptions);

            writer.WriteStartArray();

            foreach (var data in value)
            {
                if (context != null)
                {
                    JsonSerializer.Serialize(writer, data, typeof(TData), context);
                }
                else
                {
                    JsonSerializer.Serialize(writer, data, jsonSerializerOptions);
                }
            }

            writer.WriteEndArray();
        }
    }
}
