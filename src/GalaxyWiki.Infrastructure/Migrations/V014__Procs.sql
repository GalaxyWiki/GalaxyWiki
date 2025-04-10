CREATE OR REPLACE PROCEDURE insert_content_revision(
    p_content VARCHAR(65536),
    p_celestial_body INT,
    p_author VARCHAR(30)
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Validate that the celestial body exists
    IF NOT EXISTS (SELECT 1 FROM celestial_bodies WHERE celestial_body_id = p_celestial_body) THEN
        RAISE EXCEPTION 'Celestial body with ID % does not exist', p_celestial_body;
    END IF;

    -- Validate that the author exists
    IF NOT EXISTS (SELECT 1 FROM users WHERE google_sub = p_author) THEN
        RAISE EXCEPTION 'User with ID % does not exist', p_author;
    END IF;

    -- Insert the content revision
    INSERT INTO content_revisions (content, celestial_body, author)
    VALUES (p_content, p_celestial_body, p_author);


    RAISE NOTICE 'Content revision inserted successfully for celestial body %',
        p_celestial_body;

END;
$$;
