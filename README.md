# Controle de Gastos Residenciais

Sistema para controlar os gastos de uma residência: cadastro de pessoas, lançamento de
transações (receitas e despesas) e consulta de totais por pessoa, com total geral da casa.

| Camada    | Tecnologia                              |
| --------- | --------------------------------------- |
| Back-end  | .NET 8 (C#) + Entity Framework Core     |
| Banco     | SQLite (arquivo local, dados persistem) |
| Front-end | React + TypeScript (Vite)               |

## Como rodar

Pré-requisitos: [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) e [Node.js 18+](https://nodejs.org/).

**1. Back-end** (primeiro terminal):

```bash
cd backend
dotnet run
```

A API sobe em `http://localhost:5000`. A documentação interativa (Swagger) fica em
`http://localhost:5000/swagger`.

**2. Front-end** (segundo terminal):

```bash
cd frontend
npm install
npm run dev
```

A interface abre em `http://localhost:5173`.

O banco (`backend/controle_gastos.db`) é criado automaticamente na primeira execução —
não é preciso instalar nem configurar nada de banco de dados.

**3. Testes** (opcional):

```bash
cd backend.Tests
dotnet test
```

As regras de negócio do desafio são cobertas por testes unitários (xUnit) rodando sobre
um SQLite em memória — o mesmo banco da aplicação, o que inclui o comportamento real da
deleção em cascata.

## Funcionalidades e regras de negócio

**Cadastro de pessoas** — criação, listagem e exclusão.

- Cada pessoa tem identificador (único, gerado automaticamente pelo banco), nome e idade.
- Ao excluir uma pessoa, **todas as transações dela são apagadas junto** (deleção em
  cascata, configurada no relacionamento do banco).

**Cadastro de transações** — criação e listagem.

- Cada transação tem identificador (único, automático), descrição, valor, tipo
  (despesa/receita) e a pessoa dona da transação.
- A pessoa informada **precisa existir** no cadastro.
- **Menores de 18 anos só podem cadastrar despesas.** A API recusa receitas para menores,
  e a interface já desabilita a opção ao selecionar um menor.
- O valor deve ser sempre maior que zero (quem define se soma ou subtrai é o tipo).

**Consulta de totais** — lista todas as pessoas com total de receitas, total de despesas
e saldo (receitas − despesas), e ao final o total geral de todas as pessoas.

- Pessoas sem nenhuma transação também aparecem na lista, com valores zerados.

## Endpoints da API

| Método | Rota                 | Descrição                                       |
| ------ | -------------------- | ----------------------------------------------- |
| GET    | `/api/pessoas`       | Lista todas as pessoas                          |
| POST   | `/api/pessoas`       | Cadastra uma pessoa (`{ nome, idade }`)         |
| DELETE | `/api/pessoas/{id}`  | Exclui uma pessoa e suas transações             |
| GET    | `/api/transacoes`    | Lista todas as transações                       |
| POST   | `/api/transacoes`    | Cadastra uma transação (`{ descricao, valor, tipo, pessoaId }`) |
| GET    | `/api/totais`        | Totais por pessoa + total geral                 |

Erros de validação retornam `400` com o corpo `{ "erro": "mensagem" }`; recursos não
encontrados retornam `404`.

## Estrutura do projeto

```
backend/
  Program.cs               ponto de entrada: configura serviços, CORS, Swagger e o banco
  Models/                  entidades do domínio (Pessoa, Transacao, TipoTransacao)
  Data/AppDbContext.cs     mapeamento das entidades para o banco (relacionamento + cascata)
  Dtos/                    formatos de entrada e saída da API, separados das entidades
  Controllers/             endpoints de pessoas, transações e totais (regras de negócio)

backend.Tests/             testes unitários das regras de negócio (xUnit + SQLite em memória)

frontend/
  src/types.ts             tipos TypeScript espelhando as respostas da API
  src/api.ts               camada de acesso à API (fetch centralizado + tratamento de erro)
  src/formatadores.ts      formatação de valores em reais (R$)
  src/App.tsx              componente raiz com a navegação por abas
  src/pages/               as três telas: Pessoas, Transações e Totais
```

## Decisões técnicas

- **SQLite**: banco em arquivo local, dispensa instalação de servidor e atende o requisito
  de persistência — os dados continuam lá após fechar e reabrir a aplicação.
- **`decimal` para valores monetários**: evita os erros de arredondamento que `float` e
  `double` introduzem em cálculos financeiros.
- **Validação no back-end como fonte da verdade**: todas as regras são aplicadas na API;
  o front-end apenas antecipa os avisos para melhorar a experiência (ex.: desabilitar
  "Receita" para menores de idade). Assim as regras valem mesmo para chamadas feitas por
  fora da interface.
- **Deleção em cascata declarada no relacionamento** (`OnDelete(DeleteBehavior.Cascade)`):
  o banco garante a consistência, sem depender de código espalhado para limpar transações.
- **`EnsureCreated` em vez de Migrations**: para o escopo deste projeto, simplifica o
  setup (clonar e rodar). Em um sistema em evolução contínua, Migrations seriam o caminho.
- **Pessoas sem transações aparecem na consulta de totais**: a consulta parte da tabela de
  pessoas (e não da de transações), garantindo a listagem completa pedida no desafio.
- **Limites de tamanho (nome: 100, descrição: 200) validados na API**: o SQLite não impõe
  tamanho de texto, então a validação nos controllers é quem garante o limite — o front
  apenas espelha com `maxLength` nos inputs.

## Possíveis evoluções

Itens deixados de fora do escopo por decisão consciente, que seriam os próximos passos:

- Paginação nas listagens (o volume esperado para uma residência não exige, mas a
  consulta de transações cresceria sem limite);
- Migrations do EF Core no lugar de `EnsureCreated`, para evoluir o schema com histórico;
- Testes de interface (e2e) complementando os testes de unidade das regras;
- Edição de pessoas e transações (fora do escopo pedido no desafio).
