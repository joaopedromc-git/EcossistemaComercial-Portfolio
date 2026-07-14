-- ============================================================
-- ECOSSISTEMA COMERCIAL - SEED DE DEMONSTRACAO
-- PostgreSQL 18+
--
-- Dados totalmente ficticios para dashboards, views e testes locais.
--
-- Credencial demonstrativa do portal:
-- senha para todos os usuarios: Portfolio@123
-- Esta senha existe somente no banco local de portfolio.
-- ============================================================

\set ON_ERROR_STOP on
\encoding UTF8

BEGIN;

SET CONSTRAINTS ALL DEFERRED;

-- ============================================================
-- CLEANUP
-- Permite executar o seed mais de uma vez.
-- ============================================================

DELETE FROM public.portal_acesso_equipes
WHERE usuario_id BETWEEN 9101 AND 9399;

DELETE FROM public.metas_notificadas
WHERE vendedor LIKE '%Demo%'
   OR vendedor IN (
       'Helena Costa',
       'Lucas Andrade',
       'Camila Martins',
       'Gustavo Freire',
       'Beatriz Lima',
       'Pedro Carvalho',
       'Larissa Campos',
       'Daniel Oliveira',
       'Ana Ribeiro'
   );

DELETE FROM public.vendas_notificadas
WHERE id_venda LIKE 'demo-%'
   OR id_venda ~ '^700[0-9]+$';

DELETE FROM public.vendas
WHERE id BETWEEN 700001 AND 700999
   OR nome LIKE 'Cliente Demonstração %';

DELETE FROM public.leads
WHERE id BETWEEN 800001 AND 800999
   OR nome LIKE 'Lead Demonstração %';

DELETE FROM public.vendedores
WHERE id BETWEEN 9101 AND 9399
   OR codigo_login LIKE 'demo.%';

DELETE FROM public.sync_control
WHERE workflow_name IN (
    'sync_vendas_demo',
    'sync_leads_demo',
    'notificacao_vendas_demo'
);

-- ============================================================
-- VENDEDORES E SUPERVISORES FICTICIOS
-- 3 equipes, cada uma com 1 supervisor e 2 vendedores.
-- ============================================================

INSERT INTO public.vendedores (
    id,
    nome_vendedor,
    meta_vendas,
    meta_arrecadacao,
    equipe,
    id_leads,
    codigo_login,
    supervisor,
    status,
    senha_hash,
    hubspot_company_id,
    acesso_superior,
    primeiro_acesso,
    senha_alterada_em
)
VALUES
(
    9101,
    'Helena Costa',
    25,
    15000.00,
    'Equipe Atlas',
    '19101',
    'demo.atlas.supervisor',
    9101,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9111,
    'Lucas Andrade',
    25,
    12000.00,
    'Equipe Atlas',
    '19111',
    'demo.atlas.01',
    9101,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9112,
    'Camila Martins',
    25,
    12000.00,
    'Equipe Atlas',
    '19112',
    'demo.atlas.02',
    9101,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9201,
    'Gustavo Freire',
    25,
    15000.00,
    'Equipe Aurora',
    '19201',
    'demo.aurora.supervisor',
    9201,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9211,
    'Beatriz Lima',
    25,
    12000.00,
    'Equipe Aurora',
    '19211',
    'demo.aurora.01',
    9201,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9212,
    'Pedro Carvalho',
    25,
    12000.00,
    'Equipe Aurora',
    '19212',
    'demo.aurora.02',
    9201,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9301,
    'Larissa Campos',
    25,
    15000.00,
    'Equipe Horizonte',
    '19301',
    'demo.horizonte.supervisor',
    9301,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9311,
    'Daniel Oliveira',
    25,
    12000.00,
    'Equipe Horizonte',
    '19311',
    'demo.horizonte.01',
    9301,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
),
(
    9312,
    'Ana Ribeiro',
    25,
    12000.00,
    'Equipe Horizonte',
    '19312',
    'demo.horizonte.02',
    9301,
    'ATIVO',
    public.crypt(
        'Portfolio@123',
        public.gen_salt('bf', 10)
    ),
    NULL,
    false,
    false,
    CURRENT_TIMESTAMP
);

-- ============================================================
-- 120 LEADS FICTICIOS NO MES ATUAL
-- Distribuidos entre os nove usuarios por id_leads.
-- ============================================================

WITH dados_leads AS (
    SELECT
        numero,
        ((numero - 1) % 9) + 1 AS posicao_vendedor
    FROM generate_series(1, 120) AS numero
)
INSERT INTO public.leads (
    id,
    nome,
    numero1,
    numero2,
    qualificacao,
    origem,
    proprietario_contato,
    data_atribuicao
)
SELECT
    800000 + numero,
    'Lead Demonstração ' ||
        lpad(numero::text, 3, '0'),
    '550000000' ||
        lpad(numero::text, 3, '0'),
    '551100000' ||
        lpad(numero::text, 3, '0'),
    CASE
        WHEN numero % 5 = 0 THEN 'Qualificado'
        WHEN numero % 5 = 1 THEN 'Novo'
        WHEN numero % 5 = 2 THEN 'Em análise'
        WHEN numero % 5 = 3 THEN 'Contato realizado'
        ELSE 'Aguardando retorno'
    END,
    CASE
        WHEN numero % 4 = 0 THEN 'Campanha Digital'
        WHEN numero % 4 = 1 THEN 'Busca Orgânica'
        WHEN numero % 4 = 2 THEN 'Indicação'
        ELSE 'Landing Page'
    END,
    (
        ARRAY[
            '19101',
            '19111',
            '19112',
            '19201',
            '19211',
            '19212',
            '19301',
            '19311',
            '19312'
        ]
    )[posicao_vendedor],
    date_trunc(
        'month',
        CURRENT_DATE::timestamp
    )
    + (
        (
            numero - 1
        )
        % greatest(
            EXTRACT(day FROM CURRENT_DATE)::integer,
            1
        )
    ) * INTERVAL '1 day'
    + (
        8 + (numero % 9)
    ) * INTERVAL '1 hour'
