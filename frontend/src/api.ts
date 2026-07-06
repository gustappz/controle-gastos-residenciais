import type { Pessoa, Totais, Transacao } from './types';

// Endereço base da API (o back-end sobe em http://localhost:5000 — ver README).
const API_URL = 'http://localhost:5000/api';

/**
 * Função auxiliar que centraliza as chamadas HTTP.
 * Quando a API responde com erro, lê o corpo ({ erro: "mensagem" }) e lança uma
 * exceção com essa mensagem, para as telas exibirem o aviso ao usuário.
 */
async function requisitar<T>(caminho: string, opcoes?: RequestInit): Promise<T> {
  const resposta = await fetch(`${API_URL}${caminho}`, {
    headers: { 'Content-Type': 'application/json' },
    ...opcoes,
  });

  if (!resposta.ok) {
    const corpo = await resposta.json().catch(() => null);
    throw new Error(corpo?.erro ?? `Falha na requisição (HTTP ${resposta.status}).`);
  }

  // 204 No Content (resposta da exclusão) não tem corpo para converter.
  if (resposta.status === 204) {
    return undefined as T;
  }

  return resposta.json();
}

/** Converte qualquer exceção em uma mensagem amigável para exibir na tela. */
export function mensagemDeErro(excecao: unknown): string {
  // O fetch lança TypeError quando não consegue alcançar o servidor.
  if (excecao instanceof TypeError) {
    return 'Não foi possível conectar à API. Verifique se o back-end está rodando.';
  }
  return excecao instanceof Error ? excecao.message : 'Ocorreu um erro inesperado.';
}

/** Funções de acesso à API, uma para cada endpoint do back-end. */
export const api = {
  listarPessoas: () => requisitar<Pessoa[]>('/pessoas'),

  criarPessoa: (nome: string, idade: number) =>
    requisitar<Pessoa>('/pessoas', {
      method: 'POST',
      body: JSON.stringify({ nome, idade }),
    }),

  excluirPessoa: (id: number) => requisitar<void>(`/pessoas/${id}`, { method: 'DELETE' }),

  listarTransacoes: () => requisitar<Transacao[]>('/transacoes'),

  criarTransacao: (dados: { descricao: string; valor: number; tipo: string; pessoaId: number }) =>
    requisitar<Transacao>('/transacoes', {
      method: 'POST',
      body: JSON.stringify(dados),
    }),

  consultarTotais: () => requisitar<Totais>('/totais'),
};
