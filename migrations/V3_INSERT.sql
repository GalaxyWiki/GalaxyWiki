CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Begin a transaction (optional)
BEGIN;

----------------------------
-- 1. Insert Roles and Users
----------------------------

-- Insert the two roles.
SELECT proc_add_role('admin');
SELECT proc_add_role('contributor');

-- Insert exactly five users (from Hitchhiker’s Guide to the Galaxy).
SELECT proc_add_user('zaphod', 'zaphod@galaxy.com', 'Zaphod Beeblebrox', 'admin');
SELECT proc_add_user('arthur', 'arthur@galaxy.com', 'Arthur Dent', 'contributor');
SELECT proc_add_user('ford', 'ford@galaxy.com', 'Ford Prefect', 'contributor');
SELECT proc_add_user('trillian', 'trillian@galaxy.com', 'Trillian', 'contributor');
SELECT proc_add_user('marvin', 'marvin@galaxy.com', 'Marvin', 'contributor');

----------------------------
-- 2. Insert Roles and Users
----------------------------

-- Merge insertion of the many body types needed.
INSERT INTO body_types (type)
VALUES 
  ('universe'), ('black hole'), ('star'), ('planet'), ('moon'),
  ('man-made'), ('comet'), ('asteroid'), ('galaxy'), ('galaxy pair'),
  ('nebula'), ('dwarf planet'), ('dust lane'), ('tidal bridge'), ('nuclear star cluster'),
  ('ring'), ('spokes'), ('tidal tail'), ('core'), ('spiral structure'),
  ('galaxy merger'), ('star cluster'), ('disk')
ON CONFLICT DO NOTHING;

----------------------------
-- 3. Insert Celestial Bodies, Content Revisions, Comments, and Star Systems
----------------------------

DO $$
DECLARE
    -- Declare UUID variables for celestial bodies.
    v_universe uuid := uuid_generate_v4();
    v_sagA uuid := uuid_generate_v4();
    v_sun uuid := uuid_generate_v4();
      v_mercury uuid := uuid_generate_v4();
      v_venus uuid := uuid_generate_v4();
      v_earth uuid := uuid_generate_v4();
        v_moon uuid := uuid_generate_v4();
        v_iss uuid := uuid_generate_v4();
      v_mars uuid := uuid_generate_v4();
        v_phobos uuid := uuid_generate_v4();
        v_deimos uuid := uuid_generate_v4();
      v_jupiter uuid := uuid_generate_v4();
        v_io uuid := uuid_generate_v4();
        v_europa uuid := uuid_generate_v4();
        v_ganymede uuid := uuid_generate_v4();
        v_callisto uuid := uuid_generate_v4();
        v_amalthea uuid := uuid_generate_v4();
        v_thebe uuid := uuid_generate_v4();
        v_himalia uuid := uuid_generate_v4();
        v_elara uuid := uuid_generate_v4();
        v_lysithea uuid := uuid_generate_v4();
      v_saturn uuid := uuid_generate_v4();
        v_titan uuid := uuid_generate_v4();
        v_enceladus uuid := uuid_generate_v4();
        v_iapetus uuid := uuid_generate_v4();
        v_rhea uuid := uuid_generate_v4();
        v_dione uuid := uuid_generate_v4();
        v_tethys uuid := uuid_generate_v4();
        v_mimas uuid := uuid_generate_v4();
        v_hyperion uuid := uuid_generate_v4();
        v_phoebe uuid := uuid_generate_v4();
        v_janus uuid := uuid_generate_v4();
        v_epimetheus uuid := uuid_generate_v4();
      v_uranus uuid := uuid_generate_v4();
        v_miranda uuid := uuid_generate_v4();
        v_ariel uuid := uuid_generate_v4();
        v_umbriel uuid := uuid_generate_v4();
        v_titania uuid := uuid_generate_v4();
        v_oberon uuid := uuid_generate_v4();
      v_neptune uuid := uuid_generate_v4();
        v_triton uuid := uuid_generate_v4();
        v_nereid uuid := uuid_generate_v4();
        v_proteus uuid := uuid_generate_v4();
        v_larissa uuid := uuid_generate_v4();
      v_pluto uuid := uuid_generate_v4();
        v_charon uuid := uuid_generate_v4();
        v_hydra uuid := uuid_generate_v4();
        v_nix uuid := uuid_generate_v4();
        v_kerberos uuid := uuid_generate_v4();
        v_styx uuid := uuid_generate_v4();
      v_halley uuid := uuid_generate_v4();
      v_encke uuid := uuid_generate_v4();
      v_halebopp uuid := uuid_generate_v4();
      v_hyakutake uuid := uuid_generate_v4();
      v_ceres uuid := uuid_generate_v4();
      v_vesta uuid := uuid_generate_v4();
      v_pallas uuid := uuid_generate_v4();
      v_hygiea uuid := uuid_generate_v4();
    -- TRAPPIST-1 system.
    v_trappist uuid := uuid_generate_v4();
      v_trappist1b uuid := uuid_generate_v4();
      v_trappist1c uuid := uuid_generate_v4();
      v_trappist1d uuid := uuid_generate_v4();
      v_trappist1e uuid := uuid_generate_v4();
      v_trappist1f uuid := uuid_generate_v4();
      v_trappist1g uuid := uuid_generate_v4();
      v_trappist1h uuid := uuid_generate_v4();
    -- Alpha Centauri system.
    v_alpha uuid := uuid_generate_v4();
      v_proximab uuid := uuid_generate_v4();
    -- Kepler-90 system.
    v_kepler90 uuid := uuid_generate_v4();
      v_kepler90b uuid := uuid_generate_v4();
      v_kepler90c uuid := uuid_generate_v4();
      v_kepler90d uuid := uuid_generate_v4();
      v_kepler90e uuid := uuid_generate_v4();
      v_kepler90f uuid := uuid_generate_v4();
      v_kepler90g uuid := uuid_generate_v4();
      v_kepler90h uuid := uuid_generate_v4();
    -- Galaxies and related structures.
    v_m31 uuid := uuid_generate_v4();
      v_m32 uuid := uuid_generate_v4();
      v_m110 uuid := uuid_generate_v4();
      v_ngc205 uuid := uuid_generate_v4();
      v_ngc147 uuid := uuid_generate_v4();
      v_ngc185 uuid := uuid_generate_v4();
    v_m33 uuid := uuid_generate_v4();
      v_ngc604 uuid := uuid_generate_v4();
      v_ngc595 uuid := uuid_generate_v4();
      v_ngc588 uuid := uuid_generate_v4();
    v_alcyoneus uuid := uuid_generate_v4();
    v_backward uuid := uuid_generate_v4();
    v_barnard uuid := uuid_generate_v4();
    v_ngc4826 uuid := uuid_generate_v4();
      v_dust_lane uuid := uuid_generate_v4();
    v_butterfly uuid := uuid_generate_v4();  -- represents the pair “NGC 4567 & NGC 4568”
      v_tidal_bridge uuid := uuid_generate_v4();
    v_bodes uuid := uuid_generate_v4();
      v_nuclear_star_cluster uuid := uuid_generate_v4();
    v_cartwheel uuid := uuid_generate_v4();
      v_outer_ring uuid := uuid_generate_v4();
      v_inner_ring uuid := uuid_generate_v4();
      v_spokes uuid := uuid_generate_v4();
    v_condor uuid := uuid_generate_v4();
      v_tidal_tail uuid := uuid_generate_v4();
    v_hoag uuid := uuid_generate_v4();
      v_hoag_core uuid := uuid_generate_v4();
      v_hoag_ring uuid := uuid_generate_v4();
    v_lindsay uuid := uuid_generate_v4();
      v_ring_structure uuid := uuid_generate_v4();
    v_meathook uuid := uuid_generate_v4();
      v_distorted_spiral uuid := uuid_generate_v4();
    v_medusa uuid := uuid_generate_v4();
    v_mayall uuid := uuid_generate_v4();
      v_collisional_ring uuid := uuid_generate_v4();
    v_porphyrion uuid := uuid_generate_v4();
    v_topsy uuid := uuid_generate_v4();
      v_warped_disk uuid := uuid_generate_v4();
    v_ngc5194 uuid := uuid_generate_v4();
      v_ngc5195 uuid := uuid_generate_v4();
    v_lmc uuid := uuid_generate_v4();
      v_30dor uuid := uuid_generate_v4();
      v_ngc1841 uuid := uuid_generate_v4();
    v_smcc uuid := uuid_generate_v4();
      v_ngc346 uuid := uuid_generate_v4();
      v_ngc602 uuid := uuid_generate_v4();
    v_ngc5128 uuid := uuid_generate_v4();

    -- Temporary variable for an extra (old) revision.
    v_revision_old int;
