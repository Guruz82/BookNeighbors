# Book Neighbors

Book Neighbors è un prototipo di applicazione web sviluppato per il Project Work PW14 del corso di laurea L-31 dell'Università Pegaso.

L'applicazione permette agli utenti di condividere i libri delle proprie librerie personali e di cercare quelli disponibili nelle vicinanze, utilizzando una mappa e la ricerca geografica.

## Funzionalità principali

- Registrazione, autenticazione e gestione del profilo utente.
- Creazione e pubblicazione di una libreria personale con posizione geografica approssimativa.
- Inserimento, modifica e gestione dei libri e delle relative categorie.
- Recupero dei metadati bibliografici tramite ISBN con Google Books e Open Library.
- Caricamento delle copertine e generazione automatica delle miniature.
- Ricerca per titolo, autore, ISBN, categoria e distanza geografica, con visualizzazione su mappa.
- Visualizzazione del dettaglio dei libri e gestione delle richieste di prestito.
- Messaggistica tra proprietario e richiedente, con aggiornamento tramite SignalR.
- Notifiche applicative, statistiche personali e dashboard amministrativa.

## Tecnologie utilizzate

- **Front-end:** Blazor WebAssembly, Bootstrap, Leaflet, JavaScript e Chart.js.
- **Back-end:** ASP.NET Core Web API, .NET 10, ASP.NET Core Identity e SignalR.
- **Database:** PostgreSQL 17, PostGIS, Entity Framework Core e NetTopologySuite.
- **Immagini:** SkiaSharp e Oracle Cloud Object Storage.
- **Ambiente di sviluppo:** Visual Studio Code, Git e Docker.

## Struttura del repository

| Percorso | Contenuto |
| --- | --- |
| `BookNeighbors.Client/` | Interfaccia web sviluppata con Blazor WebAssembly. |
| `BookNeighbors.API/` | API, servizi applicativi, autenticazione e accesso al database. |
| `BookNeighbors.Common/` | DTO condivisi tra client e API. |
| `Database/` | Script SQL per la struttura del database e i dati iniziali. |
| `BookNeighbors.sln` | Soluzione .NET. |

## Database

È necessario predisporre un database PostgreSQL con l'estensione PostGIS disponibile.

Gli script presenti nella cartella [`Database`](Database/) vanno eseguiti nell'ordine seguente:

1. `schema.sql`: crea lo schema, le tabelle, le relazioni, gli indici e l'estensione PostGIS.
2. `Categories.sql` e `LoanRequestStatuses.sql`: inseriscono le categorie e gli stati delle richieste di prestito.
3. `Test data.sql` (**facoltativo**): inserisce utenti, librerie, libri, prestiti, visualizzazioni, messaggi e notifiche dimostrativi.

Per ulteriori dettagli consultare [`Database/Readme.md`](Database/Readme.md).

## Configurazione e avvio

Per eseguire il progetto in locale sono necessari l'SDK **.NET 10** e un'istanza accessibile di PostgreSQL con PostGIS.

La configurazione privata del back-end deve includere:

- `ConnectionStrings:DefaultConnection`: stringa di connessione PostgreSQL.
- `AppOptions:OracleStorage:ReadUrl`: URL PAR per la lettura delle immagini.
- `AppOptions:OracleStorage:WriteUrl`: URL PAR per il caricamento delle immagini.

Per lo sviluppo locale queste impostazioni possono essere conservate nei **.NET User Secrets** del progetto `BookNeighbors.API`. Le credenziali e gli URL PAR non devono essere pubblicati nel repository.

Nel file `BookNeighbors.Client/wwwroot/appsettings.json` la proprietà `AppOptions:Services:ApiBaseUrl` deve indicare l'indirizzo effettivo delle API. Il back-end deve inoltre consentire l'origine del client tramite `AppOptions:Client:AllowedOrigins`.

Dalla cartella principale del repository, dopo aver configurato il database e le impostazioni dell'ambiente, è possibile avviare i due progetti in terminali separati:

```bash
dotnet run --project BookNeighbors.API
```

```bash
dotnet run --project BookNeighbors.Client
```

Gli indirizzi e le porte effettive dipendono dalla configurazione locale di avvio. Per il caricamento delle immagini è necessario configurare un bucket Oracle Object Storage e le relative PAR.

## Note sul prototipo

La posizione delle librerie viene memorizzata in forma approssimativa per limitare l'esposizione dei dati geografici degli utenti. La consegna e la restituzione fisica dei libri sono organizzate direttamente tra gli utenti.

Il progetto è stato realizzato a scopo didattico.

## Avvio dell'applicazione con Docker

Book Neighbors può essere eseguito tramite Docker, senza dover installare manualmente .NET, PostgreSQL o PostGIS.

L'ambiente è composto da tre container:

- **Database:** PostgreSQL 17 con estensione PostGIS.
- **API:** ASP.NET Core .NET 10, che gestisce autenticazione, prestiti, dati e notifiche.
- **Frontend:** Blazor WebAssembly distribuito tramite Nginx, utilizzato anche come reverse proxy per le API e SignalR.

### Prerequisiti

È necessario avere installato Docker Desktop oppure Docker Engine con Docker Compose.

### Configurazione iniziale

Dalla cartella principale del repository, dove si trova `docker-compose.yml`, creare il file `.env` copiando quello di esempio:

