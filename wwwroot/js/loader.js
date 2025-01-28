document.getElementById('formAuthentication').addEventListener('submit', function (event) {
    // Show loader inside the button
    const Button = document.querySelector('.btn-primary');
    Button.innerHTML = `<div class="loader">
                                    <div class="loader__bounce first"></div>
                                    <div class="loader__bounce second"></div>
                                    <div class="loader__bounce third"></div>
                                 </div>`;
    Button.setAttribute('disabled', 'true'); // Disable the button
});