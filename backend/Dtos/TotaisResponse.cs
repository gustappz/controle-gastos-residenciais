namespace ControleGastos.Api.Dtos;

/// <summary>Totais de uma pessoa na consulta de totais.</summary>
public record TotaisPessoaResponse(
    int Id,
    string Nome,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo);

/// <summary>Total geral da residência (todas as pessoas somadas).</summary>
public record TotalGeralResponse(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal SaldoLiquido);

/// <summary>Resposta completa do endpoint de consulta de totais.</summary>
public record TotaisResponse(
    List<TotaisPessoaResponse> Pessoas,
    TotalGeralResponse Geral);
