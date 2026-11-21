
const instances = new WeakMap();
// Aggiorna l'area di ricerca sulla mappa in base al raggio specificato
function updateRadius(state) {
    const L = window.L;

    if (!state.marker || !state.radiusKm || state.radiusKm <= 0) {
        if (state.circle) {
            state.map.removeLayer(state.circle);
            state.circle = null;
        }
        return;
    }

    const position = state.marker.getLatLng();

    if (!state.circle) {
        state.circle = L.circle(position, {
            radius: state.radiusKm * 1000
        }).addTo(state.map);
    } else {
        state.circle.setLatLng(position);
        state.circle.setRadius(state.radiusKm * 1000);
    }
}
// Imposta la posizione del marker sulla mappa e aggiorna il raggio se necessario
function setPosition(state, latitude, longitude, moveMap = false) {
    const L = window.L;
    const position = [latitude, longitude];
    // Se il marker non esiste, crealo e aggiungilo alla mappa   
    if (!state.marker) {
        state.marker = L.marker(position, 
        {
            draggable: state.editable,
            keyboard: false,
            alt: "Posizione selezionata sulla mappa",
            title: "Posizione selezionata"
        }).addTo(state.map);
        // Aggiungi un listener per l'evento "dragend" se il marker è modificabile      
        if (state.editable) {
            state.marker.on("dragend", () => {
                const p = state.marker.getLatLng();

                setPosition(state, p.lat, p.lng);

                state.dotNet.invokeMethodAsync(
                    "PositionChangedFromJs",
                    p.lat,
                    p.lng
                );
            });
        }
    } else {
        state.marker.setLatLng(position);
    }

    if (moveMap) {
        state.map.panTo(position);
    }

    state.latitude = latitude;
    state.longitude = longitude;

    updateRadius(state);
}
// Renderizza i marker dei risultati sulla mappa
function renderMarkers(state, markers) {
    const L = window.L;

    state.resultMarkers.clearLayers();
     // Aggiungi i marker dei risultati alla mappa
    for (const marker of markers ?? []) {
        //  Verifica che le coordinate del marker siano valide
        if (!Number.isFinite(marker.latitude) ||
            !Number.isFinite(marker.longitude)) {
            continue;
        }

        const popup = document.createElement("div");

        const title = document.createElement("strong");
        title.textContent = marker.title;
        popup.appendChild(title);
        // Aggiungi la descrizione del marker se presente
        if (marker.description) {
            const description = document.createElement("div");
            description.textContent = marker.description;
            popup.appendChild(description);
        }

        const label = [marker.title, marker.description]
    .filter(Boolean)
    .join(", ");

        L.marker(
            [marker.latitude, marker.longitude],
            {
                keyboard: true,
                alt: label,
                title: label
            }
        )
        .bindPopup(popup)
        .addTo(state.resultMarkers);
    }
}
// Inizializza la mappa con le impostazioni specificate
export function initialize(
    element,
    latitude,
    longitude,
    hasSelection,
    editable,
    radiusKm,
    dotNet
) {
    const L = window.L;

    if (!L) {
        throw new Error("Leaflet non è stato caricato.");
    }

    const map = L.map(element, {
        keyboard: true,
        zoomControl: false
    }).setView([latitude, longitude], 12);

    L.control.zoom({
        zoomInTitle: "Ingrandisci la mappa",
        zoomOutTitle: "Riduci la mappa"
    }).addTo(map);

    L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    const state = {
        map,
        marker: null,
        circle: null,
        latitude: null,
        longitude: null,
        editable,
        radiusKm,
        dotNet,
        resultMarkers: L.layerGroup().addTo(map),
    };

    instances.set(element, state);
// Se è presente una selezione iniziale, posiziona il marker sulla mappa
    if (hasSelection) {
        setPosition(state, latitude, longitude);
    }
   // Aggiungi un listener per l'evento "click" sulla mappa se è modificabile
    if (editable) {
        map.on("click", e => {
            const { lat, lng } = e.latlng;

            setPosition(state, lat, lng);

            dotNet.invokeMethodAsync(
                "PositionChangedFromJs",
                lat,
                lng
            );
        });
    }

    requestAnimationFrame(() => map.invalidateSize());
}
// Aggiorna la mappa con la nuova posizione, raggio e marker dei risultati
export function update(
    element,
    latitude,
    longitude,
    radiusKm,
    markers = []
) {
    const state = instances.get(element);

    if (!state) return;

    state.radiusKm = radiusKm;

    renderMarkers(state, markers);
     // Aggiorna la posizione del marker e il raggio sulla mappa
    if (latitude == null || longitude == null) {
        if (state.marker) {
            state.map.removeLayer(state.marker);
            state.marker = null;
        }

        state.latitude = null;
        state.longitude = null;

        updateRadius(state);
        return;
    }

    const changed =
        state.latitude !== latitude ||
        state.longitude !== longitude;
   // Aggiorna la posizione del marker e il raggio sulla mappa
    setPosition(state, latitude, longitude, changed);
}
// Seleziona il centro della mappa e aggiorna la posizione del marker
export async function selectMapCenter(element) {
    const state = instances.get(element);

    if (!state || !state.editable) {
        return;
    }

    const center = state.map.getCenter();

    // Posiziona il marker al centro della mappa
    setPosition(state, center.lat, center.lng);

    // Comunica la nuova posizione al componente Blazor
    await state.dotNet.invokeMethodAsync(
        "PositionChangedFromJs",
        center.lat,
        center.lng
    );
}
//  Rimuove la mappa e libera le risorse associate all'elemento specificato
export function dispose(element) {
    const state = instances.get(element);

    if (!state) return;

    state.map.remove();
    instances.delete(element);
}
