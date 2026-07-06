namespace ControleGastos.Api.Dtos;

/// <summary>
/// Dados recebidos pela API no cadastro de uma transação.
/// O tipo chega como texto ("Despesa" ou "Receita") e é validado no controller.
/// </summary>
public record CriarTransacaoRequest(string? Descricao, decimal? Valor, string? Tipo, int? PessoaId);
