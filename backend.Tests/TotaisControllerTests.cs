using ControleGastos.Api.Controllers;
using ControleGastos.Api.Dtos;
using ControleGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.Api.Tests;

/// <summary>Testes da consulta de totais: cálculos por pessoa e total geral.</summary>
public class TotaisControllerTests : TesteComBanco
{
    private async Task<TotaisResponse> Consultar()
    {
        var resultado = await new TotaisController(Db).Consultar();
        var ok = Assert.IsType<OkObjectResult>(resultado);
        return Assert.IsType<TotaisResponse>(ok.Value);
    }

    [Fact]
    public async Task Consultar_PessoaSemTransacoes_ApareceComValoresZerados()
    {
        // Requisito do desafio: listar TODAS as pessoas, mesmo sem lançamentos.
        Db.Pessoas.Add(new Pessoa { Nome = "Ana", Idade = 17 });
        await Db.SaveChangesAsync();

        var totais = await Consultar();

        var ana = Assert.Single(totais.Pessoas);
        Assert.Equal(0m, ana.TotalReceitas);
        Assert.Equal(0m, ana.TotalDespesas);
        Assert.Equal(0m, ana.Saldo);
    }

    [Fact]
    public async Task Consultar_CalculaSaldoPorPessoaETotalGeral()
    {
        var maria = new Pessoa { Nome = "Maria", Idade = 30 };
        maria.Transacoes.Add(new Transacao { Descricao = "Salário", Valor = 3500m, Tipo = TipoTransacao.Receita });
        maria.Transacoes.Add(new Transacao { Descricao = "Mercado", Valor = 450.75m, Tipo = TipoTransacao.Despesa });

        var joao = new Pessoa { Nome = "João", Idade = 15 };
        joao.Transacoes.Add(new Transacao { Descricao = "Lanche", Valor = 25.50m, Tipo = TipoTransacao.Despesa });

        Db.Pessoas.AddRange(maria, joao);
        await Db.SaveChangesAsync();

        var totais = await Consultar();

        // Totais individuais (saldo = receitas - despesas).
        var totalMaria = totais.Pessoas.Single(pessoa => pessoa.Nome == "Maria");
        Assert.Equal(3500m, totalMaria.TotalReceitas);
        Assert.Equal(450.75m, totalMaria.TotalDespesas);
        Assert.Equal(3049.25m, totalMaria.Saldo);

        var totalJoao = totais.Pessoas.Single(pessoa => pessoa.Nome == "João");
        Assert.Equal(0m, totalJoao.TotalReceitas);
        Assert.Equal(-25.50m, totalJoao.Saldo); // saldo negativo é permitido

        // Total geral da residência.
        Assert.Equal(3500m, totais.Geral.TotalReceitas);
        Assert.Equal(476.25m, totais.Geral.TotalDespesas);
        Assert.Equal(3023.75m, totais.Geral.SaldoLiquido);
    }

    [Fact]
    public async Task Consultar_SemPessoas_RetornaListaVaziaETotalZerado()
    {
        var totais = await Consultar();

        Assert.Empty(totais.Pessoas);
        Assert.Equal(0m, totais.Geral.SaldoLiquido);
    }
}
