SELECT consultor, vendas, arrecadacao, batida_vendas, batida_arrecadacao, leads, conversao from vw_kpi_consultor
WHERE equipe IN (
    'Equipe Atlas',
    'Equipe Aurora',
    'Equipe Horizonte'
)
order by vendas desc, arrecadacao desc;