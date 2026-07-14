select nome, stts_matricula, valor_entrada, forma_pgto, data_entrada, tipo_publi, curso_ofertado from vendas_mes
where 1=1 
[[ AND {{id_vendas}} ]]
