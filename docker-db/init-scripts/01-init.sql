--
-- PostgreSQL database dump
--


-- Dumped from database version 18.1 (Debian 18.1-1.pgdg13+2)
-- Dumped by pg_dump version 18.1 (Debian 18.1-1.pgdg13+2)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: update_media_avg_score(); Type: FUNCTION; Schema: public; Owner: admin
--

CREATE FUNCTION public.update_media_avg_score() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE media_entries
    SET avg_score = (
        SELECT COALESCE(AVG(stars), 0)
        FROM ratings 
        WHERE media_id = COALESCE(NEW.media_id, OLD.media_id)
    ),
    updated_at = CURRENT_TIMESTAMP
    WHERE id = COALESCE(NEW.media_id, OLD.media_id);
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.update_media_avg_score() 

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: favorites; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.favorites (
    user_id integer NOT NULL,
    media_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.favorites 

--
-- Name: media_entries; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.media_entries (
    id integer NOT NULL,
    user_id integer,
    title character varying(200) NOT NULL,
    description text,
    media_type character varying(20),
    release_year integer,
    age_restriction character varying(10) DEFAULT 'FSK0'::character varying,
    avg_score numeric(3,2) DEFAULT 0.0,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT media_entries_media_type_check CHECK (((media_type)::text = ANY ((ARRAY['movie'::character varying, 'series'::character varying, 'game'::character varying])::text[])))
);


ALTER TABLE public.media_entries 

--
-- Name: media_entries_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.media_entries_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.media_entries_id_seq 

--
-- Name: media_entries_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.media_entries_id_seq OWNED BY public.media_entries.id;


--
-- Name: media_genres; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.media_genres (
    media_id integer NOT NULL,
    genre character varying(50) NOT NULL
);


ALTER TABLE public.media_genres 

--
-- Name: rating_likes; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.rating_likes (
    rating_id integer NOT NULL,
    user_id integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.rating_likes 

--
-- Name: ratings; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.ratings (
    id integer NOT NULL,
    user_id integer,
    media_id integer,
    stars integer NOT NULL,
    comment text,
    comment_published boolean DEFAULT false,
    likes_count integer DEFAULT 0,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ratings_stars_check CHECK (((stars >= 1) AND (stars <= 5)))
);


ALTER TABLE public.ratings 

--
-- Name: ratings_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.ratings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.ratings_id_seq 

--
-- Name: ratings_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.ratings_id_seq OWNED BY public.ratings.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.users (
    id integer NOT NULL,
    username character varying(50) NOT NULL,
    password_hash character varying(255) NOT NULL,
    salt character varying(100) NOT NULL,
    token character varying(255),
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    token_created_at timestamp without time zone
);


ALTER TABLE public.users 

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq 

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: admin
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: media_entries id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.media_entries ALTER COLUMN id SET DEFAULT nextval('public.media_entries_id_seq'::regclass);


--
-- Name: ratings id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ratings ALTER COLUMN id SET DEFAULT nextval('public.ratings_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: favorites favorites_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.favorites
    ADD CONSTRAINT favorites_pkey PRIMARY KEY (user_id, media_id);


--
-- Name: media_entries media_entries_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.media_entries
    ADD CONSTRAINT media_entries_pkey PRIMARY KEY (id);


--
-- Name: media_genres media_genres_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.media_genres
    ADD CONSTRAINT media_genres_pkey PRIMARY KEY (media_id, genre);


--
-- Name: rating_likes rating_likes_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.rating_likes
    ADD CONSTRAINT rating_likes_pkey PRIMARY KEY (rating_id, user_id);


--
-- Name: ratings ratings_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ratings
    ADD CONSTRAINT ratings_pkey PRIMARY KEY (id);


--
-- Name: ratings ratings_user_id_media_id_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ratings
    ADD CONSTRAINT ratings_user_id_media_id_key UNIQUE (user_id, media_id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_username_key; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);


--
-- Name: favorites favorites_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.favorites
    ADD CONSTRAINT favorites_media_id_fkey FOREIGN KEY (media_id) REFERENCES public.media_entries(id) ON DELETE CASCADE;


--
-- Name: favorites favorites_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.favorites
    ADD CONSTRAINT favorites_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: media_entries media_entries_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.media_entries
    ADD CONSTRAINT media_entries_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: media_genres media_genres_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.media_genres
    ADD CONSTRAINT media_genres_media_id_fkey FOREIGN KEY (media_id) REFERENCES public.media_entries(id) ON DELETE CASCADE;


--
-- Name: rating_likes rating_likes_rating_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.rating_likes
    ADD CONSTRAINT rating_likes_rating_id_fkey FOREIGN KEY (rating_id) REFERENCES public.ratings(id) ON DELETE CASCADE;


--
-- Name: rating_likes rating_likes_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.rating_likes
    ADD CONSTRAINT rating_likes_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: ratings ratings_media_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ratings
    ADD CONSTRAINT ratings_media_id_fkey FOREIGN KEY (media_id) REFERENCES public.media_entries(id) ON DELETE CASCADE;


--
-- Name: ratings ratings_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.ratings
    ADD CONSTRAINT ratings_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--


