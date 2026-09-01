namespace LeveInvestimentos.Application.Interfaces;

/// <summary>
/// Abstrai o envio de e-mails, permitindo trocar o provedor
/// sem alterar as regras de negócio. As falhas de envio nunca devem interromper
/// o fluxo principal da aplicação (ver implementação em Infrastructure).
/// </summary>
public interface IEmailService
{
    Task EnviarAsync(string destinatario, string assunto, string corpoHtml);
}
