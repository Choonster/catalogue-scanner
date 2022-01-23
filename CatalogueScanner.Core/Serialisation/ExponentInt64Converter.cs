using CatalogueScanner.Core.Serialisation;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogueScanner.SaleFinder.Dto.SaleFinder
{
    /// <summary>
    /// <para>Converter that can parse numbers in scientific notation/exponent format (e.g. 3.07446E+18) as <see cref="long"/> values, in addition to the standard integer/floating-point formats.</para>
    /// <para>Note that the scientific notation/floating-point formats are only supported in the range of <see cref="double"/>.</para>
    /// </summary>
    public class ExponentInt64Converter : BaseJsonNullableStructConverterFactory<long>
    {
        protected override JsonConverter<long> CreateBaseConverter(Type typeToConvert, JsonSerializerOptions options) =>
            new Converter();

        private class Converter : JsonConverter<long>
        {
            public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        return reader.GetInt64();

                    case JsonTokenType.String:
                        var stringValue = reader.GetString();

                        if (long.TryParse(stringValue, out var longValue))
                        {
                            return longValue;
                        }

                        if (double.TryParse(stringValue, out var doubleValue))
                        {
                            return (long)doubleValue;
                        }

                        break;
                }

                throw new JsonException($"Error reading {typeof(long)} from {typeof(Utf8JsonReader)}. Current item is not a number or numeric string: {reader.TokenType}");
            }

            public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
            {
                #region null checks
                if (writer is null)
                {
                    throw new ArgumentNullException(nameof(writer));
                }

                if (options is null)
                {
                    throw new ArgumentNullException(nameof(options));
                }
                #endregion

                writer.WriteNumberValue(value);
            }
        }
    }
}
