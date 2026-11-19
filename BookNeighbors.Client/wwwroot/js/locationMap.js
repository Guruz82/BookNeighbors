
const instances = new WeakMap();

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

function setPosition(state, latitude, longitude, moveMap = false) {
    const L = window.L;
    const position = [latitude, longitude];

    if (!state.marker) {
        state.marker = L.marker(position, {
            draggable: state.editable
        }).addTo(state.map);

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

function renderMarkers(state, markers) {
    const L = window.L;

    state.resultMarkers.clearLayers();

    for (const marker of markers ?? []) {
        if (!Number.isFinite(marker.latitude) ||
            !Number.isFinite(marker.longitude)) {
            continue;
        }

        const popup = document.createElement("div");

        const title = document.createElement("strong");
        title.textContent = marker.title;
        popup.appendChild(title);

        if (marker.description) {
            const description = document.createElement("div");
            description.textContent = marker.description;
            popup.appendChild(description);
        }

        L.circleMarker(
            [marker.latitude, marker.longitude],
            {
                radius: 8,
                color: "#c0392b",
                fillColor: "#e74c3c",
                fillOpacity: 0.9
            }
        )
        .bindPopup(popup)
        .addTo(state.resultMarkers);
    }
}

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

    const map = L.map(element).setView(
        [latitude, longitude],
        12
    );

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

    if (hasSelection) {
        setPosition(state, latitude, longitude);
    }

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

    setPosition(state, latitude, longitude, changed);
}

export function dispose(element) {
    const state = instances.get(element);

    if (!state) return;

    state.map.remove();
    instances.delete(element);
}
