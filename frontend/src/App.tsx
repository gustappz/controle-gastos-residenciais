import { useState } from 'react';
import PessoasPage from './pages/PessoasPage';
import TransacoesPage from './pages/TransacoesPage';
import TotaisPage from './pages/TotaisPage';

// Abas disponíveis na aplicação.
type Aba = 'pessoas' | 'transacoes' | 'totais';

const ABAS: { id: Aba; rotulo: string }[] = [
  { id: 'pessoas', rotulo: 'Pessoas' },
  { id: 'transacoes', rotulo: 'Transações' },
  { id: 'totais', rotulo: 'Totais' },
];

/**
 * Componente raiz: exibe o cabeçalho e alterna entre as três telas do sistema.
 * Para um app deste porte, a navegação por abas dispensa uma biblioteca de rotas.
 */
export default function App() {
  const [abaAtiva, setAbaAtiva] = useState<Aba>('pessoas');

  return (
    <div className="container">
      <header className="cabecalho">
        <h1>Controle de Gastos Residenciais</h1>
        <nav className="abas">
          {ABAS.map((aba) => (
            <button
              key={aba.id}
              className={abaAtiva === aba.id ? 'aba ativa' : 'aba'}
              onClick={() => setAbaAtiva(aba.id)}
            >
              {aba.rotulo}
            </button>
          ))}
        </nav>
      </header>

      <main>
        {/* Cada tela só é montada quando a aba correspondente está ativa;
            ao entrar, ela busca os dados atualizados na API. */}
        {abaAtiva === 'pessoas' && <PessoasPage />}
        {abaAtiva === 'transacoes' && <TransacoesPage />}
        {abaAtiva === 'totais' && <TotaisPage />}
      </main>
    </div>
  );
}
