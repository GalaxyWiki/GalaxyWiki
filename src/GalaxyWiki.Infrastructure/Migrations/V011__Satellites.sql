-- Majot satellites orbiting Earth
INSERT INTO celestial_bodies (body_name, orbits, body_type_id) VALUES
('International Space Station', 160, 5),
('Hubble Space Telescope', 160, 5),
('James Webb Space Telescope', 64, 5), -- JWST orbits the Sun, not Earth
('Voyager 1', 64, 5), -- Now in interstellar space
('Voyager 2', 64, 5), -- Also in interstellar space
('New Horizons', 64, 5), -- Pluto mission, now in Kuiper Belt
('GPS Constellation', 160, 5),
('Galileo Constellation', 160, 5),
('GLONASS Constellation', 160, 5),
('Landsat 9', 160, 5),
('Sentinel-2', 160, 5),
('GOES-16', 160, 5),
('Tiangong Space Station', 160, 5);