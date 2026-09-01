using System.Security.Cryptography;
using LeveInvestimentos.Application.Interfaces;

namespace LeveInvestimentos.Infrastructure.Services;

/// <summary>
/// Implementa hashing de senha com PBKDF2, nativo do .NET,
/// sem dependência de pacotes de terceiros. O salt é gerado por senha e
/// armazenado junto ao hash no mesmo campo, no formato: {iterações}.{salt}.{hash}.
/// </summary>
public class HashService : IHashService
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    public string GerarHash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerificarSenha(string senha, string hash)
    {
        var partes = hash.Split('.', 3);
        if (partes.Length != 3)
            return false;

        var iteracoes = int.Parse(partes[0]);
        var salt = Convert.FromBase64String(partes[1]);
        var hashArmazenado = Convert.FromBase64String(partes[2]);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, hashArmazenado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashArmazenado);
    }
}
