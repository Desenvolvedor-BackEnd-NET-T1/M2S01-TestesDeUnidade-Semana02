using CadastroClientes.Api.Repositorios;
using CadastroClientes.Core.Repositorios;
using CadastroClientes.Core.Servicos;

namespace CadastroClientes.Api;

/// <summary>
/// Configuração clássica da aplicação ASP.NET Core (estilo "Startup.cs"),
/// em vez do modelo de hospedagem mínima com endpoints definidos direto no Program.cs.
/// </summary>
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Repositório em memória registrado como singleton: uma única instância
        // compartilhada por toda a aplicação, com acesso seguro para requisições
        // concorrentes (ver ClienteRepositoryEmMemoria).
        services.AddSingleton<IClienteRepository, ClienteRepositoryEmMemoria>();

        // O serviço de cadastro não guarda estado próprio (apenas depende do
        // repositório), então também pode ser singleton.
        services.AddSingleton<ClienteService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
