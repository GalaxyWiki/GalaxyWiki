SELECT * FROM celestial_bodies;
SELECT * FROM comments;

-- Number of comments per cb
SELECT COUNT(*), cb.celestial_body_id FROM celestial_bodies cb
INNER JOIN comments c
ON cb.celestial_body_id = c.celestial_body_id
GROUP BY cb.celestial_body_id
ORDER BY COUNT(*) DESC;

SELECT * FROM content_revisions WHERE celestial_body = 'c3a64bf3-b42f-4c4f-9388-132bdf5e097b';

-- Number of revisions per cb
SELECT COUNT(*), cb.celestial_body_id FROM celestial_bodies cb
INNER JOIN content_revisions cr
ON cb.celestial_body_id = cr.celestial_body
GROUP BY cb.celestial_body_id
ORDER BY COUNT(*) DESC;

-- Users with roles
SELECT google_sub, email, display_name, role_name FROM users u
INNER JOIN roles r
ON u.role_id = r.role_id;

-- Searches
SELECT * FROM proc_search_celestial_bodies('th');
SELECT * FROM proc_search_wiki_pages('sun');