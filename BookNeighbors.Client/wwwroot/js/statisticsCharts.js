// Riferimenti ai grafici attivi.
let statusChart = null;
let trendChart = null;

// Disegna i grafici con Chart.js.
export function renderCharts(
    statusCanvas,
    trendCanvas,
    statusLabels,
    statusCounts,
    dayLabels,
    dayCounts
) {
    if (!window.Chart) {
        throw new Error("Chart.js non è disponibile.");
    }

    disposeCharts();

    // Distribuzione delle richieste per stato.
    statusChart = new window.Chart(statusCanvas, {
        type: "doughnut",
        data: {
            labels: statusLabels,
            datasets: [{
                label: "Richieste",
                data: statusCounts,
                backgroundColor: [
                    "#ffc107",
                    "#198754",
                    "#dc3545",
                    "#0d6efd",
                    "#6c757d"
                ]
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });

    // Andamento giornaliero delle richieste.
    trendChart = new window.Chart(trendCanvas, {
        type: "line",
        data: {
            labels: dayLabels,
            datasets: [{
                label: "Richieste per giorno",
                data: dayCounts,
                borderColor: "#0a58ca",
                backgroundColor: "rgba(10,88,202,0.12)",
                pointRadius: 2,
                tension: 0.15,
                fill: true
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
}

// Libera le risorse quando si abbandona la pagina.
export function disposeCharts() {
    statusChart?.destroy();
    trendChart?.destroy();

    statusChart = null;
    trendChart = null;
}