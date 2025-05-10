
    drop table if exists public.body_types cascade

    drop table if exists public.celestial_bodies cascade

    drop table if exists public.comments cascade

    drop table if exists public.content_revisions cascade

    drop table if exists public.People cascade

    drop table if exists public.roles cascade

    drop table if exists public.star_systems cascade

    drop table if exists public.users cascade

    create table public.body_types (
        body_type_id  serial,
       Type varchar(255),
       primary key (body_type_id)
    )

    create table public.celestial_bodies (
        celestial_body_id uuid not null,
       Name varchar(255),
       orbits uuid,
       body_type_id int4,
       primary key (celestial_body_id)
    )

    create table public.comments (
        comment_id uuid not null,
       celestial_body_id uuid,
       user_id varchar(255),
       comment varchar(255),
       created_at timestamp,
       primary key (comment_id)
    )

    create table public.content_revisions (
        revision_id  serial,
       Content varchar(255),
       created_at timestamp,
       celestial_body uuid,
       author varchar(255),
       primary key (revision_id)
    )

    create table public.People (
        Id  serial,
       Name varchar(255),
       primary key (Id)
    )

    create table public.roles (
        role_id  serial,
       role_name varchar(255),
       primary key (role_id)
    )

    create table public.star_systems (
        system_id uuid not null,
       name varchar(255) not null,
       center_cb_id uuid not null,
       primary key (system_id)
    )

    create table public.users (
        google_sub varchar(255) not null,
       Email varchar(255),
       display_name varchar(255),
       role_id int4,
       primary key (google_sub)
    )

    alter table public.celestial_bodies 
        add constraint FK_F4946BCF 
        foreign key (orbits) 
        references public.celestial_bodies

    alter table public.celestial_bodies 
        add constraint FK_4A373306 
        foreign key (body_type_id) 
        references public.body_types

    alter table public.comments 
        add constraint FK_5DA92E5B 
        foreign key (celestial_body_id) 
        references public.celestial_bodies

    alter table public.comments 
        add constraint FK_4410ED20 
        foreign key (user_id) 
        references public.users

    alter table public.content_revisions 
        add constraint FK_14D667CD 
        foreign key (celestial_body) 
        references public.celestial_bodies

    alter table public.content_revisions 
        add constraint FK_382C6E9A 
        foreign key (author) 
        references public.users

    alter table public.star_systems 
        add constraint FK_A825B424 
        foreign key (center_cb_id) 
        references public.celestial_bodies

    alter table public.users 
        add constraint FK_535E54E7 
        foreign key (role_id) 
        references public.roles
