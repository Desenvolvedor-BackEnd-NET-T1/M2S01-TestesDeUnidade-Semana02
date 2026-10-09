using CadastroClientes.Core.Dtos;

namespace CadastroClientes.Tests.TestDoubles;

/// <summary>
/// Fábrica de DTOs de cadastro válidos para uso nos testes.
///
/// Cada chamada a <see cref="CriarValido"/> retorna uma NOVA instância (nunca o
/// mesmo objeto compartilhado), para que os testes possam alterar apenas o campo
/// relevante em cada caso sem risco de um teste "contaminar" outro por reaproveitar
/// o mesmo objeto mutável.
/// </summary>
public static class CadastroClienteDtoFabrica
{
    public static CadastroClienteDto CriarValido()
    {
        return new CadastroClienteDto
        {
            Nome = "Ana Silva",
            Email = "ana.silva@example.com",
            Telefone = "11987654321",
            Sexo = "F",
            Profissao = "Engenheira",
            Logradouro = "Rua das Flores",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Uf = "SP",
            Cep = "01310930",
        };
    }
}
