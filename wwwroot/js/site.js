// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('input', (event) => {
    const element = event.target;
    if (!(element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement)) {
        return;
    }

    const filter = element.getAttribute('data-filter');
    if (!filter) {
        return;
    }

    let value = element.value || '';
    if (filter === 'letters') {
        value = value.replace(/[^A-Za-z ]/g, '');
    } else if (filter === 'digits') {
        value = value.replace(/[^0-9]/g, '');
    } else if (filter === 'no-angle') {
        value = value.replace(/[<>]/g, '');
    }

    const maxRaw = element.getAttribute('data-max');
    const max = maxRaw ? parseInt(maxRaw, 10) : NaN;
    if (!Number.isNaN(max) && max > 0) {
        value = value.slice(0, max);
    }

    element.value = value;
});
