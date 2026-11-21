// Richiede la posizione soltanto quando viene chiamata la funzione.
export function getCurrentPosition() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error("Il browser non supporta la geolocalizzazione.")) ;
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => {
                resolve({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude
                });
            },
            error => {
                const messages = {
                    1: "Permesso di localizzazione negato.",
                    2: "Posizione non disponibile.",
                    3: "Tempo di rilevamento scaduto."
                };

                reject(new Error(
                    messages[error.code] ??
                    "Impossibile rilevare la posizione."
                ));
            },
            {
                enableHighAccuracy: false,
                timeout: 10000,
                maximumAge: 60000
            }
        );
    });
}