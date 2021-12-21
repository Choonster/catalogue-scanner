using System;

namespace CatalogueScanner.Core.Host.ApplicationInsights
{
    [Serializable]
    public class ApplicationInsightsStreamException : Exception
    {
        public ApplicationInsightsStreamException()
        {
        }

        public ApplicationInsightsStreamException(string message) : base(message)
        {
        }

        public ApplicationInsightsStreamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ApplicationInsightsStreamException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
