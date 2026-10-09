using System.Net.Mail;
using CadastroClientes.Core.Dtos;
using CadastroClientes.Core.Modelos;
using CadastroClientes.Core.Repositorios;
using CadastroClientes.Core.Validacao;

namespace CadastroClientes.Core.Servicos;

/// <summary>
/// Serviço responsável por validar e cadastrar um novo cliente.
///
/// Objetivo didático: todas as regras de negócio do cadastro estão explícitas aqui,
/// em métodos pequenos e nomeados por campo, para que o relatório de cobertura
/// (gerado a partir deste projeto) permita enxergar claramente quais condições
/// foram ou não exercitadas pelos testes.
///
/// As condições de cada campo seguem sempre a mesma ordem didática:
/// 1) obrigatoriedade, 2) tamanho mínimo, 3) tamanho máximo, 4) formato.
/// Isso facilita comparar, no relatório de cobertura, quais desses caminhos
/// cada etapa de testes passou a cobrir.
/// </summary>
public class ClienteService
{
    private const int NomeTamanhoMinimo = 3;
    private const int NomeTamanhoMaximo = 10;

    private const int EmailTamanhoMaximo = 100;

    private const int TelefoneTamanhoMinimo = 10;
    private const int TelefoneTamanhoMaximo = 11;

    private const int ProfissaoTamanhoMinimo = 2;
    private const int ProfissaoTamanhoMaximo = 80;

    private const int LogradouroTamanhoMinimo = 3;
    private const int LogradouroTamanhoMaximo = 100;

    private const int NumeroTamanhoMinimo = 1;
    private const int NumeroTamanhoMaximo = 10;

    private const int BairroTamanhoMinimo = 2;
    private const int BairroTamanhoMaximo = 60;

    private const int CidadeTamanhoMinimo = 2;
    private const int CidadeTamanhoMaximo = 60;

    private const int CepTamanho = 8;

    private static readonly string[] SexosValidos = { "F", "M", "NI" };

    private readonly IClienteRepository _repositorio;

    public ClienteService(IClienteRepository repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Valida e, se tudo estiver correto, cadastra um novo cliente.
    /// Nenhum cliente é persistido quando há qualquer erro de validação.
    /// </summary>
    public ResultadoCadastro Cadastrar(CadastroClienteDto dto)
    {
        // Remove espaços das extremidades antes de validar e persistir.
        var nome = dto.Nome?.Trim();
        var email = dto.Email?.Trim();
        var telefone = dto.Telefone?.Trim();
        var sexo = dto.Sexo?.Trim();
        var profissao = dto.Profissao?.Trim();
        var logradouro = dto.Logradouro?.Trim();
        var numero = dto.Numero?.Trim();
        var bairro = dto.Bairro?.Trim();
        var cidade = dto.Cidade?.Trim();
        var uf = dto.Uf?.Trim();
        var cep = dto.Cep?.Trim();

        var erros = new List<ErroValidacao>();

        ValidarNome(nome, erros);
        ValidarEmail(email, erros);
        ValidarTelefone(telefone, erros);
        ValidarSexo(sexo, erros);
        ValidarProfissao(profissao, erros);
        ValidarLogradouro(logradouro, erros);
        ValidarNumero(numero, erros);
        ValidarBairro(bairro, erros);
        ValidarCidade(cidade, erros);
        ValidarUf(uf, erros);
        ValidarCep(cep, erros);

        // A verificação de duplicidade só é feita quando o próprio email é válido:
        // não faz sentido consultar o repositório com um email malformado ou ausente.
        var emailJaTemErroDeFormato = erros.Any(erro => erro.Campo == nameof(Cliente.Email));
        var emailDuplicado = false;
        if (!emailJaTemErroDeFormato && _repositorio.ExisteClienteComEmail(email!))
        {
            emailDuplicado = true;
            erros.Add(new ErroValidacao(nameof(Cliente.Email), "Já existe um cliente cadastrado com este email."));
        }

        if (erros.Count > 0)
        {
            // Caminho de retorno de falha: a persistência nunca é chamada aqui.
            return ResultadoCadastro.ComFalha(erros, emailDuplicado);
        }

        // Caminho do cadastro válido: só chegamos até aqui quando nenhum erro foi acumulado.
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = nome!,
            Email = email!,
            Telefone = telefone!,
            Sexo = sexo!,
            Profissao = profissao!,
            Logradouro = logradouro!,
            Numero = numero!,
            Bairro = bairro!,
            Cidade = cidade!,
            Uf = uf!,
            Cep = cep!,
        };

        _repositorio.Adicionar(cliente);

        return ResultadoCadastro.ComSucesso(cliente);
    }

