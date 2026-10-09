using CadastroClientes.Core.Servicos;
using CadastroClientes.Tests.TestDoubles;

namespace CadastroClientes.Tests.Servicos;

/// <summary>
/// Suíte inicial da aula: propositalmente contém um único teste.
///
/// Este teste cobre apenas o "caminho feliz" do cadastro (dados válidos em todos
/// os campos) e, mesmo passando com sucesso, NÃO exercita nenhuma das demais
/// regras de validação implementadas em <see cref="ClienteService"/> (campos
/// obrigatórios, tamanhos mínimos/máximos, formatos inválidos, email duplicado).
///
/// Essa é exatamente a lacuna que o relatório de cobertura vai revelar: o teste
/// passa, mas boa parte do código de validação permanece sem cobertura.
/// </summary>
public class ClienteServiceTests
{
    [Fact]
    public void Cadastrar_ComDadosValidos_DevePersistirCliente()
    {
        // Cada teste cria seu próprio repositório, para evitar vazamento de estado
        // entre testes. Não é necessário usar mocks aqui: um repositório real e
        // simples (em memória) já é suficiente para verificar o comportamento.
        var repositorio = new RepositorioClienteEmMemoriaDeTeste();
        var servico = new ClienteService(repositorio);
        var dto = CadastroClienteDtoFabrica.CriarValido();

        var resultado = servico.Cadastrar(dto);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Cliente);
        Assert.NotEqual(Guid.Empty, resultado.Cliente!.Id);
        Assert.Equal(dto.Nome, resultado.Cliente.Nome);
        Assert.Equal(dto.Email, resultado.Cliente.Email);
        Assert.Equal(dto.Telefone, resultado.Cliente.Telefone);
        Assert.Equal(dto.Sexo, resultado.Cliente.Sexo);
        Assert.Equal(dto.Profissao, resultado.Cliente.Profissao);
        Assert.Equal(dto.Logradouro, resultado.Cliente.Logradouro);
        Assert.Equal(dto.Numero, resultado.Cliente.Numero);
        Assert.Equal(dto.Bairro, resultado.Cliente.Bairro);
        Assert.Equal(dto.Cidade, resultado.Cliente.Cidade);
        Assert.Equal(dto.Uf, resultado.Cliente.Uf);
        Assert.Equal(dto.Cep, resultado.Cliente.Cep);

        var clienteArmazenado = repositorio.ObterPorId(resultado.Cliente.Id);
        Assert.NotNull(clienteArmazenado);
        Assert.Equal(resultado.Cliente.Email, clienteArmazenado!.Email);

        Assert.Single(repositorio.Listar());
    }
}
