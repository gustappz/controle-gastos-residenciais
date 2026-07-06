using ControleGastos.Api.Data;
using ControleGastos.Api.Dtos;
using ControleGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Api.Controllers;

/// <summary>
/// Endpoints do cadastro de transações: criação e listagem
/// (edição e exclusão não fazem parte do escopo do desafio).
/// </summary>
[ApiController]
[Route("api/transacoes")]
public class TransacoesController : ControllerBase
{
    private const int MaioridadeEmAnos = 18;
    private const int TamanhoMaximoDescricao = 200;

    private readonly AppDbContext _db;

    public TransacoesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lista todas as transações, já com o nome da pessoa de cada uma.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var transacoes = await _db.Transacoes
            .OrderByDescending(transacao => transacao.Id) // mais recentes primeiro
            .Select(transacao => new
            {
                transacao.Id,
                transacao.Descricao,
                transacao.Valor,
                Tipo = transacao.Tipo.ToString(),
                transacao.PessoaId,
                PessoaNome = transacao.Pessoa!.Nome
            })
            .ToListAsync();

        return Ok(transacoes);
    }

    /// <summary>Cadastra uma nova transação, aplicando as regras de negócio do desafio.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar(CriarTransacaoRequest request)
    {
        // Validações de entrada.
        if (string.IsNullOrWhiteSpace(request.Descricao))
            return BadRequest(new { erro = "A descrição é obrigatória." });

        var descricao = request.Descricao.Trim();
        if (descricao.Length > TamanhoMaximoDescricao)
            return BadRequest(new { erro = $"A descrição pode ter no máximo {TamanhoMaximoDescricao} caracteres." });

        if (request.Valor is null or <= 0)
            return BadRequest(new { erro = "O valor deve ser maior que zero." });

        // Converte o texto recebido ("Despesa"/"Receita") para o enum, sem
        // diferenciar maiúsculas. O IsDefined barra valores numéricos fora da faixa.
        if (!Enum.TryParse<TipoTransacao>(request.Tipo, ignoreCase: true, out var tipo)
            || !Enum.IsDefined(tipo))
            return BadRequest(new { erro = "Tipo inválido: informe 'Despesa' ou 'Receita'." });

        if (request.PessoaId is null)
            return BadRequest(new { erro = "Informe a pessoa da transação." });

        // Regra do desafio: a pessoa informada precisa existir no cadastro.
        var pessoa = await _db.Pessoas.FindAsync(request.PessoaId.Value);
        if (pessoa is null)
            return BadRequest(new { erro = "Pessoa não encontrada: cadastre a pessoa antes de lançar transações." });

        // Regra do desafio: menores de 18 anos só podem cadastrar despesas.
        if (pessoa.Idade < MaioridadeEmAnos && tipo == TipoTransacao.Receita)
            return BadRequest(new { erro = "Pessoas menores de 18 anos podem cadastrar apenas despesas." });

        var transacao = new Transacao
        {
            Descricao = descricao,
            Valor = request.Valor.Value,
            Tipo = tipo,
            PessoaId = pessoa.Id
        };

        _db.Transacoes.Add(transacao);
        await _db.SaveChangesAsync(); // ao salvar, o banco gera o Id automaticamente

        return Created($"api/transacoes/{transacao.Id}", new
        {
            transacao.Id,
            transacao.Descricao,
            transacao.Valor,
            Tipo = transacao.Tipo.ToString(),
            transacao.PessoaId,
            PessoaNome = pessoa.Nome
        });
    }
}
