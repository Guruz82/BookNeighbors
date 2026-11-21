# Book Neighbors – Avvio Docker (ambiente locale)

Questi file vanno copiati nella **radice** del repository `BookNeighbors`, mantenendo le cartelle `docker/nginx/` e `docker/scripts/`. Sono necessari `Dockerfile.postgis` e la cartella `Database` già presenti nel repository.

## 1. Configurazione

Copia `.env.example` in `.env`:

```bash
cp .env.example .env
```

Modifica `POSTGRES_PASSWORD`. Inserisci le PAR Oracle in `ORACLE_READ_URL` e `ORACLE_WRITE_URL` per abilitare upload e lettura delle copertine. Non pubblicare `.env` e non metterlo nel repository. Il nome host `db` è risolto all'interno della rete Compose.

Controlla che `.gitignore` contenga queste righe:

```gitignore
.env
docker/certs/
```

## 2. Avvio

Nella radice del progetto:

```bash
docker compose up -d --build
docker compose ps
docker compose logs -f api
```

Apri **https://localhost:8443**. Nginx genera un certificato HTTPS autosigned per il solo sviluppo: il browser mostrerà un avviso perché non è attendibile. Accettalo solo in locale oppure importa una CA locale attendibile. Non usare questi certificati per un server pubblico.

## 3. Database

Alla **prima creazione del volume** PostgreSQL esegue automaticamente:

1. `Database/schema.sql`
2. `Database/Categories.sql`
3. `Database/LoanRequestStatuses.sql`

`Database/Test data.sql` resta opzionale. Per caricarlo in un database già avviato:

```bash
docker compose exec -T db psql -U bookneighbors -d bookneighbors < "Database/Test data.sql"
```

**Attenzione:** gli script di `/docker-entrypoint-initdb.d/` non vengono rieseguiti ai successivi avvii se il volume del database è già stato creato. In caso di migrazioni successive, usa EF Core migrations o uno script di aggiornamento, senza cancellare i dati.

## 4. Porte e sicurezza

- Browser: `https://localhost:8443` (Nginx e Blazor WebAssembly)
- API: stesso dominio, percorsi `/api/` e `POST /login` / `POST /logout` (reverse proxy)
- SignalR: stesso dominio, percorso `/hubs/loan-chat`
- PostgreSQL: porta 5432 solo nella rete Docker; non esposta sull'host

Questa configurazione è per **test locali**, non è un deployment di produzione. L'API opera in `Development` per facilitare il collaudo. Per produzione servono certificati attendibili, gestione sicura delle credenziali, reverse proxy con limiti/monitoraggio, protezione endpoint e configurazione di produzione.

## 5. Comandi utili

```bash
docker compose logs --tail=100 db
docker compose logs --tail=100 api
docker compose logs --tail=100 frontend
docker compose down
```

`docker compose down` mantiene il volume PostgreSQL. **Non usare `docker compose down -v`**, a meno di voler eliminare tutti i dati del database del progetto containerizzato.

## 6. Problemi possibili

- Se `docker compose build` fallisce, controlla il messaggio di `dotnet restore` o `dotnet publish`.
- Se il browser rifiuta la connessione, controlla i log del container `frontend` e la porta 8443.
- Se il login fallisce, verifica che stai usando `https://localhost:8443` (non `http`) e che il browser accetti il certificato.
- Se le API rispondono con 500, controlla `docker compose logs api` e la password PostgreSQL in `.env`.
- Se le copertine non vengono caricate, controlla le PAR Oracle e i permessi/validità; le variabili della configurazione Docker devono contenere URL completi.
