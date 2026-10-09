namespace CadastroClientes.Core.Validacao;

/// <summary>
/// Lista fixa das siglas de Unidade Federativa (UF) brasileiras válidas.
/// Mantida explícita em código (sem depender de bibliotecas externas) para que
/// a regra "UF deve pertencer à lista de UFs brasileiras" apareça de forma clara
/// no relatório de cobertura.
/// </summary>
public static class UfsBrasileiras
{
    public static readonly IReadOnlySet<string> Codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO",
    };

    public static bool EhValida(string uf)
    {
        return Codigos.Contains(uf);
    }
}
