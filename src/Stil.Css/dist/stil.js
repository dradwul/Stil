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
//   - Calendar grids (table.stil-calendar-grid with role="grid"): the arrow keys move between the
//     days, Home and End go to the start and end of the week, and Enter or Space clicks the day.
//     Moving past the first or last day fires a bubbling stilcalendaredge event (detail: the key
//     and the day cell), so the page can show the previous or next month.
//   - Week grids with data-stil-editable (div.stil-week): drag an event to move it (also to
//     another day), drag its .stil-event-resize handle to change its end, or drag on empty time
//     to mark a new event. Times snap to 15 minutes and Escape cancels. On release a bubbling
//     stilweekedit event fires (detail: action "move", "resize" or "create"; item, the event
//     element or null; day, the index of the day column; start and end in minutes from
//     midnight). The page saves the change and redraws: the grid puts the event back first.
//   - Keys that a widget handles itself (arrows in a tree, tab list or combobox; Enter and Escape
//     in an open combobox) do not also scroll the page, submit the form or close a dialog.
//   - Language links (a[data-stil-culture="sv"]) store the culture in the ASP.NET Core culture
//     cookie and in local storage (key stil-culture), then reload the page.
//   - In a right-to-left page the arrow keys of the calendar grid follow the reading direction.
// Stil for Blazor loads this module automatically and also uses showDialog/closeDialog.

const initialized = Symbol.for('stil.initialized');

