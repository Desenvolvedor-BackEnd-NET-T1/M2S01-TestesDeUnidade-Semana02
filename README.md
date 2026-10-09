# Cadastro de Clientes — Laboratório de Cobertura de Testes

Projeto didático em **C# / .NET 10 / xUnit**, criado para uma aula sobre
**cobertura de testes**. A aplicação é uma Web API de cadastro de clientes
com persistência em memória, propositalmente simples, para que a atenção da
aula fique nas regras de validação e no que a cobertura de testes revela (ou
não revela) sobre elas.

> **A mensagem central da aula:** um teste passando não significa que todas
> as regras do sistema foram testadas, e uma cobertura alta não garante que
> as asserções dos testes sejam úteis. Este repositório foi construído para
> demonstrar isso na prática — veja o [roteiro de demonstração](docs/roteiro-demonstracao.md).

A implementação funciona corretamente desde o início. **As lacunas estão nos
testes, não em bugs propositais no código.**

## Sumário

- [Pré-requisitos](#pré-requisitos)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Executando a API](#executando-a-api)
- [Executando os testes](#executando-os-testes)
- [Regras de cadastro](#regras-de-cadastro)
- [Cobertura de testes](#cobertura-de-testes)
- [Testando a API com o arquivo .http](#testando-a-api-com-o-arquivo-http)
- [CI ilustrativo no GitHub Actions](#ci-ilustrativo-no-github-actions)
- [Incorporando os testes adicionais gradualmente](#incorporando-os-testes-adicionais-gradualmente)
- [Limitações conhecidas (propositais)](#limitações-conhecidas-propositais)

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) (verificado com `dotnet --version`).
- [Visual Studio Code](https://code.visualstudio.com/) com a extensão **C# Dev Kit** (ou C#).
- Opcional, para testar a API manualmente pelo arquivo `.http`: a extensão
  **REST Client** (ou o cliente HTTP embutido do C# Dev Kit) no VS Code.
- Acesso à internet na primeira execução, para o `dotnet restore` baixar os
  pacotes NuGet (xUnit, Coverlet, ReportGenerator etc.).

Este projeto **não usa banco de dados externo** nem precisa de nenhuma outra
ferramenta além do SDK do .NET.

## Estrutura do projeto

```
CadastroClientes.slnx              # arquivo de solução (.slnx, novo formato do .NET 10)
src/
  CadastroClientes.Core/           # entidades, DTOs, validações, serviço de cadastro e interface do repositório
    Modelos/Cliente.cs
    Dtos/CadastroClienteDto.cs
    Dtos/ErroValidacao.cs
    Dtos/ResultadoCadastro.cs
    Validacao/UfsBrasileiras.cs
    Repositorios/IClienteRepository.cs
    Servicos/ClienteService.cs     # <- regras de validação explícitas, método Cadastrar
  CadastroClientes.Api/            # Web API no estilo clássico (Program.cs + Startup.cs), sem minimal API
    Program.cs
    Startup.cs
    Controllers/ClientesController.cs
    Repositorios/ClienteRepositoryEmMemoria.cs   # repositório em memória (singleton, thread-safe)
    Dtos/ClienteRespostaDto.cs
    Dtos/ErroRespostaDto.cs
tests/
  CadastroClientes.Tests/          # suíte xUnit
    Servicos/ClienteServiceTests.cs            # <- suíte inicial: exatamente 1 teste
    TestDoubles/RepositorioClienteEmMemoriaDeTeste.cs
    TestDoubles/CadastroClienteDtoFabrica.cs
docs/
  roteiro-demonstracao.md          # roteiro passo a passo para a aula
  relatorio-cobertura/             # relatório HTML gerado pelo ReportGenerator (suíte inicial)
  testes-adicionais/               # testes extras em .cs.txt, para incorporar durante a aula
CadastroClientes.http              # exemplos de requisições para o VS Code
```

Observações de design:

- `CadastroClientes.Core` **não depende** de `CadastroClientes.Api` — só o
  contrário. O Core conhece apenas a interface `IClienteRepository`; quem
  decide como persistir é a camada de API (ou os testes).
- Todas as classes de domínio são **classes comuns**, não `record`s, e as
  regras de validação estão escritas em C# simples — nada de DataAnnotations,
  FluentValidation, AutoMapper, MediatR ou Entity Framework.
- A API usa o estilo "clássico" do ASP.NET Core (`Program.cs` chamando
  `Host.CreateDefaultBuilder().ConfigureWebHostDefaults(...).UseStartup<Startup>()`,
  com um `Startup.cs` separado), e não o modelo de hospedagem mínima (minimal API).

## Executando a API

Todos os comandos abaixo devem ser executados na raiz do repositório.

```bash
# 1) Restaurar as dependências de toda a solução
dotnet restore CadastroClientes.slnx

# 2) Compilar a solução
dotnet build CadastroClientes.slnx

# 3) Executar a API (fica ouvindo em http://localhost:5252)
dotnet run --project src/CadastroClientes.Api/CadastroClientes.Api.csproj --urls "http://localhost:5252"
```

Com a API em execução, abra o arquivo [`CadastroClientes.http`](CadastroClientes.http)
no VS Code (extensão REST Client) e use os botões "Send Request" para testar
os exemplos — veja a seção [Testando a API com o arquivo .http](#testando-a-api-com-o-arquivo-http).

Para parar a API, use `Ctrl+C` no terminal onde ela está rodando.

## Executando os testes

```bash
# Executar a suíte inicial (exatamente 1 teste)
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj
```

Resultado esperado:

```
Aprovado! – Com falha: 0, Aprovado: 1, Ignorado: 0, Total: 1
```

## Regras de cadastro

Todas as regras abaixo estão implementadas, de forma explícita, em
[`ClienteService`](src/CadastroClientes.Core/Servicos/ClienteService.cs), em
métodos pequenos e nomeados por campo (`ValidarNome`, `ValidarEmail`, etc.).
Todo campo é obrigatório; strings nulas, vazias ou só com espaços são
rejeitadas. Os valores são "trimados" (espaços das extremidades removidos)
antes de validar e persistir.

| Campo | Regra |
|---|---|
| Nome | 3 a 10 caracteres |
| Email | até 100 caracteres; formato validado com `System.Net.Mail.MailAddress` (um recurso padrão do .NET) |
| Telefone | 10 a 11 dígitos, somente números, sem pontuação |
| Sexo | apenas os códigos `F`, `M` ou `NI` (não informado) — o campo continua obrigatório |
| Profissão | 2 a 80 caracteres |
| Logradouro | 3 a 100 caracteres |
| Número do endereço | 1 a 10 caracteres; aceita `"S/N"` |
| Bairro | 2 a 60 caracteres |
| Cidade | 2 a 60 caracteres |
| UF | exatamente 2 letras e deve pertencer à lista de UFs brasileiras |
| CEP | exatamente 8 dígitos, sem pontuação |

Além disso: **não é permitido cadastrar dois clientes com o mesmo email**
(comparação sem diferenciar maiúsculas/minúsculas).

> ⚠️ **Importante:** a validação de formato do email com `MailAddress` prova
> apenas que o texto está em uma sintaxe de email válida — ela **não** prova
> que o endereço realmente existe ou que alguém o recebe. E as faixas de
> tamanho acima são simplificações para fins didáticos, não um modelo
> completo de validação de clientes para produção.

O serviço **acumula todos os erros** encontrados (não para no primeiro) e
**nunca persiste** o cliente quando há qualquer erro. O retorno é um objeto
estruturado (`ResultadoCadastro`, com `Sucesso`, `Cliente` e `Erros`) — não
depende de lançar exceções genéricas para sinalizar entrada inválida.

A API expõe:

| Endpoint | Descrição | Respostas |
|---|---|---|
| `POST /clientes` | Cadastra um cliente | `201` + `Location` (sucesso) · `400` com erros por campo (entrada inválida) · `409` (email duplicado) |
| `GET /clientes` | Lista os clientes cadastrados | `200` |
| `GET /clientes/{id}` | Consulta um cliente | `200` · `404` (não encontrado) |

As respostas de erro trazem apenas `campo` e `mensagem` por erro — nenhum
detalhe interno da aplicação é exposto.

## Cobertura de testes

A coleta de cobertura usa **Coverlet** (via o pacote `coverlet.collector`,
já referenciado no projeto de testes) integrado ao executor padrão do
`dotnet test` (baseado em VSTest) — não há mistura com o Microsoft Testing
Platform. A geração do relatório HTML usa o **ReportGenerator**, instalado
como ferramenta local da solução (manifesto em `.config/dotnet-tools.json`).

O relatório principal desta aula **mede apenas o projeto `CadastroClientes.Core`**
(onde vivem as regras de validação). Isso é proposital: a métrica de
cobertura aqui **não mede o comportamento HTTP da API**, nem o projeto
`CadastroClientes.Api` como um todo — apenas a lógica de negócio do Core, que
é o foco pedagógico da aula.

### Comandos

```bash
# 1) Restaurar dependências (projetos + ferramentas locais, como o ReportGenerator)
dotnet restore CadastroClientes.slnx
dotnet tool restore

# 2) Compilar
dotnet build CadastroClientes.slnx

# 3) Executar o(s) teste(s) (sem coleta de cobertura, só para conferir que passam)
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj

# 4) Coletar cobertura
#    IMPORTANTE: antes de repetir a coleta, remova os resultados antigos para
#    não misturar execuções diferentes no mesmo relatório.
rm -rf tests/CadastroClientes.Tests/TestResults
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj --collect:"XPlat Code Coverage"

# 5) Gerar o relatório HTML (escopo: apenas CadastroClientes.Core)
dotnet tool run reportgenerator \
  -reports:"tests/CadastroClientes.Tests/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"docs/relatorio-cobertura" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"+CadastroClientes.Core.*"

# 6) Abrir o relatório
open docs/relatorio-cobertura/index.html   # macOS
# xdg-open docs/relatorio-cobertura/index.html   # Linux
# start docs\relatorio-cobertura\index.html      # Windows
```

Para repetir a coleta do zero (por exemplo, depois de incorporar uma nova
etapa de testes), sempre rode o passo 4 novamente — o `rm -rf .../TestResults`
evita que o ReportGenerator junte números de uma execução antiga com os da
execução nova.

### O que o relatório mostra

O `index.html` lista, por classe, o percentual de **linhas** e de
**ramificações (branches)** cobertas, e permite navegar até o código-fonte
para ver exatamente quais trechos (ex.: um `if` de validação de limite
máximo) nunca foram executados por nenhum teste.

Com a suíte inicial (exatamente o teste `Cadastrar_ComDadosValidos_DevePersistirCliente`),
os números **realmente obtidos** ao gerar o relatório neste repositório foram:

```
CadastroClientes.Core                              57.5%
  CadastroClientes.Core.Dtos.CadastroClienteDto     100%
  CadastroClientes.Core.Dtos.ErroValidacao            0%
  CadastroClientes.Core.Dtos.ResultadoCadastro     70.5%
  CadastroClientes.Core.Servicos.ClienteService    52.2%
  CadastroClientes.Core.Validacao.UfsBrasileiras    100%

Cobertura de linhas:        57.5% (161 de 280 linhas cobríveis)
Cobertura de ramificações:  51%   (51 de 100 ramificações)
```

Ou seja: o único teste passa, mas quase metade das ramificações de validação
de `ClienteService` nunca é exercitada — e a classe `ErroValidacao` fica a
**0%**, porque nenhum erro é produzido no caminho feliz. **Não há nenhuma
promessa de porcentagem fixa aqui**: os números acima foram obtidos rodando
a coleta de fato, e podem variar ligeiramente em outra máquina/versão de
ferramenta — o importante é observar que ficam bem abaixo de 100%.

## Testando a API com o arquivo .http

O arquivo [`CadastroClientes.http`](CadastroClientes.http) na raiz do
repositório contém exemplos prontos para o VS Code (extensão REST Client),
simulando o "formulário" de cadastro como um DTO JSON:

1. Um **cadastro válido** (deve responder `201 Created` com cabeçalho `Location`).
2. Um **cadastro inválido** — nome ausente, telefone com letras e CEP com
   tamanho errado ao mesmo tempo (deve responder `400 Bad Request` com um
   erro por campo, sem persistir nada).
3. Uma **tentativa de email duplicado** — mesmo email do cadastro válido,
   porém em caixa alta (deve responder `409 Conflict`).

Também há exemplos de `GET /clientes` (lista) e `GET /clientes/{id}`
(inclusive para um id inexistente, retornando `404`).

Para usar: inicie a API (veja [Executando a API](#executando-a-api)), abra o
arquivo no VS Code e clique em "Send Request" acima de cada requisição, na
ordem em que aparecem no arquivo (a consulta por id reaproveita o id
retornado pelo cadastro válido).

## CI ilustrativo no GitHub Actions

Este repositório inclui uma esteira de CI em
[`.github/workflows/ci.yml`](.github/workflows/ci.yml) que roda a cada
`push` e `pull request`. Ela é dividida em **5 jobs encadeados** (com
`needs:`), para aparecerem como etapas visualmente separadas no grafo de
execução do GitHub Actions:

1. **Build** (real) — restaura e compila a solução.
2. **Testes e cobertura** (real) — roda a suíte xUnit com coleta de
   cobertura (Coverlet), gera o relatório com o ReportGenerator (escopo só
   no projeto Core), publica o resumo da cobertura na tela da execução e
   sobe o relatório HTML completo como artefato para download.
3. **Qualidade de código** (mock) — não executa nenhuma ferramenta real;
   apenas imprime uma mensagem e espera alguns segundos, simulando onde um
   linter ou um SonarQube/SonarCloud entraria.
4. **Testes de integração** (mock) — mesma ideia: só mensagem + espera,
   simulando onde uma suíte de testes de integração entraria.
5. **Deploy em homologação** (mock) — idem, simulando um passo de deploy
   para um ambiente de homologação/staging.

**Esta esteira é apenas ilustrativa e nunca bloqueia merge.** Todo job (e
cada passo dentro dele) usa `continue-on-error: true`, então cada etapa — e
o workflow como um todo — sempre termina como sucesso (ou "sucesso com
avisos"), mesmo que o build, os testes ou qualquer mock falhem. Isso também
garante que os jobs seguintes sempre rodem, mesmo que um anterior falhe —
o objetivo é só mostrar a esteira completa em execução. Além disso, não há
nenhuma regra de proteção de branch marcando essa checagem como obrigatória.

Para ver o resultado: abra a aba **Actions** do repositório no GitHub e
entre na execução desejada. O grafo mostra as 5 etapas encadeadas; o resumo
(**Summary**) do job "Testes e cobertura" traz a cobertura, e o artefato
`relatorio-cobertura` (também nessa execução) tem o relatório HTML completo
para baixar e abrir localmente.

## Incorporando os testes adicionais gradualmente

A pasta [`docs/testes-adicionais/`](docs/testes-adicionais/) contém a
evolução da aula: arquivos de texto com extensão `.cs.txt`, que **não são
compilados nem executados** pela suíte inicial (eles ficam fora do projeto
`CadastroClientes.Tests`).

Cada arquivo representa uma etapa:

1. `01-ObrigatoriedadeDosCampos.cs.txt` — campos obrigatórios ausentes.
2. `02-LimitesMinimoEMaximo.cs.txt` — valores no limite, um abaixo do mínimo
   e um acima do máximo (dados de fronteira).
3. `03-FormatosInvalidos.cs.txt` — formatos inválidos (email, telefone, sexo,
   UF, CEP).
4. `04-EmailDuplicado.cs.txt` — a regra de email duplicado.
5. `05-EntradasInvalidasNaoPersistem.cs.txt` — confirmação concentrada de que
   nenhuma entrada inválida é persistida.

Para incorporar uma etapa durante a aula: copie o conteúdo do `.cs.txt` para
um novo arquivo `.cs` dentro de `tests/CadastroClientes.Tests/Servicos/`,
rode `dotnet test` de novo e gere a cobertura de novo (passos 4 a 6 da seção
anterior) para comparar com o relatório anterior. Instruções detalhadas estão
em [`docs/testes-adicionais/LEIA-ME.md`](docs/testes-adicionais/LEIA-ME.md).

Há ainda um arquivo **separado**, fora dessa sequência:
[`exemplo-teste-sem-assercao-util.cs.txt`](docs/testes-adicionais/exemplo-teste-sem-assercao-util.cs.txt) —
um teste que executa o cadastro mas não verifica nada de útil sobre o
resultado. Ele serve para mostrar que **executar código aumenta a cobertura,
mas não necessariamente verifica o comportamento esperado**. Não deve ser
incorporado junto com as etapas 1 a 5.

Depois de incorporar todas as etapas (1 a 5), a cobertura obtida neste
repositório subiu para:

```
CadastroClientes.Core                              98.5%
Cobertura de linhas:        98.5% (276 de 280 linhas cobríveis)
Cobertura de ramificações:  99%   (99 de 100 ramificações)
```

## Limitações conhecidas (propositais)

- **Persistência apenas em memória**: os dados existem somente enquanto o
  processo da API está em execução. Ao encerrar a aplicação (`Ctrl+C`), tudo
  é perdido. Isso é intencional e serve só para este laboratório — não use
  esse repositório como base para um sistema real sem trocar por uma
  persistência durável.
- **Regras de validação simplificadas**: os limites de tamanho e os formatos
  aceitos são escolhas didáticas para tornar a aula clara, não um modelo
  completo de cadastro de clientes para produção.
- **Sem integração contínua**: este repositório não inclui pipelines de CI,
  por decisão explícita do escopo da aula.
