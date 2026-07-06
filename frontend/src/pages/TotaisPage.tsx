import { useEffect, useState } from 'react';
import { api, mensagemDeErro } from '../api';
import { formatarMoeda } from '../formatadores';
import type { Totais } from '../types';

/**
 * Tela de consulta de totais: receitas, despesas e saldo de cada pessoa,
 * com o total geral da residência ao final da listagem.
 */
export default function TotaisPage() {
  const [totais, setTotais] = useState<Totais | null>(null);
  const [erro, setErro] = useState('');

  useEffect(() => {
    api
      .consultarTotais()
      .then(setTotais)
      .catch((excecao) => setErro(mensagemDeErro(excecao)));
  }, []);

  if (erro) return <p className="mensagem-erro">{erro}</p>;
  if (!totais) return <p>Carregando...</p>;

  const { pessoas, geral } = totais;

  return (
    <section className="cartao">
      <h2>Totais por pessoa</h2>

      {pessoas.length === 0 ? (
        <p className="vazio">Nenhuma pessoa cadastrada ainda.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Pessoa</th>
              <th className="direita">Receitas</th>
              <th className="direita">Despesas</th>
              <th className="direita">Saldo</th>
            </tr>
          </thead>
          <tbody>
            {pessoas.map((pessoa) => (
              <tr key={pessoa.id}>
                <td>{pessoa.nome}</td>
                <td className="direita positivo">{formatarMoeda(pessoa.totalReceitas)}</td>
                <td className="direita negativo">{formatarMoeda(pessoa.totalDespesas)}</td>
                {/* Saldo em vermelho quando a pessoa gastou mais do que recebeu */}
                <td className={pessoa.saldo < 0 ? 'direita negativo' : 'direita positivo'}>
                  {formatarMoeda(pessoa.saldo)}
                </td>
              </tr>
            ))}
          </tbody>
          {/* Linha final com o total geral de todas as pessoas (requisito do desafio) */}
          <tfoot>
            <tr>
              <td>Total geral</td>
              <td className="direita positivo">{formatarMoeda(geral.totalReceitas)}</td>
              <td className="direita negativo">{formatarMoeda(geral.totalDespesas)}</td>
              <td className={geral.saldoLiquido < 0 ? 'direita negativo' : 'direita positivo'}>
                {formatarMoeda(geral.saldoLiquido)}
              </td>
            </tr>
          </tfoot>
        </table>
      )}
    </section>
  );
}
