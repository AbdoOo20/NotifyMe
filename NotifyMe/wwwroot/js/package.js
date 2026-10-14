const toggle = document.getElementById('billingToggle');
const proPrice = document.getElementById('pro-price');
const bizPrice = document.getElementById('biz-price');
const periods = document.querySelectorAll('.period');

function updatePricingDisplay() {
    if (toggle.checked) {
        // Yearly Prices
        proPrice.innerText = '$80';
        bizPrice.innerText = '$250';
        periods.forEach(p => p.innerText = 'Year');
    } else {
        // Monthly Prices
        proPrice.innerText = '$8';
        bizPrice.innerText = '$25';
        periods.forEach(p => p.innerText = 'Month');
    }
}

toggle.addEventListener('change', updatePricingDisplay);

window.addEventListener('load', updatePricingDisplay);

async function upgradePlan(packageType) {
    const duration = toggle.checked ? 2 : 1; // 2 = Year, 1 = Month

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const response = await fetch('/Package/Upgrade', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `packageType=${packageType}&duration=${duration}`
    });

    if (response.ok) {
        window.location.href = '/Package/Index';
    } else {
        window.location.href = '/Package/Index';
    }
}