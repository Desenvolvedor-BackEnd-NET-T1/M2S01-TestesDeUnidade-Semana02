using CadastroClientes.Core.Dtos;

namespace CadastroClientes.Api.Dtos;

/// <summary>
/// Formato de resposta da API para um erro de validação de campo.
/// </summary>
public class ErroRespostaDto
{
    public string Campo { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public static ErroRespostaDto ApartirDe(ErroValidacao erro)
    {
        return new ErroRespostaDto
        {
            Campo = erro.Campo,
            Mensagem = erro.Mensagem,
        };
    }
}

/// <summary>
/// Envelope de resposta usado pela API para retornar a lista de erros de validação
/// nos casos de cadastro inválido (400) ou email duplicado (409).
/// Não expõe nenhum detalhe interno da aplicação: apenas o campo e a mensagem
/// já produzidos pelo <see cref="CadastroClientes.Core.Servicos.ClienteService"/>.
/// </summary>
public class RespostaDeErroDto
{
    public List<ErroRespostaDto> Erros { get; set; } = new();
}
