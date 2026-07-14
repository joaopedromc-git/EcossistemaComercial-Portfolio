-- =====================================
-- Tabela: vendedores
-- =====================================
CREATE TABLE public.vendedores (
    id integer,
    nome_vendedor character varying(255),
    meta_vendas integer,
    meta_arrecadacao numeric(12,2),
    equipe character varying(255),
    id_leads text,
    codigo_login character varying(100),
    supervisor bigint,
    status character varying(20) DEFAULT 'ATIVO'::character varying NOT NULL,
    senha_hash text,
    hubspot_company_id bigint,
    acesso_superior boolean DEFAULT false NOT NULL,
    primeiro_acesso boolean DEFAULT true NOT NULL,
    senha_alterada_em timestamp with time zone,
    CONSTRAINT chk_vendedores_status CHECK (((status)::text = ANY ((ARRAY['ATIVO'::character varying, 'DESATIVADO'::character varying])::text[])))
);

-- =====================================
-- Tabela: vendas
-- =====================================
CREATE TABLE public.vendas (
    nome character varying(255),
    id bigint CONSTRAINT vendas_mes_id_not_null NOT NULL,
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

-- =====================================
-- Tabela: leads
-- =====================================
CREATE TABLE public.leads (
    id bigint NOT NULL,
    nome character varying(255),
    numero1 character varying(255),
    numero2 character varying(255),
    qualificacao character varying(255),
    origem character varying(255),
    proprietario_contato character varying(255),
    data_atribuicao timestamp without time zone
);

-- =====================================
-- Tabela: feriado
-- =====================================
CREATE TABLE public.feriado (
    data date NOT NULL,
    descricao character varying(255)
);

-- =====================================
-- Tabela: sync_control
-- =====================================
CREATE TABLE public.sync_control (
    workflow_name text NOT NULL,
    last_success_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone DEFAULT now()
);

-- =====================================
-- Tabela: vendas_notificadas
-- =====================================
CREATE TABLE public.vendas_notificadas (
    id_venda text NOT NULL,
    vendedor text,
    equipe text,
    valor numeric,
    forma_pagamento text,
    tipo_publicacao text,
    status_matricula text,
    notificado_em timestamp without time zone DEFAULT now()
);

