# Makefile simples para o laboratório de cobertura de testes.
#
# Uso:
#   make            -> mostra esta ajuda
#   make run        -> sobe a Web API
#   make test       -> roda os testes de unidade e já gera o relatório de cobertura
#   make open-coverage -> abre o relatório HTML de cobertura (gerado por "make test") no navegador

API_PROJECT       := src/CadastroClientes.Api/CadastroClientes.Api.csproj
TESTS_PROJECT     := tests/CadastroClientes.Tests/CadastroClientes.Tests.csproj
TEST_RESULTS_DIR  := tests/CadastroClientes.Tests/TestResults
COVERAGE_DIR      := docs/relatorio-cobertura
API_URL           := http://localhost:5252

UNAME_S := $(shell uname -s)
ifeq ($(UNAME_S),Darwin)
OPEN_CMD := open
else ifeq ($(UNAME_S),Linux)
OPEN_CMD := xdg-open
else
OPEN_CMD := start
endif

.DEFAULT_GOAL := help
.PHONY: help run test open-coverage

help:
	@echo "Alvos disponiveis:"
	@echo "  make run            - sobe a Web API em $(API_URL)"
	@echo "  make test           - roda os testes de unidade e gera o relatorio de cobertura"
	@echo "  make open-coverage  - abre o relatorio HTML de cobertura no navegador"

## Sobe a Web API (Ctrl+C para parar).
run:
	dotnet run --project $(API_PROJECT) --urls "$(API_URL)"

## Roda os testes de unidade e, no mesmo comando, gera o relatorio de cobertura em HTML.
## Remove resultados de execucoes antigas antes, para nao misturar cobertura de rodadas diferentes.
test:
	rm -rf $(TEST_RESULTS_DIR)
	dotnet test $(TESTS_PROJECT) --collect:"XPlat Code Coverage"
	dotnet tool restore
	dotnet tool run reportgenerator \
		-reports:"$(TEST_RESULTS_DIR)/*/coverage.cobertura.xml" \
		-targetdir:"$(COVERAGE_DIR)" \
		-reporttypes:"Html;TextSummary" \
		-classfilters:"+CadastroClientes.Core.*"
	@echo ""
	@echo "Relatorio gerado em $(COVERAGE_DIR)/index.html (rode 'make open-coverage' para abrir)."
	@cat $(COVERAGE_DIR)/Summary.txt

## Abre no navegador o relatorio HTML de cobertura gerado por "make test".
open-coverage:
	@if [ ! -f "$(COVERAGE_DIR)/index.html" ]; then \
		echo "Relatorio de cobertura nao encontrado em $(COVERAGE_DIR)/index.html."; \
		echo "Rode 'make test' primeiro."; \
		exit 1; \
	fi
	$(OPEN_CMD) "$(COVERAGE_DIR)/index.html"
