SELECT data_entrada::date AS data_entrada,
    count(*) AS qnt_vendas,
    COALESCE(sum(valor_entrada), 0::numeric) AS arrecadacao
   FROM vendas_mes
where 1=1
[[ AND {{id_vendas}}]]
  GROUP BY (data_entrada::date);
