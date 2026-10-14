document.addEventListener("DOMContentLoaded", function () {
    const chartElement = document.getElementById('subscriptionChart');

    if (chartElement) {
        const stats = {
            active: chartElement.getAttribute('data-active'),
            free: chartElement.getAttribute('data-free'),
            pending: chartElement.getAttribute('data-pending'),
            expired: chartElement.getAttribute('data-expired'),
            cancelled: chartElement.getAttribute('data-cancelled')
        };

        const ctx = chartElement.getContext('2d');

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Active', 'Free', 'Pending', 'Expired', 'Cancelled'],
                datasets: [{
                    label: 'Subscriptions Count',
                    data: [stats.active, stats.free, stats.pending, stats.expired, stats.cancelled],
                    backgroundColor: ['#198754', '#0d6efd', '#ffc107', '#dc3545', '#6c757d'],
                    borderRadius: 10,
                    barThickness: 40
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { drawBorder: false }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }
});