using CadastroClientes.Core.Modelos;

namespace CadastroClientes.Core.Dtos;

/// <summary>
/// Resultado estruturado do cadastro de um cliente.
/// Em vez de depender de exceções genéricas para sinalizar entrada inválida,
/// o serviço sempre retorna um objeto deste tipo, com um indicador de sucesso,
/// o cliente cadastrado (quando aplicável) e a lista completa de erros encontrados.
/// </summary>
public class ResultadoCadastro
{
    private ResultadoCadastro(bool sucesso, Cliente? cliente, IReadOnlyList<ErroValidacao> erros, bool emailDuplicado)
    {
        Sucesso = sucesso;
        Cliente = cliente;
        Erros = erros;
        EmailDuplicado = emailDuplicado;
    }

    /// <summary>
    /// Indica se o cadastro foi realizado (e, portanto, persistido) com sucesso.
    /// </summary>
    public bool Sucesso { get; }

    /// <summary>
    /// Cliente efetivamente cadastrado. Só é preenchido quando <see cref="Sucesso"/> é verdadeiro.
    /// </summary>
    public Cliente? Cliente { get; }

    /// <summary>
    /// Lista de erros de validação encontrados. Vazia quando o cadastro teve sucesso.
    /// </summary>
    public IReadOnlyList<ErroValidacao> Erros { get; }

    /// <summary>
    /// Indica especificamente que a falha ocorreu (entre outros motivos possíveis)
    /// porque já existe um cliente cadastrado com o mesmo email. A API usa esse
    /// indicador para decidir entre responder 400 (entrada inválida) ou 409 (conflito).
    /// </summary>
    public bool EmailDuplicado { get; }

    public static ResultadoCadastro ComSucesso(Cliente cliente)
    {
        return new ResultadoCadastro(sucesso: true, cliente, Array.Empty<ErroValidacao>(), emailDuplicado: false);
    }

    public static ResultadoCadastro ComFalha(IReadOnlyList<ErroValidacao> erros, bool emailDuplicado = false)
    {
        return new ResultadoCadastro(sucesso: false, cliente: null, erros, emailDuplicado);
    }
}
