namespace LeveInvestimentos.Application.Interfaces;

/// <summary>
/// Abstrai o algoritmo de hashing de senhas. Mantido como interface para permitir
/// trocar a implementação (ex.: PBKDF2, BCrypt) sem impactar as camadas superiores.
/// </summary>
public interface IHashService
{
    string GerarHash(string senha);
    bool VerificarSenha(string senha, string hash);
}
