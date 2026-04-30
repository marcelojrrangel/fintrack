namespace FinTrack.Application.Common.Models;

/// <summary>
/// Representa uma requisição paginada
/// </summary>
public sealed record PagedRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 5;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Número da página (base 1)
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Tamanho da página (padrão: 5, máximo: 100)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? DefaultPageSize : value > MaxPageSize ? MaxPageSize : value;
    }

    public PagedRequest()
    {
    }

    public PagedRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
