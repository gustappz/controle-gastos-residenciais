import { useEffect, useState, type FormEvent } from 'react';
import { api, mensagemDeErro } from '../api';
import type { Pessoa } from '../types';

/**
 * Tela de cadastro de pessoas: formulário de criação + listagem com exclusão.
 * A exclusão pede confirmação num modal, avisando que as transações vão junto.
 */
export default function PessoasPage() {
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [nome, setNome] = useState('');
  const [idade, setIdade] = useState('');
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);

  // Pessoa aguardando confirmação de exclusão (null = modal fechado).
  const [pessoaParaExcluir, setPessoaParaExcluir] = useState<Pessoa | null>(null);

  // Busca a lista de pessoas na API assim que a tela abre.
  useEffect(() => {
    carregarPessoas();
  }, []);

  async function carregarPessoas() {
    try {
      setPessoas(await api.listarPessoas());
      setErro('');
    } catch (excecao) {
      setErro(mensagemDeErro(excecao));
    } finally {
      setCarregando(false);
    }
  }

  async function cadastrar(evento: FormEvent) {
    evento.preventDefault(); // impede o recarregamento padrão da página ao enviar o form

    try {
      await api.criarPessoa(nome.trim(), Number(idade));
      setNome('');
      setIdade('');
      await carregarPessoas();
    } catch (excecao) {
      setErro(mensagemDeErro(excecao));
    }
  }

  // Chamada quando o usuário confirma a exclusão no modal.
  async function confirmarExclusao() {
    if (!pessoaParaExcluir) return;

    try {
      await api.excluirPessoa(pessoaParaExcluir.id);
      await carregarPessoas();
    } catch (excecao) {
      setErro(mensagemDeErro(excecao));
    } finally {
      setPessoaParaExcluir(null); // fecha o modal em qualquer desfecho
    }
  }

  return (
    <>
      <section className="cartao">
        <h2>Nova pessoa</h2>
        <form className="formulario" onSubmit={cadastrar}>
          <label>
            Nome
            <input
              value={nome}
              onChange={(evento) => setNome(evento.target.value)}
              placeholder="Ex.: Maria"
              maxLength={100}
              required
            />
          </label>

          <label>
            Idade
            <input
              type="number"
              min={0}
              value={idade}
              onChange={(evento) => setIdade(evento.target.value)}
              placeholder="Ex.: 30"
              required
            />
          </label>

          <button type="submit">Cadastrar</button>
        </form>
      </section>

      {erro && <p className="mensagem-erro">{erro}</p>}

      <section className="cartao">
        <h2>Pessoas cadastradas</h2>
        {carregando ? (
          <p>Carregando...</p>
        ) : pessoas.length === 0 ? (
          <p className="vazio">Nenhuma pessoa cadastrada ainda.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Nome</th>
                <th>Idade</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {pessoas.map((pessoa) => (
                <tr key={pessoa.id}>
                  <td>{pessoa.id}</td>
                  <td>{pessoa.nome}</td>
                  <td>
                    {pessoa.idade} {pessoa.idade === 1 ? 'ano' : 'anos'}
                    {/* Aviso visual de que a pessoa só pode ter despesas */}
                    {pessoa.idade < 18 && <span className="etiqueta menor">menor de idade</span>}
                  </td>
                  <td className="direita">
                    <button className="botao-perigo" onClick={() => setPessoaParaExcluir(pessoa)}>
                      Excluir
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>

      {/* Modal de confirmação de exclusão (clicar no fundo escuro cancela) */}
      {pessoaParaExcluir && (
        <div className="modal-fundo" onClick={() => setPessoaParaExcluir(null)}>
          <div
            className="modal"
            role="dialog"
            aria-modal="true"
            onClick={(evento) => evento.stopPropagation()}
          >
            <h3>Excluir {pessoaParaExcluir.nome}?</h3>
            <p>
              Todas as transações dessa pessoa também serão apagadas. Essa ação não pode ser
              desfeita.
            </p>
            <div className="modal-acoes">
              <button className="botao-neutro" onClick={() => setPessoaParaExcluir(null)}>
                Cancelar
              </button>
              <button className="botao-perigo-solido" onClick={confirmarExclusao}>
                Excluir
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
