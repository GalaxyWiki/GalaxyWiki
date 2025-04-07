
    drop table People cascade

    drop table roles cascade

    drop table users cascade

    drop sequence hibernate_sequence

    create table People (
        Id int4 not null,
       Name varchar(255),
       primary key (Id)
    )

    create table roles (
        role_id int4 not null,
       role_name varchar(255),
       primary key (role_id)
    )

    create table users (
        google_sub varchar(255) not null,
       Email varchar(255),
       display_name varchar(255),
       role_id int4,
       primary key (google_sub)
    )

    alter table users 
        add constraint FK_535E54E7 
        foreign key (role_id) 
        references roles

    create sequence hibernate_sequence
