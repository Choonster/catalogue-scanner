using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Host
{
    public class TracingStream : MemoryStream
    {
        private const byte NewLine = (byte)'\n';

        private readonly LogLevel logLevel;
        private readonly StreamReader reader;

        public TracingStream(LogLevel logLevel)
        {
            this.logLevel = logLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, LogLevel logLevel) : base(buffer)
        {
            this.logLevel = logLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(int capacity, LogLevel logLevel) : base(capacity)
        {
            this.logLevel = logLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, bool writable, LogLevel logLevel) : base(buffer, writable)
        {
            this.logLevel = logLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, int index, int count, LogLevel logLevel) : base(buffer, index, count)
        {
            this.logLevel = logLevel;
       
            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, int index, int count, bool writable, LogLevel logLevel) : base(buffer, index, count, writable)
        {
            this.logLevel = logLevel;
        
            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible, LogLevel logLevel) : base(buffer, index, count, writable, publiclyVisible)
        {
            this.logLevel = logLevel;
        
            reader = new StreamReader(this);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);

            if (Array.IndexOf(buffer, NewLine) != -1)
            {
                LogContents();
            }
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            base.Write(buffer);

            if (buffer.Contains(NewLine))
            {
                LogContents();
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
            await base.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'

            if (Array.IndexOf(buffer, NewLine) != -1)
            {
                LogContents();
            }
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await base.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (buffer.Span.Contains(NewLine))
            {
                LogContents();
            }
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);

            if (value == NewLine)
            {
                LogContents();
            }
        }

        private void LogContents()
        {
            Position = 0;

            var line = reader.ReadLine();
            while (line != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        Trace.WriteLine(line);
                        break;

                        case LogLevel.Information:
                        Trace.TraceInformation(line);
                        break;

                    case LogLevel.Warning:
                        Trace.TraceWarning(line);
                        break;

                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Trace.TraceError(line);
                        break;
                }

                line = reader.ReadLine();
            }

            SetLength(0);
        }
    }
}
