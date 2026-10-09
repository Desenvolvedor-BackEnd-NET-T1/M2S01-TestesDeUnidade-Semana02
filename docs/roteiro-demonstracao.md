# Roteiro de demonstração — Cobertura de testes

Este roteiro guia a aula passo a passo. Todos os comandos devem ser
executados na raiz do repositório, a menos que indicado o contrário.

**Mensagem central a transmitir:** um teste passando não significa que todas
as regras foram testadas; alta cobertura não garante qualidade das
asserções.

---

## Passo 1 — Mostrar as regras do cadastro

Abra [`src/CadastroClientes.Core/Servicos/ClienteService.cs`](../src/CadastroClientes.Core/Servicos/ClienteService.cs)
no VS Code e percorra o método `Cadastrar` e os métodos `Validar*` logo
abaixo dele. Destaque:

- Cada campo tem seu próprio método de validação (`ValidarNome`,
  `ValidarEmail`, ...), sempre na mesma ordem didática: obrigatoriedade →
  mínimo → máximo → formato.
- Os erros são **acumulados** em uma lista (`erros`), não interrompem a
  validação no primeiro problema encontrado.
- Se `erros.Count > 0`, o método retorna **sem chamar** `_repositorio.Adicionar`
  — ou seja, nada é persistido quando há qualquer erro.
- A tabela de regras por campo está na seção
  "[Regras de cadastro](../README.md#regras-de-cadastro)" do README.

Também é um bom momento para abrir a tabela de endpoints e mostrar os
códigos de status HTTP (`201`, `400`, `409`, `404`) no
[`ClientesController`](../src/CadastroClientes.Api/Controllers/ClientesController.cs).

## Passo 2 — Executar o único teste e observar que passou

```bash
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj
```

Resultado esperado:

```
Aprovado! – Com falha: 0, Aprovado: 1, Ignorado: 0, Total: 1
```

Abra [`ClienteServiceTests.cs`](../tests/CadastroClientes.Tests/Servicos/ClienteServiceTests.cs)
e mostre que existe **exatamente um teste**:
`Cadastrar_ComDadosValidos_DevePersistirCliente`. Ele usa dados válidos em
todos os campos e verifica o sucesso do cadastro, o id gerado, os dados
retornados e a persistência no repositório.

## Passo 3 — Perguntar à turma

> "Esse teste passou. Isso significa que todas as regras de cadastro estão
> testadas?"

Deixe a turma discutir antes de revelar a resposta nos próximos passos.
(Resposta: não — o teste só percorreu o caminho totalmente válido.)

## Passo 4 — Gerar a cobertura

```bash
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

Lembre a turma de que o relatório mede **apenas o projeto Core** (não a API
nem o comportamento HTTP) — isso é proposital, para manter o foco nas regras
de negócio.

## Passo 5 — Abrir o serviço e identificar caminhos não exercitados

No `index.html` aberto, clique na classe `ClienteService`. Mostre:

- O percentual de cobertura de linhas e de ramificações (no primeiro
  relatório gerado neste repositório: **57,5% de linhas / 51% de
  ramificações** — valores obtidos de fato, e não uma meta prometida).
- No código-fonte exibido pelo relatório, aponte linhas marcadas como **não
  cobertas** — por exemplo, os `if` de "abaixo do mínimo", "acima do
  máximo" e "formato inválido" de vários campos, e o bloco de email
  duplicado.
- Abra também a classe `ErroValidacao`: ela aparece com **0%** de
  cobertura, porque o único teste nunca gera nenhum erro (o construtor de
  `ErroValidacao` nunca é chamado no caminho feliz).

Esse é o momento de reforçar: o teste passou, o código funciona, mas a
maior parte das regras de validação nunca foi exercitada por um teste.

## Passo 6 — Incorporar uma etapa de testes

Siga as instruções de [`docs/testes-adicionais/LEIA-ME.md`](testes-adicionais/LEIA-ME.md):
copie o conteúdo de `docs/testes-adicionais/01-ObrigatoriedadeDosCampos.cs.txt`
para um novo arquivo, por exemplo
`tests/CadastroClientes.Tests/Servicos/01-ObrigatoriedadeDosCampos.cs`, e
rode os testes de novo:

```bash
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj
```

Agora a suíte deve mostrar bem mais testes aprovados (cada campo obrigatório
testado com `null`, `""` e `"   "`).

(Se o tempo de aula permitir, repita o processo incorporando também as
etapas 2 a 5 — limites, formatos, email duplicado e confirmação de não
persistência — antes do próximo passo.)

## Passo 7 — Gerar novamente e comparar

```bash
rm -rf tests/CadastroClientes.Tests/TestResults
dotnet test tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj --collect:"XPlat Code Coverage"

dotnet tool run reportgenerator \
  -reports:"tests/CadastroClientes.Tests/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"docs/relatorio-cobertura" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"+CadastroClientes.Core.*"

open docs/relatorio-cobertura/index.html
```

Compare com o relatório do Passo 4. Neste repositório, incorporando todas as
cinco etapas (1 a 5), a cobertura subiu de **57,5%/51%** (linhas/ramificações)
para **98,5%/99%**. Mostre, no relatório, como as linhas que antes
apareciam em vermelho (não cobertas) agora aparecem em verde.

## Discussão final — o exemplo de teste sem asserção útil

Para fechar a aula, mostre
[`docs/testes-adicionais/exemplo-teste-sem-assercao-util.cs.txt`](testes-adicionais/exemplo-teste-sem-assercao-util.cs.txt)
**sem incorporá-lo junto com as etapas anteriores** (incorpore-o isolado,
em um teste à parte, só para a demonstração, e depois remova).

Esse teste chama `Cadastrar` com um nome ausente — ou seja, **executa** boa
parte do código de validação — mas a única asserção é `Assert.NotNull(resultado)`,
que seria verdadeira mesmo que o serviço estivesse quebrado (por exemplo, se
aceitasse indevidamente um nome ausente). Rode a cobertura com esse teste
isolado e mostre que ele também aumenta a cobertura de linhas, exatamente
como qualquer teste "de verdade" — reforçando a mensagem final da aula:

> Cobertura é uma métrica sobre o que foi **executado**, não sobre o que foi
> **verificado**. Um teste só é útil quando suas asserções realmente
> confirmam o comportamento esperado.
