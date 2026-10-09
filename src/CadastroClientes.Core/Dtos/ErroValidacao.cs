namespace CadastroClientes.Core.Dtos;

/// <summary>
/// Representa um erro de validação associado a um campo específico do cadastro.
/// Usar uma classe simples (em vez de lançar exceções) permite acumular todos os
/// erros encontrados em uma única tentativa de cadastro.
/// </summary>
public class ErroValidacao
{
    public ErroValidacao(string campo, string mensagem)
    {
        Campo = campo;
        Mensagem = mensagem;
    }

    public string Campo { get; }

    public string Mensagem { get; }
}
