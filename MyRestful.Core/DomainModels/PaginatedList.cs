using System.Collections.Generic;

namespace MyRestful.Core.DomainModels
{
    public class PaginatedList<T> : List<T> where T : class
    {
        public PaginationBase PaginationBase { get; }

        public PaginatedList(PaginationBase paginationBase, IEnumerable<T> data)
        {
            PaginationBase = paginationBase;
            AddRange(data);
        }
    }
}
