namespace CadastroClientes.Api;

/// <summary>
/// Ponto de entrada da aplicação, no estilo clássico do ASP.NET Core
/// (Host.CreateDefaultBuilder + UseStartup), e não no modelo de hospedagem mínima
/// (minimal API) com WebApplication.CreateBuilder/endpoints no próprio Program.cs.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
