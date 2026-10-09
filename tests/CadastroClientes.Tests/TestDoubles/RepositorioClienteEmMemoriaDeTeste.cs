using CadastroClientes.Core.Modelos;
using CadastroClientes.Core.Repositorios;

namespace CadastroClientes.Tests.TestDoubles;

/// <summary>
/// Implementação simples de <see cref="IClienteRepository"/> exclusiva da suíte de
/// testes. Não é um mock: é um repositório real, só que guardado em um dicionário
/// comum, sem a complexidade de concorrência necessária em produção (ver
/// CadastroClientes.Api.Repositorios.ClienteRepositoryEmMemoria).
///
/// Cada teste deve criar sua própria instância para evitar vazamento de estado
/// entre testes.
/// </summary>
public class RepositorioClienteEmMemoriaDeTeste : IClienteRepository
{
    private readonly Dictionary<Guid, Cliente> _clientes = new();

    public void Adicionar(Cliente cliente)
    {
        _clientes[cliente.Id] = cliente;
    }

    public Cliente? ObterPorId(Guid id)
    {
        return _clientes.TryGetValue(id, out var cliente) ? cliente : null;
    }

    public IReadOnlyList<Cliente> Listar()
    {
        return _clientes.Values.ToList();
    }

    public bool ExisteClienteComEmail(string email)
    {
        return _clientes.Values.Any(cliente =>
            string.Equals(cliente.Email, email, StringComparison.OrdinalIgnoreCase));
    }
}
