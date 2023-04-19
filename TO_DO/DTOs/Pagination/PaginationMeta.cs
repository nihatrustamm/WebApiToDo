namespace TO_DO.DTOs.Pagination;

public class PaginationMeta
{

    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public PaginationMeta(int page, int pageSize, int count)
    {
        Page = page;
        PageSize = pageSize;
        TotalPages = Convert.ToInt32(Math.Ceiling(1.0 * count / pageSize));
    }


}