FROM dados_leads;

-- ============================================================
-- 36 VENDAS FICTICIAS NO MES ATUAL
-- 12 vendas por equipe e 4 por usuario.
-- ============================================================

WITH dados_vendas AS (
    SELECT
        numero,
        ((numero - 1) % 9) + 1 AS posicao_vendedor
    FROM generate_series(1, 36) AS numero
)
INSERT INTO public.vendas (
    nome,
    id,
    proprietario_contato,
    equipe,
    stts_matricula,
    valor_entrada,
    forma_pgto,
    data_entrada,
    origem,
    tipo_publi,
    curso_ofertado
)
SELECT
    'Cliente Demonstração ' ||
        lpad(numero::text, 3, '0'),
    700000 + numero,
    (
        ARRAY[
            '9101',
            '9111',
            '9112',
            '9201',
            '9211',
            '9212',
            '9301',
            '9311',
            '9312'
        ]
    )[posicao_vendedor],
    CASE
        WHEN posicao_vendedor BETWEEN 1 AND 3
            THEN 'Equipe Atlas'
        WHEN posicao_vendedor BETWEEN 4 AND 6
            THEN 'Equipe Aurora'
        ELSE 'Equipe Horizonte'
    END,
    CASE
        WHEN numero % 6 = 0
            THEN '💰 Entrada Paga'
        WHEN numero % 6 = 1
            THEN '🔔 Pendente'
        ELSE '🟢 Matricula Ativa'
    END,
    (
        450.00 +
        numero * 37.50
    )::numeric(12,2),
    CASE
        WHEN numero % 4 = 0
            THEN '💠 Pix A Vista'
        WHEN numero % 4 = 1
            THEN '💳 Cartao A Vista'
        WHEN numero % 4 = 2
            THEN '💳 Cartao Parcelado'
        ELSE '💠 Pix Parcelado'
    END,
    date_trunc(
        'month',
        CURRENT_DATE::timestamp
    )
    + (
        (
            numero - 1
        )
        % greatest(
            EXTRACT(day FROM CURRENT_DATE)::integer,
            1
        )
    ) * INTERVAL '1 day'
    + (
        9 + (numero % 8)
    ) * INTERVAL '1 hour',
    CASE
        WHEN numero % 4 = 0
            THEN 'Campanha Digital'
        WHEN numero % 4 = 1
            THEN 'Busca Orgânica'
        WHEN numero % 4 = 2
            THEN 'Indicação'
        ELSE 'Landing Page'
    END,
    CASE
        WHEN numero % 3 = 0
            THEN '💻 SED'
        ELSE '📰 Diario Oficial'
    END,
    CASE
        WHEN numero % 5 = 0
            THEN 'Profissionalizante'
        WHEN numero % 5 = 1
            THEN 'Curso Técnico'
        ELSE 'Curso Demonstração'
    END
FROM dados_vendas;

-- ============================================================
-- CONTROLE DE SINCRONIZACAO FICTICIO
-- ============================================================

INSERT INTO public.sync_control (
    workflow_name,
    last_success_at,
    updated_at
)
VALUES
(
    'sync_vendas_demo',
    CURRENT_TIMESTAMP - INTERVAL '5 minutes',
    CURRENT_TIMESTAMP
),
(
    'sync_leads_demo',
    CURRENT_TIMESTAMP - INTERVAL '1 hour',
    CURRENT_TIMESTAMP
),
(
    'notificacao_vendas_demo',
    CURRENT_TIMESTAMP - INTERVAL '2 minutes',
    CURRENT_TIMESTAMP
);

-- Mantem as sequences acima dos IDs inseridos manualmente.
SELECT setval(
    'public.vendas_mes_id_seq',
    greatest(
        COALESCE(
            (
                SELECT MAX(id)
                FROM public.vendas
            ),
            1
        ),
        1
    ),
    true
);

SELECT setval(
    'public.metas_notificadas_id_seq',
    greatest(
        COALESCE(
            (
                SELECT MAX(id)
                FROM public.metas_notificadas
            ),
            1
        ),
        1
    ),
    true
);

COMMIT;

-- ============================================================
-- VALIDACAO RAPIDA
-- ============================================================

SELECT
    (
        SELECT COUNT(*)
        FROM public.vendedores
    ) AS vendedores,
    (
        SELECT COUNT(*)
        FROM public.leads
    ) AS leads,
    (
        SELECT COUNT(*)
        FROM public.vendas
    ) AS vendas;

SELECT
    equipe,
    COUNT(*) AS vendas,
    COALESCE(
        SUM(valor_entrada),
        0
    ) AS arrecadacao
FROM public.vendas
GROUP BY equipe
ORDER BY equipe;

SELECT
    *
FROM public.vw_kpi_geral;
