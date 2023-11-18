using System;

namespace CatalogueScanner.SaleFinder.Service;

[Serializable]
public class UnableToFindSaleIdException : Exception
{
    public UnableToFindSaleIdException() { }
    public UnableToFindSaleIdException(string message) : base(message) { }
    public UnableToFindSaleIdException(string message, Exception inner) : base(message, inner) { }
}
