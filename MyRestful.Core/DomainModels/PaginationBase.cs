using MyRestful.Core.Interfaces;

namespace MyRestful.Core.DomainModels
{
    public class PaginationBase
    {
        private int _pageSize = 10;
        public int PageIndex { get; set; } = 0;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public int Count { get; set; }
        public string OrderBy { get; set; } = nameof(IEntity.Id);

        public int MaxPageSize { get; set; } = 100;
        public int PageCount => Count / PageSize + (Count % PageSize > 0 ? 1 : 0);

        public bool HasPrevious => PageIndex > 0;
        public bool HasNext => PageIndex < PageCount - 1;

        public PaginationBase Clone()
        {
            return new PaginationBase
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                OrderBy = OrderBy,
                Count = Count,
                MaxPageSize = MaxPageSize
            };
        }
    }
}
