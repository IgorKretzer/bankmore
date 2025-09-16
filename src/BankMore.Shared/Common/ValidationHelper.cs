using System.Text.RegularExpressions;

namespace BankMore.Shared.Common;

public static class ValidationHelper
{
    public static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = Regex.Replace(cpf, @"[^\d]", "");

        if (cpf.Length != 11)
            return false;

        if (cpf.All(c => c == cpf[0]))
            return false;

        int soma = 0;
        for (int i = 0; i < 9; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * (10 - i);
        }
        int resto = soma % 11;
        int primeiroDigito = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != primeiroDigito)
            return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * (11 - i);
        }
        resto = soma % 11;
        int segundoDigito = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == segundoDigito;
    }

    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return password.Length >= 6;
    }

    public static bool IsValidValue(decimal value)
    {
        return value > 0;
    }

    public static void LogValidationAttempt(string type, string value, bool isValid)
    {
    }
}