    private static void ValidarNome(string? nome, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Nome), "O nome é obrigatório."));
            return;
        }

        if (nome.Length < NomeTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Nome), $"O nome deve ter no mínimo {NomeTamanhoMinimo} caracteres."));
            return;
        }

        if (nome.Length > NomeTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Nome), $"O nome deve ter no máximo {NomeTamanhoMaximo} caracteres."));
        }
    }

    private static void ValidarEmail(string? email, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Email), "O email é obrigatório."));
            return;
        }

        if (email.Length > EmailTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Email), $"O email deve ter no máximo {EmailTamanhoMaximo} caracteres."));
            return;
        }

        if (!EmailTemFormatoValido(email))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Email), "O email informado não está em um formato válido."));
        }
    }

    /// <summary>
    /// Verifica apenas o FORMATO (sintaxe) do email usando o tipo <see cref="MailAddress"/>,
    /// um recurso padrão do próprio .NET (namespace System.Net.Mail).
    /// Observação didática importante: essa verificação não comprova que o endereço
    /// realmente existe ou que alguém recebe emails nele — apenas que a sintaxe é válida.
    /// </summary>
    private static bool EmailTemFormatoValido(string email)
    {
        try
        {
            var endereco = new MailAddress(email);
            return endereco.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static void ValidarTelefone(string? telefone, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(telefone))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Telefone), "O telefone é obrigatório."));
            return;
        }

        if (!ContemApenasDigitos(telefone))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Telefone), "O telefone deve conter somente dígitos, sem pontuação."));
            return;
        }

        if (telefone.Length < TelefoneTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Telefone), $"O telefone deve ter no mínimo {TelefoneTamanhoMinimo} dígitos."));
            return;
        }

        if (telefone.Length > TelefoneTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Telefone), $"O telefone deve ter no máximo {TelefoneTamanhoMaximo} dígitos."));
        }
    }

    private static void ValidarSexo(string? sexo, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(sexo))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Sexo), "O sexo é obrigatório (utilize F, M ou NI para não informado)."));
            return;
        }

        if (!SexosValidos.Contains(sexo))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Sexo), "O sexo deve ser um dos códigos aceitos: F, M ou NI."));
        }
    }

    private static void ValidarProfissao(string? profissao, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(profissao))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Profissao), "A profissão é obrigatória."));
            return;
        }

        if (profissao.Length < ProfissaoTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Profissao), $"A profissão deve ter no mínimo {ProfissaoTamanhoMinimo} caracteres."));
            return;
        }

        if (profissao.Length > ProfissaoTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Profissao), $"A profissão deve ter no máximo {ProfissaoTamanhoMaximo} caracteres."));
        }
    }

    private static void ValidarLogradouro(string? logradouro, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Logradouro), "O logradouro é obrigatório."));
            return;
        }

        if (logradouro.Length < LogradouroTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Logradouro), $"O logradouro deve ter no mínimo {LogradouroTamanhoMinimo} caracteres."));
            return;
        }

        if (logradouro.Length > LogradouroTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Logradouro), $"O logradouro deve ter no máximo {LogradouroTamanhoMaximo} caracteres."));
        }
    }

    /// <summary>
    /// Número do endereço. Aceita o valor especial "S/N" (sem número), que já se
    /// encaixa naturalmente dentro do limite de 1 a 10 caracteres — não é preciso
    /// nenhuma regra extra para permiti-lo.
    /// </summary>
    private static void ValidarNumero(string? numero, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Numero), "O número do endereço é obrigatório (utilize S/N quando não houver número)."));
            return;
        }

        if (numero.Length < NumeroTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Numero), $"O número do endereço deve ter no mínimo {NumeroTamanhoMinimo} caractere."));
            return;
        }

        if (numero.Length > NumeroTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Numero), $"O número do endereço deve ter no máximo {NumeroTamanhoMaximo} caracteres."));
        }
    }

    private static void ValidarBairro(string? bairro, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(bairro))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Bairro), "O bairro é obrigatório."));
            return;
        }

        if (bairro.Length < BairroTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Bairro), $"O bairro deve ter no mínimo {BairroTamanhoMinimo} caracteres."));
            return;
        }

        if (bairro.Length > BairroTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Bairro), $"O bairro deve ter no máximo {BairroTamanhoMaximo} caracteres."));
        }
    }

    private static void ValidarCidade(string? cidade, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(cidade))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Cidade), "A cidade é obrigatória."));
            return;
        }

        if (cidade.Length < CidadeTamanhoMinimo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Cidade), $"A cidade deve ter no mínimo {CidadeTamanhoMinimo} caracteres."));
            return;
        }

        if (cidade.Length > CidadeTamanhoMaximo)
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Cidade), $"A cidade deve ter no máximo {CidadeTamanhoMaximo} caracteres."));
        }
    }

    private static void ValidarUf(string? uf, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(uf))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Uf), "A UF é obrigatória."));
            return;
        }

        if (uf.Length != 2 || !uf.All(char.IsLetter))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Uf), "A UF deve conter exatamente duas letras."));
            return;
        }

        if (!UfsBrasileiras.EhValida(uf))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Uf), "A UF informada não pertence à lista de UFs brasileiras."));
        }
    }

    private static void ValidarCep(string? cep, List<ErroValidacao> erros)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Cep), "O CEP é obrigatório."));
            return;
        }

        if (cep.Length != CepTamanho || !cep.All(char.IsDigit))
        {
            erros.Add(new ErroValidacao(nameof(Cliente.Cep), $"O CEP deve conter exatamente {CepTamanho} dígitos, sem pontuação."));
        }
    }

    private static bool ContemApenasDigitos(string valor)
    {
        foreach (var caractere in valor)
        {
            if (!char.IsDigit(caractere))
            {
                return false;
            }
        }

        return true;
    }
}
