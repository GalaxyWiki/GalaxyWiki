-- Planets orbiting the Sun
CALL insert_celestial_body('Mercury', 'Sun', 'Planet');
CALL insert_celestial_body('Venus', 'Sun', 'Planet');
CALL insert_celestial_body('Earth', 'Sun', 'Planet');
CALL insert_celestial_body('Mars', 'Sun', 'Planet');
CALL insert_celestial_body('Jupiter', 'Sun', 'Planet');
CALL insert_celestial_body('Saturn', 'Sun', 'Planet');
CALL insert_celestial_body('Uranus', 'Sun', 'Planet');
CALL insert_celestial_body('Neptune', 'Sun', 'Planet');

-- Dwarf planets orbiting the Sun
CALL insert_celestial_body('Pluto', 'Sun', 'Dwarf Planet');
CALL insert_celestial_body('Ceres', 'Sun', 'Dwarf Planet');
CALL insert_celestial_body('Eris', 'Sun', 'Dwarf Planet');
CALL insert_celestial_body('Haumea', 'Sun', 'Dwarf Planet');
CALL insert_celestial_body('Makemake', 'Sun', 'Dwarf Planet');