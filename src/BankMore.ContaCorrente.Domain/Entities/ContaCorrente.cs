using BankMore.Shared.Common;

namespace BankMore.ContaCorrente.Domain.Entities;

public class ContaCorrente
{
    public string IdContaCorrente { get; private set; }
    public int Numero { get; private set; }
    public string Nome { get; private set; }
    public bool Ativo { get; private set; }
    public string Senha { get; private set; }
    public string Salt { get; private set; }

    private ContaCorrente() { }

    public ContaCorrente(string idContaCorrente, int numero, string nome, string senha, string salt)
    {
        IdContaCorrente = idContaCorrente;
        Numero = numero;
        Nome = nome;
        Senha = senha;
        Salt = salt;
        Ativo = true;
    }

    public void Inativar()
    {
        Ativo = false;
    }

    public bool VerificarSenha(string senha, string hash, string salt)
    {
        return BankMore.Shared.Security.PasswordHasher.VerifyPassword(senha, hash, salt);
    }
}
