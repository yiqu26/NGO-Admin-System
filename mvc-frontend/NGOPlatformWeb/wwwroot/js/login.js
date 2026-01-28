document.addEventListener("DOMContentLoaded", function () {
    const toggle = document.getElementById("togglePassword");
    const input = document.getElementById("passwordInput");
    toggle.addEventListener("click", function () {
        const isPwd = input.type === "password";
        input.type = isPwd ? "text" : "password";
        this.classList.toggle("fa-eye");
        this.classList.toggle("fa-eye-slash");
    });
});