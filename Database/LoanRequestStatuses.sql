-- Script per inserire gli Stati delle Richieste di Prestito iniziali del catalogo
-- DB : BookNeighbors 
-- Server: PostgreSQL 17 / PostGis

BEGIN;
INSERT INTO public."LoanRequestStatuses" ("Id", "Name", "CreatedAt", "UpdatedAt")
SELECT gen_random_uuid(), v.name, NOW(), NULL
FROM (VALUES
    ('Pending'),
    ('Accepted'),
    ('Rejected'),
    ('Returned'),
    ('Cancelled')
) AS v(name)
ON CONFLICT ("Name") DO NOTHING;
COMMIT;