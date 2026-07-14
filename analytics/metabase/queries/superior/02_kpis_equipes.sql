select equipe, qnt_vendas, arrecadacao, conversao, meta_vendas_batida, meta_arrecadacao_batida from kpi_equipes
WHERE equipe IN (
    'Equipe Atlas',
    'Equipe Aurora',
    'Equipe Horizonte'
)