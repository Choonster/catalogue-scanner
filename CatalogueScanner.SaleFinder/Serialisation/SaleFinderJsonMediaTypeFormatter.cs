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
            var newReadStream = WrapStreamIfRequired(readStream, effectiveEncoding);

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
            var newReadStream = WrapStreamIfRequired(readStream, effectiveEncoding);

            return base.ReadFromStreamAsync(type, newReadStream, content, formatterLogger);
        }

        private static Stream WrapStreamIfRequired(Stream readStream, Encoding effectiveEncoding)
        {
            // If the stream is already wrapped, return it as-is
            if (readStream is WrappedReadStream)
            {
                return readStream;
            }

            // Get the byte counts of the opening and closing parenthesis characters in the current encoding
            var openingParenthesisByteCount = effectiveEncoding.GetByteCount(new char[] { OPENING_PARENTHESIS });
            var closingParenthesisByteCount = effectiveEncoding.GetByteCount(new char[] { CLOSING_PARENTHESIS });

            // StreamReader reads into an internal buffer, changing the stream position;
            // so record the original position before creating and reading from the StreamReader
            var originalPosition = readStream.Position;

            // Disposing the StreamReader would dispose the readStream and prevent the rest of the deserialisation process from reading it, so we don't wrap it in a using statement
            var streamReader = new StreamReader(readStream, effectiveEncoding, detectEncodingFromByteOrderMarks: true, bufferSize: openingParenthesisByteCount);

            // Check if the first character is an opening parenthesis
            var hasLeadingParenthesis = OPENING_PARENTHESIS == streamReader.Peek();

            // Reset the stream to the original position 
            readStream.Seek(originalPosition, SeekOrigin.Begin);

            // If the stream starts with an opening parenthesis, assume it ends with a closing parenthesis and ignore them when deserialising
            if (hasLeadingParenthesis)
            {
                return new WrappedReadStream(readStream, openingParenthesisByteCount, closingParenthesisByteCount);
            }

            return readStream;
        }
    }
}
