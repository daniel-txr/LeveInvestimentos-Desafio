namespace LeveInvestimentos.Infrastructure.Services;

/// <summary>Configurações de SMTP lidas de appsettings.json (seção "EmailSettings").</summary>
public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Porta { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string RemetenteNome { get; set; } = "Leve Investimentos";
    public string RemetenteEmail { get; set; } = string.Empty;
    public bool UsarSsl { get; set; } = true;

    /// <summary>Quando true, o serviço apenas grava o e-mail no log/console em vez de enviar de fato.</summary>
    public bool ModoSimulado { get; set; } = true;
}
