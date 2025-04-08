-- Insert a new user into the users table.
CREATE OR REPLACE FUNCTION proc_add_user(
    p_google_sub varchar(30),
    p_email varchar(255),
    p_display_name varchar(100),
    p_role_name varchar(255)
)
RETURNS void AS '
DECLARE
    v_role_id int;
BEGIN
    -- Look up the role_id from the roles table using the provided role name.
    SELECT role_id INTO v_role_id FROM roles WHERE role_name = p_role_name;
    IF v_role_id IS NULL THEN
        RAISE EXCEPTION ''Role "%" not found'', p_role_name;
    END IF;
    
    INSERT INTO users (google_sub, email, display_name, role_id)
    VALUES (p_google_sub, p_email, p_display_name, v_role_id);
END;
' LANGUAGE plpgsql;



-- Update a user's email and display name.
CREATE OR REPLACE FUNCTION proc_update_user_profile(
    p_google_sub varchar(30),
    p_new_email varchar(255),
    p_new_display_name varchar(100)
)
RETURNS void AS '
BEGIN
    UPDATE users
    SET email = p_new_email,
        display_name = p_new_display_name
    WHERE google_sub = p_google_sub;
END;
' LANGUAGE plpgsql;


-- Change the role of a user.
CREATE OR REPLACE FUNCTION proc_change_user_role(
    p_google_sub varchar(30),
    p_new_role_name varchar(255)
)
RETURNS void AS '
DECLARE
    v_role_id int;
BEGIN
    -- Look up the new role_id using the role name.
    SELECT role_id INTO v_role_id FROM roles WHERE role_name = p_new_role_name;
    IF v_role_id IS NULL THEN
        RAISE EXCEPTION ''Role "%" not found'', p_new_role_name;
    END IF;
    
    UPDATE users
    SET role_id = v_role_id
    WHERE google_sub = p_google_sub;
END;
' LANGUAGE plpgsql;



-- Insert a new role into the roles table.
-- Returns: the new role_id
CREATE OR REPLACE FUNCTION proc_add_role(
    p_role_name varchar(255)
)
RETURNS int AS '
DECLARE
    v_role_id int;
BEGIN
    INSERT INTO roles (role_name)
    VALUES (p_role_name)
    RETURNING role_id INTO v_role_id;
    RETURN v_role_id;
END;
' LANGUAGE plpgsql;


-- Insert a new body type.
-- Returns: the new body_type_id
CREATE OR REPLACE FUNCTION proc_add_body_type(
    p_type varchar(100)
)
RETURNS int AS '
DECLARE
    v_body_type_id int;
BEGIN
    INSERT INTO body_types (type)
    VALUES (p_type)
    RETURNING body_type_id INTO v_body_type_id;
    RETURN v_body_type_id;
END;
' LANGUAGE plpgsql;


-- Update the body type description.
CREATE OR REPLACE FUNCTION proc_update_body_type(
    p_body_type_id int,
    p_new_type varchar(100)
)
RETURNS void AS '
BEGIN
    UPDATE body_types
    SET type = p_new_type
    WHERE body_type_id = p_body_type_id;
END;
' LANGUAGE plpgsql;


-- Insert a new celestial body.
CREATE OR REPLACE FUNCTION proc_add_celestial_body(
    p_celestial_body_id uuid,
    p_name varchar(255),
    p_body_type varchar(100),
    p_orbits uuid DEFAULT NULL
)
RETURNS void AS '
DECLARE
    v_body_type_id int;
BEGIN
    -- Look up the body_type_id using the human‐readable type.
    SELECT body_type_id INTO v_body_type_id FROM body_types WHERE type = p_body_type;
    IF v_body_type_id IS NULL THEN
        RAISE EXCEPTION ''Body type "%" not found'', p_body_type;
    END IF;
    
    INSERT INTO celestial_bodies (celestial_body_id, name, orbits, body_type_id)
    VALUES (p_celestial_body_id, p_name, p_orbits, v_body_type_id);
END;
' LANGUAGE plpgsql;


-- Update details of an existing celestial body.
CREATE OR REPLACE FUNCTION proc_update_celestial_body(
    p_celestial_body_id uuid,
    p_new_name varchar(255),
    p_new_body_type varchar(100),
    p_new_orbits uuid DEFAULT NULL
)
RETURNS void AS '
DECLARE
    v_body_type_id int;
BEGIN
    -- Look up the new body type ID.
    SELECT body_type_id INTO v_body_type_id FROM body_types WHERE type = p_new_body_type;
    IF v_body_type_id IS NULL THEN
        RAISE EXCEPTION ''Body type "%" not found'', p_new_body_type;
    END IF;
    
    UPDATE celestial_bodies
    SET name = p_new_name,
        orbits = p_new_orbits,
        body_type_id = v_body_type_id
    WHERE celestial_body_id = p_celestial_body_id;
END;
' LANGUAGE plpgsql;


-- Set the active revision for a celestial body.
CREATE OR REPLACE FUNCTION proc_set_active_revision(
    p_celestial_body_id uuid,
    p_revision_id int
)
RETURNS void AS '
BEGIN
    UPDATE celestial_bodies
    SET active_revision = p_revision_id
    WHERE celestial_body_id = p_celestial_body_id;
END;
' LANGUAGE plpgsql;


