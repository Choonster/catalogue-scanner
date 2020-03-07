using System;

namespace CatalogueScanner
{
    [Serializable]
    public class UnableToFindSaleIdException : Exception
    {
        public UnableToFindSaleIdException() { }
        public UnableToFindSaleIdException(string message) : base(message) { }
        public UnableToFindSaleIdException(string message, Exception inner) : base(message, inner) { }
        protected UnableToFindSaleIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
