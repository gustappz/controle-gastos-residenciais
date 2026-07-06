using ControleGastos.Api.Data;
using ControleGastos.Api.Dtos;
using ControleGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Api.Controllers;

/// <summary>
/// Endpoints do cadastro de pessoas: criação, listagem e exclusão.
/// </summary>
[ApiController]
[Route("api/pessoas")]
public class PessoasController : ControllerBase
{
    private const int TamanhoMaximoNome = 100;

    private readonly AppDbContext _db;

    // O AppDbContext chega pronto via injeção de dependência (configurada no Program.cs).
    public PessoasController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lista todas as pessoas cadastradas, em ordem alfabética.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var pessoas = await _db.Pessoas
            .OrderBy(pessoa => pessoa.Nome)
            .Select(pessoa => new { pessoa.Id, pessoa.Nome, pessoa.Idade })
            .ToListAsync();

        return Ok(pessoas);
    }

    /// <summary>Cadastra uma nova pessoa. O identificador é gerado pelo banco.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar(CriarPessoaRequest request)
    {
        // Validações de entrada: devolvem 400 (Bad Request) com uma mensagem clara.
        if (string.IsNullOrWhiteSpace(request.Nome))
            return BadRequest(new { erro = "O nome é obrigatório." });

        var nome = request.Nome.Trim();
        if (nome.Length > TamanhoMaximoNome)
            return BadRequest(new { erro = $"O nome pode ter no máximo {TamanhoMaximoNome} caracteres." });

        if (request.Idade is null or < 0)
            return BadRequest(new { erro = "A idade deve ser um número maior ou igual a zero." });

        var pessoa = new Pessoa
        {
            Nome = nome,
            Idade = request.Idade.Value
        };

        _db.Pessoas.Add(pessoa);
        await _db.SaveChangesAsync(); // ao salvar, o banco gera o Id automaticamente

        return Created($"api/pessoas/{pessoa.Id}", new { pessoa.Id, pessoa.Nome, pessoa.Idade });
    }

    /// <summary>
    /// Exclui uma pessoa. As transações dela são apagadas junto, em cascata
    /// (comportamento configurado no relacionamento, em AppDbContext).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var pessoa = await _db.Pessoas.FindAsync(id);

        if (pessoa is null)
            return NotFound(new { erro = "Pessoa não encontrada." });

        _db.Pessoas.Remove(pessoa);
        await _db.SaveChangesAsync();

        return NoContent(); // 204: exclusão concluída, sem corpo na resposta
    }
}
