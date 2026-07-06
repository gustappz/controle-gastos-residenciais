// Tipos TypeScript espelhando os dados que a API devolve.
// Com eles, o editor e o compilador avisam na hora se algum campo for usado errado.

export interface Pessoa {
  id: number;
  nome: string;
  idade: number;
}

export type TipoTransacao = 'Despesa' | 'Receita';

export interface Transacao {
  id: number;
  descricao: string;
  valor: number;
  tipo: TipoTransacao;
  pessoaId: number;
  pessoaNome: string;
}

/** Totais de uma pessoa na consulta de totais. */
export interface TotaisPessoa {
  id: number;
  nome: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

/** Total geral da residência (todas as pessoas somadas). */
export interface TotalGeral {
  totalReceitas: number;
  totalDespesas: number;
  saldoLiquido: number;
}

/** Resposta completa do endpoint /api/totais. */
export interface Totais {
  pessoas: TotaisPessoa[];
  geral: TotalGeral;
}
