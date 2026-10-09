using System.Collections.Concurrent;
using CadastroClientes.Core.Modelos;
using CadastroClientes.Core.Repositorios;

namespace CadastroClientes.Api.Repositorios;

/// <summary>
/// Implementação de <see cref="IClienteRepository"/> que mantém os clientes apenas
/// em memória, durante a execução do processo.
///
/// IMPORTANTE (apenas para este laboratório didático): esta é uma persistência
/// de demonstração. Os dados NÃO são salvos em disco ou em banco de dados — eles
/// desaparecem completamente quando a aplicação é encerrada.
///
/// É registrada como singleton no <see cref="Startup"/> (uma única instância para
/// toda a aplicação) e usa <see cref="ConcurrentDictionary{TKey,TValue}"/> para que
/// múltiplas requisições simultâneas possam ler e escrever com segurança, sem usar
/// uma lista estática global nem travas manuais.
/// </summary>
public class ClienteRepositoryEmMemoria : IClienteRepository
{
    private readonly ConcurrentDictionary<Guid, Cliente> _clientes = new();

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
