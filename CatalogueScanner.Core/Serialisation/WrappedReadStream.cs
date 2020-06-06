using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Serialisation
{
    public class WrappedReadStream : Stream
    {
        private readonly Stream stream;

        public WrappedReadStream(Stream stream, long startOffset, long endOffset)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            StartOffset = startOffset;
            EndOffset = endOffset;

            this.stream.Seek(StartOffset, SeekOrigin.Current);
        }

        public long StartOffset { get; }

        public long EndOffset { get; }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => stream.Length - StartOffset - EndOffset;

        public override long Position { get => stream.Position - StartOffset; set => stream.Position = value + StartOffset; }

        public override bool CanTimeout => stream.CanTimeout;

        public override int ReadTimeout { get => stream.ReadTimeout; set => stream.ReadTimeout = value; }

        /// <summary>
        /// Limits the number of bytes being read to the range of this stream.
        /// </summary>
        /// <param name="count">The number of bytes requested to be read</param>
        /// <returns>The number of bytes to read</returns>
        private int LimitReadCount(int count)
        {
            if (Position + count >= Length)
            {
                return (int)Math.Max(Position + count - Length, 0);
            }

            return count;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, LimitReadCount(count));
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return stream.ReadAsync(buffer, offset, LimitReadCount(count), cancellationToken);
        }

        public override int ReadByte()
        {
            if (LimitReadCount(1) == 0)
            {
                return -1;
            }

            return stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset + StartOffset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            stream.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await stream.DisposeAsync().ConfigureAwait(false);
        }
    }
}
