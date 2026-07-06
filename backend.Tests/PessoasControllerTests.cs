using ControleGastos.Api.Controllers;
using ControleGastos.Api.Dtos;
using ControleGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControleGastos.Api.Tests;

/// <summary>Testes do cadastro de pessoas: criação, validações e exclusão em cascata.</summary>
public class PessoasControllerTests : TesteComBanco
{
    private PessoasController CriarController() => new(Db);

    [Fact]
    public async Task Criar_GeraIdentificadorAutomaticamente()
    {
        var resultado = await CriarController().Criar(new CriarPessoaRequest("Maria", 30));

        Assert.IsType<CreatedResult>(resultado);
        var pessoa = Assert.Single(Db.Pessoas);
        Assert.True(pessoa.Id > 0); // o banco gerou o identificador
        Assert.Equal("Maria", pessoa.Nome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Criar_SemNome_Retorna400(string? nome)
    {
        var resultado = await CriarController().Criar(new CriarPessoaRequest(nome, 30));

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(Db.Pessoas);
    }

    [Fact]
    public async Task Criar_ComNomeAcimaDoLimite_Retorna400()
    {
        var nomeGigante = new string('a', 101); // limite é 100

        var resultado = await CriarController().Criar(new CriarPessoaRequest(nomeGigante, 30));

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(Db.Pessoas);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    public async Task Criar_ComIdadeInvalida_Retorna400(int? idade)
    {
        var resultado = await CriarController().Criar(new CriarPessoaRequest("Maria", idade));

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(Db.Pessoas);
    }

    [Fact]
    public async Task Excluir_ApagaTransacoesDaPessoaEmCascata()
    {
        // Regra do desafio: excluir a pessoa deve apagar todas as transações dela.
        var pessoa = new Pessoa { Nome = "Carlos", Idade = 40 };
        pessoa.Transacoes.Add(new Transacao { Descricao = "Aluguel", Valor = 1200m, Tipo = TipoTransacao.Despesa });
        pessoa.Transacoes.Add(new Transacao { Descricao = "Freela", Valor = 800m, Tipo = TipoTransacao.Receita });
        Db.Pessoas.Add(pessoa);
        await Db.SaveChangesAsync();

        var resultado = await CriarController().Excluir(pessoa.Id);

        Assert.IsType<NoContentResult>(resultado);
        Assert.Empty(Db.Pessoas);
        Assert.Empty(Db.Transacoes); // as transações foram apagadas junto
    }

    [Fact]
    public async Task Excluir_NaoAfetaTransacoesDeOutrasPessoas()
    {
        var carlos = new Pessoa { Nome = "Carlos", Idade = 40 };
        carlos.Transacoes.Add(new Transacao { Descricao = "Aluguel", Valor = 1200m, Tipo = TipoTransacao.Despesa });
        var maria = new Pessoa { Nome = "Maria", Idade = 30 };
        maria.Transacoes.Add(new Transacao { Descricao = "Salário", Valor = 3500m, Tipo = TipoTransacao.Receita });
        Db.Pessoas.AddRange(carlos, maria);
        await Db.SaveChangesAsync();

        await CriarController().Excluir(carlos.Id);

        var transacaoRestante = Assert.Single(Db.Transacoes);
        Assert.Equal("Salário", transacaoRestante.Descricao); // a da Maria ficou intacta
    }

    [Fact]
    public async Task Excluir_PessoaInexistente_Retorna404()
    {
        var resultado = await CriarController().Excluir(999);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
