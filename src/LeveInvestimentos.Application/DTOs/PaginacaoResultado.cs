namespace LeveInvestimentos.Application.DTOs;

/// <summary>
/// Envelope genérico e reutilizável para qualquer listagem paginada da aplicação
/// (usuários, tarefas, e o que mais vier a existir). Mantém a lógica de cálculo
/// de total de páginas em um único lugar, em vez de repetir em cada Service.
/// </summary>
public class PaginacaoResultado<T>
{
    public IEnumerable<T> Itens { get; set; } = Enumerable.Empty<T>();
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public int TotalRegistros { get; set; }

    public int TotalPaginas => TamanhoPagina == 0
        ? 0
        : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);

    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;
}
