namespace ControleGastos.Api.Dtos;

/// <summary>
/// Dados recebidos pela API no cadastro de uma pessoa.
/// Os campos são anuláveis de propósito: assim dá para diferenciar "campo não
/// enviado" de "valor zero" e devolver mensagens de validação claras.
/// </summary>
public record CriarPessoaRequest(string? Nome, int? Idade);
