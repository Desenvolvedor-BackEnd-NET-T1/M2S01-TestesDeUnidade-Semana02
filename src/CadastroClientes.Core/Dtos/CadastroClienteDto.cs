namespace CadastroClientes.Core.Dtos;

/// <summary>
/// Representa o "formulário" de cadastro de cliente. Nesta demonstração não existe
/// interface gráfica: este DTO é o objeto enviado em formato JSON no corpo da
/// requisição POST /clientes.
/// Todas as propriedades são opcionais em termos de compilação (strings anuláveis)
/// porque a ausência, o valor vazio ou o valor em branco de qualquer campo é uma
/// condição de validação tratada explicitamente pelo <see cref="CadastroClientes.Core.Servicos.ClienteService"/>,
/// e não algo resolvido via atributos de DataAnnotations.
/// </summary>
public class CadastroClienteDto
{
    public string? Nome { get; set; }

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string? Sexo { get; set; }

    public string? Profissao { get; set; }

    public string? Logradouro { get; set; }

    public string? Numero { get; set; }

    public string? Bairro { get; set; }

    public string? Cidade { get; set; }

    public string? Uf { get; set; }

    public string? Cep { get; set; }
}
