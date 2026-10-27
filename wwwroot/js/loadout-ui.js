(() => {
    const dialogs = new WeakMap();
    window.loadoutUI = {
        openDialog(dialog, reference) {
            const opener = document.activeElement;
            const cancel = event => { event.preventDefault(); reference.invokeMethodAsync("CloseFromBrowser"); };
            const click = event => {
                const box = dialog.getBoundingClientRect();
                if (event.target === dialog && (event.clientX < box.left || event.clientX > box.right || event.clientY < box.top || event.clientY > box.bottom)) reference.invokeMethodAsync("CloseFromBrowser");
            };
            dialogs.set(dialog, {opener,cancel,click});
            dialog.addEventListener('cancel', cancel);
            dialog.addEventListener('click', click);
            dialog.showModal();
            dialog.querySelector('[autofocus]')?.focus();
        },
        closeDialog(dialog) {
            const state = dialogs.get(dialog);
            dialog.close();
            if (state) {
                dialog.removeEventListener('cancel', state.cancel);
                dialog.removeEventListener('click', state.click);
                if (state.opener?.isConnected && state.opener.getClientRects().length) state.opener.focus();
                else document.querySelector('[data-focus-after-dialog]')?.focus();
                dialogs.delete(dialog);
            }
        }
    };
    document.addEventListener('click', event => {
        const menuAction = event.target.closest('.game-menu button');
        if (menuAction) menuAction.closest('details').open = false;
        const toggle = event.target.closest('[data-password-toggle]');
        if (!toggle) return;
        const input = document.getElementById(toggle.dataset.passwordToggle);
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        toggle.textContent = show ? 'Hide' : 'Show';
        toggle.setAttribute('aria-label', (show ? 'Hide' : 'Show') + (input.name === 'ConfirmPassword' ? ' password confirmation' : ' password'));
        toggle.setAttribute('aria-pressed', String(show));
    });
    document.addEventListener('input', event => {
        const form = event.target.closest('[data-password-match]');
        if (form) {
            const confirm = form.elements.ConfirmPassword;
            confirm.setCustomValidity(confirm.value && form.elements.Password.value !== confirm.value ? 'Passwords do not match.' : '');
        }
    });
    document.addEventListener('submit', event => {
        if (event.target.method !== 'post') return;
        if (event.target.dataset.submitting) { event.preventDefault(); return; }
        event.target.dataset.submitting = 'true';
        const button = event.target.querySelector('[data-pending]');
        if (button) { button.disabled = true; button.textContent = button.dataset.pending; }
    });
    document.addEventListener('error', event => {
        const img = event.target;
        if (img instanceof HTMLImageElement && img.dataset.coverFallback && img.getAttribute('src') !== img.dataset.coverFallback) img.src = img.dataset.coverFallback;
    }, true);
    window.addEventListener('pageshow', event => {
        if (event.persisted) location.reload();
    });
})();
