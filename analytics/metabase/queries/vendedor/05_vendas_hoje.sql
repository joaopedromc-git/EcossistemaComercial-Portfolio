    select * from vendas_dia_atual v
left join vendedores c
on c.nome_vendedor = v.proprietario_contato
where 1=1
[[AND {{id_vendas}}]]