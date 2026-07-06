using ControleGastos.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Api.Tests;

/// <summary>
/// Base dos testes: cria um banco SQLite em memória novo para cada teste,
/// garantindo isolamento total (nenhum teste enxerga dados de outro).
/// Usar SQLite (e não um banco fake) mantém o comportamento igual ao de
/// produção — inclusive a deleção em cascata configurada no modelo.
/// </summary>
public abstract class TesteComBanco : IDisposable
{
    private readonly SqliteConnection _conexao;

    protected AppDbContext Db { get; }

    protected TesteComBanco()
    {
        // O banco em memória existe enquanto esta conexão estiver aberta.
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexao)
            .Options;

        Db = new AppDbContext(opcoes);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _conexao.Dispose();
    }
}
