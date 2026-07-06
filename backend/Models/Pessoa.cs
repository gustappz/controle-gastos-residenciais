namespace ControleGastos.Api.Models;

/// <summary>
/// Pessoa da residência que pode possuir transações (receitas e despesas).
/// </summary>
public class Pessoa
{
    /// <summary>Identificador único, gerado automaticamente pelo banco.</summary>
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Idade em anos. Menores de 18 só podem registrar despesas.</summary>
    public int Idade { get; set; }

    /// <summary>
    /// Transações da pessoa. Propriedade de navegação usada pelo Entity Framework
    /// para montar o relacionamento 1-para-N entre Pessoa e Transacao.
    /// </summary>
    public List<Transacao> Transacoes { get; set; } = new();
}
