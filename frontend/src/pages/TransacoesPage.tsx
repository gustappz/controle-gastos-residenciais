import { useEffect, useState, type FormEvent } from 'react';
import { api, mensagemDeErro } from '../api';
import { formatarMoeda } from '../formatadores';
import type { Pessoa, Transacao } from '../types';

/**
 * Tela de transações: formulário de criação + listagem.
 * (Edição e exclusão de transações estão fora do escopo do desafio.)
 */
export default function TransacoesPage() {
  const [transacoes, setTransacoes] = useState<Transacao[]>([]);
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState('');
  const [tipo, setTipo] = useState('Despesa');
  const [pessoaId, setPessoaId] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);

  // Pessoa escolhida no formulário (se houver) e se ela é menor de idade.
  const pessoaSelecionada = pessoas.find((pessoa) => pessoa.id === Number(pessoaId));
  const menorDeIdade = pessoaSelecionada !== undefined && pessoaSelecionada.idade < 18;

  useEffect(() => {
    carregar();
  }, []);

  // Menor de idade só pode ter despesas: se a pessoa escolhida for menor,
  // o tipo volta para "Despesa". A API valida de novo no cadastro — a
  // interface apenas evita que o usuário monte um pedido que seria recusado.
  useEffect(() => {
    if (menorDeIdade) setTipo('Despesa');
  }, [menorDeIdade]);

  async function carregar() {
    try {
      // As duas buscas são independentes, então rodam em paralelo.
      const [listaTransacoes, listaPessoas] = await Promise.all([
        api.listarTransacoes(),
        api.listarPessoas(),
      ]);
      setTransacoes(listaTransacoes);
      setPessoas(listaPessoas);
      setErro('');
    } catch (excecao) {
      setErro(mensagemDeErro(excecao));
    } finally {
      setCarregando(false);
    }
  }

  async function cadastrar(evento: FormEvent) {
    evento.preventDefault();

    try {
      await api.criarTransacao({
        descricao: descricao.trim(),
        valor: Number(valor),
        tipo,
        pessoaId: Number(pessoaId),
      });
      setDescricao('');
      setValor('');
      await carregar();
    } catch (excecao) {
      setErro(mensagemDeErro(excecao));
    }
  }

  return (
    <>
      <section className="cartao">
        <h2>Nova transação</h2>

        {!carregando && pessoas.length === 0 ? (
          <p className="aviso">Cadastre uma pessoa primeiro para poder lançar transações.</p>
        ) : (
          <form className="formulario" onSubmit={cadastrar}>
            <label>
              Descrição
              <input
                value={descricao}
                onChange={(evento) => setDescricao(evento.target.value)}
                placeholder="Ex.: Supermercado"
                maxLength={200}
                required
              />
            </label>

            <label>
              Valor (R$)
              <input
                type="number"
                min="0.01"
                step="0.01"
                value={valor}
                onChange={(evento) => setValor(evento.target.value)}
                placeholder="Ex.: 250.00"
                required
              />
            </label>

            <label>
              Tipo
              <select value={tipo} onChange={(evento) => setTipo(evento.target.value)}>
                <option value="Despesa">Despesa</option>
                {/* Receita fica indisponível para menores de idade (regra do desafio) */}
                <option value="Receita" disabled={menorDeIdade}>
                  Receita
                </option>
              </select>
            </label>

            <label>
              Pessoa
              <select
                value={pessoaId}
                onChange={(evento) => setPessoaId(evento.target.value)}
                required
              >
                <option value="">Selecione...</option>
                {pessoas.map((pessoa) => (
                  <option key={pessoa.id} value={pessoa.id}>
                    {pessoa.nome} ({pessoa.idade} anos)
                  </option>
                ))}
              </select>
            </label>

            <button type="submit">Cadastrar</button>
          </form>
        )}

        {menorDeIdade && (
          <p className="aviso">
            {pessoaSelecionada.nome} é menor de idade, então só pode ter despesas.
          </p>
        )}
      </section>

      {erro && <p className="mensagem-erro">{erro}</p>}

      <section className="cartao">
        <h2>Transações cadastradas</h2>
        {carregando ? (
          <p>Carregando...</p>
        ) : transacoes.length === 0 ? (
          <p className="vazio">Nenhuma transação cadastrada ainda.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Descrição</th>
                <th>Pessoa</th>
                <th>Tipo</th>
                <th className="direita">Valor</th>
              </tr>
            </thead>
            <tbody>
              {transacoes.map((transacao) => (
                <tr key={transacao.id}>
                  <td>{transacao.id}</td>
                  <td>{transacao.descricao}</td>
                  <td>{transacao.pessoaNome}</td>
                  <td>
                    <span
                      className={
                        transacao.tipo === 'Receita' ? 'etiqueta receita' : 'etiqueta despesa'
                      }
                    >
                      {transacao.tipo}
                    </span>
                  </td>
                  <td className="direita">{formatarMoeda(transacao.valor)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </>
  );
}
