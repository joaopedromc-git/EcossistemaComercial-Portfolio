select r.* from ranking_mensal r
left join vendedores c
on c.nome_vendedor = r.consultor
where equipe not in('Aurora')
order by rank_vendas
