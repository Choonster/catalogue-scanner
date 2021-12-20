using Microsoft.ApplicationInsights;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogueScanner.Core.Host
{
    /// <summary>
    /// <para>Stream that tracks its contents as an exception in Application Insights after a line is written to it.</para>
    /// <para>Designed to be used as the <see cref="Console.Error"/> stream so that stderr messages are recorded in Application Insights.</para>
    /// </summary>
    public class ApplicationInsightsStream : MemoryStream
    {
        private const byte NewLine = (byte)'\n';

        private readonly TelemetryClient telemetryClient;
        private readonly StreamReader reader;

        public ApplicationInsightsStream(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public ApplicationInsightsStream(byte[] buffer, TelemetryClient telemetryClient) : base(buffer)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public ApplicationInsightsStream(int capacity, TelemetryClient telemetryClient) : base(capacity)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public ApplicationInsightsStream(byte[] buffer, bool writable, TelemetryClient telemetryClient) : base(buffer, writable)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public ApplicationInsightsStream(byte[] buffer, int index, int count, TelemetryClient telemetryClient) : base(buffer, index, count)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public ApplicationInsightsStream(byte[] buffer, int index, int count, bool writable, TelemetryClient telemetryClient) : base(buffer, index, count, writable)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public ApplicationInsightsStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible, TelemetryClient telemetryClient) : base(buffer, index, count, writable, publiclyVisible)
        {
            this.telemetryClient = telemetryClient;

            reader = new StreamReader(this);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);

            if (Array.IndexOf(buffer, NewLine) != -1)
            {
                TrackContents();
            }
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            base.Write(buffer);

            if (buffer.Contains(NewLine))
            {
                TrackContents();
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
            await base.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'

            if (Array.IndexOf(buffer, NewLine) != -1)
            {
                TrackContents();
            }
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await base.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (buffer.Span.Contains(NewLine))
            {
                TrackContents();
            }
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);

            if (value == NewLine)
            {
                TrackContents();
            }
        }

        private void TrackContents()
        {
            Position = 0;

            var line = reader.ReadLine();
            while (line != null)
            {
                try
                {
                    throw new ApplicationInsightsStreamException(line);
                }
                catch (ApplicationInsightsStreamException ex)
                {
                    telemetryClient.TrackException(ex);
                }

                line = reader.ReadLine();
            }

            SetLength(0);
        }
    }
}
