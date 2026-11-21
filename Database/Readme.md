Gli script devono essere eseguiti nel seguente ordine:

1- schema.sql – Crea le tabelle, le relazioni, gli indici e l'estensione PostGIS.

2- Categories.sql e LoanRequestStatuses.sql – Inseriscono le categorie dei libri e gli stati delle richieste di prestito.

3- Test data.sql – Inserisce i dati dimostrativi utilizzati per i test, come utenti, librerie, libri, prestiti, visualizzazioni, messaggi e notifiche.

L'ultimo script è facoltativo e permette di provare le funzionalità dell'applicazione senza dover inserire manualmente tutti i dati.  