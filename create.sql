CREATE TABLE public.cards (
    id character varying(60) NOT NULL,
    element integer,
    cardtype integer,
    damage double precision,
    indeck boolean,
    intrade boolean,
    userid integer,
    cardname character varying(60),
    packageid character varying(60)
);

CREATE TABLE public.packages (
    id character varying(60) NOT NULL,
    bought boolean DEFAULT false
);

CREATE SEQUENCE public.player_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE public.trades (
    id character varying(60) NOT NULL,
    requiredtype character varying(60),
    requireddamage double precision,
    cardid character varying
);

CREATE SEQUENCE public.user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE SEQUENCE public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE public.users (
    id integer DEFAULT nextval('public.users_id_seq'::regclass) NOT NULL,
    username character varying(50) NOT NULL,
    password character varying(50) NOT NULL,
    coins integer NOT NULL,
    elo integer NOT NULL,
    wins integer,
    losses integer,
    name character varying(20),
    bio character varying(50),
    image character varying(20)
);

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.packages
    ADD CONSTRAINT packages_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.trades
    ADD CONSTRAINT trades_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);

ALTER TABLE ONLY public.cards
    ADD CONSTRAINT cards_userid_fkey FOREIGN KEY (userid) REFERENCES public.users(id);

ALTER TABLE ONLY public.trades
    ADD CONSTRAINT trades_cardid_fkey FOREIGN KEY (cardid) REFERENCES public.cards(id);