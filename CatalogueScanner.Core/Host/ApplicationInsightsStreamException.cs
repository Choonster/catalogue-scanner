using System;

namespace CatalogueScanner.Core.Host
{
    [Serializable]
    public class ApplicationInsightsStreamException : Exception
    {
        public ApplicationInsightsStreamException() { }
        public ApplicationInsightsStreamException(string message) : base(message) { }
        public ApplicationInsightsStreamException(string message, Exception inner) : base(message, inner) { }
        protected ApplicationInsightsStreamException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
