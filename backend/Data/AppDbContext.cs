using ControleGastos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleGastos.Api.Data;

/// <summary>
/// Ponte entre as classes do sistema e o banco de dados (Entity Framework Core).
/// Cada DbSet abaixo vira uma tabela no SQLite.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opcoes) : base(opcoes) { }

    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relacionamento 1-para-N: uma pessoa possui várias transações.
        // O DeleteBehavior.Cascade implementa a regra do desafio: ao excluir
        // uma pessoa, o banco apaga automaticamente todas as transações dela.
        modelBuilder.Entity<Transacao>()
            .HasOne(transacao => transacao.Pessoa)
            .WithMany(pessoa => pessoa.Transacoes)
            .HasForeignKey(transacao => transacao.PessoaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Os limites de tamanho são metadados do modelo; como o SQLite não impõe
        // tamanho de texto, quem garante mesmo são as validações nos controllers.
        modelBuilder.Entity<Pessoa>()
            .Property(pessoa => pessoa.Nome)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Transacao>()
            .Property(transacao => transacao.Descricao)
            .IsRequired()
            .HasMaxLength(200);
    }
}
