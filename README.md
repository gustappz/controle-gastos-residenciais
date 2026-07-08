# Controle de Gastos Residenciais

Aplicação para controlar os gastos de uma casa: cadastro de pessoas, lançamento de
receitas e despesas, e uma consulta de totais por pessoa e da casa inteira.

Back-end em .NET 8 (C#) com Entity Framework Core e banco SQLite; front-end em React
com TypeScript.

## Como rodar

Pré-requisitos: [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) e
[Node.js 18+](https://nodejs.org/).

Back-end (num terminal):

```bash
cd backend
dotnet run
```

A API sobe em `http://localhost:5000`, e a documentação interativa fica em
`http://localhost:5000/swagger`. O banco é um arquivo SQLite criado automaticamente na
primeira execução — não precisa instalar nem configurar nada de banco.

Front-end (em outro terminal):

```bash
cd frontend
npm install
npm run dev
```

A interface abre em `http://localhost:5173`.

Para rodar os testes:

```bash
cd backend.Tests
dotnet test
```

## Funcionalidades

Pessoas — criação, listagem e exclusão. Cada pessoa tem um identificador gerado
automaticamente, nome e idade. Ao excluir uma pessoa, as transações dela são apagadas
junto (deleção em cascata).

Transações — criação e listagem. Cada transação tem identificador automático, descrição,
valor, tipo (despesa ou receita) e a pessoa dona. A pessoa informada precisa existir, e
menores de 18 anos só podem cadastrar despesas.

Totais — lista cada pessoa com o total de receitas, o total de despesas e o saldo
(receitas − despesas), e ao final o total geral da casa. Quem não tem transação também
aparece na lista, com os valores zerados.

## Endpoints da API

| Método | Rota                | Descrição                                   |
| ------ | ------------------- | ------------------------------------------- |
| GET    | `/api/pessoas`      | Lista as pessoas                            |
| POST   | `/api/pessoas`      | Cadastra uma pessoa (`{ nome, idade }`)     |
| DELETE | `/api/pessoas/{id}` | Exclui uma pessoa e suas transações         |
| GET    | `/api/transacoes`   | Lista as transações                         |
| POST   | `/api/transacoes`   | Cadastra uma transação (`{ descricao, valor, tipo, pessoaId }`) |
| GET    | `/api/totais`       | Totais por pessoa e total geral             |

Erros de validação voltam com status `400` e um corpo `{ "erro": "mensagem" }`.

## Estrutura do projeto

```
backend/
  Program.cs               configuração da aplicação (serviços, CORS, Swagger, banco)
  Models/                  as entidades: Pessoa, Transacao, TipoTransacao
  Data/AppDbContext.cs     mapeamento das entidades para o banco
  Dtos/                    formatos de entrada e saída da API
  Controllers/             os endpoints de pessoas, transações e totais

backend.Tests/             testes das regras de negócio (xUnit)

frontend/
  src/types.ts             tipos que espelham as respostas da API
  src/api.ts               chamadas à API
  src/formatadores.ts      formatação de valores em reais
  src/App.tsx              componente raiz e navegação entre as telas
  src/pages/               as telas de Pessoas, Transações e Totais
```

## Algumas decisões

- Usei SQLite para guardar os dados num arquivo, atendendo o requisito de persistência
  sem precisar de um servidor de banco separado.
- Os valores monetários usam `decimal` em vez de `double`, para evitar erros de
  arredondamento em contas de dinheiro.
- As validações e regras ficam no back-end, então valem mesmo para chamadas feitas por
  fora da interface. O front-end só antecipa os avisos (por exemplo, desabilitando a
  opção "Receita" quando a pessoa é menor de idade).
- A deleção em cascata é configurada no relacionamento entre pessoa e transação, deixando
  o próprio banco responsável por manter tudo consistente.