export function init(root = document) {
    if (root[initialized]) return;
    root[initialized] = true;
    root.addEventListener('click', onCultureClick, true);
    root.addEventListener('click', onClick);
    root.addEventListener('keydown', onKeyDown);
    // The toggle event does not bubble, so listen in the capture phase.
    root.addEventListener('toggle', onToggle, true);
    root.addEventListener('pointerdown', onPointerDown);
    root.addEventListener('pointermove', onPointerMove);
    root.addEventListener('pointerup', onPointerUp);
    root.addEventListener('pointercancel', cancelDrag);
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

// Language links (data-stil-culture="sv", as StilLanguageSelector renders them): store the choice
// in the standard ASP.NET Core culture cookie and in local storage (for WebAssembly), then reload
// without the ?culture= fallback the link carries for pages without this script. Runs in the
// capture phase, before a framework's own link handling.
function onCultureClick(event) {
    const link = event.target instanceof Element ? event.target.closest('a[data-stil-culture]') : null;
    if (!link || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    event.preventDefault();
    event.stopPropagation();

    const culture = link.getAttribute('data-stil-culture');
    const cookie = encodeURIComponent(`c=${culture}|uic=${culture}`);
    document.cookie = `.AspNetCore.Culture=${cookie}; path=/; max-age=31536000; samesite=lax`;
    try {
        localStorage.setItem('stil-culture', culture);
    } catch {
        // Storage can be blocked; the cookie still works on the server.
    }

    const url = new URL(location.href);
    url.searchParams.delete('culture');
    url.searchParams.delete('ui-culture');
    if (url.href === location.href) location.reload();
    else location.replace(url.href);
}

function onKeyDown(event) {
    if (drag && event.key === 'Escape') {
        cancelDrag();
        event.preventDefault();
        return;
    }
    if (preventHandledKey(event)) return;
    if (moveInDropdown(event)) return;
    if (moveInCalendar(event)) return;
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

// ArrowLeft and ArrowRight as "earlier" and "later": swapped when the element is right to left.
function logicalArrow(key, element) {
    if (getComputedStyle(element).direction !== 'rtl') return key;
    return key === 'ArrowLeft' ? 'ArrowRight' : key === 'ArrowRight' ? 'ArrowLeft' : key;
}

// The WAI-ARIA grid pattern for a month: one day is in the tab order (tabindex="0"), the
// arrow keys move it. Day cells are the td elements with role="gridcell".
const calendarKeys = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End', 'Enter', ' '];

function moveInCalendar(event) {
    if (!calendarKeys.includes(event.key) || event.altKey || event.ctrlKey || event.metaKey) return false;
    const cell = event.target instanceof Element ? event.target.closest('.stil-calendar-grid td[role="gridcell"]') : null;
    if (!cell || event.target !== cell) return false;
    event.preventDefault();

    if (event.key === 'Enter' || event.key === ' ') {
        cell.click();
        return true;
    }

    // In a right-to-left page the next day is to the left, so the keys are swapped. The edge
    // event reports the logical key: ArrowRight always means "later".
    const key = logicalArrow(event.key, cell);
    const row = cell.parentElement;
    const days = [...row.querySelectorAll(':scope > td[role="gridcell"]')];
    const rows = [...cell.closest('table').querySelectorAll(':scope > tbody > tr')];
    const column = days.indexOf(cell);
    const rowIndex = rows.indexOf(row);
    const dayIn = r => rows[r]?.querySelectorAll(':scope > td[role="gridcell"]')[column];

    const next = {
        ArrowLeft: days[column - 1] ?? rows[rowIndex - 1]?.querySelector(':scope > td[role="gridcell"]:last-child'),
        ArrowRight: days[column + 1] ?? rows[rowIndex + 1]?.querySelector(':scope > td[role="gridcell"]'),
        ArrowUp: dayIn(rowIndex - 1),
        ArrowDown: dayIn(rowIndex + 1),
        Home: days[0],
        End: days[days.length - 1],
    }[key];

    if (!next) {
        cell.dispatchEvent(new CustomEvent('stilcalendaredge', { bubbles: true, detail: { key, cell } }));
        return true;
    }

    cell.tabIndex = -1;
    next.tabIndex = 0;
    next.focus();
    return true;
}

// Editing in a week grid. The grid reads its first hour and number of hours from the computed
// --_from and --_hours of the stil-week, so the drag follows whatever the markup set.
const snapMinutes = 15;
let drag = null;

function onPointerDown(event) {
    if (event.button !== 0 || !event.isPrimary) return;
    const target = event.target instanceof Element ? event.target : null;
    const column = target?.closest('.stil-week[data-stil-editable] .stil-week-col');
    if (!column) return;
    const item = target.closest('.stil-week-col > .stil-event');
    if (!item && target !== column && !target.matches('.stil-week-free')) return;

    const week = column.closest('.stil-week');
    const styles = getComputedStyle(week);
    const first = parseFloat(styles.getPropertyValue('--_from')) * 60;
    const length = item ? parseFloat(item.style.getPropertyValue('--stil-length')) : 0;
    drag = {
        week,
        column,
        item,
        action: !item ? 'create' : target.closest('.stil-event-resize') ? 'resize' : 'move',
        first,
        last: first + parseFloat(styles.getPropertyValue('--_hours')) * 60,
        start: item ? parseFloat(item.style.getPropertyValue('--stil-start')) : 0,
        length,
        style: item?.getAttribute('style') ?? null,
        pointerId: event.pointerId,
        x: event.clientX,
        y: event.clientY,
        moved: false,
        ghost: null,
        result: null,
    };
}

function onPointerMove(event) {
    if (!drag || event.pointerId !== drag.pointerId) return;
    const d = drag;
    if (!d.moved) {
        if (Math.hypot(event.clientX - d.x, event.clientY - d.y) < 4) return;
        d.moved = true;
        try { (d.item ?? d.column).setPointerCapture(d.pointerId); } catch { /* the pointer is gone */ }
        if (d.item) {
            d.item.classList.add('stil-event-dragging');
        } else {
            d.ghost = document.createElement('div');
            d.ghost.className = 'stil-event stil-event-draft stil-event-dragging';
            d.ghost.setAttribute('aria-hidden', 'true');
            d.column.append(d.ghost);
        }
    }

    const rect = d.column.getBoundingClientRect();
    const perMinute = rect.height / (d.last - d.first);
    const snap = minutes => Math.round(minutes / snapMinutes) * snapMinutes;
    const clamp = (value, min, max) => Math.min(max, Math.max(min, value));
    const columns = [...d.week.querySelectorAll('.stil-week-col')];
    let day = columns.indexOf(d.column);
    let start;
    let end;

    if (d.action === 'move') {
        start = clamp(d.start + snap((event.clientY - d.y) / perMinute), d.first, d.last - d.length);
        end = start + d.length;
        // The day column under the pointer, or the nearest one.
        const target = columns.reduce((best, column) => {
            const r = column.getBoundingClientRect();
            const distance = event.clientX < r.left ? r.left - event.clientX : event.clientX > r.right ? event.clientX - r.right : 0;
            return distance < best.distance ? { column, distance } : best;
        }, { column: d.column, distance: Infinity }).column;
        day = columns.indexOf(target);
        d.item.style.setProperty('--stil-start', start);
        d.item.style.translate = `${target.getBoundingClientRect().left - rect.left}px 0`;
    } else if (d.action === 'resize') {
        start = d.start;
        end = clamp(d.start + snap(d.length + (event.clientY - d.y) / perMinute), d.start + snapMinutes, d.last);
        d.item.style.setProperty('--stil-length', end - start);
    } else {
        const at = clientY => clamp(snap(d.first + (clientY - rect.top) / perMinute), d.first, d.last);
        const anchor = at(d.y);
        const current = at(event.clientY);
        start = Math.min(anchor, current);
        end = Math.max(anchor, current, start + snapMinutes);
        d.ghost.style.setProperty('--stil-start', start);
        d.ghost.style.setProperty('--stil-length', end - start);
    }

    d.result = { day, start, end };
    event.preventDefault();
}

function onPointerUp(event) {
    if (!drag || event.pointerId !== drag.pointerId) return;
    const d = drag;
    cancelDrag();
    if (!d.moved || !d.result) return;

    // The click that follows a drag must not open the event or the free block.
    const swallow = click => { click.stopPropagation(); click.preventDefault(); };
    window.addEventListener('click', swallow, { capture: true, once: true });
    setTimeout(() => window.removeEventListener('click', swallow, true), 0);

    (d.item ?? d.column).dispatchEvent(new CustomEvent('stilweekedit', {
        bubbles: true,
        detail: { action: d.action, item: d.item, ...d.result },
    }));
}

// Puts the grid back as the markup drew it.
function cancelDrag() {
    const d = drag;
    drag = null;
    if (!d) return;
    if (d.item) {
        d.item.classList.remove('stil-event-dragging');
        if (d.style === null) d.item.removeAttribute('style'); else d.item.setAttribute('style', d.style);
    }
    d.ghost?.remove();
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
