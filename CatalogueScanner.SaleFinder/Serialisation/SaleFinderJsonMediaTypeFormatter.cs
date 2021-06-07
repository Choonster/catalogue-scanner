using CatalogueScanner.Core.Serialisation;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.SaleFinder.Serialisation
{
    /// <summary>
    /// Supports deserialising SaleFinder JSON responses (which are really JSONP).
    /// </summary>
    public class SaleFinderJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private const char OPENING_PARENTHESIS = '(';
        private const char CLOSING_PARENTHESIS = ')';

        private static readonly char[] CLOSING_PARENTHESIS_ARRAY = new[] { CLOSING_PARENTHESIS };

        private readonly char[] callbackWithOpeningParenthesis;

        public SaleFinderJsonMediaTypeFormatter(string callbackName)
        {
            #region null checks
            if (callbackName is null)
            {
                throw new ArgumentNullException(nameof(callbackName));
            }
            #endregion

            callbackWithOpeningParenthesis = (callbackName + OPENING_PARENTHESIS).ToCharArray();
        }

        public override object ReadFromStream(Type type, Stream readStream, Encoding effectiveEncoding, IFormatterLogger formatterLogger)
        {
            #region null checks
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (readStream is null)
            {
                throw new ArgumentNullException(nameof(readStream));
            }

            if (effectiveEncoding is null)
            {
                throw new ArgumentNullException(nameof(effectiveEncoding));
            }
            #endregion

            using var newReadStream = WrapStreamIfRequired(readStream, effectiveEncoding);

            return base.ReadFromStream(type, newReadStream, effectiveEncoding, formatterLogger);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            #region null checks
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (readStream is null)
            {
                throw new ArgumentNullException(nameof(readStream));
            }
            #endregion

            var contentHeaders = content?.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);

            // Base method asks implementers not to close stream, so we don't wrap it in a using statement
#pragma warning disable CA2000 // Dispose objects before losing scope
            var newReadStream = WrapStreamIfRequired(readStream, effectiveEncoding);
#pragma warning restore CA2000 // Dispose objects before losing scope

            return base.ReadFromStreamAsync(type, newReadStream, content, formatterLogger, cancellationToken);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            #region null checks
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (readStream is null)
            {
                throw new ArgumentNullException(nameof(readStream));
            }
            #endregion

            var contentHeaders = content?.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);

            // Base method asks implementers not to close stream, so we don't wrap it in a using statement
#pragma warning disable CA2000 // Dispose objects before losing scope
            var newReadStream = WrapStreamIfRequired(readStream, effectiveEncoding);
#pragma warning restore CA2000 // Dispose objects before losing scope

            return base.ReadFromStreamAsync(type, newReadStream, content, formatterLogger);
        }

        private Stream WrapStreamIfRequired(Stream readStream, Encoding effectiveEncoding)
        {
            // If the stream is already wrapped, return it as-is
            if (readStream is WrappedReadStream)
            {
                return readStream;
            }

            // Get the byte counts of the callback name with opening parenthesis and of the closing parenthesis character in the current encoding
            var callbackByteCount = effectiveEncoding.GetByteCount(callbackWithOpeningParenthesis);
            var closingParenthesisByteCount = effectiveEncoding.GetByteCount(CLOSING_PARENTHESIS_ARRAY);

            // StreamReader reads into an internal buffer, changing the stream position;
            // so record the original position before creating and reading from the StreamReader
            var originalPosition = readStream.Position;

            // Disposing the StreamReader would dispose the readStream and prevent the rest of the deserialisation process from reading it, so we don't wrap it in a using statement
#pragma warning disable CA2000 // Dispose objects before losing scope
            var streamReader = new StreamReader(readStream, effectiveEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: callbackByteCount);
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Check if the first character in the stream is the first character of the callback name
            if (callbackWithOpeningParenthesis[0] == streamReader.Peek())
            {
                // Check if the rest of the callback name matches
                var buffer = (Span<char>)new char[callbackWithOpeningParenthesis.Length];
                streamReader.Read(buffer);

                var openingCharactersMatchCallback = buffer.SequenceEqual(callbackWithOpeningParenthesis);

                // If the stream starts with the callback name and an opening parenthesis, assume it ends with a closing parenthesis and ignore them when deserialising
                if (openingCharactersMatchCallback)
                {
                    // Reset the stream to the original position 
                    readStream.Seek(originalPosition, SeekOrigin.Begin);

                    return new WrappedReadStream(readStream, callbackByteCount, closingParenthesisByteCount);
                }
            }

            // Reset the stream to the original position 
            readStream.Seek(originalPosition, SeekOrigin.Begin);

            return readStream;
        }
    }
}
