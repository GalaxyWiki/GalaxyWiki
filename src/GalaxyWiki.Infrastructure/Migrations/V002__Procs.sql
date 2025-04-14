CREATE OR REPLACE PROCEDURE insert_celestial_body(
    p_body_name VARCHAR(255),
    p_orbit_body_name VARCHAR(255),
    p_type_name VARCHAR(100)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_orbit_body_id INT;
    v_body_type_id INT;
BEGIN
    -- Get the orbit body ID
    SELECT celestial_body_id INTO v_orbit_body_id 
    FROM celestial_bodies 
    WHERE body_name = p_orbit_body_name;

    IF v_orbit_body_id IS NULL THEN
        RAISE EXCEPTION 'Celestial body with name ''%'' does not exist',
            p_orbit_body_name;
    END IF;

    -- Get the body type ID
    SELECT body_type_id INTO v_body_type_id 
    FROM body_types 
    WHERE type_name = p_type_name;

    IF v_body_type_id IS NULL THEN
        RAISE EXCEPTION 'Body type with name ''%'' does not exist', p_type_name;
    END IF;

    INSERT INTO celestial_bodies (body_name, orbits, body_type_id)
        VALUES (p_body_name, v_orbit_body_id, v_body_type_id);

    RAISE NOTICE 'Celestial body % inserted successfully', p_body_name;

    EXCEPTION
        WHEN others THEN
            RAISE EXCEPTION 'Error inserting into "celestial_bodies": %',
                SQLERRM;
END;
$$;

CREATE OR REPLACE PROCEDURE insert_content_revision(
    p_content VARCHAR(65536),
    p_celestial_body_id INT,
    p_author VARCHAR(30)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_revision_id INT;
    v_celestial_body_id INT;
BEGIN
    -- Validate that the celestial body exists

    SELECT celestial_body_id INTO v_celestial_body_id   
    FROM celestial_bodies
    WHERE celestial_body_id = p_celestial_body_id;

    IF v_celestial_body_id IS NULL THEN
        RAISE EXCEPTION 'Celestial body with ID ''%'' does not exist',
            p_celestial_body_id;
    END IF;

    -- Validate that the author exists
    IF NOT EXISTS (SELECT 1 FROM users WHERE google_sub = p_author) THEN
        RAISE EXCEPTION 'User with ID % does not exist', p_author;
    END IF;

    -- Insert the content revision
    INSERT INTO content_revisions (content, celestial_body, author)
    VALUES (p_content, v_celestial_body_id, p_author)
    RETURNING revision_id INTO v_revision_id;

    -- Set active revision of the celestial body
    UPDATE celestial_bodies SET active_revision = v_revision_id
    WHERE celestial_body_id = v_celestial_body_id;

    RAISE NOTICE 'Content revision inserted successfully for celestial body %',
        p_celestial_body_id;

    EXCEPTION
        WHEN others THEN
            -- Not success! :(
            RAISE EXCEPTION 'Error inserting into "content_revisions": %',
                SQLERRM;

END;
$$;

CREATE OR REPLACE PROCEDURE insert_star_system(
    p_system_name VARCHAR(255),
    p_center_cb_name VARCHAR(255)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_center_cb_id INT;
BEGIN
    -- Get the center celestial body ID
    SELECT celestial_body_id INTO v_center_cb_id 
    FROM celestial_bodies 
    WHERE body_name = p_center_cb_name;

    IF v_center_cb_id IS NULL THEN
        RAISE EXCEPTION 'Celestial body with name ''%'' does not exist',
            p_center_cb_name;
    END IF;

    INSERT INTO star_systems (system_name, center_cb)
        VALUES (p_system_name, v_center_cb_id);

    RAISE NOTICE 'Star system % inserted successfully', p_system_name;

    EXCEPTION
        WHEN others THEN
            RAISE EXCEPTION 'Error inserting into "star_systems": %',
                SQLERRM;
END;
$$;

CREATE OR REPLACE PROCEDURE insert_comment(
    p_celestial_body_name VARCHAR(255),
    p_user_id VARCHAR(30),
    p_comment VARCHAR(255)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_celestial_body_id INT;
BEGIN
    -- Get the celestial body ID
    SELECT celestial_body_id INTO v_celestial_body_id
    FROM celestial_bodies
    WHERE body_name = p_celestial_body_name;

    IF v_celestial_body_id IS NULL THEN
        RAISE EXCEPTION 'Celestial body with name ''%'' does not exist',
            p_celestial_body_name;
    END IF;

    INSERT INTO comments (celestial_body_id, user_id, comment)
        VALUES (v_celestial_body_id, p_user_id, p_comment);

    RAISE NOTICE 'Comment inserted successfully for celestial body %',
        p_celestial_body_name;

    EXCEPTION
        WHEN others THEN
            RAISE EXCEPTION 'Error inserting into "comments": %',
                SQLERRM;
END;
$$;