```bash
cp .env.example .env
```

Aprire il file `.env` e configurare le seguenti variabili:

```dotenv
POSTGRES_PASSWORD=INSERIRE_PASSWORD
PUBLIC_URL=https://localhost:8443
ORACLE_READ_URL=
ORACLE_WRITE_URL=
BOOTSTRAP_ADMIN_EMAIL=
```

`POSTGRES_PASSWORD` rappresenta la password utilizzata dal database PostgreSQL.

`ORACLE_READ_URL` e `ORACLE_WRITE_URL` devono contenere le Pre-Authenticated Requests (PAR) configurate su Oracle Cloud Object Storage per la lettura e il caricamento delle copertine.

`BOOTSTRAP_ADMIN_EMAIL` è facoltativo e permette di assegnare il ruolo amministratore a un utente già registrato, nell'ambiente di sviluppo.

Il file `.env` contiene informazioni riservate ed è escluso dal repository tramite `.gitignore`.

### Primo avvio

Verificare la configurazione Docker:

```bash
docker compose config --quiet
```

Compilare le immagini e avviare i container:

```bash
docker compose up -d --build
```

Verificare lo stato dei servizi:

```bash
docker compose ps
```

Quando i container risultano avviati, l'applicazione è raggiungibile all'indirizzo:

**https://localhost:8443**

Per l'ambiente locale viene utilizzato un certificato HTTPS autofirmato, che potrebbe generare un avviso nel browser.

### Inizializzazione del database

Al primo avvio, quando il volume PostgreSQL è vuoto, vengono eseguiti automaticamente gli script presenti nella cartella `Database`, nel seguente ordine:

1. `schema.sql` — Creazione delle tabelle, delle relazioni, degli indici e dell'estensione PostGIS.
2. `Categories.sql` — Inserimento delle categorie dei libri.
3. `LoanRequestStatuses.sql` — Inserimento degli stati delle richieste di prestito.

Per caricare anche i dati dimostrativi è possibile eseguire:

```bash
docker compose exec -T db psql -v ON_ERROR_STOP=1 -U bookneighbors -d bookneighbors < "Database/Test data.sql"
```

Lo script aggiunge utenti, librerie, libri, richieste di prestito, messaggi, notifiche e visualizzazioni utilizzati per i test.

**Gli script iniziali vengono eseguiti automaticamente soltanto alla creazione di un database vuoto.**

### Aggiornamento del frontend

Dopo aver modificato il codice Blazor WebAssembly, per ricompilare e aggiornare soltanto il frontend eseguire:

```bash
docker compose up -d --build --no-deps frontend
```

Questo comando non ricrea il database e non riavvia le API.

### Aggiornamento del backend

Per ricompilare e aggiornare soltanto le API ASP.NET Core:

```bash
docker compose up -d --build --no-deps api
```

Il container PostgreSQL e i dati presenti nel volume rimangono invariati.

Eventuali modifiche allo schema del database devono essere gestite separatamente attraverso le migrazioni di Entity Framework Core.

### Gestione dei container

Per visualizzare lo stato dei container:

```bash
docker compose ps
```

Per visualizzare i log:

```bash
docker compose logs -f
```

Per visualizzare soltanto i log delle API:

```bash
docker compose logs -f api
```

Per arrestare i container senza eliminare i dati:

```bash
docker compose down
```

Per riavviare l'ambiente:

```bash
docker compose up -d
```

Il database utilizza un volume Docker persistente chiamato `postgres_data`, che permette di conservare i dati anche quando i container vengono arrestati o ricreati.

**Attenzione:** il comando `docker compose down -v` elimina anche il volume del database, causando la perdita dei dati memorizzati.

### Note

La configurazione Docker è pensata per il collaudo locale. Il frontend è esposto sulla porta `8443` dell'indirizzo `127.0.0.1`, mentre API e database sono raggiungibili soltanto attraverso la rete interna Docker.

Per una pubblicazione su Internet sarebbe necessario configurare un dominio, un certificato HTTPS attendibile e le opportune impostazioni di sicurezza.


### Utenti dimostrativi

Lo script `Database/Test data.sql` crea 10 utenti dimostrativi, ciascuno con una libreria personale e alcuni libri, per permettere di testare le principali funzionalità dell'applicazione.

Tutti gli account utilizzano la stessa password:

**Password: `BookTest#2026`**

| Utente | Email |
|---|---|
| Alice | demo.milano01@bookneighbors.example |
| Bruno | demo.milano02@bookneighbors.example |
| Carla | demo.milano03@bookneighbors.example |
| Davide | demo.milano04@bookneighbors.example |
| Elena | demo.milano05@bookneighbors.example |
| Fabio | demo.milano06@bookneighbors.example |
| Giulia | demo.milano07@bookneighbors.example |
| Luca | demo.milano08@bookneighbors.example |
| Marta | demo.milano09@bookneighbors.example |
| Paolo | demo.milano10@bookneighbors.example |

Gli utenti possono essere utilizzati per verificare la ricerca dei libri, le richieste di prestito, la gestione degli stati, la messaggistica e le notifiche.

Gli account dimostrativi sono disponibili soltanto dopo aver eseguito lo script `Test data.sql` e sono destinati esclusivamente all'ambiente di sviluppo e collaudo.

**Attenzione:** le credenziali sono pubbliche e non devono essere utilizzate in un ambiente di produzione.