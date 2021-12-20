using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Host
{
    /// <summary>
    /// <para>Stream that traces its contents in Application Insights after a line is written to it.</para>
    /// <para>Designed to be used as the <see cref="Console.Error"/> stream so that stderr messages are recorded in Application Insights.</para>
    /// </summary>
    public class TracingStream : MemoryStream
    {
        private const byte NewLine = (byte)'\n';

        private readonly TelemetryClient telemetryClient;
        private readonly SeverityLevel severityLevel;
        private readonly StreamReader reader;

        public TracingStream(TelemetryClient telemetryClient, SeverityLevel severityLevel)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, TelemetryClient telemetryClient, SeverityLevel severityLevel) : base(buffer)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(int capacity, TelemetryClient telemetryClient, SeverityLevel severityLevel) : base(capacity)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, bool writable, TelemetryClient telemetryClient, SeverityLevel severityLevel) : base(buffer, writable)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, int index, int count, TelemetryClient telemetryClient, SeverityLevel severityLevel) : base(buffer, index, count)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, int index, int count, bool writable, TelemetryClient telemetryClient, SeverityLevel severityLevel) : base(buffer, index, count, writable)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

            reader = new StreamReader(this);
        }

        public TracingStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible, TelemetryClient telemetryClient, SeverityLevel severityLevel) : base(buffer, index, count, writable, publiclyVisible)
        {
            this.telemetryClient = telemetryClient;
            this.severityLevel = severityLevel;

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
                telemetryClient.TrackTrace(line, severityLevel);

                line = reader.ReadLine();
            }

            SetLength(0);
        }
    }
}
