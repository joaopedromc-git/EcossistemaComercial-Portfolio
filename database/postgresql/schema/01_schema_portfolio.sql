-- ============================================================
-- ECOSSISTEMA COMERCIAL - SCHEMA DE PORTFOLIO
-- PostgreSQL 18+
--
-- Este arquivo recria o schema completo usado no projeto de
-- demonstracao. Todos os nomes e dados empresariais foram removidos
-- ou substituidos por exemplos ficticios.
--
-- ATENCAO: este script remove e recria o schema public.
-- Execute somente em um banco vazio ou exclusivo para portfolio.
-- ============================================================

\set ON_ERROR_STOP on
\encoding UTF8

BEGIN;

DROP EXTENSION IF EXISTS pgcrypto CASCADE;
DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public;

COMMENT ON SCHEMA public IS
'Schema demonstrativo do ecossistema comercial de portfolio.';

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;

SET search_path = public, pg_catalog;
SET standard_conforming_strings = on;
SET client_min_messages = warning;

-- ============================================================
-- SEQUENCES
-- ============================================================

CREATE SEQUENCE public.vendas_mes_id_seq
    AS bigint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE public.metas_notificadas_id_seq
    AS bigint
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

-- ============================================================
-- TABLES
-- ============================================================

CREATE TABLE public.leads (
    id bigint PRIMARY KEY,
    nome character varying(255),
    numero1 character varying(255),
    numero2 character varying(255),
    qualificacao character varying(255),
    origem character varying(255),
    proprietario_contato character varying(255),
    data_atribuicao timestamp without time zone
);

