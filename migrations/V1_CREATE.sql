-- Create roles table
CREATE TABLE roles (
    role_id serial PRIMARY KEY,
    role_name varchar(255) NOT NULL
);

-- Create body_types table
CREATE TABLE body_types (
    body_type_id serial PRIMARY KEY,
    type varchar(100) NOT NULL UNIQUE
);

-- Create users table
CREATE TABLE users (
    google_sub varchar(30) PRIMARY KEY NOT NULL,
    email varchar(255) NOT NULL UNIQUE,
    display_name varchar(100) NOT NULL,
    role_id int REFERENCES roles(role_id)
);

-- Create celestial_bodies table without the active_revision foreign key constraint yet.
CREATE TABLE celestial_bodies (
    celestial_body_id uuid PRIMARY KEY NOT NULL,
    name varchar(255) NOT NULL UNIQUE,
    orbits uuid REFERENCES celestial_bodies(celestial_body_id),  -- Self reference; may be null.
    active_revision int,  -- will add the foreign key later
    body_type_id int NOT NULL REFERENCES body_types(body_type_id)
);

-- Create content_revisions table.
CREATE TABLE content_revisions (
    revision_id serial PRIMARY KEY NOT NULL,
    content varchar(65536) NOT NULL,
    created_at timestamp,
    celestial_body uuid REFERENCES celestial_bodies(celestial_body_id),
    author varchar(30) REFERENCES users(google_sub)
);

-- Create comments table.
CREATE TABLE comments (
    comment_id uuid PRIMARY KEY NOT NULL,
    celestial_body_id uuid REFERENCES celestial_bodies(celestial_body_id),
    user_id varchar(30) REFERENCES users(google_sub),
    comment varchar(255) NOT NULL,
    created_at timestamp NOT NULL
);

-- Create star_systems table.
CREATE TABLE star_systems (
    system_id uuid PRIMARY KEY NOT NULL,
    name varchar(255) NOT NULL,
    center_cb uuid REFERENCES celestial_bodies(celestial_body_id)
);

-- After both celestial_bodies and content_revisions have been created,
-- add the foreign key constraint from celestial_bodies.active_revision to content_revisions.revision_id.
ALTER TABLE celestial_bodies
    ADD CONSTRAINT fk_active_revision
    FOREIGN KEY (active_revision)
    REFERENCES content_revisions(revision_id);
