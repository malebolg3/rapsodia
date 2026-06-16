namespace Rapsodia.Silver.Domain.Common;

public record Result<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static Result<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static Result<T> Fail(string message, IEnumerable<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors is null ? Array.Empty<string>() : errors.ToList()
    };
}

public record PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}