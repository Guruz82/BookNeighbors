-- Script per inserire utenti e libri di test iniziali nel database
-- DB : BookNeighbors 
-- Server: PostgreSQL 17 / PostGis

-- 10 utenti e 10 librerie fittizie, tutte in zone di Milano.
 

BEGIN;

-- Verifica prerequisiti e hash sostituiti PRIMA di inserire qualsiasi dato.
DO $$
BEGIN
    IF (SELECT COUNT(*) FROM public."Categories" WHERE "Name" IN ('Romanzi', 'Informatica', 'Gialli e thriller', 'Viaggi', 'Poesia', 'Storia', 'Filosofia', 'Fantascienza', 'Cucina e gastronomia', 'Arte e fotografia', 'Libri per bambini', 'Lingue straniere', 'Fantasy', 'Scienze', 'Fumetti', 'Economia e finanza', 'Avventura')) <> 17 THEN
        RAISE EXCEPTION 'Prima eseguire categories.sql';
    END IF;
    IF (SELECT COUNT(*) FROM public."LoanRequestStatuses" WHERE "Name" IN ('Pending', 'Accepted', 'Rejected', 'Returned', 'Cancelled')) <> 5 THEN
        RAISE EXCEPTION 'Prima eseguire loanrequeststatuses.sql';
    END IF;
END $$;

-- 1. Utenti fittizi Identity. Sostituire gli hash nel blocco VALUES.
CREATE TEMP TABLE demo_identity_users (
    id uuid NOT NULL, username text NOT NULL, email text NOT NULL, display_name text NOT NULL, password_hash text NOT NULL
) ON COMMIT DROP;


INSERT INTO demo_identity_users (id, username, email, display_name, password_hash)
VALUES
    ('10000000-0000-4000-8000-000000000001', 'demo.milano01@bookneighbors.example', 'demo.milano01@bookneighbors.example', 'Alice (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000002', 'demo.milano02@bookneighbors.example', 'demo.milano02@bookneighbors.example', 'Bruno (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000003', 'demo.milano03@bookneighbors.example', 'demo.milano03@bookneighbors.example', 'Carla (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000004', 'demo.milano04@bookneighbors.example', 'demo.milano04@bookneighbors.example', 'Davide (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000005', 'demo.milano05@bookneighbors.example', 'demo.milano05@bookneighbors.example', 'Elena (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000006', 'demo.milano06@bookneighbors.example', 'demo.milano06@bookneighbors.example', 'Fabio (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000007', 'demo.milano07@bookneighbors.example', 'demo.milano07@bookneighbors.example', 'Giulia (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000008', 'demo.milano08@bookneighbors.example', 'demo.milano08@bookneighbors.example', 'Luca (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000009', 'demo.milano09@bookneighbors.example', 'demo.milano09@bookneighbors.example', 'Marta (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w=='),
    ('10000000-0000-4000-8000-000000000010', 'demo.milano10@bookneighbors.example', 'demo.milano10@bookneighbors.example', 'Paolo (demo)', 'AQAAAAIAAYagAAAAEOHHz2WKn3iaxCIZrS6qzw6bc/3Uo/V7CYJjyB5afEjwysCD0YdRf5eSrdNpXCXC3w==');

 

INSERT INTO public."AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
    "DisplayName", "PrivacyNoticeAcknowledged", "PrivacyNoticeAcknowledgedAt",
    "CreatedAt", "UpdatedAt", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
)
SELECT id, username, upper(username), email, upper(email), TRUE, display_name, TRUE, NOW(),
       NOW() - INTERVAL '60 days', NULL, password_hash, gen_random_uuid()::text, gen_random_uuid()::text, FALSE, FALSE, TRUE, 0
FROM demo_identity_users
ON CONFLICT ("Id") DO NOTHING;

