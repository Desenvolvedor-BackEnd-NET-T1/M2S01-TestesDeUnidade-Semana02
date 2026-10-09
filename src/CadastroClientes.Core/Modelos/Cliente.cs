namespace CadastroClientes.Core.Modelos;

/// <summary>
/// Representa um cliente já cadastrado com sucesso.
/// É uma classe simples (sem comportamento), usada apenas para transportar os dados
/// persistidos pelo repositório em memória.
/// </summary>
public class Cliente
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string Sexo { get; set; } = string.Empty;

    public string Profissao { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    public string Cep { get; set; } = string.Empty;
}
