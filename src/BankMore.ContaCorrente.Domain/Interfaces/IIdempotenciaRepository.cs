namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IIdempotenciaRepository
{
    Task<string?> ObterResultadoAsync(string chaveIdempotencia);
    Task SalvarAsync(string chaveIdempotencia, string requisicao, string resultado);
}
