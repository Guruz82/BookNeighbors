-- Script per inserire le Categorie iniziali del catalogo
-- DB : BookNeighbors 
-- Server: PostgreSQL 17 / PostGis


BEGIN;
INSERT INTO public."Categories" ("Id", "Name", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), v.name, NOW(), NULL
FROM (VALUES
    ('Romanzi'),
    ('Gialli e thriller'),
    ('Fantascienza'),
    ('Fantasy'),
    ('Saggi'),
    ('Biografie'),
    ('Storia'),
    ('Informatica'),
    ('Libri per bambini'),
    ('Fumetti'),
    ('Poesia'),
    ('Arte e fotografia'),
    ('Viaggi'),
    ('Cucina e gastronomia'),
    ('Salute e benessere'),
    ('Sport'),
    ('Musica'),
    ('Cinema e teatro'),
    ('Religione e spiritualità'),
    ('Filosofia'),
    ('Scienze'),
    ('Economia e finanza'),
    ('Politica'),
    ('Educazione e pedagogia'),
    ('Lingue straniere'),
    ('Manuali e guide pratiche'),
    ('Hobby e tempo libero'),
    ('Collezionismo'),
    ('Horror'),
    ('Avventura'),
    ('Dramma'),
    ('Commedia'),
    ('Satira')
 
)  as v(name)
ON CONFLICT ("Name") DO NOTHING;
COMMIT;