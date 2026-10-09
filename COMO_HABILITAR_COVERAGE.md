# Como habilitar cobertura de testes em um projeto .NET 10 + xUnit

Este guia explica, passo a passo, como configurar **qualquer** projeto de
testes xUnit em .NET 10 para coletar cobertura de código e gerar um
relatório HTML navegável — do zero, incluindo o pacote NuGet necessário e
os comandos de linha de comando. Ele é genérico (serve para outros
projetos), mas usa os caminhos deste repositório como exemplo aplicado na
última seção.

## Sumário

- [Visão geral das peças envolvidas](#visão-geral-das-peças-envolvidas)
- [Pré-requisitos](#pré-requisitos)
- [Passo 1 — Ter (ou criar) um projeto de testes xUnit](#passo-1--ter-ou-criar-um-projeto-de-testes-xunit)
- [Passo 2 — Garantir o pacote de cobertura (coverlet.collector)](#passo-2--garantir-o-pacote-de-cobertura-coverletcollector)
- [Passo 3 — Rodar os testes coletando cobertura](#passo-3--rodar-os-testes-coletando-cobertura)
- [Passo 4 — Instalar o ReportGenerator](#passo-4--instalar-o-reportgenerator)
- [Passo 5 — Gerar o relatório HTML](#passo-5--gerar-o-relatório-html)
- [Passo 6 — Abrir e interpretar o relatório](#passo-6--abrir-e-interpretar-o-relatório)
- [Repetindo a coleta sem misturar execuções antigas](#repetindo-a-coleta-sem-misturar-execuções-antigas)
- [Restringindo a cobertura a um projeto específico](#restringindo-a-cobertura-a-um-projeto-específico)
- [Cuidado: não misture VSTest com Microsoft Testing Platform](#cuidado-não-misture-vstest-com-microsoft-testing-platform)
- [Exemplo aplicado neste repositório](#exemplo-aplicado-neste-repositório)
- [Solução de problemas comuns](#solução-de-problemas-comuns)

## Visão geral das peças envolvidas

| Peça | Papel |
|---|---|
| **xUnit** (`xunit` + `xunit.runner.visualstudio`) | Framework de testes e o adaptador que permite ao `dotnet test` descobrir e executar os testes. |
| **Microsoft.NET.Test.Sdk** | SDK de testes do .NET; integra o projeto de testes ao `dotnet test`. |
| **Coverlet** (`coverlet.collector`) | Instrumenta o código em tempo de execução e produz o arquivo de cobertura (`coverage.cobertura.xml`) durante o `dotnet test`. |
| **ReportGenerator** (`dotnet-reportgenerator-globaltool`) | Lê o(s) arquivo(s) de cobertura e gera um relatório legível (HTML, texto, etc.), navegável até o código-fonte. |

Nenhuma dessas ferramentas exige banco de dados, serviço externo ou
configuração de infraestrutura — tudo roda localmente, por linha de
comando.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) instalado
  (confirme com `dotnet --version`).
- Acesso à internet na primeira execução, para o NuGet baixar os pacotes.

## Passo 1 — Ter (ou criar) um projeto de testes xUnit

Se você ainda não tem um projeto de testes, crie um com o template padrão
do .NET 10 (ele já referencia tudo que o xUnit precisa):

```bash
dotnet new xunit -n MeuProjeto.Tests -o tests/MeuProjeto.Tests
```

Isso gera um `.csproj` parecido com este (versões podem variar um pouco
conforme a versão do SDK instalada):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
```

Repare que o template do .NET 10 **já inclui `coverlet.collector`** por
padrão — ou seja, em um projeto novo, o Passo 2 abaixo já vem pronto. Se o
seu projeto já existe e não tem esse pacote (por exemplo, foi criado em uma
versão antiga do template, ou o pacote foi removido), siga o Passo 2.

Se você já tem um projeto de testes com referência ao projeto que quer
medir (`dotnet add <tests>.csproj reference <seu-projeto>.csproj`), pode
pular para o Passo 2.

## Passo 2 — Garantir o pacote de cobertura (coverlet.collector)

O pacote que faz o Coverlet funcionar via `dotnet test --collect` é o
`coverlet.collector`. Para adicioná-lo (ou confirmar a versão) em um
projeto de testes existente:

```bash
dotnet add <caminho-do-seu-projeto-de-testes>.csproj package coverlet.collector
```

Isso adiciona uma linha como esta ao `.csproj`:

```xml
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

> ⚠️ Não confunda com o pacote `coverlet.msbuild` (outra forma de coletar
> cobertura, via `dotnet test /p:CollectCoverage=true`) nem com a ferramenta
> global `coverlet` (CLI standalone). Este guia usa o **collector**, que é
> a opção mais simples de integrar ao `dotnet test` baseado em VSTest — a
> mesma combinação usada no restante deste repositório. Veja também a seção
> [Cuidado: não misture VSTest com Microsoft Testing Platform](#cuidado-não-misture-vstest-com-microsoft-testing-platform).

## Passo 3 — Rodar os testes coletando cobertura

Com o pacote `coverlet.collector` presente, basta passar a flag
`--collect` para o `dotnet test`:

```bash
dotnet restore
dotnet build
dotnet test <caminho-do-seu-projeto-de-testes>.csproj --collect:"XPlat Code Coverage"
```

Ao final, o `dotnet test` imprime, em "Anexos" (*Attachments*), o caminho
do arquivo de cobertura gerado, algo como:

```
Anexos:
  .../<pasta-do-projeto-de-testes>/TestResults/<guid>/coverage.cobertura.xml
```

Cada execução cria uma **nova pasta com um GUID** dentro de `TestResults/`
— isso é esperado, e é justamente por isso que a próxima seção recomenda
limpar essa pasta antes de repetir a coleta.

## Passo 4 — Instalar o ReportGenerator

O arquivo `coverage.cobertura.xml` é apenas dados (XML) — para visualizar
de forma legível, use o **ReportGenerator**. A forma recomendada é
instalá-lo como **ferramenta local** da solução (fica versionado junto com
o projeto, sem precisar de instalação global na máquina de cada
desenvolvedor):

```bash
# Só a primeira vez (cria o manifesto de ferramentas, se ainda não existir)
dotnet new tool-manifest

# Instala o ReportGenerator como ferramenta local
dotnet tool install dotnet-reportgenerator-globaltool
```

Isso cria (ou atualiza) o arquivo `.config/dotnet-tools.json` na raiz do
repositório, parecido com:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-reportgenerator-globaltool": {
      "version": "5.5.11",
      "commands": ["reportgenerator"],
      "rollForward": false
    }
  }
}
```

Em uma máquina nova (ex.: um colega clonando o repositório, ou uma esteira
de CI), basta restaurar as ferramentas já declaradas nesse manifesto:

```bash
dotnet tool restore
```

## Passo 5 — Gerar o relatório HTML

Com o arquivo de cobertura em mãos, gere o relatório:

```bash
dotnet tool run reportgenerator \
  -reports:"<caminho-do-seu-projeto-de-testes>/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"<pasta-de-saida-do-relatorio>" \
  -reporttypes:"Html;TextSummary"
```

Parâmetros principais:

- `-reports`: caminho (ou padrão com `*`/`**`) para o(s) arquivo(s)
  `coverage.cobertura.xml`. Aceita vários arquivos separados por `;` para
  combinar a cobertura de mais de uma execução/projeto de testes.
- `-targetdir`: pasta onde o relatório será escrito (ex.: `docs/relatorio-cobertura`).
- `-reporttypes`: formatos de saída. `Html` gera o site navegável;
  `TextSummary` gera um `Summary.txt` com os números principais — útil para
  ler rápido no terminal ou colar em um PR/documentação.

## Passo 6 — Abrir e interpretar o relatório

```bash
# macOS
open <pasta-de-saida-do-relatorio>/index.html

# Linux
xdg-open <pasta-de-saida-do-relatorio>/index.html

# Windows (PowerShell/cmd)
start <pasta-de-saida-do-relatorio>\index.html
```

No `index.html`, você vê, por assembly e por classe:

- **Cobertura de linhas**: percentual de linhas executáveis que foram
  executadas por algum teste.
- **Cobertura de ramificações (branches)**: percentual de caminhos de
  decisão (`if`/`else`, operadores condicionais, etc.) exercitados.
- Clicando em uma classe, o relatório mostra o **código-fonte** com cada
  linha marcada em verde (coberta) ou vermelho (não coberta) — é assim que
  você navega até os trechos que nenhum teste exercitou.

Não existe uma porcentagem "certa" universal: o valor real depende de quão
completa é a sua suíte de testes. Gere o relatório e leia o número que ele
realmente informa, em vez de assumir um percentual.

## Repetindo a coleta sem misturar execuções antigas

Cada chamada a `dotnet test --collect:"XPlat Code Coverage"` cria uma nova
pasta com GUID dentro de `TestResults/`, sem apagar as anteriores. Se o seu
`-reports` usa um padrão com `*` (ex.: `TestResults/*/coverage.cobertura.xml`),
o ReportGenerator vai ler **todas** as pastas que baterem com o padrão —
inclusive as de execuções antigas, misturando números de rodadas diferentes
no mesmo relatório.

Para evitar isso, **limpe a pasta de resultados antes de cada nova
coleta**:

```bash
rm -rf <caminho-do-seu-projeto-de-testes>/TestResults
dotnet test <caminho-do-seu-projeto-de-testes>.csproj --collect:"XPlat Code Coverage"
```

(No Windows/PowerShell: `Remove-Item -Recurse -Force <caminho>\TestResults`.)

## Restringindo a cobertura a um projeto específico

Se a sua solução tem vários projetos (ex.: `Core`, `Api`, `Infra`) e você
quer medir a cobertura de apenas um deles — porque é onde está a lógica de
negócio que importa para a aula/revisão —, use `-classfilters` (ou
`-assemblyfilters`) ao gerar o relatório, sem precisar mudar nada na
coleta:

```bash
dotnet tool run reportgenerator \
  -reports:"<...>/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"<pasta-de-saida-do-relatorio>" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"+MeuProjeto.Core.*"
```

O prefixo `+` inclui o padrão (namespaces/classes que começam com
`MeuProjeto.Core.`); prefixe com `-` para excluir. É possível combinar
várias regras separadas por `;`. Isso é só um filtro de **apresentação** do
relatório — a coleta em si continua medindo o processo de testes como um
todo; o filtro apenas decide o que aparece no relatório final.

## Cuidado: não misture VSTest com Microsoft Testing Platform

A partir do xUnit v3, existe a opção de os testes rodarem sobre o
**Microsoft Testing Platform** (MTP) em vez do executor clássico baseado em
**VSTest**. Os dois mundos têm comandos e pacotes diferentes, e misturá-los
no mesmo projeto costuma gerar erro ou comportamento inesperado na coleta
de cobertura. Para este fluxo (Coverlet + `dotnet test --collect`), use a
combinação clássica, baseada em VSTest:

- `xunit` (v2) + `xunit.runner.visualstudio` + `Microsoft.NET.Test.Sdk` +
  `coverlet.collector`.
- Executado com `dotnet test <projeto>.csproj --collect:"XPlat Code Coverage"`.

Essa é exatamente a combinação que o template `dotnet new xunit` do .NET 10
gera por padrão (veja o Passo 1) — ou seja, seguindo o template padrão sem
trocar manualmente para `xunit.v3`/MTP, você já está na combinação
compatível.

## Exemplo aplicado neste repositório

Os comandos abaixo, usando os caminhos reais deste repositório, já estão
automatizados no [`Makefile`](Makefile) (`make test`) e documentados no
[`README.md`](README.md#cobertura-de-testes):

```bash
dotnet restore CadastroClientes.slnx
dotnet build CadastroClientes.slnx
dotnet tool restore

rm -rf tests/CadastroClientes.Tests/TestResults
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj --collect:"XPlat Code Coverage"

dotnet tool run reportgenerator \
  -reports:"tests/CadastroClientes.Tests/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"docs/relatorio-cobertura" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"+CadastroClientes.Core.*"

open docs/relatorio-cobertura/index.html
```

## Solução de problemas comuns

- **`dotnet test` não produz nenhum "Anexos"/arquivo de cobertura**:
  confirme que o projeto de testes tem a referência a `coverlet.collector`
  (Passo 2) e que você usou exatamente a flag `--collect:"XPlat Code Coverage"`
  (com esse nome de coletor, entre aspas).
- **`dotnet tool run reportgenerator` diz que o comando não existe**: rode
  `dotnet tool restore` na raiz do repositório (onde está o
  `.config/dotnet-tools.json`) antes de usar `dotnet tool run`.
- **O relatório mistura números de execuções diferentes**: veja
  [Repetindo a coleta sem misturar execuções antigas](#repetindo-a-coleta-sem-misturar-execuções-antigas).
- **Quero medir só um projeto da solução, mas o relatório mostra todos**:
  veja [Restringindo a cobertura a um projeto específico](#restringindo-a-cobertura-a-um-projeto-específico).
- **Erro estranho ao misturar pacotes de xUnit v3/Microsoft Testing
  Platform com `coverlet.collector`**: veja
  [Cuidado: não misture VSTest com Microsoft Testing Platform](#cuidado-não-misture-vstest-com-microsoft-testing-platform)
  — a correção normalmente é voltar para `xunit` v2 clássico + `xunit.runner.visualstudio`.
