using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.Core.Serialisation
{
    public class CultureInfoConverter : JsonConverter<CultureInfo>
    {
        public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Error reading {typeof(CultureInfo)} from {typeof(Utf8JsonReader)}. Current item is not a string: {reader.TokenType}");
            }

            var stringValue = reader.GetString();

            if (stringValue == null)
            {
                return null;
            }

            try
            {
                return CultureInfo.GetCultureInfo(stringValue);
            }
            catch (CultureNotFoundException ex)
            {
                throw new JsonException($"Error reading {typeof(CultureInfo)} from {typeof(Utf8JsonReader)}. Culture not found.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
        {
            #region null checks
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            #endregion

            writer.WriteStringValue(value.Name);
        }
    }
}
