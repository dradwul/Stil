// Optional behaviours for Stil CSS: an ES module with no dependencies.
// <script type="module" src="stil.js"></script> turns on:
//   - Dropdowns (<details class="stil-dropdown">): close on an outside click, on Escape and after
//     choosing an item; opening one closes the others.
//   - data-stil-dismiss on a button removes the closest .stil-alert or .stil-toast, or the element
//     matched by the attribute's value (a CSS selector).
//   - data-stil-dialog-open="dialog-id" opens that <dialog> as a modal; data-stil-dialog-close
//     closes the dialog it is in.
// Stil for Blazor loads this module automatically and also uses showDialog/closeDialog.

const initialized = Symbol.for('stil.initialized');

export function init(root = document) {
    if (root[initialized]) return;
    root[initialized] = true;
    root.addEventListener('click', onClick);
    root.addEventListener('keydown', onKeyDown);
    // The toggle event does not bubble, so listen in the capture phase.
    root.addEventListener('toggle', onToggle, true);
}

const openDropdowns = () => document.querySelectorAll('details.stil-dropdown[open]');

function onClick(event) {
    const target = event.target instanceof Element ? event.target : null;
    if (!target) return;

    for (const dropdown of openDropdowns()) {
        if (!dropdown.contains(target) || target.closest('.stil-dropdown-item')) {
            dropdown.open = false;
        }
    }

    const dismiss = target.closest('[data-stil-dismiss]');
    if (dismiss) {
        const selector = dismiss.getAttribute('data-stil-dismiss');
        (selector ? document.querySelector(selector) : dismiss.closest('.stil-alert, .stil-toast'))?.remove();
    }

    const opener = target.closest('[data-stil-dialog-open]');
    if (opener) {
        document.getElementById(opener.getAttribute('data-stil-dialog-open'))?.showModal();
    }

    const closer = target.closest('[data-stil-dialog-close]');
    if (closer) {
        closer.closest('dialog')?.close();
    }
}

function onKeyDown(event) {
    if (event.key !== 'Escape') return;
    for (const dropdown of openDropdowns()) {
        dropdown.open = false;
        dropdown.querySelector('summary')?.focus();
    }
}

function onToggle(event) {
    const dropdown = event.target;
    if (!(dropdown instanceof HTMLDetailsElement) || !dropdown.matches('.stil-dropdown') || !dropdown.open) return;
    for (const other of openDropdowns()) {
        if (other !== dropdown) other.open = false;
    }
}

// Opens a <dialog> as a modal. When it closes (Escape, a form with method="dialog", close()),
// dotNetRef.OnDialogClosed is called, so a Blazor component can update its state.
export function showDialog(dialog, dotNetRef) {
    if (!dialog) return;
    dialog.stilDotNetRef = dotNetRef;
    if (!dialog.stilCloseListener) {
        dialog.stilCloseListener = () => dialog.stilDotNetRef?.invokeMethodAsync('OnDialogClosed').catch(() => { });
        dialog.addEventListener('close', dialog.stilCloseListener);
    }
    if (!dialog.open) dialog.showModal();
}

export function closeDialog(dialog) {
    if (dialog?.open) dialog.close();
}

if (typeof document !== 'undefined') init();
