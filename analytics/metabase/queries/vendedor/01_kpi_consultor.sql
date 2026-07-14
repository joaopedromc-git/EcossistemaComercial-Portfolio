SELECT consultor, vendas, arrecadacao, batida_vendas, batida_arrecadacao, leads, conversao from vw_kpi_consultor
WHERE equipe IN (
    'Equipe Atlas',
    'Equipe Aurora',
    'Equipe Horizonte'
)
left join vendedores c 
on consultor = c.nome_vendedor
where 1=1 
[[AND {{id_vendas}}]]
[[AND {{id_leads}}]]
order by vendas desc, arrecadacao desc;