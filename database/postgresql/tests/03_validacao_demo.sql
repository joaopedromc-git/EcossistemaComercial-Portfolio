-- ============================================================
-- VALIDAÇÃO DO BANCO DE DEMONSTRAÇÃO
-- ============================================================

\echo ''
\echo '1. Quantidade de registros'
\echo '--------------------------'

SELECT
    (SELECT COUNT(*) FROM vendedores) AS vendedores,
    (SELECT COUNT(*) FROM leads) AS leads,
    (SELECT COUNT(*) FROM vendas) AS vendas;

\echo ''
\echo '2. Vendas por equipe'
\echo '---------------------'

SELECT
    equipe,
    COUNT(*) AS quantidade_vendas,
    SUM(valor_entrada) AS arrecadacao
FROM vendas
GROUP BY equipe
ORDER BY equipe;

\echo ''
\echo '3. Vendas por vendedor'
\echo '-----------------------'

SELECT
    vd.nome_vendedor,
    vd.equipe,
    COUNT(v.id) AS quantidade_vendas,
    COALESCE(
        SUM(v.valor_entrada),
        0
    ) AS arrecadacao
FROM vendedores vd
LEFT JOIN vendas v
    ON v.proprietario_contato = vd.id::text
GROUP BY
    vd.id,
    vd.nome_vendedor,
    vd.equipe
ORDER BY
    vd.equipe,
    vd.nome_vendedor;

\echo ''
\echo '4. Leads por vendedor'
\echo '----------------------'

SELECT
    vd.nome_vendedor,
    vd.equipe,
    COUNT(l.id) AS quantidade_leads
FROM vendedores vd
LEFT JOIN leads l
    ON l.proprietario_contato = vd.id_leads
GROUP BY
    vd.id,
    vd.nome_vendedor,
    vd.equipe
ORDER BY
    vd.equipe,
    vd.nome_vendedor;

\echo ''
\echo '5. Conversão por vendedor'
\echo '--------------------------'

WITH vendas_vendedor AS (
    SELECT
        proprietario_contato,
        COUNT(*) AS quantidade_vendas
    FROM vendas
    GROUP BY proprietario_contato
),
leads_vendedor AS (
    SELECT
        proprietario_contato,
        COUNT(*) AS quantidade_leads
    FROM leads
    GROUP BY proprietario_contato
)
SELECT
    vd.nome_vendedor,
    vd.equipe,

    COALESCE(
        lv.quantidade_leads,
        0
    ) AS leads,

    COALESCE(
        vv.quantidade_vendas,
        0
    ) AS vendas,

    ROUND(
        COALESCE(
            vv.quantidade_vendas,
            0
        )::numeric
        /
        NULLIF(
            COALESCE(
                lv.quantidade_leads,
                0
            ),
            0
        )
        * 100,
        2
    ) AS conversao_percentual

FROM vendedores vd

LEFT JOIN vendas_vendedor vv
    ON vv.proprietario_contato = vd.id::text

LEFT JOIN leads_vendedor lv
    ON lv.proprietario_contato = vd.id_leads

ORDER BY
    vd.equipe,
    vd.nome_vendedor;

\echo ''
\echo '6. Verificação de relacionamentos inválidos'
\echo '--------------------------------------------'

SELECT
    COUNT(*) AS vendas_sem_vendedor
FROM vendas v
LEFT JOIN vendedores vd
    ON vd.id::text = v.proprietario_contato
WHERE vd.id IS NULL;

SELECT
    COUNT(*) AS leads_sem_vendedor
FROM leads l
LEFT JOIN vendedores vd
    ON vd.id_leads = l.proprietario_contato
WHERE vd.id IS NULL;

\echo ''
\echo '7. Dados obrigatórios ausentes'
\echo '-------------------------------'

SELECT
    COUNT(*) AS vendas_sem_id
FROM vendas
WHERE id IS NULL;

SELECT
    COUNT(*) AS vendedores_com_status_invalido
FROM vendedores
WHERE status NOT IN (
    'ATIVO',
    'DESATIVADO'
);

\echo ''
\echo '8. Views disponíveis'
\echo '---------------------'

SELECT
    table_name
FROM information_schema.views
WHERE table_schema = 'public'
ORDER BY table_name;