using CadastroClientes.Core.Modelos;

namespace CadastroClientes.Core.Repositorios;

/// <summary>
/// Abstração de persistência de clientes. O projeto Core depende apenas desta
/// interface; cada implementação concreta (em memória na API, ou uma implementação
/// de teste exclusiva da suíte) decide como armazenar os dados.
/// </summary>
public interface IClienteRepository
{
    /// <summary>
    /// Adiciona um cliente já validado ao repositório.
    /// </summary>
    void Adicionar(Cliente cliente);

    /// <summary>
    /// Retorna o cliente com o identificador informado, ou null se não existir.
    /// </summary>
    Cliente? ObterPorId(Guid id);

    /// <summary>
    /// Retorna todos os clientes cadastrados.
    /// </summary>
    IReadOnlyList<Cliente> Listar();

    /// <summary>
    /// Indica se já existe um cliente cadastrado com o email informado.
    /// A comparação deve ser feita sem diferenciar maiúsculas e minúsculas.
    /// </summary>
    bool ExisteClienteComEmail(string email);
}
