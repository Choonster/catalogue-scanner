using System;

namespace CatalogueScanner.Core.Dto.Api.Request
{
    public class ListEntityRequest
    {
        public PageInfo Page { get; set; } = new PageInfo();

        public DateTime? LastOperationFrom { get; set; }
        
        public DateTime? LastOperationTo { get; set; }
    }
}
