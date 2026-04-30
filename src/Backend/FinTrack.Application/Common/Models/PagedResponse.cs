namespace FinTrack.Application.Common.Models;

/// <summary>
/// Representa uma resposta paginada
/// </summary>
/// <typeparam name="T">Tipo dos itens</typeparam>
public sealed record PagedResponse<T>
{
    /// <summary>
    /// Lista de itens da página atual
    /// </summary>
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Número da página atual (base 1)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total de itens em todas as páginas
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total de páginas disponíveis
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indica se há uma página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se há uma próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse()
    {
    }

    public PagedResponse(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Cria uma resposta paginada vazia
    /// </summary>
    public static PagedResponse<T> Empty(int pageNumber = 1, int pageSize = 5) =>
        new(Array.Empty<T>(), 0, pageNumber, pageSize);

    /// <summary>
    /// Cria uma resposta paginada com os itens fornecidos
    /// </summary>
    public static PagedResponse<T> Create(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize) =>
        new(items, totalCount, pageNumber, pageSize);
}
