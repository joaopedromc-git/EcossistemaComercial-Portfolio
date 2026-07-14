# Catálogo dos dashboards

Este documento relaciona os cards apresentados no Metabase com seus respectivos arquivos SQL e screenshots.

## Visão superior

| Ordem | Card | Query | Screenshot |
|---:|---|---|---|
| 1 | Indicadores por consultor | `queries/superior/01_kpi_consultores.sql` | `screenshots/superior/01_kpi_consultores.jpeg` |
| 2 | Indicadores por equipe | `queries/superior/02_kpis_equipes.sql` | `screenshots/superior/02_kpi_equipes.jpeg` |
| 3 | Conversão por vendedor | `queries/superior/03_conversao_vendas.sql` | `screenshots/superior/03_tabela_conversao.jpeg` |
| 4 | Vendas por dia | `queries/superior/04_vendas_por_dia.sql` | `screenshots/superior/04_vendas_por_dia.jpeg` |
| 5 | Indicadores gerais | `queries/superior/05_kpi_geral.sql` | `screenshots/superior/05_kpi_geral.jpeg` |
| 6 | Vendas do dia atual | `queries/superior/06_vendas_dia_atual.sql` | `screenshots/superior/06_vendas_dia_atual.jpeg` |
| 7 | Ranking por pontos | `queries/superior/07_rank_pontos_vendas.sql` | `screenshots/superior/07_rank_pontos.jpeg` |

## Visão do vendedor

| Ordem | Card | Query | Screenshot |
|---:|---|---|---|
| 1 | Indicadores individuais | `queries/vendedor/01_kpi_consultor.sql` | `screenshots/vendedor/01_02_kpi_consultor_e_vendas_consultor.jpeg` |
| 2 | Resumo de vendas | `queries/vendedor/02_vendas_consultor.sql` | `screenshots/vendedor/01_02_kpi_consultor_e_vendas_consultor.jpeg` |
| 3 | Conversão individual | `queries/vendedor/03_tabela_conversao.sql` | `screenshots/vendedor/03_04_conversao_e_vendas_dia.jpeg` |
| 4 | Vendas por dia | `queries/vendedor/04_vendas_dia.sql` | `screenshots/vendedor/03_04_conversao_e_vendas_dia.jpeg` |
| 5 | Vendas do dia | `queries/vendedor/05_vendas_hoje.sql` | `screenshots/vendedor/05_06_vendas_hoje_e_ranking.jpeg` |
| 6 | Ranking individual | `queries/vendedor/06_ranking_pontos.sql` | `screenshots/vendedor/05_06_vendas_hoje_e_ranking.jpeg` |

## Parâmetros

As consultas individuais utilizam os identificadores autorizados pelo Portal:

```text
ids_vendas
ids_leads