CREATE TABLE public.vendedores (
    id integer PRIMARY KEY,
    nome_vendedor character varying(255) NOT NULL,
    meta_vendas integer,
    meta_arrecadacao numeric(12,2),
    equipe character varying(255),
    id_leads text,
    codigo_login character varying(100),
    supervisor bigint,
    status character varying(20) NOT NULL DEFAULT 'ATIVO',
    senha_hash text,
    hubspot_company_id bigint,
    acesso_superior boolean NOT NULL DEFAULT false,
    primeiro_acesso boolean NOT NULL DEFAULT true,
    senha_alterada_em timestamp with time zone,
    CONSTRAINT chk_vendedores_status
        CHECK (status IN ('ATIVO', 'DESATIVADO')),
    CONSTRAINT vendedores_nome_vendedor_unique
        UNIQUE (nome_vendedor),
    CONSTRAINT fk_vendedores_supervisor
        FOREIGN KEY (supervisor)
        REFERENCES public.vendedores(id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
        DEFERRABLE INITIALLY DEFERRED
);

CREATE TABLE public.feriado (
    data date PRIMARY KEY,
    descricao character varying(255)
);

CREATE TABLE public.vendas (
    nome character varying(255),
    id bigint NOT NULL DEFAULT nextval('public.vendas_mes_id_seq'::regclass),
    proprietario_contato character varying(255),
    equipe character varying(255),
    stts_matricula character varying(255),
    valor_entrada numeric(12,2),
    forma_pgto character varying(255),
    data_entrada timestamp without time zone,
    origem character varying(255),
    tipo_publi character varying(255),
    curso_ofertado character varying(255),
    CONSTRAINT vendas_mes_pkey PRIMARY KEY (id)
);

ALTER SEQUENCE public.vendas_mes_id_seq
    OWNED BY public.vendas.id;

CREATE TABLE public.exp_temp_leads (
    id bigint PRIMARY KEY,
    nome character varying(255),
    numero1 character varying(255),
    numero2 character varying(255),
    qualificacao character varying(255),
    origem character varying(255),
    proprietario_contato character varying(255),
    data_atribuicao timestamp without time zone
);

CREATE TABLE public.exp_temp_vendas (
    nome character varying(255),
    id bigint PRIMARY KEY,
    proprietario_contato character varying(255),
    equipe character varying(255),
    stts_matricula character varying(255),
    valor_entrada numeric(12,2),
    forma_pgto character varying(255),
    data_entrada timestamp without time zone,
    origem character varying(255),
    tipo_publi character varying(255),
    curso_ofertado character varying(255)
);

CREATE TABLE public.metas_notificadas (
    id bigint NOT NULL DEFAULT nextval('public.metas_notificadas_id_seq'::regclass),
    vendedor text NOT NULL,
    equipe text,
    tipo_meta text NOT NULL,
    mes_referencia date NOT NULL,
    valor_meta numeric,
    valor_atual numeric,
    notificado_em timestamp without time zone DEFAULT now(),
    CONSTRAINT metas_notificadas_pkey PRIMARY KEY (id),
    CONSTRAINT metas_notificadas_vendedor_tipo_meta_mes_referencia_key
        UNIQUE (vendedor, tipo_meta, mes_referencia)
);

ALTER SEQUENCE public.metas_notificadas_id_seq
    OWNED BY public.metas_notificadas.id;

CREATE TABLE public.portal_acesso_equipes (
    usuario_id bigint NOT NULL,
    equipe character varying(255) NOT NULL,
    CONSTRAINT pk_portal_acesso_equipes
        PRIMARY KEY (usuario_id, equipe),
    CONSTRAINT fk_portal_acesso_equipes_usuario
        FOREIGN KEY (usuario_id)
        REFERENCES public.vendedores(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE TABLE public.sync_control (
    workflow_name text PRIMARY KEY,
    last_success_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone DEFAULT now()
);

CREATE TABLE public.vendas_notificadas (
    id_venda text PRIMARY KEY,
    vendedor text,
    equipe text,
    valor numeric,
    forma_pagamento text,
    tipo_publicacao text,
    status_matricula text,
    notificado_em timestamp without time zone DEFAULT now()
);

-- ============================================================
-- INDEXES
-- ============================================================

CREATE INDEX idx_leads_data_atribuicao
    ON public.leads (data_atribuicao);

CREATE INDEX idx_leads_proprietario_contato
    ON public.leads (proprietario_contato);

CREATE INDEX idx_vendas_data_entrada
    ON public.vendas (data_entrada);

CREATE INDEX idx_vendas_proprietario_contato
    ON public.vendas (proprietario_contato);

CREATE INDEX idx_vendas_equipe
    ON public.vendas (equipe);

CREATE INDEX idx_vendedores_acesso_superior
    ON public.vendedores (acesso_superior);

CREATE INDEX idx_vendedores_status
    ON public.vendedores (status);

CREATE INDEX idx_vendedores_supervisor
    ON public.vendedores (supervisor);

CREATE UNIQUE INDEX uq_vendedores_codigo_login
    ON public.vendedores (
        lower(btrim(codigo_login))
    )
    WHERE codigo_login IS NOT NULL
      AND btrim(codigo_login) <> '';

CREATE UNIQUE INDEX uq_vendedores_hubspot_company
    ON public.vendedores (hubspot_company_id)
    WHERE hubspot_company_id IS NOT NULL;

CREATE UNIQUE INDEX uq_vendedores_id_leads
    ON public.vendedores (btrim(id_leads))
    WHERE id_leads IS NOT NULL
      AND btrim(id_leads) <> '';

-- ============================================================
-- VIEWS
-- Os nomes das views foram preservados para manter compatibilidade
-- com as queries e dashboards do projeto.
-- ============================================================

CREATE VIEW public.vw_calendario AS
SELECT
    d::date AS data,
    EXTRACT(year FROM d) AS ano,
    EXTRACT(month FROM d) AS mes,
    EXTRACT(day FROM d) AS dia,
    EXTRACT(week FROM d) AS semana,
    EXTRACT(quarter FROM d) AS trimestre,
    EXTRACT(isodow FROM d) NOT IN (6, 7) AS dia_util
FROM generate_series(
    date_trunc('year', CURRENT_DATE)::date - INTERVAL '1 year',
    date_trunc('year', CURRENT_DATE)::date + INTERVAL '3 years' - INTERVAL '1 day',
    INTERVAL '1 day'
) AS calendario(d);

CREATE VIEW public.dias_uteis_mes AS
SELECT
    COUNT(*) AS dias_uteis_totais,
    COUNT(*) FILTER (
        WHERE c.data <= CURRENT_DATE
    ) AS dias_uteis_passados,
    COUNT(*) FILTER (
        WHERE c.data > CURRENT_DATE
    ) AS dias_uteis_restantes
FROM public.vw_calendario c
LEFT JOIN public.feriado f
    ON f.data = c.data
WHERE c.dia_util = true
  AND f.data IS NULL
  AND c.ano = EXTRACT(year FROM CURRENT_DATE)
  AND c.mes = EXTRACT(month FROM CURRENT_DATE);

CREATE VIEW public.leads_mes AS
SELECT
    l.id,
    l.nome,
    l.numero1,
    l.numero2,
    l.qualificacao,
    l.origem,
    v.nome_vendedor AS proprietario_contato,
    l.data_atribuicao,
    l.proprietario_contato AS id_vendedor
FROM public.leads l
JOIN public.vendedores v
    ON (
        btrim(l.proprietario_contato) = v.id::text
        OR btrim(l.proprietario_contato) = btrim(v.id_leads)
        OR lower(btrim(l.proprietario_contato)) =
           lower(btrim(v.nome_vendedor))
    )
WHERE l.data_atribuicao >= date_trunc('month', CURRENT_DATE)
  AND l.data_atribuicao < date_trunc('month', CURRENT_DATE) + INTERVAL '1 month'
  AND v.status = 'ATIVO'
  AND v.equipe IS NOT NULL;

CREATE VIEW public.vendas_mes AS
SELECT
    v.nome,
    v.id,
    c.nome_vendedor AS proprietario_contato,
    c.equipe,
    v.stts_matricula,
    v.valor_entrada,
    v.forma_pgto,
    v.data_entrada,
    v.origem,
    v.tipo_publi,
    v.curso_ofertado,
    c.id AS id_vendedor
FROM public.vendas v
JOIN public.vendedores c
    ON (
        btrim(v.proprietario_contato) = c.id::text
        OR lower(btrim(v.proprietario_contato)) =
           lower(btrim(c.nome_vendedor))
    )
WHERE v.data_entrada >= date_trunc('month', CURRENT_DATE)
  AND v.data_entrada < date_trunc('month', CURRENT_DATE) + INTERVAL '1 month'
  AND c.status = 'ATIVO'
  AND v.stts_matricula IN (
      '💰 Entrada Paga',
      '🔔 Pendente',
      '🔔 Pendente Adm',
      '🔔 Pendente Vendedor',
      '🟢 Matricula Ativa',
      '📌 Aguardando Certificação',
      '📦 Em Certificação',
      '📬 Certificado Enviado',
      '🎓 Aluno Formado',
      '🟢 Matricula Tecnico',
      '💼 Profissionalizante'
  );

CREATE VIEW public.atribuidos_hoje AS
WITH base AS (
    SELECT
        proprietario_contato AS nome_vendedor,
        COUNT(*) AS qnt_leads
    FROM public.leads_mes
    WHERE data_atribuicao::date = CURRENT_DATE
    GROUP BY proprietario_contato
),
resultado AS (
    SELECT
        nome_vendedor,
        qnt_leads,
        1 AS ordem
    FROM base

    UNION ALL

    SELECT
        'Total'::character varying(255) AS nome_vendedor,
        COALESCE(SUM(qnt_leads), 0)::bigint AS qnt_leads,
        2 AS ordem
    FROM base
)
SELECT
    nome_vendedor,
    qnt_leads
FROM resultado
ORDER BY ordem, qnt_leads DESC;

CREATE VIEW public.vw_leads_vendedor AS
SELECT
    proprietario_contato AS consultor,
    COUNT(*) AS qnt_leads
FROM public.leads_mes
GROUP BY proprietario_contato;

CREATE VIEW public.vw_leads_equipes AS
SELECT
    c.equipe,
    COALESCE(SUM(l.qnt_leads), 0) AS qnt_leads
FROM public.vendedores c
LEFT JOIN public.vw_leads_vendedor l
    ON l.consultor = c.nome_vendedor
WHERE c.status = 'ATIVO'
  AND c.equipe IS NOT NULL
GROUP BY c.equipe;

CREATE VIEW public.vw_vendas_vendedor AS
SELECT
    proprietario_contato AS consultor,
    equipe,
    COUNT(*) FILTER (
        WHERE curso_ofertado IS DISTINCT FROM 'Profissionalizante'
    ) AS qnt_vendas,
    COALESCE(SUM(valor_entrada), 0) AS arrecadacao,
    COUNT(*) FILTER (
        WHERE tipo_publi = '💻 SED'
    ) AS qnt_sed
FROM public.vendas_mes
GROUP BY proprietario_contato, equipe;

CREATE VIEW public.vw_vendas_equipes AS
SELECT
    c.equipe,
    COALESCE(SUM(v.qnt_vendas), 0) AS qnt_vendas,
    COALESCE(SUM(v.arrecadacao), 0) AS arrecadacao,
    COALESCE(SUM(c.meta_vendas), 0) AS meta_vendas,
    COALESCE(SUM(c.meta_arrecadacao), 0) AS meta_arrecadacao
FROM public.vendedores c
LEFT JOIN public.vw_vendas_vendedor v
    ON v.consultor = c.nome_vendedor
WHERE c.status = 'ATIVO'
  AND c.equipe IS NOT NULL
GROUP BY c.equipe;

CREATE VIEW public.kpi_equipes AS
SELECT
    v.equipe,
    v.qnt_vendas,
    v.arrecadacao,
    COALESCE(l.qnt_leads, 0) AS qnt_leads,
    ROUND(
        v.qnt_vendas::numeric
        / NULLIF(COALESCE(l.qnt_leads, 0), 0)
        * 100,
        2
    ) AS conversao,
    ROUND(
        v.qnt_vendas::numeric
        / NULLIF(v.meta_vendas, 0)
        * 100,
        2
    ) AS meta_vendas_batida,
    ROUND(
        v.arrecadacao
        / NULLIF(v.meta_arrecadacao, 0)
        * 100,
        2
    ) AS meta_arrecadacao_batida
FROM public.vw_vendas_equipes v
LEFT JOIN public.vw_leads_equipes l
    ON l.equipe = v.equipe;

CREATE VIEW public.ranking_mensal AS
WITH vendas_pontuadas AS (
    SELECT
        proprietario_contato,
        tipo_publi,
        forma_pgto,
        CASE
            WHEN tipo_publi = '📰 Diario Oficial'
             AND forma_pgto = '💠 Pix A Vista'
                THEN 2::numeric

            WHEN tipo_publi = '📰 Diario Oficial'
             AND forma_pgto IN (
                 '💳 Cartao A Vista',
                 '💳 Cartao Parcelado'
             )
                THEN 1.5::numeric

            WHEN tipo_publi = '📰 Diario Oficial'
             AND forma_pgto = '💠 Pix Parcelado'
                THEN 1::numeric

            WHEN tipo_publi = '💻 SED'
             AND forma_pgto IN (
                 '💠 Pix A Vista',
                 '💳 Cartao A Vista',
                 '💳 Cartao Parcelado'
             )
                THEN 5::numeric

            WHEN tipo_publi = '💻 SED'
             AND forma_pgto = '💠 Pix Parcelado'
                THEN 2::numeric

            ELSE 0::numeric
        END AS pontos
    FROM public.vendas_mes
),
ranking AS (
    SELECT
        proprietario_contato AS consultor,
        SUM(pontos) AS pontos_total
    FROM vendas_pontuadas
    GROUP BY proprietario_contato
)
SELECT
    RANK() OVER (
        ORDER BY pontos_total DESC
    ) AS rank_vendas,
    consultor,
    pontos_total
FROM ranking
ORDER BY rank_vendas;

CREATE VIEW public.vendas_dia_atual AS
SELECT
    proprietario_contato,
    equipe,
    forma_pgto,
    valor_entrada,
    stts_matricula
FROM public.vendas_mes
WHERE data_entrada::date = CURRENT_DATE
ORDER BY
    CASE stts_matricula
        WHEN '🟢 Matricula Ativa' THEN 1
        WHEN '💰 Entrada Paga' THEN 2
        WHEN '🔔 Pendente' THEN 3
        WHEN '🔔 Pendente Adm' THEN 4
        WHEN '🔔 Pendente Vendedor' THEN 5
        ELSE 99
    END;

CREATE VIEW public.vw_classificacao_portal AS
SELECT
    u.id,
    u.id_leads,
    u.nome_vendedor,
    u.codigo_login,
    u.equipe,
    u.supervisor,
    u.status,
    u.acesso_superior,
    CASE
        WHEN u.acesso_superior = true
            THEN 'SUPERIOR'::text
        WHEN EXISTS (
            SELECT 1
            FROM public.vendedores subordinado
            WHERE subordinado.status = 'ATIVO'
              AND subordinado.supervisor = u.id
              AND subordinado.id <> u.id
        )
            THEN 'SUPERVISOR'::text
        ELSE 'VENDEDOR'::text
    END AS tipo_acesso
FROM public.vendedores u;

CREATE VIEW public.vw_kpi_consultor AS
SELECT
    c.nome_vendedor AS consultor,
    c.equipe,
    COALESCE(v.qnt_vendas, 0)::numeric AS vendas,
    COALESCE(v.arrecadacao, 0)::numeric AS arrecadacao,
    ROUND(
        COALESCE(v.qnt_vendas, 0)::numeric
        / NULLIF(c.meta_vendas, 0)
        * 100,
        2
    ) AS batida_vendas,
    ROUND(
        COALESCE(v.arrecadacao, 0)::numeric
        / NULLIF(c.meta_arrecadacao, 0)
        * 100,
        2
    ) AS batida_arrecadacao,
    COALESCE(l.qnt_leads, 0)::numeric AS leads,
    ROUND(
        COALESCE(v.qnt_vendas, 0)::numeric
        / NULLIF(COALESCE(l.qnt_leads, 0), 0)
        * 100,
        2
    ) AS conversao
FROM public.vendedores c
LEFT JOIN public.vw_vendas_vendedor v
    ON v.consultor = c.nome_vendedor
LEFT JOIN public.vw_leads_vendedor l
    ON l.consultor = c.nome_vendedor
WHERE c.status = 'ATIVO'
  AND c.nome_vendedor IS NOT NULL
  AND c.equipe IS NOT NULL
ORDER BY
    COALESCE(v.qnt_vendas, 0) DESC,
    COALESCE(v.arrecadacao, 0) DESC;

CREATE VIEW public.vw_kpi_geral AS
WITH vendas_totais AS (
    SELECT
        COUNT(*) AS vendas_total,
        COALESCE(SUM(valor_entrada), 0) AS arrecadacao_total
    FROM public.vendas_mes
),
leads_totais AS (
    SELECT
        COUNT(*) AS leads_total
    FROM public.leads_mes
)
SELECT
    v.vendas_total,
    v.arrecadacao_total,
    l.leads_total,
    ROUND(
        v.vendas_total::numeric
        / NULLIF(l.leads_total, 0)
        * 100,
        2
    ) AS conv_geral,
    ROUND(
        v.arrecadacao_total
        / NULLIF(d.dias_uteis_passados, 0)
        * d.dias_uteis_totais,
        2
    ) AS projecao
FROM vendas_totais v
CROSS JOIN leads_totais l
CROSS JOIN public.dias_uteis_mes d;

CREATE VIEW public.vw_permissoes_dashboard AS
SELECT DISTINCT
    usuario.id AS usuario_id_vendas,
    usuario.id_leads AS usuario_id_leads,
    usuario.codigo_login,
    usuario.nome_vendedor AS nome_usuario,
    permitido.id AS id_vendas_permitido,
    permitido.id_leads AS id_leads_permitido,
    permitido.nome_vendedor AS nome_permitido,
    permitido.equipe AS equipe_permitida
FROM public.vendedores usuario
JOIN public.vendedores permitido
    ON permitido.status = 'ATIVO'
   AND (
       usuario.acesso_superior = true
       OR permitido.id = usuario.id
       OR permitido.supervisor = usuario.id
   )
WHERE usuario.status = 'ATIVO'
  AND usuario.codigo_login IS NOT NULL;

CREATE VIEW public.vw_tabela_conversao AS
SELECT
    consultor,
    equipe,
    vendas,
    leads,
    conversao
FROM public.vw_kpi_consultor
ORDER BY conversao DESC NULLS LAST;

CREATE VIEW public.vw_vendas_dias AS
SELECT
    data_entrada::date AS data_entrada,
    COUNT(*) AS qnt_vendas,
    COALESCE(SUM(valor_entrada), 0) AS arrecadacao
FROM public.vendas_mes
GROUP BY data_entrada::date;

-- ============================================================
-- FUNCTIONS
-- ============================================================

CREATE FUNCTION public.alterar_senha_primeiro_acesso(
    p_id text,
    p_nova_senha text
)
RETURNS boolean
LANGUAGE plpgsql
SECURITY DEFINER
SET search_path TO pg_catalog, public
AS $$
DECLARE
    linhas_afetadas integer;
BEGIN
    IF p_id IS NULL
       OR btrim(p_id) !~ '^[0-9]+$'
       OR p_nova_senha IS NULL
       OR length(btrim(p_nova_senha)) < 6
    THEN
        RETURN false;
    END IF;

    UPDATE public.vendedores
    SET
        senha_hash = public.crypt(
            p_nova_senha,
            public.gen_salt('bf', 12)
        ),
        primeiro_acesso = false,
        senha_alterada_em = now()
    WHERE id = p_id::integer
      AND status = 'ATIVO'
      AND codigo_login IS NOT NULL;

    GET DIAGNOSTICS linhas_afetadas = ROW_COUNT;

    RETURN linhas_afetadas = 1;
END;
$$;

CREATE FUNCTION public.autenticar_usuario_portal(
    p_codigo_login text,
    p_senha text
)
RETURNS TABLE (
    id text,
    id_leads text,
    nome_vendedor text,
    codigo_login text,
    equipe text,
    tipo_acesso text,
    acesso_superior boolean,
    primeiro_acesso boolean
)
LANGUAGE sql
SECURITY DEFINER
SET search_path TO pg_catalog, public
AS $$
    SELECT
        v.id::text,
        v.id_leads::text,
        v.nome_vendedor::text,
        v.codigo_login::text,
        v.equipe::text,
        c.tipo_acesso::text,
        v.acesso_superior,
        v.primeiro_acesso
    FROM public.vendedores v
    JOIN public.vw_classificacao_portal c
        ON c.id = v.id
    WHERE v.status = 'ATIVO'
      AND p_codigo_login IS NOT NULL
      AND p_senha IS NOT NULL
      AND lower(btrim(v.codigo_login)) =
          lower(btrim(p_codigo_login))
      AND v.senha_hash IS NOT NULL
      AND v.senha_hash = public.crypt(
          p_senha,
          v.senha_hash
      )
    LIMIT 1;
$$;

CREATE FUNCTION public.obter_permissoes_metabase(
    p_usuario_id text
)
RETURNS TABLE (
    ids_vendas text[],
    ids_leads text[]
)
LANGUAGE sql
SECURITY DEFINER
SET search_path TO pg_catalog, public
AS $$
    SELECT
        COALESCE(
            ARRAY_AGG(
                DISTINCT p.id_vendas_permitido::text
                ORDER BY p.id_vendas_permitido::text
            ),
            ARRAY[]::text[]
        ) AS ids_vendas,
        COALESCE(
            ARRAY_AGG(
                DISTINCT p.id_leads_permitido::text
                ORDER BY p.id_leads_permitido::text
            ) FILTER (
                WHERE p.id_leads_permitido IS NOT NULL
            ),
            ARRAY[]::text[]
        ) AS ids_leads
    FROM public.vw_permissoes_dashboard p
    WHERE p.usuario_id_vendas::text = p_usuario_id;
$$;

CREATE FUNCTION public.obter_permissoes_metabase(
    p_usuario_id text,
    p_modo text DEFAULT 'COMUM'
)
RETURNS TABLE (
    ids_vendas text[],
    ids_leads text[]
)
LANGUAGE sql
SECURITY DEFINER
SET search_path TO pg_catalog, public
AS $$
    WITH usuario_atual AS (
        SELECT
            id,
            acesso_superior
        FROM public.vendedores
        WHERE id::text = p_usuario_id
          AND status = 'ATIVO'
    ),
    permissoes AS (
        -- Modo comum:
        -- vendedor ve a si mesmo;
        -- supervisor ve a si mesmo e subordinados;
        -- superior ve todos os usuarios ativos.
        SELECT
            p.id_vendas_permitido::text AS id_vendas,
            p.id_leads_permitido::text AS id_leads
        FROM public.vw_permissoes_dashboard p
        WHERE p.usuario_id_vendas::text = p_usuario_id
          AND upper(COALESCE(p_modo, 'COMUM')) = 'COMUM'

        UNION

        -- Modo equipe para usuarios sem acesso superior.
        SELECT
            p.id_vendas_permitido::text,
            p.id_leads_permitido::text
        FROM public.vw_permissoes_dashboard p
        JOIN usuario_atual u
            ON u.acesso_superior = false
        WHERE p.usuario_id_vendas::text = p_usuario_id
          AND upper(COALESCE(p_modo, 'COMUM')) = 'EQUIPE'

        UNION

        -- Superior em modo equipe ve apenas as equipes liberadas.
        SELECT
            v.id::text,
            v.id_leads::text
        FROM public.portal_acesso_equipes a
        JOIN usuario_atual u
            ON u.id = a.usuario_id
           AND u.acesso_superior = true
        JOIN public.vendedores v
            ON v.equipe = a.equipe
           AND v.status = 'ATIVO'
        WHERE upper(COALESCE(p_modo, 'COMUM')) = 'EQUIPE'
    )
    SELECT
        COALESCE(
            ARRAY_AGG(
                DISTINCT id_vendas
                ORDER BY id_vendas
            ),
            ARRAY[]::text[]
        ) AS ids_vendas,
        COALESCE(
            ARRAY_AGG(
                DISTINCT id_leads
                ORDER BY id_leads
            ) FILTER (
                WHERE id_leads IS NOT NULL
            ),
            ARRAY[]::text[]
        ) AS ids_leads
    FROM permissoes;
$$;

-- SECURITY DEFINER functions should not be executable by every database role.
REVOKE ALL
ON FUNCTION public.alterar_senha_primeiro_acesso(text, text)
FROM PUBLIC;

REVOKE ALL
ON FUNCTION public.autenticar_usuario_portal(text, text)
FROM PUBLIC;

REVOKE ALL
ON FUNCTION public.obter_permissoes_metabase(text)
FROM PUBLIC;

REVOKE ALL
ON FUNCTION public.obter_permissoes_metabase(text, text)
FROM PUBLIC;

-- Grant EXECUTE to the dedicated application role during deployment.
-- Example:
-- GRANT EXECUTE ON FUNCTION public.autenticar_usuario_portal(text, text)
-- TO portfolio_portal_app;

COMMIT;
