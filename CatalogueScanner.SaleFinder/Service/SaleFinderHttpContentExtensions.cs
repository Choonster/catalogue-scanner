using CatalogueScanner.Core.Serialisation;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Service
{
    public static class SaleFinderHttpContentExtensions
    {
        private const char OPENING_PARENTHESIS = '(';
        private const char CLOSING_PARENTHESIS = ')';

        private static readonly int ClosingParenthesisUtf8ByteCount = Encoding.UTF8.GetByteCount(new[] { CLOSING_PARENTHESIS });

        private static readonly ConcurrentDictionary<string, byte[]> CallbackUtf8ByteArrays = new();

        public static async Task<T?> ReadSaleFinderResponseAsAync<T>(this HttpContent content, JsonTypeInfo<T> jsonTypeInfo, string? callbackName, CancellationToken cancellationToken = default)
        {
            #region null checks
            ArgumentNullException.ThrowIfNull(content);

            ArgumentNullException.ThrowIfNull(jsonTypeInfo);
            #endregion

            var sourceEncoding = GetEncoding(content.Headers.ContentType?.CharSet);

            try
            {
                using var contentStream = await GetContentStream(content, sourceEncoding, cancellationToken).ConfigureAwait(false);
                using var wrappedStream = await WrapStreamIfRequired(contentStream, callbackName ?? string.Empty, cancellationToken).ConfigureAwait(false);

                return await JsonSerializer.DeserializeAsync(wrappedStream, jsonTypeInfo, cancellationToken).ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                var stringContent = await GetStringContent(content, cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(stringContent))
                {
                    throw new JsonException($"Failed to parse JSON. Original content: {stringContent}", ex);
                }

                throw;
            }

            static async Task<string?> GetStringContent(HttpContent content, CancellationToken cancellationToken)
            {
                string? stringContent = null;
                try
                {
                    stringContent = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // Ignored
                }

                return stringContent;
            }
        }

        private static async Task<Stream> GetContentStream(HttpContent content, Encoding? sourceEncoding, CancellationToken cancellationToken)
        {
            var contentStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            // Wrap content stream into a transcoding stream that buffers the data transcoded from the sourceEncoding to UTF-8.
            if (sourceEncoding != null && sourceEncoding != Encoding.UTF8)
            {
                return Encoding.CreateTranscodingStream(contentStream, innerStreamEncoding: sourceEncoding, outerStreamEncoding: Encoding.UTF8);
            }

            return contentStream;
        }

        private static async Task<Stream> WrapStreamIfRequired(Stream readStream, string callbackName, CancellationToken cancellationToken)
        {
            // If the stream is already wrapped, return it as-is
            if (readStream is WrappedReadStream)
            {
                return readStream;
            }

            // Get the byte array of the callback name with opening parenthesis
            var callbackByteArray = CallbackUtf8ByteArrays.GetOrAdd(callbackName, GetCallbackUtf8ByteArray);

            // Record the original strean position before checking for the callback name
            var originalPosition = readStream.Position;

            // Check if the stream starts with the callback name and an opening parenthesis
            var streamBytes = (Memory<byte>) new byte[callbackByteArray.Length];
            var bytesRead = await readStream.ReadAsync(streamBytes, cancellationToken).ConfigureAwait(false);

            // Reset the stream to the original position 
            readStream.Seek(originalPosition, SeekOrigin.Begin);

            // If the stream starts with the callback name and an opening parenthesis, assume it ends with a closing parenthesis and ignore them when deserialising
            if (bytesRead == callbackByteArray.Length && streamBytes.Span.SequenceEqual(callbackByteArray))
            {
                return new WrappedReadStream(readStream, callbackByteArray.Length, ClosingParenthesisUtf8ByteCount);
            }

            return readStream;
        }

        private static Encoding? GetEncoding(string? charset)
        {
            Encoding? encoding = null;

            if (charset != null)
            {
                try
                {
                    // Remove at most a single set of quotes.
                    if (charset.Length > 2 && charset[0] == '\"' && charset[^1] == '\"')
                    {
                        encoding = Encoding.GetEncoding(charset[1..^1]);
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException("The character set provided in ContentType is invalid.", e);
                }
            }

            return encoding;
        }

        private static byte[] GetCallbackUtf8ByteArray(string callbackName) => Encoding.UTF8.GetBytes((callbackName + OPENING_PARENTHESIS).ToCharArray());
    }
}
