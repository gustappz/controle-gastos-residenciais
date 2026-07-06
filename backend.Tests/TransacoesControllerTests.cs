using ControleGastos.Api.Controllers;
using ControleGastos.Api.Dtos;
using ControleGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.Api.Tests;

/// <summary>Testes do cadastro de transações, com foco nas regras de negócio do desafio.</summary>
public class TransacoesControllerTests : TesteComBanco
{
    private TransacoesController CriarController() => new(Db);

    /// <summary>Cadastra uma pessoa direto no banco para servir de massa de teste.</summary>
    private async Task<Pessoa> CriarPessoa(string nome, int idade)
    {
        var pessoa = new Pessoa { Nome = nome, Idade = idade };
        Db.Pessoas.Add(pessoa);
        await Db.SaveChangesAsync();
        return pessoa;
    }

    [Fact]
    public async Task Criar_ReceitaParaMenorDeIdade_Retorna400()
    {
        // Regra central do desafio: menor de 18 anos não pode ter receita.
        var menor = await CriarPessoa("João", 15);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Mesada", 100m, "Receita", menor.Id));

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(Db.Transacoes);
    }

    [Fact]
    public async Task Criar_DespesaParaMenorDeIdade_Funciona()
    {
        // Menor pode ter DESPESA normalmente — a restrição é só para receitas.
        var menor = await CriarPessoa("João", 15);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Lanche", 25.50m, "Despesa", menor.Id));

        Assert.IsType<CreatedResult>(resultado);
        Assert.Single(Db.Transacoes);
    }

    [Fact]
    public async Task Criar_ReceitaParaQuemTemExatos18Anos_Funciona()
    {
        // Caso de fronteira: 18 anos já é maior de idade.
        var adulto = await CriarPessoa("Ana", 18);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Estágio", 900m, "Receita", adulto.Id));

        Assert.IsType<CreatedResult>(resultado);
    }

    [Fact]
    public async Task Criar_ComPessoaInexistente_Retorna400()
    {
        // Regra do desafio: a pessoa da transação precisa existir no cadastro.
        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Qualquer", 10m, "Despesa", 999));

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(Db.Transacoes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0.0)]
    [InlineData(-10.0)]
    public async Task Criar_ComValorInvalido_Retorna400(double? valor)
    {
        var pessoa = await CriarPessoa("Maria", 30);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Teste", (decimal?)valor, "Despesa", pessoa.Id));

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Investimento")]
    [InlineData("5")] // número fora da faixa do enum também deve ser recusado
    public async Task Criar_ComTipoInvalido_Retorna400(string? tipo)
    {
        var pessoa = await CriarPessoa("Maria", 30);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Teste", 10m, tipo, pessoa.Id));

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Criar_SemDescricao_Retorna400()
    {
        var pessoa = await CriarPessoa("Maria", 30);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("   ", 10m, "Despesa", pessoa.Id));

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Criar_TipoAceitaMaiusculasEMinusculas()
    {
        // "despesa" minúsculo também vale — evita erro bobo de integração.
        var pessoa = await CriarPessoa("Maria", 30);

        var resultado = await CriarController()
            .Criar(new CriarTransacaoRequest("Mercado", 50m, "despesa", pessoa.Id));

        Assert.IsType<CreatedResult>(resultado);
    }
}
