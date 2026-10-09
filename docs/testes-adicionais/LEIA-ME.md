# Testes adicionais (evolução da aula)

Os arquivos `.cs.txt` desta pasta **não são compilados nem executados** na
demonstração inicial. Eles ficam fora do projeto `CadastroClientes.Tests`
justamente para que a suíte inicial continue tendo exatamente um teste.

Cada arquivo representa uma etapa da aula. Para incorporar uma etapa:

1. Copie o conteúdo do arquivo `NN-Etapa.cs.txt` desejado.
2. Crie um novo arquivo `.cs` dentro de `tests/CadastroClientes.Tests/` (por
   exemplo, `tests/CadastroClientes.Tests/Servicos/01-ObrigatoriedadeDosCampos.cs`)
   e cole o conteúdo.
3. Rode `dotnet test` novamente e observe quantos testes passam.
4. Gere a cobertura novamente (veja o README) e compare com o relatório anterior:
   quais caminhos do `ClienteService` passaram a ficar verdes?

Repita o processo, incorporando uma etapa por vez, na ordem sugerida:

| Ordem | Arquivo | Assunto |
|-------|---------|---------|
| 1 | `01-ObrigatoriedadeDosCampos.cs.txt` | Campos obrigatórios ausentes (nulo, vazio, espaços) |
| 2 | `02-LimitesMinimoEMaximo.cs.txt` | Valores no limite, um abaixo do mínimo e um acima do máximo |
| 3 | `03-FormatosInvalidos.cs.txt` | Formatos inválidos (email, telefone, sexo, UF, CEP) |
| 4 | `04-EmailDuplicado.cs.txt` | Regra de email duplicado (sem diferenciar maiúsculas/minúsculas) |
| 5 | `05-EntradasInvalidasNaoPersistem.cs.txt` | Confirmação de que nenhuma entrada inválida é persistida |

Há ainda um arquivo separado, **fora dessa sequência**:

- `exemplo-teste-sem-assercao-util.cs.txt`: um teste que executa o cadastro mas
  não verifica o comportamento de forma útil. Ele serve só para a discussão em
  aula sobre cobertura alta x qualidade de asserção — **não deve** ser incorporado
  junto com as etapas acima.
