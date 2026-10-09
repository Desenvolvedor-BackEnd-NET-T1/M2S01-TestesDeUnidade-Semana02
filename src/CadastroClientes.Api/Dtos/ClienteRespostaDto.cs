using CadastroClientes.Core.Modelos;

namespace CadastroClientes.Api.Dtos;

/// <summary>
/// Formato de resposta da API para um cliente cadastrado.
/// Simples espelho dos dados públicos de <see cref="Cliente"/>.
/// </summary>
public class ClienteRespostaDto
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

    public static ClienteRespostaDto ApartirDe(Cliente cliente)
    {
        return new ClienteRespostaDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Sexo = cliente.Sexo,
            Profissao = cliente.Profissao,
            Logradouro = cliente.Logradouro,
            Numero = cliente.Numero,
            Bairro = cliente.Bairro,
            Cidade = cliente.Cidade,
            Uf = cliente.Uf,
            Cep = cliente.Cep,
        };
    }
}
