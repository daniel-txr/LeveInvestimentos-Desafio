namespace LeveInvestimentos.Web.Authorization;

/// <summary>Nomes das políticas de autorização registradas em Program.cs.</summary>
public static class PoliticasAutorizacao
{
    /// <summary>Exige que o usuário logado possua o perfil Gestor.</summary>
    public const string SomenteGestor = "SomenteGestor";
}
