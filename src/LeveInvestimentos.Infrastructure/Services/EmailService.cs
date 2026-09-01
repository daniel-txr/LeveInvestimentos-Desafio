using System.Net;
using System.Net.Mail;
using LeveInvestimentos.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeveInvestimentos.Infrastructure.Services;

/// <summary>
/// Envia e-mails via SMTP usando System.Net.Mail (biblioteca nativa do .NET,
/// sem dependências externas). Em ambientes sem SMTP configurado (ModoSimulado = true),
/// apenas registra o envio no log.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _configuracoes;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> configuracoes, ILogger<EmailService> logger)
    {
        _configuracoes = configuracoes.Value;
        _logger = logger;
    }

    public async Task EnviarAsync(string destinatario, string assunto, string corpoHtml)
    {
        if (_configuracoes.ModoSimulado)
        {
            _logger.LogInformation(
                "[E-mail simulado] Para: {Destinatario} | Assunto: {Assunto}",
                destinatario, assunto);
            return;
        }

        using var mensagem = new MailMessage
        {
            From = new MailAddress(_configuracoes.RemetenteEmail, _configuracoes.RemetenteNome),
            Subject = assunto,
            Body = corpoHtml,
            IsBodyHtml = true
        };
        mensagem.To.Add(destinatario);

        using var cliente = new SmtpClient(_configuracoes.Host, _configuracoes.Porta)
        {
            Credentials = new NetworkCredential(_configuracoes.Usuario, _configuracoes.Senha),
            EnableSsl = _configuracoes.UsarSsl
        };

        await cliente.SendMailAsync(mensagem);
    }
}
