namespace LeveInvestimentos.Application.Exceptions;

/// <summary>
/// Exceção quando uma regra de negócio é violada. É capturada nos
/// Controllers para ser exibida ao usuário, sem vazar detalhes técnicos.
/// </summary>
public class DominioException : Exception
{
    public DominioException(string mensagem) : base(mensagem) { }
}
