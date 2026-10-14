document.addEventListener("DOMContentLoaded", function () {
    console.log("JavaScript is running!"); // Debugging message

    const sidebar = document.querySelector(".sidebar");
    const toggleButton = document.getElementById("toggleSidebar");

    if (toggleButton && sidebar) {
        toggleButton.addEventListener("click", function () {
            console.log("Sidebar toggled!"); // Debugging message
            sidebar.classList.toggle("hidden"); // Toggle sidebar visibility
        });
    } else {
        console.log("Sidebar or button not found!"); // Debugging error
    }
});