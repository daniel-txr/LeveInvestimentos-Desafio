namespace LeveInvestimentos.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string? Mensagem { get; set; }
}
