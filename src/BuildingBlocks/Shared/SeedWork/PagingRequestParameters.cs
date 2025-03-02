namespace Shared.SeedWork
{
    public class PagingRequestParameters : RequestParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public const int MaxPageSize = 50;

        public void SetPageSize(int size)
        {
            PageSize = (size > MaxPageSize) ? MaxPageSize : size;
        }
    }
}
