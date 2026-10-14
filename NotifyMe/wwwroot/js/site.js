const userTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

if (document.cookie.indexOf("UserTimeZone") === -1) {
    document.cookie = "UserTimeZone=" + userTimeZone + ";path=/;max-age=31536000";
}