using CadastroClientes.Api.Dtos;
using CadastroClientes.Core.Dtos;
using CadastroClientes.Core.Repositorios;
using CadastroClientes.Core.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace CadastroClientes.Api.Controllers;

/// <summary>
/// Endpoints HTTP de cadastro de clientes.
/// Este controller é apenas uma camada fina: toda a regra de negócio e validação
/// vive no <see cref="ClienteService"/> (projeto Core). O controller só traduz
/// entrada/saída HTTP e escolhe o status code adequado para cada resultado.
/// </summary>
[ApiController]
[Route("clientes")]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;
    private readonly IClienteRepository _repositorio;

    public ClientesController(ClienteService clienteService, IClienteRepository repositorio)
    {
        _clienteService = clienteService;
        _repositorio = repositorio;
    }

    /// <summary>
    /// Cadastra um novo cliente.
    /// 201 + Location quando válido; 400 com erros por campo quando inválido;
    /// 409 quando o email já está cadastrado.
    /// </summary>
    [HttpPost]
    public IActionResult Cadastrar([FromBody] CadastroClienteDto dto)
    {
        var resultado = _clienteService.Cadastrar(dto);

        if (!resultado.Sucesso)
        {
            var respostaDeErro = new RespostaDeErroDto
            {
                Erros = resultado.Erros.Select(ErroRespostaDto.ApartirDe).ToList(),
            };

            if (resultado.EmailDuplicado)
            {
                return Conflict(respostaDeErro);
            }

            return BadRequest(respostaDeErro);
        }

        var clienteCadastrado = ClienteRespostaDto.ApartirDe(resultado.Cliente!);

        return CreatedAtAction(nameof(ObterPorId), new { id = clienteCadastrado.Id }, clienteCadastrado);
    }

    /// <summary>
    /// Lista todos os clientes cadastrados nesta execução da aplicação.
    /// </summary>
    [HttpGet]
    public IActionResult Listar()
    {
        var clientes = _repositorio.Listar().Select(ClienteRespostaDto.ApartirDe);

        return Ok(clientes);
    }

    /// <summary>
    /// Consulta um cliente pelo identificador. 404 quando não existe.
    /// </summary>
    [HttpGet("{id:guid}")]
    public IActionResult ObterPorId(Guid id)
    {
        var cliente = _repositorio.ObterPorId(id);

        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(ClienteRespostaDto.ApartirDe(cliente));
    }
}