-- Insert a new content revision and set it as active
-- Returns: the new revision_id
CREATE OR REPLACE FUNCTION proc_write_content_revision(
    p_content varchar(65536),
    p_created_at timestamp,
    p_celestial_body uuid,
    p_author varchar(30)
)
RETURNS int AS '
DECLARE
    v_revision_id int;
BEGIN
    -- Insert a new revision into content_revisions.
    INSERT INTO content_revisions (content, created_at, celestial_body, author)
    VALUES (p_content, p_created_at, p_celestial_body, p_author)
    RETURNING revision_id INTO v_revision_id;
    
    -- Update the celestial body record to set active_revision to the new revision.
    UPDATE celestial_bodies
    SET active_revision = v_revision_id
    WHERE celestial_body_id = p_celestial_body;
    
    RETURN v_revision_id;
END;
' LANGUAGE plpgsql;



-- Retrieve all revisions for a given celestial body.
-- Returns: TABLE of revision_id, content, created_at, author
CREATE OR REPLACE FUNCTION proc_get_wiki_page_history(
    p_celestial_body uuid
)
RETURNS TABLE(
    revision_id int,
    content varchar(65536),
    created_at timestamp,
    author varchar(30)
) AS '
BEGIN
    RETURN QUERY
    SELECT revision_id, content, created_at, author
    FROM content_revisions
    WHERE celestial_body = p_celestial_body
    ORDER BY created_at DESC;
END;
' LANGUAGE plpgsql;


-- Insert a new comment into the comments table.
CREATE OR REPLACE FUNCTION proc_add_comment(
    p_comment_id uuid,
    p_celestial_body_id uuid,
    p_user_id varchar(30),
    p_comment varchar(255),
    p_created_at timestamp
)
RETURNS void AS '
BEGIN
    INSERT INTO comments (comment_id, celestial_body_id, user_id, comment, created_at)
    VALUES (p_comment_id, p_celestial_body_id, p_user_id, p_comment, p_created_at);
END;
' LANGUAGE plpgsql;


-- Update the content of an existing comment.
CREATE OR REPLACE FUNCTION proc_update_comment(
    p_comment_id uuid,
    p_new_comment varchar(255)
)
RETURNS void AS '
BEGIN
    UPDATE comments
    SET comment = p_new_comment
    WHERE comment_id = p_comment_id;
END;
' LANGUAGE plpgsql;


-- Delete a comment from the comments table.
CREATE OR REPLACE FUNCTION proc_delete_comment(
    p_comment_id uuid
)
RETURNS void AS '
BEGIN
    DELETE FROM comments
    WHERE comment_id = p_comment_id;
END;
' LANGUAGE plpgsql;


-- Retrieve all comments for a given celestial body.
-- Returns: TABLE of comment_id, user_id, comment, created_at
CREATE OR REPLACE FUNCTION proc_get_comments_for_celestial_body(
    p_celestial_body_id uuid
)
RETURNS TABLE(
    comment_id uuid,
    user_id varchar(30),
    comment varchar(255),
    created_at timestamp
) AS '
BEGIN
    RETURN QUERY
    SELECT comment_id, user_id, comment, created_at
    FROM comments
    WHERE celestial_body_id = p_celestial_body_id
    ORDER BY created_at ASC;
END;
' LANGUAGE plpgsql;


-- Insert a new star system.
CREATE OR REPLACE FUNCTION proc_add_star_system(
    p_system_id uuid,
    p_name varchar(255),
    p_center_cb uuid
)
RETURNS void AS '
BEGIN
    INSERT INTO star_systems (system_id, name, center_cb)
    VALUES (p_system_id, p_name, p_center_cb);
END;
' LANGUAGE plpgsql;


-- Update details of an existing star system.
CREATE OR REPLACE FUNCTION proc_update_star_system(
    p_system_id uuid,
    p_new_name varchar(255),
    p_new_center_cb uuid
)
RETURNS void AS '
BEGIN
    UPDATE star_systems
    SET name = p_new_name,
        center_cb = p_new_center_cb
    WHERE system_id = p_system_id;
END;
' LANGUAGE plpgsql;


-- Search wiki pages for a keyword in their content.
-- Returns: TABLE of celestial_body, revision_id, content, created_at, author
CREATE OR REPLACE FUNCTION proc_search_wiki_pages(
    p_keyword text
)
RETURNS TABLE(
    celestial_body uuid,
    revision_id int,
    content varchar(65536),
    created_at timestamp,
    author varchar(30)
) AS '
BEGIN
    RETURN QUERY
    SELECT cr.celestial_body, cr.revision_id, cr.content, cr.created_at, cr.author
    FROM content_revisions cr
    WHERE cr.content ILIKE ''%'' || p_keyword || ''%''
    ORDER BY cr.created_at DESC;
END;
' LANGUAGE plpgsql;


-- Search celestial bodies by name or by body type.
-- Returns: TABLE of celestial_body_id, name, body_type_id
CREATE OR REPLACE FUNCTION proc_search_celestial_bodies(
    p_search_term text
)
RETURNS TABLE(
    celestial_body_id uuid,
    name varchar(255),
    body_type_id int
) AS '
BEGIN
    RETURN QUERY
    SELECT cb.celestial_body_id,
           cb.name,
           cb.body_type_id
    FROM celestial_bodies AS cb
    WHERE cb.name ILIKE ''%'' || p_search_term || ''%'';
END;
' LANGUAGE plpgsql;