-- 2. Librerie geolocalizzate (coordinate approssimative nel quartiere, non indirizzi privati).
INSERT INTO public."Libraries" ("Id", user_id, "Name", "City", "AreaName", "Location", "LocationConsent", "IsPublished", "CreatedAt", "UpdatedAt")
VALUES
    ('20000000-0000-4000-8000-000000000001', '10000000-0000-4000-8000-000000000001', 'Libreria di Alice (demo)', 'Milano', 'Porta Venezia', ST_SetSRID(ST_MakePoint(9.207, 45.474), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000002', '10000000-0000-4000-8000-000000000002', 'Libreria di Bruno (demo)', 'Milano', 'Isola', ST_SetSRID(ST_MakePoint(9.190, 45.488), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000003', '10000000-0000-4000-8000-000000000003', 'Libreria di Carla (demo)', 'Milano', 'Navigli', ST_SetSRID(ST_MakePoint(9.178, 45.451), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000004', '10000000-0000-4000-8000-000000000004', 'Libreria di Davide (demo)', 'Milano', 'Brera', ST_SetSRID(ST_MakePoint(9.187, 45.473), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000005', '10000000-0000-4000-8000-000000000005', 'Libreria di Elena (demo)', 'Milano', 'Città Studi', ST_SetSRID(ST_MakePoint(9.225, 45.479), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000006', '10000000-0000-4000-8000-000000000006', 'Libreria di Fabio (demo)', 'Milano', 'Porta Romana', ST_SetSRID(ST_MakePoint(9.201, 45.454), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000007', '10000000-0000-4000-8000-000000000007', 'Libreria di Giulia (demo)', 'Milano', 'Bovisa', ST_SetSRID(ST_MakePoint(9.164, 45.500), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000008', '10000000-0000-4000-8000-000000000008', 'Libreria di Luca (demo)', 'Milano', 'Lambrate', ST_SetSRID(ST_MakePoint(9.242, 45.484), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000009', '10000000-0000-4000-8000-000000000009', 'Libreria di Marta (demo)', 'Milano', 'Bicocca', ST_SetSRID(ST_MakePoint(9.216, 45.517), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL),
    ('20000000-0000-4000-8000-000000000010', '10000000-0000-4000-8000-000000000010', 'Libreria di Paolo (demo)', 'Milano', 'San Siro', ST_SetSRID(ST_MakePoint(9.145, 45.478), 4326)::geography, TRUE, TRUE, NOW() - INTERVAL '55 days', NULL)
ON CONFLICT ("Id") DO NOTHING;

-- 3. Venti libri fittizi, due per libreria; immagini nulle (il segnaposto frontend le gestisce).

DO $$
DECLARE
    demo_image_url TEXT := 'demo/default-cover.jpg';
BEGIN
INSERT INTO public."Books" ("Id", library_id, "Title", "Author", "Description", "ISBN", "ImageUrl", "ThumbnailUrl", "IsPublished", "PublicationYear", "CreatedAt", "UpdatedAt")
VALUES
    ('30000000-0000-4000-8000-000000000001', '20000000-0000-4000-8000-000000000001', 'La biblioteca nascosta', 'Anna Rossi', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2020, NOW() - INTERVAL '50 days', NULL),
    ('30000000-0000-4000-8000-000000000002', '20000000-0000-4000-8000-000000000001', 'Manuale di basi di dati', 'Marco Ferri', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2023, NOW() - INTERVAL '49 days', NULL),
    ('30000000-0000-4000-8000-000000000003', '20000000-0000-4000-8000-000000000002', 'Un mistero in città', 'Laura Conti', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2019, NOW() - INTERVAL '49 days', NULL),
    ('30000000-0000-4000-8000-000000000004', '20000000-0000-4000-8000-000000000002', 'La guida del viaggiatore', 'Paolo Neri', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2022, NOW() - INTERVAL '48 days', NULL),
    ('30000000-0000-4000-8000-000000000005', '20000000-0000-4000-8000-000000000003', 'Cronache sui Navigli', 'Giulia Moretti', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2021, NOW() - INTERVAL '48 days', NULL),
    ('30000000-0000-4000-8000-000000000006', '20000000-0000-4000-8000-000000000003', 'Pagine di poesia', 'Elisa Bianchi', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2018, NOW() - INTERVAL '47 days', NULL),
    ('30000000-0000-4000-8000-000000000007', '20000000-0000-4000-8000-000000000004', 'Milano nella storia', 'Alessandro Villa', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2017, NOW() - INTERVAL '47 days', NULL),
    ('30000000-0000-4000-8000-000000000008', '20000000-0000-4000-8000-000000000004', 'Introduzione alla filosofia', 'Francesca Riva', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2024, NOW() - INTERVAL '46 days', NULL),
    ('30000000-0000-4000-8000-000000000009', '20000000-0000-4000-8000-000000000005', 'Le stelle lontane', 'Roberto Costa', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2020, NOW() - INTERVAL '46 days', NULL),
    ('30000000-0000-4000-8000-000000000010', '20000000-0000-4000-8000-000000000005', 'Sistemi e reti', 'Davide Greco', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2022, NOW() - INTERVAL '45 days', NULL),
    ('30000000-0000-4000-8000-000000000011', '20000000-0000-4000-8000-000000000006', 'Sapori della tradizione', 'Martina Sala', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2021, NOW() - INTERVAL '45 days', NULL),
    ('30000000-0000-4000-8000-000000000012', '20000000-0000-4000-8000-000000000006', 'Racconti di quartiere', 'Chiara Fontana', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2016, NOW() - INTERVAL '44 days', NULL),
    ('30000000-0000-4000-8000-000000000013', '20000000-0000-4000-8000-000000000007', 'Disegnare la città', 'Federico Galli', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2023, NOW() - INTERVAL '44 days', NULL),
    ('30000000-0000-4000-8000-000000000014', '20000000-0000-4000-8000-000000000007', 'Il piccolo astronauta', 'Silvia Colombo', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2020, NOW() - INTERVAL '43 days', NULL),
    ('30000000-0000-4000-8000-000000000015', '20000000-0000-4000-8000-000000000008', 'Linguaggi e parole', 'Andrea Marchetti', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2019, NOW() - INTERVAL '43 days', NULL),
    ('30000000-0000-4000-8000-000000000016', '20000000-0000-4000-8000-000000000008', 'La montagna incantata', 'Valentina Rizzo', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2021, NOW() - INTERVAL '42 days', NULL),
    ('30000000-0000-4000-8000-000000000017', '20000000-0000-4000-8000-000000000009', 'Storie di scienza', 'Michele Russo', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2018, NOW() - INTERVAL '42 days', NULL),
    ('30000000-0000-4000-8000-000000000018', '20000000-0000-4000-8000-000000000009', 'Fumetti in viaggio', 'Sofia Lombardi', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2022, NOW() - INTERVAL '41 days', NULL),
    ('30000000-0000-4000-8000-000000000019', '20000000-0000-4000-8000-000000000010', 'Economia quotidiana', 'Pietro Marino', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2024, NOW() - INTERVAL '41 days', NULL),
    ('30000000-0000-4000-8000-000000000020', '20000000-0000-4000-8000-000000000010', 'Il sentiero degli enigmi', 'Irene Romano', 'Volume dimostrativo per il collaudo di Book Neighbors.', NULL, demo_image_url, demo_image_url, TRUE, 2020, NOW() - INTERVAL '40 days', NULL)
ON CONFLICT ("Id") DO NOTHING;
END $$;
-- 4. Categorie per ciascun libro (relazione molti-a-molti).
INSERT INTO public."BookCategories" ("BookId", "CategoryId")
SELECT v.book_id::uuid, c."Id"
FROM (VALUES
    ('30000000-0000-4000-8000-000000000001', 'Romanzi'),
    ('30000000-0000-4000-8000-000000000002', 'Informatica'),
    ('30000000-0000-4000-8000-000000000003', 'Gialli e thriller'),
    ('30000000-0000-4000-8000-000000000004', 'Viaggi'),
    ('30000000-0000-4000-8000-000000000005', 'Romanzi'),
    ('30000000-0000-4000-8000-000000000006', 'Poesia'),
    ('30000000-0000-4000-8000-000000000007', 'Storia'),
    ('30000000-0000-4000-8000-000000000008', 'Filosofia'),
    ('30000000-0000-4000-8000-000000000009', 'Fantascienza'),
    ('30000000-0000-4000-8000-000000000010', 'Informatica'),
    ('30000000-0000-4000-8000-000000000011', 'Cucina e gastronomia'),
    ('30000000-0000-4000-8000-000000000012', 'Romanzi'),
    ('30000000-0000-4000-8000-000000000013', 'Arte e fotografia'),
    ('30000000-0000-4000-8000-000000000014', 'Libri per bambini'),
    ('30000000-0000-4000-8000-000000000015', 'Lingue straniere'),
    ('30000000-0000-4000-8000-000000000016', 'Fantasy'),
    ('30000000-0000-4000-8000-000000000017', 'Scienze'),
    ('30000000-0000-4000-8000-000000000018', 'Fumetti'),
    ('30000000-0000-4000-8000-000000000019', 'Economia e finanza'),
    ('30000000-0000-4000-8000-000000000020', 'Avventura')) AS v(book_id, category_name)
JOIN public."Categories" c ON c."Name" = v.category_name
ON CONFLICT ("BookId", "CategoryId") DO NOTHING;

-- 5. Richieste di prestito (richiedente diverso dal proprietario del libro).
INSERT INTO public."LoanRequests" ("Id", book_id, requester_id, status_id, "DueDate", "CreatedAt", "UpdatedAt")
SELECT v.id::uuid, v.book_id::uuid, v.requester_id::uuid, s."Id", v.due_date, v.created_at, v.updated_at
FROM (VALUES
    ('40000000-0000-4000-8000-000000000001', '30000000-0000-4000-8000-000000000001', '10000000-0000-4000-8000-000000000002', 'Pending', NULL::timestamptz, NOW() - INTERVAL '2 days', NULL::timestamptz),
    ('40000000-0000-4000-8000-000000000002', '30000000-0000-4000-8000-000000000003', '10000000-0000-4000-8000-000000000001', 'Accepted', NOW() + INTERVAL '10 days', NOW() - INTERVAL '8 days', NOW() - INTERVAL '7 days'),
    ('40000000-0000-4000-8000-000000000003', '30000000-0000-4000-8000-000000000005', '10000000-0000-4000-8000-000000000004', 'Returned', NOW() - INTERVAL '3 days', NOW() - INTERVAL '20 days', NOW() - INTERVAL '5 days'),
    ('40000000-0000-4000-8000-000000000004', '30000000-0000-4000-8000-000000000007', '10000000-0000-4000-8000-000000000005', 'Rejected', NULL::timestamptz, NOW() - INTERVAL '12 days', NOW() - INTERVAL '11 days'),
    ('40000000-0000-4000-8000-000000000005', '30000000-0000-4000-8000-000000000009', '10000000-0000-4000-8000-000000000006', 'Cancelled', NULL::timestamptz, NOW() - INTERVAL '6 days', NOW() - INTERVAL '5 days'),
    ('40000000-0000-4000-8000-000000000006', '30000000-0000-4000-8000-000000000011', '10000000-0000-4000-8000-000000000007', 'Pending', NULL::timestamptz, NOW() - INTERVAL '1 days', NULL::timestamptz),
    ('40000000-0000-4000-8000-000000000007', '30000000-0000-4000-8000-000000000013', '10000000-0000-4000-8000-000000000008', 'Accepted', NOW() + INTERVAL '12 days', NOW() - INTERVAL '9 days', NOW() - INTERVAL '8 days'),
    ('40000000-0000-4000-8000-000000000008', '30000000-0000-4000-8000-000000000015', '10000000-0000-4000-8000-000000000009', 'Returned', NOW() - INTERVAL '2 days', NOW() - INTERVAL '24 days', NOW() - INTERVAL '3 days'),
    ('40000000-0000-4000-8000-000000000009', '30000000-0000-4000-8000-000000000017', '10000000-0000-4000-8000-000000000010', 'Pending', NULL::timestamptz, NOW() - INTERVAL '4 days', NULL::timestamptz),
    ('40000000-0000-4000-8000-000000000010', '30000000-0000-4000-8000-000000000019', '10000000-0000-4000-8000-000000000001', 'Accepted', NOW() + INTERVAL '14 days', NOW() - INTERVAL '11 days', NOW() - INTERVAL '10 days'),
    ('40000000-0000-4000-8000-000000000011', '30000000-0000-4000-8000-000000000002', '10000000-0000-4000-8000-000000000003', 'Rejected', NULL::timestamptz, NOW() - INTERVAL '15 days', NOW() - INTERVAL '13 days'),
    ('40000000-0000-4000-8000-000000000012', '30000000-0000-4000-8000-000000000004', '10000000-0000-4000-8000-000000000006', 'Returned', NOW() - INTERVAL '8 days', NOW() - INTERVAL '30 days', NOW() - INTERVAL '10 days')) AS v(id, book_id, requester_id, status_name, due_date, created_at, updated_at)
JOIN public."LoanRequestStatuses" s ON s."Name" = v.status_name
ON CONFLICT ("Id") DO NOTHING;

-- 6. Visualizzazioni per statistiche (due per libro).
INSERT INTO public."BookViews" ("Id", book_id, "ViewedAt")
VALUES
    ('50000000-0000-4000-8000-000000000001', '30000000-0000-4000-8000-000000000001', NOW() - INTERVAL '2 days'),
    ('50000000-0000-4000-8000-000000000002', '30000000-0000-4000-8000-000000000001', NOW() - INTERVAL '3 days'),
    ('50000000-0000-4000-8000-000000000003', '30000000-0000-4000-8000-000000000002', NOW() - INTERVAL '4 days'),
    ('50000000-0000-4000-8000-000000000004', '30000000-0000-4000-8000-000000000002', NOW() - INTERVAL '5 days'),
    ('50000000-0000-4000-8000-000000000005', '30000000-0000-4000-8000-000000000003', NOW() - INTERVAL '6 days'),
    ('50000000-0000-4000-8000-000000000006', '30000000-0000-4000-8000-000000000003', NOW() - INTERVAL '7 days'),
    ('50000000-0000-4000-8000-000000000007', '30000000-0000-4000-8000-000000000004', NOW() - INTERVAL '8 days'),
    ('50000000-0000-4000-8000-000000000008', '30000000-0000-4000-8000-000000000004', NOW() - INTERVAL '9 days'),
    ('50000000-0000-4000-8000-000000000009', '30000000-0000-4000-8000-000000000005', NOW() - INTERVAL '10 days'),
    ('50000000-0000-4000-8000-000000000010', '30000000-0000-4000-8000-000000000005', NOW() - INTERVAL '11 days'),
    ('50000000-0000-4000-8000-000000000011', '30000000-0000-4000-8000-000000000006', NOW() - INTERVAL '12 days'),
    ('50000000-0000-4000-8000-000000000012', '30000000-0000-4000-8000-000000000006', NOW() - INTERVAL '13 days'),
    ('50000000-0000-4000-8000-000000000013', '30000000-0000-4000-8000-000000000007', NOW() - INTERVAL '14 days'),
    ('50000000-0000-4000-8000-000000000014', '30000000-0000-4000-8000-000000000007', NOW() - INTERVAL '15 days'),
    ('50000000-0000-4000-8000-000000000015', '30000000-0000-4000-8000-000000000008', NOW() - INTERVAL '16 days'),
    ('50000000-0000-4000-8000-000000000016', '30000000-0000-4000-8000-000000000008', NOW() - INTERVAL '17 days'),
    ('50000000-0000-4000-8000-000000000017', '30000000-0000-4000-8000-000000000009', NOW() - INTERVAL '18 days'),
    ('50000000-0000-4000-8000-000000000018', '30000000-0000-4000-8000-000000000009', NOW() - INTERVAL '19 days'),
    ('50000000-0000-4000-8000-000000000019', '30000000-0000-4000-8000-000000000010', NOW() - INTERVAL '20 days'),
    ('50000000-0000-4000-8000-000000000020', '30000000-0000-4000-8000-000000000010', NOW() - INTERVAL '21 days'),
    ('50000000-0000-4000-8000-000000000021', '30000000-0000-4000-8000-000000000011', NOW() - INTERVAL '22 days'),
    ('50000000-0000-4000-8000-000000000022', '30000000-0000-4000-8000-000000000011', NOW() - INTERVAL '1 days'),
    ('50000000-0000-4000-8000-000000000023', '30000000-0000-4000-8000-000000000012', NOW() - INTERVAL '2 days'),
    ('50000000-0000-4000-8000-000000000024', '30000000-0000-4000-8000-000000000012', NOW() - INTERVAL '3 days'),
    ('50000000-0000-4000-8000-000000000025', '30000000-0000-4000-8000-000000000013', NOW() - INTERVAL '4 days'),
    ('50000000-0000-4000-8000-000000000026', '30000000-0000-4000-8000-000000000013', NOW() - INTERVAL '5 days'),
    ('50000000-0000-4000-8000-000000000027', '30000000-0000-4000-8000-000000000014', NOW() - INTERVAL '6 days'),
    ('50000000-0000-4000-8000-000000000028', '30000000-0000-4000-8000-000000000014', NOW() - INTERVAL '7 days'),
    ('50000000-0000-4000-8000-000000000029', '30000000-0000-4000-8000-000000000015', NOW() - INTERVAL '8 days'),
    ('50000000-0000-4000-8000-000000000030', '30000000-0000-4000-8000-000000000015', NOW() - INTERVAL '9 days'),
    ('50000000-0000-4000-8000-000000000031', '30000000-0000-4000-8000-000000000016', NOW() - INTERVAL '10 days'),
    ('50000000-0000-4000-8000-000000000032', '30000000-0000-4000-8000-000000000016', NOW() - INTERVAL '11 days'),
    ('50000000-0000-4000-8000-000000000033', '30000000-0000-4000-8000-000000000017', NOW() - INTERVAL '12 days'),
    ('50000000-0000-4000-8000-000000000034', '30000000-0000-4000-8000-000000000017', NOW() - INTERVAL '13 days'),
    ('50000000-0000-4000-8000-000000000035', '30000000-0000-4000-8000-000000000018', NOW() - INTERVAL '14 days'),
    ('50000000-0000-4000-8000-000000000036', '30000000-0000-4000-8000-000000000018', NOW() - INTERVAL '15 days'),
    ('50000000-0000-4000-8000-000000000037', '30000000-0000-4000-8000-000000000019', NOW() - INTERVAL '16 days'),
    ('50000000-0000-4000-8000-000000000038', '30000000-0000-4000-8000-000000000019', NOW() - INTERVAL '17 days'),
    ('50000000-0000-4000-8000-000000000039', '30000000-0000-4000-8000-000000000020', NOW() - INTERVAL '18 days'),
    ('50000000-0000-4000-8000-000000000040', '30000000-0000-4000-8000-000000000020', NOW() - INTERVAL '19 days')
ON CONFLICT ("Id") DO NOTHING;

-- 7. Messaggi su prestiti esistenti (per testare la chat).
INSERT INTO public."LoanMessages" ("Id", "LoanRequestId", "SenderId", "Text", "CreatedAt")
VALUES
    ('60000000-0000-4000-8000-000000000001', '40000000-0000-4000-8000-000000000002', '10000000-0000-4000-8000-000000000001', 'Ciao, posso prendere in prestito il libro?', NOW() - INTERVAL '7 days'),
    ('60000000-0000-4000-8000-000000000002', '40000000-0000-4000-8000-000000000002', '10000000-0000-4000-8000-000000000002', 'Certamente, ci organizziamo!', NOW() - INTERVAL '6 days'),
    ('60000000-0000-4000-8000-000000000003', '40000000-0000-4000-8000-000000000003', '10000000-0000-4000-8000-000000000004', 'Ho terminato il libro, grazie.', NOW() - INTERVAL '4 days'),
    ('60000000-0000-4000-8000-000000000004', '40000000-0000-4000-8000-000000000007', '10000000-0000-4000-8000-000000000008', 'Possiamo concordare il ritiro?', NOW() - INTERVAL '4 days'),
    ('60000000-0000-4000-8000-000000000005', '40000000-0000-4000-8000-000000000010', '10000000-0000-4000-8000-000000000001', 'Quando sarebbe disponibile?', NOW() - INTERVAL '3 days')
ON CONFLICT ("Id") DO NOTHING;

-- 8. Notifiche di esempio.
INSERT INTO public."Notifications" ("Id", "UserId", "Title", "Message", "Url", "IsRead", "CreatedAt", "ReadAt")
VALUES
    ('70000000-0000-4000-8000-000000000001', '10000000-0000-4000-8000-000000000001', 'Nuova richiesta', 'Hai ricevuto una richiesta di prestito.', '/prestiti', TRUE, NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 days'),
    ('70000000-0000-4000-8000-000000000002', '10000000-0000-4000-8000-000000000002', 'Prestito confermato', 'Il proprietario ha accettato il prestito.', '/prestiti', FALSE, NOW() - INTERVAL '7 days', NULL),
    ('70000000-0000-4000-8000-000000000003', '10000000-0000-4000-8000-000000000003', 'Prestito restituito', 'Il libro è stato restituito.', '/prestiti', TRUE, NOW() - INTERVAL '4 days', NOW() - INTERVAL '3 days'),
    ('70000000-0000-4000-8000-000000000004', '10000000-0000-4000-8000-000000000004', 'Nuova richiesta', 'Hai ricevuto una richiesta.', '/prestiti', FALSE, NOW() - INTERVAL '1 days', NULL),
    ('70000000-0000-4000-8000-000000000005', '10000000-0000-4000-8000-000000000005', 'Nuovo prestito', 'Hai una nuova richiesta di prestito.', '/prestiti', FALSE, NOW() - INTERVAL '3 days', NULL)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