BEGIN
    ----------------------------
    -- Insert Celestial Bodies (hierarchically)
    ----------------------------
    -- Universe (no parent)
    PERFORM proc_add_celestial_body(v_universe, 'Universe', 'universe', NULL);
    -- Write an early revision (old version) then update with current revision.
    v_revision_old := proc_write_content_revision('The Universe contains all of space and time in an incomprehensibly vast and enigmatic way. Early drafts noted infinite possibility.', '2025-04-08 10:00:00', v_universe, 'zaphod');
    PERFORM proc_write_content_revision('The Universe is the totality of existence—a dazzling expanse full of mysterious wonders and chaotic beauty. It continues to inspire curious minds across all galaxies.', '2025-04-08 12:00:00', v_universe, 'zaphod');
    PERFORM proc_add_comment(uuid_generate_v4(), v_universe, 'arthur', 'The vastness of the Universe always reminds me that we are small – though sometimes my towel feels extra important!', '2025-04-08 13:00:00');

    -- Sagittarius A* – black hole, orbits: none (child of Universe)
    PERFORM proc_add_celestial_body(v_sagA, 'Sagittarius A*', 'black hole', v_universe);
    PERFORM proc_write_content_revision('Sagittarius A* is a supermassive black hole residing in the centre of our Milky Way. Its gravitational pull governs the motion of surrounding stars in a mysterious dance.', '2025-04-08 12:05:00', v_sagA, 'ford');
    PERFORM proc_add_comment(uuid_generate_v4(), v_sagA, 'trillian', 'Black holes never fail to evoke wonder – though I wish there were less existential peril!', '2025-04-08 13:10:00');
    -- Create star system for Sagittarius A* (Milky Way Galaxy)
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Milky Way Galaxy', v_sagA);

    -- Sun – star; orbits: Sagittarius A*
    PERFORM proc_add_celestial_body(v_sun, 'Sun', 'star', v_sagA);
    -- Add an extra revision history for Sun.
    v_revision_old := proc_write_content_revision('The Sun is our life‐giving star, originally described with rudimentary details. Its power was underestimated in early chronicles.', '2025-04-08 10:05:00', v_sun, 'marvin');
    PERFORM proc_write_content_revision('The Sun shines as a brilliant star, essential for sustaining life in its vicinity. It has been observed for millennia with awe and curiosity.', '2025-04-08 12:10:00', v_sun, 'marvin');
    PERFORM proc_add_comment(uuid_generate_v4(), v_sun, 'arthur', 'I almost had a cup of tea in its light – though I still prefer a good cup brewed in a cozy kettle.', '2025-04-08 13:15:00');
    -- Create star system for Sun ([The Solar System])
    PERFORM proc_add_star_system(uuid_generate_v4(), 'The Solar System', v_sun);

      -- Mercury (planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_mercury, 'Mercury', 'planet', v_sun);
      PERFORM proc_write_content_revision('Mercury is the innermost planet, scorched by its proximity to the Sun. Its surface, pockmarked by craters, tells a tale of meteorite bombardments.', '2025-04-08 12:12:00', v_mercury, 'ford');
      -- Venus (planet)
      PERFORM proc_add_celestial_body(v_venus, 'Venus', 'planet', v_sun);
      PERFORM proc_write_content_revision('Venus is cloaked in a thick atmosphere of carbon dioxide, making it the hottest planet. Its cloudy veil hides a surface of volcanic plains and rugged highlands.', '2025-04-08 12:14:00', v_venus, 'trillian');

      -- Earth (planet)
      PERFORM proc_add_celestial_body(v_earth, 'Earth', 'planet', v_sun);
      -- Extra revision for Earth.
      v_revision_old := proc_write_content_revision('Early observations of Earth remarked on its unique blue oceans and verdant lands. Initial records were sparse yet optimistic.', '2025-04-08 10:15:00', v_earth, 'arthur');
      PERFORM proc_write_content_revision('Earth is a vibrant planet teeming with life, water, and diversity. It remains a constant source of fascination and occasional existential dread.', '2025-04-08 12:16:00', v_earth, 'arthur');
      PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'marvin', 'I find it odd how life clings here despite all the absurdity of the cosmos.', '2025-04-08 13:20:00');

          -- Moon (moon; orbits Earth)
          PERFORM proc_add_celestial_body(v_moon, 'Moon', 'moon', v_earth);
          PERFORM proc_write_content_revision('Earth’s Moon is a silent companion that has inspired poets and astronomers alike. Its cratered face is a record of cosmic impacts.', '2025-04-08 12:18:00', v_moon, 'ford');
          PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'trillian', 'One small step for moon, one giant leap for lunar enthusiasts!', '2025-04-08 13:25:00');

          -- ISS (man-made; orbits Earth)
          PERFORM proc_add_celestial_body(v_iss, 'ISS', 'man-made', v_earth);
          PERFORM proc_write_content_revision('The International Space Station is a marvel of modern engineering orbiting our planet. It serves as a laboratory and a beacon of international cooperation.', '2025-04-08 12:20:00', v_iss, 'ford');
          PERFORM proc_add_comment(uuid_generate_v4(), v_iss, 'zaphod', 'I must say, floating about in space on a station is almost as cool as piloting a stolen spaceship!', '2025-04-08 13:30:00');

      -- Mars (planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_mars, 'Mars', 'planet', v_sun);
      PERFORM proc_write_content_revision('Mars has long been a subject of fascination due to its red hue and the possibility of life. Its rugged terrain and enigmatic past continue to captivate scientists.', '2025-04-08 12:22:00', v_mars, 'trillian');
      PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'arthur', 'Mars is intriguing—but I’d rather not pack a bag for an interplanetary hitchhike.', '2025-04-08 13:35:00');

          -- Phobos (moon; orbits Mars)
          PERFORM proc_add_celestial_body(v_phobos, 'Phobos', 'moon', v_mars);
          PERFORM proc_write_content_revision('Phobos is one of Mars’ two small moons, speeding along its orbit. Its irregular shape and proximity to the planet add to its mystery.', '2025-04-08 12:24:00', v_phobos, 'marvin');
          -- Deimos (moon)
          PERFORM proc_add_celestial_body(v_deimos, 'Deimos', 'moon', v_mars);
          PERFORM proc_write_content_revision('Deimos is the smaller and more distant moon of Mars. Its quiet, almost forlorn orbit contrasts with its companion Phobos.', '2025-04-08 12:25:00', v_deimos, 'ford');

      -- Jupiter (planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_jupiter, 'Jupiter', 'planet', v_sun);
      PERFORM proc_write_content_revision('Jupiter is the giant of our Solar System, with a swirling atmosphere and many moons. Its immense mass and dynamic weather systems have been studied for decades.', '2025-04-08 12:28:00', v_jupiter, 'trillian');
      PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'ford', 'Jupiter is massive – almost as big as my improbability drive is unpredictable!', '2025-04-08 13:40:00');

          -- Io, Europa, Ganymede, Callisto, Amalthea, Thebe, Himalia, Elara, Lysithea (moons of Jupiter)
          PERFORM proc_add_celestial_body(v_io, 'Io', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Io is volcanically hyperactive and features extreme surface activity. Its vivid eruptions set it apart among Jupiter’s moons.', '2025-04-08 12:30:00', v_io, 'marvin');

          PERFORM proc_add_celestial_body(v_europa, 'Europa', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Europa is famed for its icy crust, beneath which may lie a subsurface ocean. This mysterious world holds clues to extraterrestrial life.', '2025-04-08 12:32:00', v_europa, 'arthur');

          PERFORM proc_add_celestial_body(v_ganymede, 'Ganymede', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Ganymede is the largest moon in the Solar System and even bigger than Mercury. Its surface exhibits a mix of old, cratered regions and younger, grooved terrains.', '2025-04-08 12:34:00', v_ganymede, 'ford');

          PERFORM proc_add_celestial_body(v_callisto, 'Callisto', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Callisto is heavily cratered and ancient, preserving a record of impacts. Its rugged landscape sparks the imagination regarding long-forgotten cosmic events.', '2025-04-08 12:36:00', v_callisto, 'trillian');

          PERFORM proc_add_celestial_body(v_amalthea, 'Amalthea', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Amalthea is a small, irregular moon that orbits close to Jupiter. Its unique shape and low brightness make it a lesser‐known world.', '2025-04-08 12:38:00', v_amalthea, 'marvin');

          PERFORM proc_add_celestial_body(v_thebe, 'Thebe', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Thebe is another small moon of Jupiter marked by an odd, irregular surface. Early observations noted its peculiar orbit.', '2025-04-08 12:40:00', v_thebe, 'ford');

          PERFORM proc_add_celestial_body(v_himalia, 'Himalia', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Himalia is the largest irregular satellite of Jupiter. Its distant orbit and dark, pockmarked surface provide insight into captured objects.', '2025-04-08 12:42:00', v_himalia, 'arthur');

          PERFORM proc_add_celestial_body(v_elara, 'Elara', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Elara is a small moon orbiting Jupiter with a roughly spherical shape. It remains one of the lesser-studied satellites in the Jovian system.', '2025-04-08 12:44:00', v_elara, 'trillian');

          PERFORM proc_add_celestial_body(v_lysithea, 'Lysithea', 'moon', v_jupiter);
          PERFORM proc_write_content_revision('Lysithea is a diminutive moon which, though not well known, contributes to the diverse satellite family of Jupiter. Its irregular orbit speaks of a turbulent past.', '2025-04-08 12:46:00', v_lysithea, 'marvin');

      -- Saturn (planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_saturn, 'Saturn', 'planet', v_sun);
      PERFORM proc_write_content_revision('Saturn is renowned for its spectacular ring system and elegant appearance. Its many moons and dynamic atmosphere have fascinated observers for centuries.', '2025-04-08 12:48:00', v_saturn, 'ford');
      PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'arthur', 'Saturn’s rings are almost as remarkable as a perfectly packed towel.', '2025-04-08 13:45:00');

          -- Titan, Enceladus, Iapetus, Rhea, Dione, Tethys, Mimas, Hyperion, Phoebe, Janus, Epimetheus (moons of Saturn)
          PERFORM proc_add_celestial_body(v_titan, 'Titan', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Titan is the largest of Saturn''s moons and hosts a thick atmosphere. Its methane lakes evoke both mystery and wonder.', '2025-04-08 12:50:00', v_titan, 'trillian');

          PERFORM proc_add_celestial_body(v_enceladus, 'Enceladus', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Enceladus bursts water vapor and ice particles from its south polar region, hinting at a subsurface ocean. Its geysers have made it a prime candidate in the search for life.', '2025-04-08 12:52:00', v_enceladus, 'marvin');

          PERFORM proc_add_celestial_body(v_iapetus, 'Iapetus', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Iapetus is distinctive for its dramatic two-tone coloration. The stark contrast on its surface has puzzled astronomers for years.', '2025-04-08 12:54:00', v_iapetus, 'ford');

          PERFORM proc_add_celestial_body(v_rhea, 'Rhea', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Rhea is one of Saturn''s larger moons with a heavily cratered surface. It has been studied for clues about the early Solar System.', '2025-04-08 12:56:00', v_rhea, 'arthur');

          PERFORM proc_add_celestial_body(v_dione, 'Dione', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Dione exhibits a bright, icy surface intersected by dark, linear features. Its dual nature has raised questions about its internal structure.', '2025-04-08 12:58:00', v_dione, 'trillian');

          PERFORM proc_add_celestial_body(v_tethys, 'Tethys', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Tethys is noted for its enormous and bright impact basin called Odysseus. Its ancient surface provides a window into Saturn’s past.', '2025-04-08 13:00:00', v_tethys, 'marvin');

          PERFORM proc_add_celestial_body(v_mimas, 'Mimas', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Mimas is small but famous for its resemblance to the Death Star from a certain sci-fi saga. Its large crater, Herschel, dominates its surface.', '2025-04-08 13:02:00', v_mimas, 'ford');

          PERFORM proc_add_celestial_body(v_hyperion, 'Hyperion', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Hyperion is an irregularly shaped moon with a sponge-like appearance. Its chaotic rotation and pitted surface are truly one-of-a-kind.', '2025-04-08 13:04:00', v_hyperion, 'arthur');

          PERFORM proc_add_celestial_body(v_phoebe, 'Phoebe', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Phoebe is a dark, outer moon of Saturn and may be a captured object from elsewhere in the Solar System. Its retrograde orbit deepens its mystery.', '2025-04-08 13:06:00', v_phoebe, 'trillian');

          PERFORM proc_add_celestial_body(v_janus, 'Janus', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Janus is remarkable for its co-orbital relationship with Epimetheus. Its shifting orbit is a fascinating demonstration of gravitational interaction.', '2025-04-08 13:08:00', v_janus, 'marvin');

          PERFORM proc_add_celestial_body(v_epimetheus, 'Epimetheus', 'moon', v_saturn);
          PERFORM proc_write_content_revision('Epimetheus shares its orbit with Janus in a delicate celestial dance. Their near-collisions are a cosmic ballet that defies expectation.', '2025-04-08 13:10:00', v_epimetheus, 'ford');

      -- Uranus (planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_uranus, 'Uranus', 'planet', v_sun);
      PERFORM proc_write_content_revision('Uranus is known for its unusual sideways rotation and serene blue-green hue. Its extreme axial tilt sets it apart from its planetary siblings.', '2025-04-08 13:12:00', v_uranus, 'arthur');
      PERFORM proc_add_comment(uuid_generate_v4(), v_uranus, 'trillian', 'Sideways spinning – not as odd as some of our interstellar adventures!', '2025-04-08 13:20:00');

          -- Miranda, Ariel, Umbriel, Titania, Oberon (moons of Uranus)
          PERFORM proc_add_celestial_body(v_miranda, 'Miranda', 'moon', v_uranus);
          PERFORM proc_write_content_revision('Miranda is a small moon with a surface that appears scarred by catastrophic geological activity. Its varied terrain is both bizarre and beautiful.', '2025-04-08 13:14:00', v_miranda, 'marvin');

          PERFORM proc_add_celestial_body(v_ariel, 'Ariel', 'moon', v_uranus);
          PERFORM proc_write_content_revision('Ariel displays signs of past tectonic reformation and possibly ancient cryovolcanism. Its smooth regions contrast sharply with heavily cratered areas.', '2025-04-08 13:16:00', v_ariel, 'ford');

          PERFORM proc_add_celestial_body(v_umbriel, 'Umbriel', 'moon', v_uranus);
          PERFORM proc_write_content_revision('Umbriel is one of the darker moons, with a surface pockmarked by impact craters. Its mysterious nature has long intrigued planetary scientists.', '2025-04-08 13:18:00', v_umbriel, 'arthur');

          PERFORM proc_add_celestial_body(v_titania, 'Titania', 'moon', v_uranus);
          PERFORM proc_write_content_revision('Titania is the largest moon of Uranus and boasts striking canyons and fault lines. Its features hint at internal processes that have shaped its face over time.', '2025-04-08 13:20:00', v_titania, 'trillian');

          PERFORM proc_add_celestial_body(v_oberon, 'Oberon', 'moon', v_uranus);
          PERFORM proc_write_content_revision('Oberon has a relatively smooth surface intermingled with scarred regions from ancient impacts. Its muted coloration is as enigmatic as some of the galaxy''s fabled tales.', '2025-04-08 13:22:00', v_oberon, 'marvin');

      -- Neptune (planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_neptune, 'Neptune', 'planet', v_sun);
      PERFORM proc_write_content_revision('Neptune is a deep blue planet known for fierce winds and dynamic storms. Its remote location has made it a challenge to study, yet it teems with surprises.', '2025-04-08 13:24:00', v_neptune, 'ford');
      PERFORM proc_add_comment(uuid_generate_v4(), v_neptune, 'arthur', 'I once almost believed Neptune to be just another watery blue gem—but its storms are rather terrifying!', '2025-04-08 13:30:00');

          -- Triton, Nereid, Proteus, Larissa (moons of Neptune)
          PERFORM proc_add_celestial_body(v_triton, 'Triton', 'moon', v_neptune);
          PERFORM proc_write_content_revision('Triton is one of the few geologically active moons and orbits Neptune in a retrograde fashion. Its geysers and thin atmosphere are a wonder to behold.', '2025-04-08 13:26:00', v_triton, 'trillian');

          PERFORM proc_add_celestial_body(v_nereid, 'Nereid', 'moon', v_neptune);
          PERFORM proc_write_content_revision('Nereid is an eccentric moon with an elongated orbit around Neptune. Its unpredictable path makes it an intriguing subject among distant satellites.', '2025-04-08 13:28:00', v_nereid, 'marvin');

          PERFORM proc_add_celestial_body(v_proteus, 'Proteus', 'moon', v_neptune);
          PERFORM proc_write_content_revision('Proteus is a dark and heavily cratered moon, nearly spherical in shape. Its close proximity to Neptune allows for fascinating gravitational interactions.', '2025-04-08 13:30:00', v_proteus, 'ford');

          PERFORM proc_add_celestial_body(v_larissa, 'Larissa', 'moon', v_neptune);
          PERFORM proc_write_content_revision('Larissa is a smaller moon that orbits Neptune and remains shrouded in mystery. Its shadowy existence challenges detailed observation.', '2025-04-08 13:32:00', v_larissa, 'arthur');

      -- Pluto (dwarf planet; orbits Sun)
      PERFORM proc_add_celestial_body(v_pluto, 'Pluto', 'dwarf planet', v_sun);
      PERFORM proc_write_content_revision('Pluto, though demoted from planetary status, continues to fascinate with its complex geology and thin atmosphere. Its reclassification only adds to its mystique.', '2025-04-08 13:34:00', v_pluto, 'trillian');
      PERFORM proc_add_comment(uuid_generate_v4(), v_pluto, 'marvin', 'Even a depressed dwarf like Pluto has its own sort of charm – much like a depressed robot might, if it weren’t so gloomy.', '2025-04-08 13:38:00');

          -- Charon, Hydra, Nix, Kerberos, Styx (moons of Pluto)
          PERFORM proc_add_celestial_body(v_charon, 'Charon', 'moon', v_pluto);
          PERFORM proc_write_content_revision('Charon is the largest of Pluto''s moons and forms a striking binary system with its dwarf planet. Its surface is a patchwork of ice and rock.', '2025-04-08 13:36:00', v_charon, 'ford');

          PERFORM proc_add_celestial_body(v_hydra, 'Hydra', 'moon', v_pluto);
          PERFORM proc_write_content_revision('Hydra is one of the smaller, irregular moons orbiting Pluto. Its odd shape has sparked debate about its origin among astronomers.', '2025-04-08 13:38:00', v_hydra, 'arthur');

          PERFORM proc_add_celestial_body(v_nix, 'Nix', 'moon', v_pluto);
          PERFORM proc_write_content_revision('Nix is an oddly shaped satellite with a surface featuring deep craters. Its discovery further enriched our understanding of Pluto’s retinue.', '2025-04-08 13:40:00', v_nix, 'trillian');

          PERFORM proc_add_celestial_body(v_kerberos, 'Kerberos', 'moon', v_pluto);
          PERFORM proc_write_content_revision('Kerberos is a small, dark moon whose name evokes mythological guardians. It stands as an enigma in Pluto’s complex system.', '2025-04-08 13:42:00', v_kerberos, 'marvin');

          PERFORM proc_add_celestial_body(v_styx, 'Styx', 'moon', v_pluto);
          PERFORM proc_write_content_revision('Styx is the innermost and one of the smallest moons orbiting Pluto. Its brief name belies the long mythological journey its name represents.', '2025-04-08 13:44:00', v_styx, 'ford');

      -- Comets and Asteroids (orbits: Sun)
      PERFORM proc_add_celestial_body(v_halley, 'Halley''s Comet', 'comet', v_sun);
      PERFORM proc_write_content_revision('Halley''s Comet is perhaps the most famous of its kind, returning approximately every 76 years. Its spectacular tail has been recorded by generations of skywatchers.', '2025-04-08 13:46:00', v_halley, 'arthur');

      PERFORM proc_add_celestial_body(v_encke, 'Comet Encke', 'comet', v_sun);
      PERFORM proc_write_content_revision('Comet Encke is known for its relatively short orbital period and frequent visits. It serves as a reminder that even small icy bodies can be regular wanderers in our Solar System.', '2025-04-08 13:48:00', v_encke, 'trillian');

      PERFORM proc_add_celestial_body(v_halebopp, 'Comet Hale-Bopp', 'comet', v_sun);
      PERFORM proc_write_content_revision('Comet Hale-Bopp burst into public consciousness with a brilliant display in the late 20th century. Its long, glowing tail captured the hearts of millions.', '2025-04-08 13:50:00', v_halebopp, 'marvin');

      PERFORM proc_add_celestial_body(v_hyakutake, 'Comet Hyakutake', 'comet', v_sun);
      PERFORM proc_write_content_revision('Comet Hyakutake made a spectacular, though brief, appearance in the sky. Its close approach once sparked excitement and a flurry of scientific inquiry.', '2025-04-08 13:52:00', v_hyakutake, 'ford');

      PERFORM proc_add_celestial_body(v_ceres, 'Ceres', 'asteroid', v_sun);
      PERFORM proc_write_content_revision('Ceres is the largest object in the asteroid belt and is classified as a dwarf planet by some. Its surface and possible brines have fueled speculation about ancient water.', '2025-04-08 13:54:00', v_ceres, 'arthur');

      PERFORM proc_add_celestial_body(v_vesta, 'Vesta', 'asteroid', v_sun);
      PERFORM proc_write_content_revision('Vesta is one of the largest asteroids in our belt, exhibiting characteristics similar to a protoplanet. Its surface is a record of violent early collisions.', '2025-04-08 13:56:00', v_vesta, 'trillian');

      PERFORM proc_add_celestial_body(v_pallas, 'Pallas', 'asteroid', v_sun);
      PERFORM proc_write_content_revision('Pallas is notable for its high inclination orbit in the asteroid belt. Observations of its surface provide insights into early Solar System conditions.', '2025-04-08 13:58:00', v_pallas, 'marvin');

      PERFORM proc_add_celestial_body(v_hygiea, 'Hygiea', 'asteroid', v_sun);
      PERFORM proc_write_content_revision('Hygiea is one of the biggest asteroids and may even be considered a dwarf planet. Its nearly spherical shape is unusual among similarly sized bodies.', '2025-04-08 14:00:00', v_hygiea, 'ford');

    ----------------------------
    -- TRAPPIST-1 System
    ----------------------------
    PERFORM proc_add_celestial_body(v_trappist, 'TRAPPIST-1', 'star', v_sagA);
    PERFORM proc_write_content_revision('TRAPPIST-1 is a very cool ultra-cool dwarf star hosting a system of Earth-sized planets. It has become famous for its potential to harbor habitable worlds.', '2025-04-08 14:02:00', v_trappist, 'trillian');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'TRAPPIST-1 System', v_trappist);

      PERFORM proc_add_celestial_body(v_trappist1b, 'TRAPPIST-1b', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1b is the innermost planet orbiting its star. Its scorching temperatures and close orbit make it an extreme world.', '2025-04-08 14:04:00', v_trappist1b, 'ford');

      PERFORM proc_add_celestial_body(v_trappist1c, 'TRAPPIST-1c', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1c follows closely behind, with a rocky surface and tight orbit. It continues the trend of extreme conditions found in this system.', '2025-04-08 14:06:00', v_trappist1c, 'marvin');

      PERFORM proc_add_celestial_body(v_trappist1d, 'TRAPPIST-1d', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1d may be one of the more temperate worlds in the system. Its potential for water has made it an object of interest.', '2025-04-08 14:08:00', v_trappist1d, 'arthur');

      PERFORM proc_add_celestial_body(v_trappist1e, 'TRAPPIST-1e', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1e is widely discussed as one of the best candidates for habitability in the system. Its rocky terrain and potential water resources excite many researchers.', '2025-04-08 14:10:00', v_trappist1e, 'trillian');

      PERFORM proc_add_celestial_body(v_trappist1f, 'TRAPPIST-1f', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1f is another intriguing planet whose characteristics add to the diversity of the system. Its orbit suggests a delicate balance between heat and cold.', '2025-04-08 14:12:00', v_trappist1f, 'ford');

      PERFORM proc_add_celestial_body(v_trappist1g, 'TRAPPIST-1g', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1g continues the trend of Earth-sized worlds orbiting this feisty star. Its climate may offer clues to habitability under unusual conditions.', '2025-04-08 14:14:00', v_trappist1g, 'marvin');

      PERFORM proc_add_celestial_body(v_trappist1h, 'TRAPPIST-1h', 'planet', v_trappist);
      PERFORM proc_write_content_revision('TRAPPIST-1h rounds out the planetary family with its faint but intriguing presence. Its remote orbit hints at secrets yet to be discovered.', '2025-04-08 14:16:00', v_trappist1h, 'arthur');

    ----------------------------
    -- Alpha Centauri System
    ----------------------------
    PERFORM proc_add_celestial_body(v_alpha, 'Alpha Centauri', 'star', v_sagA);
    PERFORM proc_write_content_revision('Alpha Centauri is the nearest star system to our own, drawing much scientific attention. Its bright star has long been a navigational beacon for explorers.', '2025-04-08 14:18:00', v_alpha, 'trillian');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Alpha Centauri System', v_alpha);

      PERFORM proc_add_celestial_body(v_proximab, 'Proxima Centauri b', 'planet', v_alpha);
      PERFORM proc_write_content_revision('Proxima Centauri b circles the red dwarf in a potentially habitable zone. Its proximity to our Solar System makes it a target of intense study.', '2025-04-08 14:20:00', v_proximab, 'ford');

    ----------------------------
    -- Kepler-90 System
    ----------------------------
    PERFORM proc_add_celestial_body(v_kepler90, 'Kepler-90', 'star', v_sagA);
    PERFORM proc_write_content_revision('Kepler-90 is known for hosting a remarkable ensemble of planets. Its planetary system is a testament to nature’s capacity for variety in orbital design.', '2025-04-08 14:22:00', v_kepler90, 'marvin');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Kepler-90 System', v_kepler90);

      PERFORM proc_add_celestial_body(v_kepler90b, 'Kepler-90b', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90b is one of the innermost worlds in its system. Its tight orbit offers clues about planetary migration.', '2025-04-08 14:24:00', v_kepler90b, 'arthur');

      PERFORM proc_add_celestial_body(v_kepler90c, 'Kepler-90c', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90c continues the family of close orbiters with interesting composition. Its discovery helped expand our view of planetary systems.', '2025-04-08 14:26:00', v_kepler90c, 'trillian');

      PERFORM proc_add_celestial_body(v_kepler90d, 'Kepler-90d', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90d is a mid-sized planet whose characteristics offer a window into planetary evolution. Detailed studies reveal a complex atmosphere.', '2025-04-08 14:28:00', v_kepler90d, 'ford');

      PERFORM proc_add_celestial_body(v_kepler90e, 'Kepler-90e', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90e is known for its intriguing orbital resonance with its siblings. Its warm surface conditions add to the system’s diversity.', '2025-04-08 14:30:00', v_kepler90e, 'marvin');

      PERFORM proc_add_celestial_body(v_kepler90f, 'Kepler-90f', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90f occupies a more distant orbit, where cooler temperatures prevail. Its discovery has deepened our knowledge of exoplanet architectures.', '2025-04-08 14:32:00', v_kepler90f, 'arthur');

      PERFORM proc_add_celestial_body(v_kepler90g, 'Kepler-90g', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90g adds to the rich tapestry of this planetary system with its unique size and orbit. Its study continues to yield surprising insights.', '2025-04-08 14:34:00', v_kepler90g, 'trillian');

      PERFORM proc_add_celestial_body(v_kepler90h, 'Kepler-90h', 'planet', v_kepler90);
      PERFORM proc_write_content_revision('Kepler-90h rounds out the system with its distant orbit and chilly environment. Its characteristics challenge our understanding of planet formation.', '2025-04-08 14:36:00', v_kepler90h, 'ford');

    ----------------------------
    -- M31 (Andromeda Galaxy) and companions
    ----------------------------
    PERFORM proc_add_celestial_body(v_m31, 'M31*', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('M31, also known as the Andromeda Galaxy, is our closest spiral galaxy neighbour. Its sprawling arms and bright core have been observed for centuries.', '2025-04-08 14:38:00', v_m31, 'arthur');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Andromeda Galaxy', v_m31);

      PERFORM proc_add_celestial_body(v_m32, 'M32', 'galaxy', v_m31);
      PERFORM proc_write_content_revision('M32 is a compact elliptical companion of Andromeda. Its dense central regions offer unique insights into galactic evolution.', '2025-04-08 14:40:00', v_m32, 'trillian');

      PERFORM proc_add_celestial_body(v_m110, 'M110', 'galaxy', v_m31);
      PERFORM proc_write_content_revision('M110 is an elliptical companion to M31 with a rich star history. Its properties provide a contrast to its spiral neighbour.', '2025-04-08 14:42:00', v_m110, 'ford');

      PERFORM proc_add_celestial_body(v_ngc205, 'NGC 205', 'galaxy', v_m31);
      PERFORM proc_write_content_revision('NGC 205 is a dwarf elliptical galaxy gravitationally bound to Andromeda. Its stellar population hints at an active past.', '2025-04-08 14:44:00', v_ngc205, 'marvin');

      PERFORM proc_add_celestial_body(v_ngc147, 'NGC 147', 'galaxy', v_m31);
      PERFORM proc_write_content_revision('NGC 147 is a faint dwarf galaxy accompanying Andromeda. Its subtle features require careful observation to appreciate.', '2025-04-08 14:46:00', v_ngc147, 'arthur');

      PERFORM proc_add_celestial_body(v_ngc185, 'NGC 185', 'galaxy', v_m31);
      PERFORM proc_write_content_revision('NGC 185 is another dwarf galaxy in the Andromeda Group that offers clues on galaxy interaction. Its stellar makeup is notably diverse.', '2025-04-08 14:48:00', v_ngc185, 'trillian');

    ----------------------------
    -- M33 (Triangulum Galaxy) and Nebulae
    ----------------------------
    PERFORM proc_add_celestial_body(v_m33, 'M33 Core', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('M33, the Triangulum Galaxy, is a nearby spiral galaxy known for its impressive star-forming regions. Its core acts as a beacon of recent stellar creation.', '2025-04-08 14:50:00', v_m33, 'ford');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Triangulum Galaxy', v_m33);

      PERFORM proc_add_celestial_body(v_ngc604, 'NGC 604', 'nebula', v_m33);
      PERFORM proc_write_content_revision('NGC 604 is one of the largest H II regions in the Local Group. Its turbulent gas clouds are teeming with newborn stars.', '2025-04-08 14:52:00', v_ngc604, 'marvin');

      PERFORM proc_add_celestial_body(v_ngc595, 'NGC 595', 'nebula', v_m33);
      PERFORM proc_write_content_revision('NGC 595 is a bright nebula illuminating the surrounding regions in the Triangulum Galaxy. Its glowing filaments have inspired countless imaginations.', '2025-04-08 14:54:00', v_ngc595, 'arthur');

      PERFORM proc_add_celestial_body(v_ngc588, 'NGC 588', 'nebula', v_m33);
      PERFORM proc_write_content_revision('NGC 588, though less famous, is a similarly dynamic nebula in M33. Its intricate clouds point to ongoing star birth.', '2025-04-08 14:56:00', v_ngc588, 'trillian');

    ----------------------------
    -- Additional Galaxies and Structures
    ----------------------------
    PERFORM proc_add_celestial_body(v_alcyoneus, 'Alcyoneus Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Alcyoneus Primary stands as a unique galaxy within the cosmic tapestry. Its diffuse glow and sprawling structure inspire wonder.', '2025-04-08 14:58:00', v_alcyoneus, 'ford');

    PERFORM proc_add_celestial_body(v_backward, 'Backward Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Backward Galaxy Primary is so named for its unusual rotation and appearance. Its curious features spark debates among astronomers.', '2025-04-08 15:00:00', v_backward, 'marvin');

    PERFORM proc_add_celestial_body(v_barnard, 'Barnard''s Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Barnard''s Galaxy Primary is a well-known dwarf galaxy with a compact shape. Its proximity and motion offer a window into galactic dynamics.', '2025-04-08 15:02:00', v_barnard, 'arthur');

    PERFORM proc_add_celestial_body(v_ngc4826, 'NGC 4826 Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('NGC 4826, the Black Eye Galaxy, is recognized for its dark, absorbing dust lane. Its peculiar structure has been a subject of many studies.', '2025-04-08 15:04:00', v_ngc4826, 'trillian');
      PERFORM proc_add_celestial_body(v_dust_lane, 'Dust Lane', 'dust lane', v_ngc4826);
      PERFORM proc_write_content_revision('The Dust Lane in NGC 4826 is a prominent, dark band cutting across its bright central regions. Its structure is a vivid reminder of cosmic interstellar material.', '2025-04-08 15:06:00', v_dust_lane, 'ford');

    PERFORM proc_add_celestial_body(v_butterfly, 'NGC 4567 & NGC 4568', 'galaxy pair', v_universe);
    PERFORM proc_write_content_revision('The Butterfly Galaxies are a pair in a graceful dance of gravitational interplay. Their tidal interactions illustrate the beauty and complexity of cosmic mergers.', '2025-04-08 15:08:00', v_butterfly, 'marvin');
      PERFORM proc_add_celestial_body(v_tidal_bridge, 'Tidal Bridge', 'tidal bridge', v_butterfly);
      PERFORM proc_write_content_revision('The Tidal Bridge connects the two galaxies in the Butterfly pair. Its structure is a tangible symbol of their ongoing interaction.', '2025-04-08 15:10:00', v_tidal_bridge, 'arthur');

    PERFORM proc_add_celestial_body(v_bodes, 'Bode''s Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Bode''s Galaxy Primary captivates with its distinctive spiral shape and bright core. Its central regions have been the focus of many stellar studies.', '2025-04-08 15:12:00', v_bodes, 'trillian');
      PERFORM proc_add_celestial_body(v_nuclear_star_cluster, 'Nuclear Star Cluster', 'nuclear star cluster', v_bodes);
      PERFORM proc_write_content_revision('The Nuclear Star Cluster in Bode''s Galaxy is a compact group of stars at its heart. Its density and brightness offer clues about galactic evolution.', '2025-04-08 15:14:00', v_nuclear_star_cluster, 'ford');

    PERFORM proc_add_celestial_body(v_cartwheel, 'Cartwheel Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('The Cartwheel Galaxy is renowned for its ring‐like structure resulting from a past collision. Its unusual morphology remains one of the most striking sights in the cosmos.', '2025-04-08 15:16:00', v_cartwheel, 'marvin');
      PERFORM proc_add_celestial_body(v_outer_ring, 'Outer Ring', 'ring', v_cartwheel);
      PERFORM proc_write_content_revision('The Outer Ring in the Cartwheel Galaxy highlights the ripples of its collisional history. Its expanse is both beautiful and mysterious.', '2025-04-08 15:18:00', v_outer_ring, 'arthur');
      PERFORM proc_add_celestial_body(v_inner_ring, 'Inner Ring', 'ring', v_cartwheel);
      PERFORM proc_write_content_revision('The Inner Ring of the Cartwheel Galaxy is a concentrated area of star formation. It contrasts sharply with the sparse outer regions.', '2025-04-08 15:20:00', v_inner_ring, 'trillian');
      PERFORM proc_add_celestial_body(v_spokes, 'Spokes', 'spokes', v_cartwheel);
      PERFORM proc_write_content_revision('The Spokes in the Cartwheel Galaxy are faint connections stretching inward from the ring. Their appearance evokes imagery from interstellar maps.', '2025-04-08 15:22:00', v_spokes, 'ford');

    PERFORM proc_add_celestial_body(v_condor, 'Condor Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('The Condor Galaxy Primary is noted for its impressive size and gracefully extended arms. It exemplifies the variety of galactic forms.', '2025-04-08 15:24:00', v_condor, 'marvin');
      PERFORM proc_add_celestial_body(v_tidal_tail, 'Tidal Tail', 'tidal tail', v_condor);
      PERFORM proc_write_content_revision('The Tidal Tail of the Condor Galaxy sweeps gracefully away from its main body. It is a dramatic signature of past gravitational encounters.', '2025-04-08 15:26:00', v_tidal_tail, 'arthur');

    PERFORM proc_add_celestial_body(v_hoag, 'Hoag''s Object Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Hoag''s Object is a ring galaxy of great curiosity with a nearly perfect spherical core. Its isolated ring structure defies easy explanation.', '2025-04-08 15:28:00', v_hoag, 'trillian');
      PERFORM proc_add_celestial_body(v_hoag_core, 'Core', 'core', v_hoag);
      PERFORM proc_write_content_revision('The core of Hoag''s Object shines brightly at its centre. Its smooth appearance contrasts with the ring around it.', '2025-04-08 15:30:00', v_hoag_core, 'ford');
      PERFORM proc_add_celestial_body(v_hoag_ring, 'Ring', 'ring', v_hoag);
      PERFORM proc_write_content_revision('The ring in Hoag''s Object is a nearly perfect circle of stars and gas. Its symmetry is both rare and enchanting.', '2025-04-08 15:32:00', v_hoag_ring, 'marvin');

    PERFORM proc_add_celestial_body(v_lindsay, 'Lindsay-Shapley Ring Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('The Lindsay-Shapley Ring is noted for its massive ring structure. Its awe-inspiring curve suggests dramatic past events.', '2025-04-08 15:34:00', v_lindsay, 'arthur');
      PERFORM proc_add_celestial_body(v_ring_structure, 'Ring Structure', 'ring', v_lindsay);
      PERFORM proc_write_content_revision('The Ring Structure here is a fine example of cosmic architecture. Its graceful arc is a focal point of study.', '2025-04-08 15:36:00', v_ring_structure, 'trillian');

    PERFORM proc_add_celestial_body(v_meathook, 'Meathook Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Meathook Galaxy Primary is distinguished by its strangely distorted spiral structure. Its appearance reminds one of a hook that has been bent by cosmic forces.', '2025-04-08 15:38:00', v_meathook, 'ford');
      PERFORM proc_add_celestial_body(v_distorted_spiral, 'Distorted Spiral Structure', 'spiral structure', v_meathook);
      PERFORM proc_write_content_revision('The Distorted Spiral Structure of the Meathook Galaxy hints at past collisions. Its irregular curves are a testament to the chaotic nature of galaxy evolution.', '2025-04-08 15:40:00', v_distorted_spiral, 'marvin');

    PERFORM proc_add_celestial_body(v_medusa, 'Medusa Merger Primary', 'galaxy merger', v_universe);
    PERFORM proc_write_content_revision('The Medusa Merger is a wild collision of galaxies resulting in tendrils of stars and gas. Its chaotic beauty is striking and unpredictable.', '2025-04-08 15:42:00', v_medusa, 'arthur');

    PERFORM proc_add_celestial_body(v_mayall, 'Mayall''s Object Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Mayall''s Object is renowned for its rare collisional ring structure. Its appearance suggests a dramatic history of galactic impact.', '2025-04-08 15:44:00', v_mayall, 'trillian');
      PERFORM proc_add_celestial_body(v_collisional_ring, 'Collisional Ring', 'ring', v_mayall);
      PERFORM proc_write_content_revision('The Collisional Ring is a spectacular band of stars arising from a head-on galactic collision. Its symmetry belies a violent past.', '2025-04-08 15:46:00', v_collisional_ring, 'ford');

    PERFORM proc_add_celestial_body(v_porphyrion, 'Porphyrion Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('Porphyrion Primary glows with a mysterious, reddish hue. Its features continue to intrigue as more data come in.', '2025-04-08 15:48:00', v_porphyrion, 'marvin');

    PERFORM proc_add_celestial_body(v_topsy, 'Topsy Turvy Galaxy Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('The Topsy Turvy Galaxy is noted for its warped disk and off-kilter appearance. Its twisting patterns challenge conventional models of galaxy formation.', '2025-04-08 15:50:00', v_topsy, 'arthur');
      PERFORM proc_add_celestial_body(v_warped_disk, 'Warped Disk', 'disk', v_topsy);
      PERFORM proc_write_content_revision('The Warped Disk in Topsy Turvy is a vivid example of gravitational distortion. Its curves are as unpredictable as the hitchhiker’s road through the galaxy.', '2025-04-08 15:52:00', v_warped_disk, 'trillian');

    PERFORM proc_add_celestial_body(v_ngc5194, 'NGC 5194 Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('NGC 5194, the Whirlpool Galaxy, is celebrated for its striking spiral arms and interaction with its companion. Its beauty has inspired many cosmic travelers.', '2025-04-08 15:54:00', v_ngc5194, 'ford');
      PERFORM proc_add_celestial_body(v_ngc5195, 'Companion NGC 5195', 'galaxy', v_ngc5194);
      PERFORM proc_write_content_revision('The Companion NGC 5195 interacts with the Whirlpool Galaxy, drawing tidal streams between them. Their relationship is a vivid dance of gravity.', '2025-04-08 15:56:00', v_ngc5195, 'marvin');

    PERFORM proc_add_celestial_body(v_lmc, 'Large Magellanic Cloud Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('The Large Magellanic Cloud is a satellite galaxy of the Milky Way brimming with stellar nurseries. Its irregular shape and active star formation are a delight for astronomers.', '2025-04-08 15:58:00', v_lmc, 'arthur');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Large Magellanic Cloud', v_lmc);
      PERFORM proc_add_celestial_body(v_30dor, '30 Doradus', 'nebula', v_lmc);
      PERFORM proc_write_content_revision('30 Doradus is a giant H II region in the Large Magellanic Cloud, bursting with the light of newborn stars. Its dazzling display has been compared to the cosmic equivalent of a supernova party.', '2025-04-08 16:00:00', v_30dor, 'trillian');
      
      PERFORM proc_add_celestial_body(v_ngc1841, 'NGC 1841', 'star cluster', v_lmc);
      PERFORM proc_write_content_revision('NGC 1841 is a sparse but ancient star cluster that offers a glimpse into early star formation. Its quiet glow contrasts with the vibrant nebulae nearby.', '2025-04-08 16:02:00', v_ngc1841, 'ford');

    PERFORM proc_add_celestial_body(v_smcc, 'Small Megallenic Cloud Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('The Small Megallenic Cloud is a diminutive, irregular galaxy accompanied by intriguing star clusters. Its modest size belies its significance in galactic studies.', '2025-04-08 16:04:00', v_smcc, 'marvin');
    PERFORM proc_add_star_system(uuid_generate_v4(), 'Small Megallenic Cloud', v_smcc);
      PERFORM proc_add_celestial_body(v_ngc346, 'NGC 346', 'star cluster', v_smcc);
      PERFORM proc_write_content_revision('NGC 346 is a luminous star cluster within the Small Megallenic Cloud. Its vibrant young stars continue to capture attention.', '2025-04-08 16:06:00', v_ngc346, 'arthur');

      PERFORM proc_add_celestial_body(v_ngc602, 'NGC 602', 'star cluster', v_smcc);
      PERFORM proc_write_content_revision('NGC 602 is another noteworthy star cluster in the Small Megallenic Cloud. Its compact arrangement of stars is both charming and significant.', '2025-04-08 16:08:00', v_ngc602, 'trillian');

    PERFORM proc_add_celestial_body(v_ngc5128, 'NGC 5128 Primary', 'galaxy', v_universe);
    PERFORM proc_write_content_revision('NGC 5128, also known as Centaurus A, is famous for its peculiar appearance and radio emissions. It continues to be a rich field for exploration and debate.', '2025-04-08 16:10:00', v_ngc5128, 'ford');

--------------------------
-- 4. Insert More Comments
--------------------------

    PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'zaphod', 'Earth: Mostly harmless, but the tea is excellent. And the towels. Don''t forget the towels!', '2025-04-08 13:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'marvin', 'The Moon. A dull rock. I''ve seen more exciting paperweights.', '2025-04-08 13:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iss, 'arthur', 'The ISS is quite a view, but I miss the ground. And a proper cup of tea.', '2025-04-08 13:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'ford', 'Mars: Red and dusty. Great for off-road space travel. Just watch out for the rovers. They have opinions.', '2025-04-08 13:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_phobos, 'trillian', 'Phobos is just a tiny rock, but it has a certain... charm. Or maybe it''s just the lack of gravity.', '2025-04-08 13:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_deimos, 'zaphod', 'Deimos? More like "Dim-os". Small and unimpressive. But hey, to each their own.', '2025-04-08 13:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'marvin', 'Jupiter. A gas giant. How exciting. If you like swirling clouds and crushing pressure.', '2025-04-08 13:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_io, 'arthur', 'Io is quite volcanic, but I find the sulfur smell rather... pungent. And the lava, a bit too hot for my taste.', '2025-04-08 13:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_europa, 'ford', 'Europa: Ice and oceans. Could be a good place for a space beach. If it weren''t for the whole "under a mile of ice" thing.', '2025-04-08 14:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ganymede, 'trillian', 'Ganymede is huge! Almost a planet. But the surface is a bit... bland. Needs more gardens.', '2025-04-08 14:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_callisto, 'zaphod', 'Callisto? Call it "Calm-isto". Quiet, serene, and utterly uninteresting. Perfect for a nap.', '2025-04-08 14:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_amalthea, 'marvin', 'Amalthea. Another small rock. Why do they even bother naming these things?', '2025-04-08 14:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_thebe, 'arthur', 'Thebe? I think I left my towel there. Or was it on Ganymede? Oh, where did I leave it?', '2025-04-08 14:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_himalia, 'ford', 'Himalia: A long way from anywhere. Good for hiding if you''re wanted in multiple star systems.', '2025-04-08 14:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_elara, 'trillian', 'Elara is quite pretty, if you like rocks. And I do, sometimes. But mostly, I like gardens.', '2025-04-08 14:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_lysithea, 'zaphod', 'Lysithea? Sounds like a sneeze. Or a small, boring moon. Either way, not worth the trip.', '2025-04-08 14:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'marvin', 'Saturn. Rings. Big deal. Just a lot of rocks and ice. And more gas.', '2025-04-08 14:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titan, 'arthur', 'Titan has lakes of methane. I tried to make tea with it. It didn''t work.', '2025-04-08 14:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_enceladus, 'ford', 'Enceladus: Geysers of water. Might be good for a space-shower. Or a space-spa.', '2025-04-08 14:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iapetus, 'trillian', 'Iapetus is two-toned! Like a giant space marble. Quite stylish, really.', '2025-04-08 14:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_rhea, 'zaphod', 'Rhea? More like "Meh-a". Bland and unremarkable. Skip it.', '2025-04-08 15:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dione, 'marvin', 'Dione. Another ice ball. How original.', '2025-04-08 15:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tethys, 'arthur', 'Tethys has a big canyon. I tried to find my towel in it. No luck.', '2025-04-08 15:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mimas, 'ford', 'Mimas looks like a Death Star. But smaller. And less... deadly? Hopefully.', '2025-04-08 15:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hyperion, 'trillian', 'Hyperion is shaped like a sponge! It''s quite bizarre, even by space standards.', '2025-04-08 15:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_phoebe, 'zaphod', 'Phoebe? Sounds like a bad romance novel. And looks like one, too. Dull.', '2025-04-08 15:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_janus, 'marvin', 'Janus. Two-faced. How fitting.', '2025-04-08 15:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_epimetheus, 'arthur', 'Epimetheus? Sounds like a headache. Or was that just the methane tea?', '2025-04-08 15:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_uranus, 'ford', 'Uranus: Tilted and blue. Might be good for space-surfing. If you don''t mind the cold.', '2025-04-08 15:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_miranda, 'trillian', 'Miranda is a patchwork of terrains. It''s quite a visual feast, if you like geological chaos.', '2025-04-08 15:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ariel, 'zaphod', 'Ariel? Sounds like a laundry detergent. And looks about as exciting.', '2025-04-08 15:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_umbriel, 'marvin', 'Umbriel. Dark and dreary. Just like my mood.', '2025-04-08 15:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titania, 'arthur', 'Titania is large. I tried to find a good spot for tea. It was too cold.', '2025-04-08 16:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_oberon, 'ford', 'Oberon: Old and cratered. Like a space-grandpa. Respectable, but not exactly thrilling.', '2025-04-08 16:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_neptune, 'trillian', 'Neptune is a deep blue beauty. It''s quite mesmerizing, really.', '2025-04-08 16:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_triton, 'zaphod', 'Triton? Cold and icy. And smells like space-fish. Pass.', '2025-04-08 16:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nereid, 'marvin', 'Nereid. Another orbit. Another rock. How thrilling.', '2025-04-08 16:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_proteus, 'arthur', 'Proteus? I think I left my spare socks there. Or was it on Titan?', '2025-04-08 16:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_larissa, 'ford', 'Larissa: Small and fast. Good for a quick space-dash. If you have a death wish.', '2025-04-08 16:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_pluto, 'trillian', 'Pluto is a dwarf, but it has heart. And ice mountains. Quite picturesque.', '2025-04-08 16:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_charon, 'zaphod', 'Charon? Sounds like a brand of space-charcoal. And looks about as useful.', '2025-04-08 16:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hydra, 'marvin', 'Hydra. Multiple heads. How annoying.', '2025-04-08 16:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nix, 'arthur', 'Nix? I think I lost my lucky coin there. Or was it on Mimas?', '2025-04-08 16:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kerberos, 'ford', 'Kerberos: Dark and mysterious. Good for brooding. If you''re a space-goth.', '2025-04-08 16:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_styx, 'trillian', 'Styx is so small! It''s like a space-pebble. Quite cute, really.', '2025-04-08 17:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_halley, 'zaphod', 'Halley''s Comet? A big, dirty snowball. Not impressed.', '2025-04-08 17:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_encke, 'marvin', 'Encke. Another comet. Another disappointment.', '2025-04-08 17:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_halebopp, 'arthur', 'Hale-Bopp? I saw it once. It was bright. But I still prefer tea.', '2025-04-08 17:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hyakutake, 'ford', 'Hyakutake: A long, bright tail. Good for space-windsurfing. If you''re brave.', '2025-04-08 17:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ceres, 'trillian', 'Ceres is a dwarf planet with a bright spot. Quite intriguing, even for a rock.', '2025-04-08 17:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_vesta, 'zaphod', 'Vesta? Sounds like a space-appliance. And looks about as exciting.', '2025-04-08 17:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_pallas, 'marvin', 'Pallas. Another asteroid. Another yawn.', '2025-04-08 17:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hygiea, 'arthur', 'Hygiea? I think I left my space-spoon there. Or was it on Callisto?', '2025-04-08 17:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist, 'ford', 'TRAPPIST-1: A whole system of planets! Might be good for a space-road trip. If you have a good map.', '2025-04-08 17:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1b, 'trillian', 'TRAPPIST-1b is close to its star. Quite toasty, I imagine.', '2025-04-08 17:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1c, 'zaphod', 'TRAPPIST-1c? Sounds like a space-snack. And looks about as appealing.', '2025-04-08 17:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1d, 'marvin', 'TRAPPIST-1d. Another planet. Another disappointment.', '2025-04-08 18:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1e, 'arthur', 'TRAPPIST-1e? I think I left my space-blanket there. Or was it on Europa?', '2025-04-08 18:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1f, 'ford', 'TRAPPIST-1f: In the habitable zone! Might be good for a space-garden. If you have seeds.', '2025-04-08 18:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1g, 'trillian', 'TRAPPIST-1g is quite large. It''s like a super-Earth! Or a super-rock, depending on your perspective.', '2025-04-08 18:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_trappist1h, 'zaphod', 'TRAPPIST-1h? Sounds like a space-sigh. And looks about as interesting.', '2025-04-08 18:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_alpha, 'marvin', 'Alpha Centauri. Another star system. Another reason to be bored.', '2025-04-08 18:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_proximab, 'arthur', 'Proxima b? I think I left my space-guidebook there. Or was it on Mars?', '2025-04-08 18:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90, 'ford', 'Kepler-90: Eight planets! Might be good for a space-family reunion. If you have a big spaceship.', '2025-04-08 18:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90b, 'trillian', 'Kepler-90b is a hot Jupiter. Quite a sight, if you like boiling gas.', '2025-04-08 18:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90c, 'zaphod', 'Kepler-90c? Sounds like a space-cough. And looks about as appealing.', '2025-04-08 18:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90d, 'marvin', 'Kepler-90d. Another planet. Another reason to be depressed.', '2025-04-08 18:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90e, 'arthur', 'Kepler-90e? I think I left my space-map there. Or was it on Titan?', '2025-04-08 18:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90f, 'ford', 'Kepler-90f: Another gas giant. Might be good for a space-balloon ride. If you have a big balloon.', '2025-04-08 19:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90g, 'trillian', 'Kepler-90g is quite large. It''s like a super-Jupiter! Or a super-cloud, depending on your perspective.', '2025-04-08 19:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_kepler90h, 'zaphod', 'Kepler-90h? Sounds like a space-sneeze. And looks about as interesting.', '2025-04-08 19:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_m31, 'marvin', 'M31. Andromeda. Another galaxy. Another reason to be unimpressed.', '2025-04-08 19:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_m32, 'arthur', 'M32? I think I left my space-notepad there. Or was it on Mars?', '2025-04-08 19:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_m110, 'ford', 'M110: A dwarf galaxy. Might be good for a space-hideaway. If you''re avoiding crowds.', '2025-04-08 19:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc205, 'trillian', 'NGC 205 is a dwarf elliptical galaxy. Quite a sight, if you like star clusters.', '2025-04-08 19:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc147, 'zaphod', 'NGC 147? Sounds like a space-cough. And looks about as appealing.', '2025-04-08 19:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc185, 'marvin', 'NGC 185. Another galaxy. Another reason to be bored.', '2025-04-08 19:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_m33, 'arthur', 'M33? I think I left my space-pen there. Or was it on Titan?', '2025-04-08 19:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc604, 'ford', 'NGC 604: A giant H II region. Might be good for a space-light show. If you like nebulae.', '2025-04-08 19:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc595, 'trillian', 'NGC 595 is a star-forming region. Quite a spectacle, really.', '2025-04-08 19:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc588, 'zaphod', 'NGC 588? Sounds like a space-sneeze. And looks about as interesting.', '2025-04-08 20:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_alcyoneus, 'marvin', 'Alcyoneus. A radio galaxy. Another reason to be unimpressed.', '2025-04-08 20:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_backward, 'arthur', 'Backward galaxy? I think I left my space-direction there. Or was it on Mars?', '2025-04-08 20:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_barnard, 'ford', 'Barnard''s Star: A red dwarf. Might be good for a space-camping trip. If you like dim light.', '2025-04-08 20:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc4826, 'trillian', 'NGC 4826, the Black Eye Galaxy. Quite a mysterious look, really.', '2025-04-08 20:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dust_lane, 'zaphod', 'Dust lane? Sounds like a space-cleanup job. And looks about as fun.', '2025-04-08 20:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_butterfly, 'marvin', 'Butterfly Galaxies. Another galaxy pair. Another reason to be bored.', '2025-04-08 20:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tidal_bridge, 'arthur', 'Tidal Bridge? I think I left my space-measuring tape there. Or was it on Titan?', '2025-04-08 20:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_bodes, 'ford', 'Bode''s Galaxy: A spiral galaxy. Might be good for a space-sightseeing tour. If you like spirals.', '2025-04-08 20:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nuclear_star_cluster, 'trillian', 'Nuclear star cluster. Quite a dense collection of stars, really.', '2025-04-08 20:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_cartwheel, 'zaphod', 'Cartwheel Galaxy? Sounds like a space-accident. And looks about as chaotic.', '2025-04-08 20:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_outer_ring, 'marvin', 'Outer ring. Another ring. Another reason to be unimpressed.', '2025-04-08 20:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_inner_ring, 'arthur', 'Inner Ring? I think I left my space-ring there. Or was it on Mars?', '2025-04-08 21:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_spokes, 'ford', 'Spokes? Might be good for a space-wheelie. If you have a space-bike.', '2025-04-08 21:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_condor, 'trillian', 'Condor Galaxy. Quite a majestic sight, really.', '2025-04-08 21:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tidal_tail, 'zaphod', 'Tidal tail? Sounds like a space-catastrophe. And looks about as messy.', '2025-04-08 21:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hoag, 'marvin', 'Hoag''s Object. Another ring galaxy. Another reason to be bored.', '2025-04-08 21:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hoag_core, 'arthur', 'Hoag''s core? I think I left my space-key there. Or was it on Titan?', '2025-04-08 21:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hoag_ring, 'ford', 'Hoag''s ring? Might be good for a space-race track. If you have a space-car.', '2025-04-08 21:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_lindsay, 'trillian', 'Lindsay-Shapley ring. Quite a beautiful structure, really.', '2025-04-08 21:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ring_structure, 'zaphod', 'Ring structure? Sounds like a space-circus. And looks about as entertaining.', '2025-04-08 21:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_meathook, 'marvin', 'Meathook Galaxy. Another distorted spiral. Another reason to be unimpressed.', '2025-04-08 21:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_distorted_spiral, 'arthur', 'Distorted spiral? I think I left my space-screwdriver there. Or was it on Mars?', '2025-04-08 21:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_medusa, 'ford', 'Medusa Merger. Might be good for a space-horror movie. If you like tentacles.', '2025-04-08 21:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mayall, 'trillian', 'Mayall''s Object. Quite a fascinating collision, really.', '2025-04-08 22:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_collisional_ring, 'zaphod', 'Collisional ring? Sounds like a space-demolition derby. And looks about as destructive.', '2025-04-08 22:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_porphyrion, 'marvin', 'Porphyrion. Another galaxy merger. Another reason to be bored.', '2025-04-08 22:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_topsy, 'arthur', 'Topsy Turvy Galaxy? I think I left my space-compass there. Or was it on Titan?', '2025-04-08 22:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_warped_disk, 'ford', 'Warped disk? Might be good for a space-skate park. If you have a space-skateboard.', '2025-04-08 22:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc5194, 'trillian', 'NGC 5194, the Whirlpool Galaxy. Quite a stunning spiral, really.', '2025-04-08 22:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc5195, 'zaphod', 'NGC 5195? Sounds like a space-yawn. And looks about as exciting.', '2025-04-08 22:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_lmc, 'marvin', 'Large Magellanic Cloud. Another galaxy. Another reason to be unimpressed.', '2025-04-08 22:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_30dor, 'arthur', '30 Doradus? I think I left my space-flashlight there. Or was it on Mars?', '2025-04-08 22:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc1841, 'ford', 'NGC 1841: A globular cluster. Might be good for a space-star party. If you like crowds.', '2025-04-08 22:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_smcc, 'trillian', 'Small Magellanic Cloud. Quite a charming dwarf galaxy, really.', '2025-04-08 22:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc346, 'zaphod', 'NGC 346? Sounds like a space-burp. And looks about as appealing.', '2025-04-08 22:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc602, 'marvin', 'NGC 602. Another star cluster. Another reason to be bored.', '2025-04-08 23:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ngc5128, 'arthur', 'NGC 5128, Centaurus A? I think I left my space-socks there. Or was it on Titan?', '2025-04-08 23:05:00');

    PERFORM proc_add_comment(uuid_generate_v4(), v_mercury, 'ford', 'Heard it''s great for sunbathing... if you don''t mind the whole "molten lead" thing.', '2025-04-08 23:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_venus, 'trillian', 'Like a sauna, but with clouds of sulfuric acid. Romantic, isn''t it?', '2025-04-08 23:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'zaphod', 'Mostly harmless, but the bureaucracy is out of this world. Literally.', '2025-04-08 23:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'marvin', 'A rock. A very large, very boring rock.', '2025-04-08 23:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'arthur', 'Dusty. Red. And I think I left my towel there. Again.', '2025-04-08 23:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'ford', 'Big enough to swallow Earth whole. And probably has. With all the missing socks.', '20205-04-08 23:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_io, 'trillian', 'Pizza-colored volcanoes and sulfur snow. It''s an acquired taste.', '2025-04-08 23:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_europa, 'zaphod', 'Ice-cold oceans. Maybe they have good space-margaritas?', '2025-04-08 23:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ganymede, 'marvin', 'The largest moon. Still just a moon.', '2025-04-08 23:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_callisto, 'arthur', 'Craters everywhere. And I mean everywhere.', '2025-04-08 23:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'ford', 'Rings! So many rings! Like a space-hoarder''s paradise.', '2025-04-09 00:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titan, 'trillian', 'Lakes of liquid methane. Smells like... well, methane.', '2025-04-09 00:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_enceladus, 'zaphod', 'Water geysers! Finally, a space-shower!', '2025-04-09 00:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iapetus, 'marvin', 'Two-toned. Like a badly painted ball.', '2025-04-09 00:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_rhea, 'arthur', 'Another icy moon. I think I left my space-mittens there.', '2025-04-09 00:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dione, 'ford', 'Ice cliffs and canyons. Great for space-skiing. If you''re a penguin.', '2025-04-09 00:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tethys, 'trillian', 'Giant crater. Like a cosmic bullseye.', '2025-04-09 00:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mimas, 'zaphod', 'Looks like a space-death star. But smaller. And less deadly. Probably.', '2025-04-09 00:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hyperion, 'marvin', 'Shaped like a potato. A very lumpy potato.', '2025-04-09 00:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_phoebe, 'arthur', 'Dark and distant. I think I left my space-sunglasses there.', '2025-04-09 00:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_uranus, 'ford', 'Sideways and blue. Like a cosmic bowling ball.', '2025-04-09 00:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_miranda, 'trillian', 'Patchwork of terrains. Like a space-quilt.', '2025-04-09 00:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ariel, 'zaphod', 'Icy and cratered. Like a space-ice rink.', '2025-04-09 01:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_umbriel, 'marvin', 'Darkest moon. Like my soul.', '2025-04-09 01:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titania, 'arthur', 'Largest moon of Uranus. I think I left my space-umbrella there.', '2025-04-09 01:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_oberon, 'ford', 'Old and cratered. Like a space-grandpa.', '2025-04-09 01:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_neptune, 'trillian', 'Deep blue and windy. Like a cosmic ocean.', '2025-04-09 01:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_triton, 'zaphod', 'Cold and icy. Smells like space-fish. Pass.', '2025-04-09 01:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nereid, 'marvin', 'Long, weird orbit. Like my life.', '2025-04-09 01:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_pluto, 'arthur', 'Dwarf planet. I think I left my space-magnifying glass there.', '2025-04-09 01:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_charon, 'ford', 'Half the size of Pluto. Like a space-mini-me.', '2025-04-09 01:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mercury, 'trillian', 'A day is longer than its year. Talk about slow.', '2025-04-09 01:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_venus, 'zaphod', 'Spins backwards. Like a rebel without a cause.', '2025-04-09 01:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'marvin', 'Mostly harmless. Mostly.', '2025-04-09 01:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'arthur', 'Tidally locked. Like my brain when I try to understand Vogons.', '2025-04-09 02:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'ford', 'Olympus Mons is huge. Like, really huge.', '2025-04-09 02:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'trillian', 'The Great Red Spot is a storm. A really, really big storm.', '2025-04-09 02:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_io, 'zaphod', 'Most volcanically active place in the solar system. Hot.', '2025-04-09 02:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_europa, 'marvin', 'Under the ice, an ocean. Probably just more ice.', '2025-04-09 02:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ganymede, 'arthur', 'Has its own magnetic field. Fancy.', '2025-04-09 02:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_callisto, 'ford', 'Oldest surface in the solar system. Like a space-antique.', '2025-04-09 02:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'trillian', 'Rings made of ice and rock. Sparkly.', '2025-04-09 02:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titan, 'zaphod', 'Thick atmosphere. Smells like space-gasoline.', '2025-04-09 02:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_enceladus, 'marvin', 'Geysers of salty water. How exciting.', '2025-04-09 02:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iapetus, 'arthur', 'Walnut-shaped ridge. I think I left my space-nutcracker there.', '2025-04-09 02:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_rhea, 'ford', 'Thin atmosphere. Great for space-kites. If you have a space-kite.', '2025-04-09 02:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dione, 'trillian', 'Ice cliffs and wispy terrain. Pretty.', '2025-04-09 03:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tethys, 'zaphod', 'Giant canyon. Like a space-scar.', '2025-04-09 03:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mimas, 'marvin', 'Impact crater. Like a space-dent.', '2025-04-09 03:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hyperion, 'arthur', 'Chaotic rotation. Like my life.', '2025-04-09 03:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_phoebe, 'ford', 'Retrograde orbit. Like a space-rebel.', '2025-04-09 03:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_uranus, 'trillian', 'Tilted on its side. Like a space-slouch.', '2025-04-09 03:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_miranda, 'zaphod', 'Weird surface features. Like a space-puzzle.', '2025-04-09 03:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ariel, 'marvin', 'Smooth and cratered. How original.', '2025-04-09 03:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_umbriel, 'arthur', 'Darkest albedo. Like my mood.', '2025-04-09 03:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titania, 'ford', 'Canyons and faults. Like a space-crack.', '2025-04-09 03:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_oberon, 'trillian', 'Old and cratered. Like a space-wrinkle.', '2025-04-09 03:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_neptune, 'zaphod', 'Fastest winds in the solar system. Like a space-hurricane.', '2025-04-09 03:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_triton, 'marvin', 'Cryovolcanoes. How exciting.', '2025-04-09 04:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nereid, 'arthur', 'Most eccentric orbit. Like my life.', '2025-04-09 04:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_pluto, 'ford', 'Heart-shaped feature. Like a space-valentine.', '2025-04-09 04:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_charon, 'trillian', 'Tidally locked with Pluto. Like a space-dance.', '2025-04-09 04:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mercury, 'zaphod', 'Shrinking planet. Like a space-raisin.', '2025-04-09 04:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_venus, 'marvin', 'Hottest planet. How thrilling.', '2025-04-09 04:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'arthur', 'Home. Mostly harmless.', '2025-04-09 04:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'ford', 'Cheese-shaped surface. Like a space-snack.', '2025-04-09 04:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'trillian', 'Red planet. Like a space-tomato.', '2025-04-09 04:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'zaphod', 'Biggest planet. Like a space-whale.', '2025-04-09 04:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_io, 'marvin', 'Volcanic moon. How exciting.', '2025-04-09 04:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_europa, 'arthur', 'Icy moon. I think I left my space-skates there.', '2025-04-09 04:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ganymede, 'ford', 'Largest moon. Like a space-king.', '2025-04-09 05:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_callisto, 'trillian', 'Cratered moon. Like a space-pimple.', '2025-04-09 05:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'zaphod', 'Ringed planet. Like a space-donut.', '2025-04-09 05:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titan, 'marvin', 'Methane lakes. How thrilling.', '2025-04-09 05:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_enceladus, 'arthur', 'Geysers. I think I left my space-towel there.', '2025-04-09 05:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iapetus, 'ford', 'Two-toned. Like a space-zebra.', '2025-04-09 05:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_rhea, 'trillian', 'Icy moon. Pretty.', '2025-04-09 05:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dione, 'zaphod', 'Ice cliffs. Like a space-wall.', '2025-04-09 05:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tethys, 'marvin', 'Giant canyon. How thrilling.', '2025-04-09 05:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mimas, 'arthur', 'Death Star look-alike. I think I left my space-laser there.', '2025-04-09 05:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hyperion, 'ford', 'Potato-shaped moon. Like a space-fry.', '2025-04-09 05:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_phoebe, 'trillian', 'Retrograde orbit. Weird.', '2025-04-09 05:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_uranus, 'zaphod', 'Sideways planet. Like a space-couch.', '2025-04-09 06:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_miranda, 'marvin', 'Weird terrain. How original.', '2025-04-09 06:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ariel, 'arthur', 'Icy moon. I think I left my space-gloves there.', '2025-04-09 06:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_umbriel, 'ford', 'Darkest moon. Like a space-shadow.', '2025-04-09 06:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titania, 'trillian', 'Canyons. Pretty.', '2025-04-09 06:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_oberon, 'zaphod', 'Old moon. Like a space-grandpa.', '2025-04-09 06:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_neptune, 'marvin', 'Windy planet. How exciting.', '2025-04-09 06:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_triton, 'arthur', 'Cryovolcanoes. I think I left my space-blanket there.', '2025-04-09 06:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nereid, 'ford', 'Eccentric orbit. Like a space-dancer.', '2025-04-09 06:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_pluto, 'trillian', 'Heart-shaped feature. Cute.', '2025-04-09 06:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_charon, 'zaphod', 'Tidally locked. Like a space-couple.', '2025-04-09 06:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mercury, 'marvin', 'Smallest planet. How thrilling.', '2025-04-09 06:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_venus, 'arthur', 'Cloudy planet. I think I left my space-umbrella there.', '2025-04-09 07:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'ford', 'Mostly harmless. Mostly.', '2025-04-09 07:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'trillian', 'Tidally locked. Like a space-pet.', '2025-04-09 07:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'zaphod', 'Dusty planet. Like a space-attic.', '2025-04-09 07:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'marvin', 'Gas giant. How thrilling.', '2025-04-09 07:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_io, 'arthur', 'Volcanic moon. I think I left my space-fire extinguisher there.', '2025-04-09 07:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_europa, 'ford', 'Icy moon. Like a space-fridge.', '2025-04-09 07:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ganymede, 'trillian', 'Biggest moon. Impressive.', '2025-04-09 07:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_callisto, 'zaphod', 'Cratered moon. Like a space-golf ball.', '2025-04-09 07:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'marvin', 'Ringed planet. How thrilling.', '2025-04-09 07:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titan, 'arthur', 'Methane seas. I think I left my space-boat there.', '2025-04-09 07:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_enceladus, 'ford', 'Geysers. Like a space-sprinkler.', '2025-04-09 07:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iapetus, 'trillian', 'Two-toned. Like a space-clown.', '2025-04-09 08:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_rhea, 'zaphod', 'Icy moon. Like a space-cube.', '2025-04-09 08:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dione, 'marvin', 'Ice cliffs. How thrilling.', '2025-04-09 08:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_tethys, 'arthur', 'Giant canyon. I think I left my space-rope there.', '2025-04-09 08:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mimas, 'ford', 'Death Star look-alike. Like a space-toy.', '2025-04-09 08:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_hyperion, 'trillian', 'Chaotic rotation. Weird.', '2025-04-09 08:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_phoebe, 'zaphod', 'Retrograde orbit. Like a space-rebel.', '2025-04-09 08:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_uranus, 'marvin', 'Sideways planet. How thrilling.', '2025-04-09 08:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_miranda, 'arthur', 'Weird features. I think I left my space-puzzle there.', '2025-04-09 08:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ariel, 'ford', 'Icy moon. Like a space-mirror.', '2025-04-09 08:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_umbriel, 'trillian', 'Darkest moon. Gloomy.', '2025-04-09 08:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titania, 'zaphod', 'Canyons. Like a space-scratch.', '2025-04-09 08:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_oberon, 'marvin', 'Old moon. How thrilling.', '2025-04-09 09:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_neptune, 'arthur', 'Windy planet. I think I left my space-kite there.', '2025-04-09 09:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_triton, 'ford', 'Cryovolcanoes. Like a space-ice cream.', '2025-04-09 09:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_nereid, 'trillian', 'Eccentric orbit. Weird.', '2025-04-09 09:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_pluto, 'zaphod', 'Heart-shaped feature. Cute.', '2025-04-09 09:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_charon, 'marvin', 'Tidally locked. How thrilling.', '2025-04-09 09:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mercury, 'arthur', 'Hottest and coldest. I think I left my space-thermos there.', '2025-04-09 09:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_venus, 'ford', 'Thick atmosphere. Like a space-sauna.', '2025-04-09 09:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_earth, 'trillian', 'The best place for tea. And towels.', '2025-04-09 09:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_moon, 'zaphod', 'One small step for man, one giant leap for boredom.', '2025-04-09 09:45:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_mars, 'marvin', 'Red dust. How exciting.', '2025-04-09 09:50:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_jupiter, 'arthur', 'Great red spot. I think I left my space-aspirin there.', '2025-04-09 09:55:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_io, 'ford', 'Volcanic moon. Like a space-furnace.', '2025-04-09 10:00:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_europa, 'trillian', 'Icy oceans. Mysterious.', '2025-04-09 10:05:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_ganymede, 'zaphod', 'Largest moon. Like a space-ball.', '2025-04-09 10:10:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_callisto, 'marvin', 'Craters. How original.', '2025-04-09 10:15:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_saturn, 'arthur', 'Rings. I think I left my space-ring toss game there.', '2025-04-09 10:20:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_titan, 'ford', 'Methane lakes. Like a space-swamp.', '2025-04-09 10:25:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_enceladus, 'trillian', 'Geysers. Refreshing.', '2025-04-09 10:30:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_iapetus, 'zaphod', 'Two-toned. Like a space-zebra.', '2025-04-09 10:35:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_rhea, 'marvin', 'Icy moon. How original.', '2025-04-09 10:40:00');
    PERFORM proc_add_comment(uuid_generate_v4(), v_dione, 'arthur', 'Ice cliffs. I think I left my space-ice pick there.', '2025-04-09 10:42:00');

END
$$;

-- Commit transaction
COMMIT;
