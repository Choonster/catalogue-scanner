using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogueScanner.Core.Dto.Api.Result
{
   public class ListEntityResult<T>
    {
        public IEnumerable<T> Entities { get; set; } = Array.Empty<T>();
        public PageInfo Page { get; set; } = null!;
    }
}
