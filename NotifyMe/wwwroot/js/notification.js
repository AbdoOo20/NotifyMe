document.addEventListener("DOMContentLoaded", function () {

    const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

    connection.on("ReceiveNotification", (title, message) => {
        showToast(title, message);
    });

    connection.start().catch(err => console.error(err.toString()));

    function showToast(title, message) {
        const toast = `
        <div class="toast show" role="alert" aria-live="assertive" aria-atomic="true"
            style="background-color: rgba(98, 44, 142, 0.9); direction: ltr; border-radius: 8px; padding: 15px; margin-bottom: 10px; color: #ffffff; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);">
            <div class="d-flex align-items-center">
                <div style="font-size: 1.5rem; margin-right: 10px; margin-left: 10px;">
                    <i class="fas fa-bell"></i>
                </div>
                <div style="flex-grow: 1;">
                    <strong style="display: block; font-size: 1rem;">${title}</strong>
                    <span>${message}</span>
                </div>
                <button type="button" class="btn-close ms-2" data-bs-dismiss="toast" aria-label="Close"
                    style="background: transparent; border: none; color: white; opacity: 0.8;"></button>
            </div>
        </div>`;

        const container = document.getElementById("toastContainer");

        container.innerHTML += toast;

        // Show the new toast
        const toastElement = container.lastChild;

        setTimeout(() => toastElement.remove(), 7000); // Automatically remove the toast after 5 seconds
    }

    function updateCounts() {
        $.get('/api/NotificationAPI/counts', function (data) {
            if (data.notifications > 0) {
                $('#nav-notification-count').text(data.notifications).show();
            }
            if (data.alerts > 0) {
                $('#nav-alerts-count').text(data.alerts).show();
            }
        });
    }

    updateCounts();
});