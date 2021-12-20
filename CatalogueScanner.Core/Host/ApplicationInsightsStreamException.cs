using System;
using System.Diagnostics;

namespace CatalogueScanner.Core.Host
{
    [Serializable]
    public class ApplicationInsightsStreamException : Exception
    {
        private readonly StackTrace? customStackTrace;

        public ApplicationInsightsStreamException()
        {
        }

        public ApplicationInsightsStreamException(string message) : base(message)
        {
        }

        public ApplicationInsightsStreamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ApplicationInsightsStreamException(StackTrace customStackTrace)
        {
            this.customStackTrace = customStackTrace;
        }

        public ApplicationInsightsStreamException(string message, StackTrace customStackTrace) : base(message)
        {
            this.customStackTrace = customStackTrace;
        }

        public ApplicationInsightsStreamException(string message, Exception innerException, StackTrace customStackTrace) : base(message, innerException)
        {
            this.customStackTrace = customStackTrace;
        }

        protected ApplicationInsightsStreamException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public override string? StackTrace => customStackTrace?.ToString() ?? base.StackTrace;
    }
}
