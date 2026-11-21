
let loanChart = null;

export function renderLoanStatisticsChart(canvas, months, sentRequests, receivedRequests) {
    if (!window.Chart) {
        throw new Error("Chart.js non è disponibile.");
    }

    disposeLoanStatisticsChart();

    loanChart = new window.Chart(canvas, {
        type: "bar",
        data: {
            labels: months,
            datasets: [
                {
                    label: "Richieste inviate",
                    data: sentRequests,
                    backgroundColor: "#0d6efd",
                    borderRadius: 4
                },
                {
                    label: "Richieste ricevute",
                    data: receivedRequests,
                    backgroundColor: "#198754",
                    borderRadius: 4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: "bottom"
                }
            },
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

export function disposeLoanStatisticsChart() {
    loanChart?.destroy();
    loanChart = null;
}
