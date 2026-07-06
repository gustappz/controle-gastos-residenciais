using ControleGastos.Api.Data;
using ControleGastos.Api.Dtos;
using ControleGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Api.Controllers;

/// <summary>
/// Endpoint da consulta de totais: receitas, despesas e saldo por pessoa,
/// mais o total geral da residência.
/// </summary>
[ApiController]
[Route("api/totais")]
public class TotaisController : ControllerBase
{
    private readonly AppDbContext _db;

    public TotaisController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Consultar()
    {
        // Carrega as pessoas já com suas transações e calcula os totais em memória.
        // Partir das pessoas (e não das transações) garante que quem ainda não tem
        // nenhum lançamento também apareça na listagem, com os valores zerados.
        var pessoas = await _db.Pessoas
            .Include(pessoa => pessoa.Transacoes)
            .OrderBy(pessoa => pessoa.Nome)
            .ToListAsync();

        var totaisPorPessoa = pessoas.Select(pessoa =>
        {
            var totalReceitas = pessoa.Transacoes
                .Where(transacao => transacao.Tipo == TipoTransacao.Receita)
                .Sum(transacao => transacao.Valor);

            var totalDespesas = pessoa.Transacoes
                .Where(transacao => transacao.Tipo == TipoTransacao.Despesa)
                .Sum(transacao => transacao.Valor);

            return new TotaisPessoaResponse(
                pessoa.Id,
                pessoa.Nome,
                totalReceitas,
                totalDespesas,
                Saldo: totalReceitas - totalDespesas); // saldo = receitas - despesas
        }).ToList();

        // Total geral da residência: soma dos totais individuais de cada pessoa.
        var totalGeral = new TotalGeralResponse(
            totaisPorPessoa.Sum(total => total.TotalReceitas),
            totaisPorPessoa.Sum(total => total.TotalDespesas),
            SaldoLiquido: totaisPorPessoa.Sum(total => total.Saldo));

        return Ok(new TotaisResponse(totaisPorPessoa, totalGeral));
    }
}
