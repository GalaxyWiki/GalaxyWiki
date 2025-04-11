CREATE TABLE users (
  google_sub VARCHAR(30) PRIMARY KEY,
  email VARCHAR(255) UNIQUE NOT NULL,
  display_name VARCHAR(100) NOT NULL,
  role_id INT
);

CREATE TABLE body_types (
  body_type_id SERIAL PRIMARY KEY NOT NULL,
  type_name VARCHAR(100) UNIQUE NOT NULL,
  description VARCHAR(100)
);

CREATE TABLE celestial_bodies (
  celestial_body_id SERIAL PRIMARY KEY NOT NULL,
  body_name VARCHAR(255) UNIQUE NOT NULL,
  orbits INT,
  active_revision INT DEFAULT NULL,
  body_type_id INT NOT NULL
);

CREATE TABLE comments (
  comment_id SERIAL PRIMARY KEY NOT NULL,
  celestial_body_id INT NOT NULL,
  user_id VARCHAR(30),
  comment VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE star_systems (
  system_id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  center_cb INT NOT NULL
);

CREATE TABLE content_revisions (
  revision_id SERIAL PRIMARY KEY,
  content VARCHAR(65536) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  celestial_body INT,
  author VARCHAR(30)
);

CREATE TABLE roles (
  role_id SERIAL PRIMARY KEY,
  role_name VARCHAR(255) NOT NULL
);

ALTER TABLE users ADD FOREIGN KEY (role_id) REFERENCES roles (role_id);

ALTER TABLE celestial_bodies ADD FOREIGN KEY (orbits) REFERENCES celestial_bodies (celestial_body_id);

ALTER TABLE celestial_bodies ADD FOREIGN KEY (body_type_id) REFERENCES body_types (body_type_id);

ALTER TABLE comments ADD FOREIGN KEY (celestial_body_id) REFERENCES celestial_bodies (celestial_body_id);

ALTER TABLE comments ADD FOREIGN KEY (user_id) REFERENCES users (google_sub);

ALTER TABLE star_systems ADD FOREIGN KEY (center_cb) REFERENCES celestial_bodies (celestial_body_id);

ALTER TABLE content_revisions ADD FOREIGN KEY (celestial_body) REFERENCES celestial_bodies (celestial_body_id);

ALTER TABLE content_revisions ADD FOREIGN KEY (author) REFERENCES users (google_sub);
