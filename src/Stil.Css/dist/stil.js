/*! Stil CSS v0.1.0 | MIT License | https://github.com/dradwul/StilCSS */
// Optional behaviours for Stil CSS: an ES module with no dependencies.
// <script type="module" src="stil.js"></script> turns on:
//   - Dropdowns (<details class="stil-dropdown">): close on an outside click, on Escape and after
//     choosing an item; opening one closes the others. Arrow keys move between the items, and
//     ArrowDown on the toggle opens the menu.
//   - data-stil-dismiss on a button removes the closest .stil-alert or .stil-toast, or the element
//     matched by the attribute's value (a CSS selector).
//   - data-stil-dialog-open="dialog-id" opens that <dialog> as a modal; data-stil-dialog-close
//     closes the dialog it is in.
//   - data-stil-toggle on a button flips its aria-expanded, which shows or hides what it controls
//     (for example the stil-navbar-collapse next to a stil-navbar-toggle). Escape collapses it.
//   - Keys that a widget handles itself (arrows in a tree, tab list or combobox; Enter and Escape
//     in an open combobox) do not also scroll the page, submit the form or close a dialog.
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

    const toggle = target.closest('[data-stil-toggle]');
    if (toggle) {
        toggle.setAttribute('aria-expanded', String(toggle.getAttribute('aria-expanded') !== 'true'));
    }

    const closer = target.closest('[data-stil-dialog-close]');
    if (closer) {
        closer.closest('dialog')?.close();
    }
}

function onKeyDown(event) {
    if (preventHandledKey(event)) return;
    if (moveInDropdown(event)) return;
    if (event.key !== 'Escape') return;
    for (const toggle of document.querySelectorAll('[data-stil-toggle][aria-expanded="true"]')) {
        toggle.setAttribute('aria-expanded', 'false');
    }
    for (const dropdown of openDropdowns()) {
        dropdown.open = false;
        dropdown.querySelector('summary')?.focus();
    }
}

// Widgets whose keys are handled by their own script (in Stil for Blazor, by the component, since
// Blazor cannot prevent the default action for some keys only).
const handledKeys = [
    ['[role="combobox"][aria-expanded="true"]', ['ArrowDown', 'ArrowUp', 'Enter', 'Escape']],
    ['[role="combobox"]', ['ArrowDown', 'ArrowUp']],
    ['[role="treeitem"]', ['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight', 'Home', 'End', ' ']],
    ['[role="tab"]', ['ArrowLeft', 'ArrowRight', 'Home', 'End']],
];

function preventHandledKey(event) {
    const target = event.target instanceof Element ? event.target : null;
    if (!target || event.altKey || event.ctrlKey || event.metaKey) return false;
    if (handledKeys.some(([selector, keys]) => keys.includes(event.key) && target.matches(selector))) {
        event.preventDefault();
        return true;
    }
    return false;
}

// Arrow keys, Home and End move focus between the enabled items of the dropdown that has focus.
function moveInDropdown(event) {
    if (!['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(event.key)) return false;
    const dropdown = event.target instanceof Element ? event.target.closest('details.stil-dropdown') : null;
    if (!dropdown) return false;

    const onToggle = event.target.matches('summary');
    if (onToggle && event.key !== 'ArrowDown') return false;
    if (onToggle && !dropdown.open) dropdown.open = true;

    const items = [...dropdown.querySelectorAll('.stil-dropdown-item')]
        .filter(item => !item.matches(':disabled, [aria-disabled="true"]'));
    if (items.length === 0) return false;

    const index = items.indexOf(document.activeElement);
    const next = event.key === 'Home' || (onToggle && event.key === 'ArrowDown') ? 0
        : event.key === 'End' ? items.length - 1
        : event.key === 'ArrowDown' ? (index + 1) % items.length
        : (index - 1 + items.length) % items.length;

    event.preventDefault();
    items[next].focus();
    return true;
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
