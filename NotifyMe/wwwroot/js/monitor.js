function copyToClipboard(text, button) {
    navigator.clipboard.writeText(text).then(() => {
        const icon = button.querySelector('i');
        icon.classList.replace('bi-clipboard', 'bi-check-lg');
        icon.classList.replace('text-primary', 'text-success');

        setTimeout(() => {
            icon.classList.replace('bi-check-lg', 'bi-clipboard');
            icon.classList.replace('text-success', 'text-primary');
        }, 2000);
    }).catch(err => {
        console.error('Failed to copy: ', err);
    });
}