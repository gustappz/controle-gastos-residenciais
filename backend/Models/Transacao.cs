namespace ControleGastos.Api.Models;

/// <summary>
/// Movimentação financeira (receita ou despesa) vinculada a uma pessoa.
/// </summary>
public class Transacao
{
    /// <summary>Identificador único, gerado automaticamente pelo banco.</summary>
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Valor da transação, sempre positivo (o que define se ele soma ou subtrai
    /// no saldo é o Tipo). O decimal é o tipo indicado para dinheiro por não
    /// sofrer os erros de arredondamento do float/double.
    /// </summary>
    public decimal Valor { get; set; }

    public TipoTransacao Tipo { get; set; }

    /// <summary>Chave estrangeira: identifica a pessoa dona da transação.</summary>
    public int PessoaId { get; set; }

    /// <summary>Propriedade de navegação para a pessoa dona da transação.</summary>
    public Pessoa? Pessoa { get; set; }
}
