/*! Stil CSS v0.1.0 | MIT License | https://github.com/dradwul/Stil */
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
//     A resource schedule (div.stil-resource[data-stil-editable]) works the same with time across:
//     its rows (.stil-resource-track) take the place of the days, and day is the row's index.
//   - Kanban boards (.stil-kanban): drag a card (.stil-kanban-card[data-stil-item]) to another
//     place or column ([data-stil-column]); on release a bubbling stilcardmove event fires
//     (detail: item, column and index among the column's other cards). The arrow keys move focus
//     between cards; a card with data-stil-grabbed is moved by the page instead.
//   - bindCommandKey calls the .NET method OpenFromKey on Ctrl+K or Cmd+K; readStorage and
//     writeStorage keep small values (such as recent commands) in local storage.
//   - A textarea with data-stil-send="enter" submits its form on Enter (Shift+Enter breaks the
//     line); with data-stil-send="ctrl-enter", on Ctrl+Enter or Cmd+Enter.
//   - Keys that a widget handles itself (arrows in a tree, tab list or combobox; Enter and Escape
//     in an open combobox) do not also scroll the page, submit the form or close a dialog.
//   - Language links (a[data-stil-culture="sv"]) store the culture in the ASP.NET Core culture
//     cookie and in local storage (key stil-culture), then reload the page.
//   - In a right-to-left page the arrow keys of the calendar grid follow the reading direction.
//   - Popovers (.stil-popover with popover) are placed next to their popovertarget toggle, below
//     it or above it (data-stil-placement="top"), and follow it on scroll and resize.
//   - Open dropdown menus and combobox lists (.stil-combobox-popup without hidden) move to the top
//     layer, so a parent with overflow (a scrolling card, a table, a dialog) never cuts them off.
//   - data-stil-mask="0000 00 00-0000" on an input formats it while typing (0 digit, A letter,
//     * any character, the rest literals).
//   - Splitters (.stil-splitter) resize by dragging or with the keyboard and fire stilsplit;
//     carousels (.stil-carousel) get working buttons and dots, optional autoplay
//     (data-stil-autoplay) and fire stilslide.
//   - Data grid columns resize (.stil-data-grid-resize, stilcolumnresize) and move by dragging
//     their header (th[data-stil-reorder], stilcolumnmove); freezeColumns keeps frozen ones in view.
//     focusEditor and focusRow move focus to a field being edited and to a tree grid row.
//   - initCropper, setCropperZoom, rotateCropper and cropImage crop an image; startCamera,
//     captureCamera and stopCamera take a photo; printFrame prints an iframe.
//   - startScanner, stopScanner and setTorch read barcodes and QR codes from the camera with the
//     browser's BarcodeDetector, or with a decoder the page passes to registerScanDecoder;
//     initScanInput tells a handheld scanner's keystrokes from typing.
//   - initSignaturePad, drawSignature and signaturePng: a signature drawn with a finger, pen or
//     mouse on a canvas, and the signature as a PNG.
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
    root.addEventListener('pointerdown', onSplitterPointerDown);
    root.addEventListener('pointermove', onSplitterPointerMove);
    root.addEventListener('pointerup', onSplitterPointerUp);
    root.addEventListener('pointercancel', onSplitterPointerUp);
    root.addEventListener('pointerdown', onCardPointerDown);
    root.addEventListener('pointermove', onCardPointerMove);
    root.addEventListener('pointerup', onCardPointerUp);
    root.addEventListener('pointercancel', onCardPointerUp);
    root.addEventListener('keydown', cancelCardDrag);
    root.addEventListener('pointerdown', onColumnPointerDown);
    root.addEventListener('pointermove', onColumnPointerMove);
    root.addEventListener('pointerup', onColumnPointerUp);
    root.addEventListener('pointercancel', onColumnPointerUp);
    root.addEventListener('click', onCarouselClick);
    // scroll does not bubble, so listen in the capture phase.
    root.addEventListener('scroll', onCarouselScroll, true);
    root.addEventListener('scrollend', onCarouselScroll, true);
    setInterval(advanceCarousels, 500);
    root.addEventListener('input', onMaskInput, true);
    addEventListener('scroll', placeOpenPopovers, { capture: true, passive: true });
    addEventListener('resize', placeOpenPopovers);
    // Combobox lists open and close through their hidden attribute (set by Blazor or the page).
    new MutationObserver(onPopupMutations).observe(root, { subtree: true, childList: true, attributes: true, attributeFilter: ['hidden'] });
    for (const popup of root.querySelectorAll('.stil-combobox-popup:not([hidden])')) liftPanel(popup);
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
    if (sendFromComposer(event)) return;
    if (moveInKanban(event)) return;
    if (preventHandledKey(event)) return;
    if (moveInDropdown(event)) return;
    if (moveInCalendar(event)) return;
    if (resizeSplitter(event)) return;
    if (resizeColumn(event)) return;
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
    ['[role="treegrid"] tr[aria-level]', ['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight', 'Home', 'End']],
    ['[role="tab"]', ['ArrowLeft', 'ArrowRight', 'Home', 'End']],
    ['.stil-kanban-card', ['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight', ' ', 'Escape']],
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
    const column = target?.closest('.stil-week[data-stil-editable] .stil-week-col, .stil-resource[data-stil-editable] .stil-resource-track');
    if (!column) return;
    const item = target.closest('.stil-week-col > .stil-event, .stil-resource-track > .stil-event');
    if (!item && target !== column && !target.matches('.stil-week-free')) return;

    const week = column.closest('.stil-week, .stil-resource');
    const styles = getComputedStyle(week);
    const first = parseFloat(styles.getPropertyValue('--_from')) * 60;
    const length = item ? parseFloat(item.style.getPropertyValue('--stil-length')) : 0;
    drag = {
        week,
        column,
        // A resource schedule runs time across and its rows down; a week the other way round.
        across: week.classList.contains('stil-resource'),
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
    // Time runs along one axis and the days (or resources) along the other; across, it runs
    // from the inline start, so right to left in a right-to-left page.
    const sign = d.across && getComputedStyle(d.week).direction === 'rtl' ? -1 : 1;
    const time = e => d.across ? sign * e.clientX : e.clientY;
    const cross = e => d.across ? e.clientY : e.clientX;
    const origin = d.across ? sign * d.x : d.y;
    const perMinute = (d.across ? rect.width : rect.height) / (d.last - d.first);
    const snap = minutes => Math.round(minutes / snapMinutes) * snapMinutes;
    const clamp = (value, min, max) => Math.min(max, Math.max(min, value));
    const columns = [...d.week.querySelectorAll(d.across ? '.stil-resource-track' : '.stil-week-col')];
    let day = columns.indexOf(d.column);
    let start;
    let end;

    if (d.action === 'move') {
        start = clamp(d.start + snap((time(event) - origin) / perMinute), d.first, d.last - d.length);
        end = start + d.length;
        // The day column (or resource row) under the pointer, or the nearest one.
        const target = columns.reduce((best, column) => {
            const r = column.getBoundingClientRect();
            const [low, high] = d.across ? [r.top, r.bottom] : [r.left, r.right];
            const at = cross(event);
            const distance = at < low ? low - at : at > high ? at - high : 0;
            return distance < best.distance ? { column, distance } : best;
        }, { column: d.column, distance: Infinity }).column;
        day = columns.indexOf(target);
        d.item.style.setProperty('--stil-start', start);
        const shift = target.getBoundingClientRect();
        d.item.style.translate = d.across ? `0 ${shift.top - rect.top}px` : `${shift.left - rect.left}px 0`;
    } else if (d.action === 'resize') {
        start = d.start;
        end = clamp(d.start + snap(d.length + (time(event) - origin) / perMinute), d.start + snapMinutes, d.last);
        d.item.style.setProperty('--stil-length', end - start);
    } else {
        const edge = d.across ? (sign > 0 ? rect.left : -rect.right) : rect.top;
        const at = position => clamp(snap(d.first + (position - edge) / perMinute), d.first, d.last);
        const anchor = at(origin);
        const current = at(time(event));
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

// Kanban: arrow keys move focus between the cards of a board (up and down in a column, left and
// right to the nearest card of the next column with cards). A grabbed card is left to the page.
function moveInKanban(event) {
    const card = event.target instanceof Element ? event.target.closest('.stil-kanban-card') : null;
    if (!card || event.target !== card || event.altKey || event.ctrlKey || event.metaKey) return false;
    // Mark a card picked up at once, before the page re-renders it, so a quick arrow key moves the
    // card and not the focus. The page renders the same state.
    if (event.key === ' ') card.toggleAttribute('data-stil-grabbed');
    if (event.key === 'Escape') card.removeAttribute('data-stil-grabbed');
    if (card.hasAttribute('data-stil-grabbed')) return false;
    if (!['ArrowDown', 'ArrowUp', 'ArrowLeft', 'ArrowRight'].includes(event.key)) return false;
    event.preventDefault();
    const columns = [...card.closest('.stil-kanban').querySelectorAll('[data-stil-column]')]
        .map(column => [...column.querySelectorAll('.stil-kanban-card')])
        .filter(cards => cards.length > 0);
    const c = columns.findIndex(cards => cards.includes(card));
    const i = columns[c].indexOf(card);
    const step = logicalArrow(event.key, card);
    let next = null;
    if (step === 'ArrowDown') next = columns[c][i + 1];
    else if (step === 'ArrowUp') next = columns[c][i - 1];
    else {
        const other = columns[c + (step === 'ArrowRight' ? 1 : -1)];
        next = other?.[Math.min(i, other.length - 1)];
    }
    next?.focus();
    return true;
}

// Focuses a card of a board by its data-stil-item, for example after the page moved it.
export function focusCard(board, item) {
    board?.querySelector(`.stil-kanban-card[data-stil-item="${CSS.escape(item)}"]`)?.focus();
}

// Dragging a card: a copy follows the pointer, the card it would land before (or the end of the
// column) is marked, and Escape cancels. The cards themselves stay where the page put them.
let cardDrag = null;

function onCardPointerDown(event) {
    const card = event.target instanceof Element ? event.target.closest('.stil-kanban-card[data-stil-item]') : null;
    if (!card || event.button !== 0 || event.target.closest('a, button, input, select, textarea, label, [contenteditable]')) return;
    cardDrag = { card, pointerId: event.pointerId, x: event.clientX, y: event.clientY, ghost: null, target: null };
}

function onCardPointerMove(event) {
    const d = cardDrag;
    if (!d || event.pointerId !== d.pointerId) return;
    if (!d.ghost) {
        if (Math.hypot(event.clientX - d.x, event.clientY - d.y) < 6) return;
        const box = d.card.getBoundingClientRect();
        d.offset = [d.x - box.left, d.y - box.top];
        d.ghost = d.card.cloneNode(true);
        d.ghost.removeAttribute('data-stil-item');
        d.ghost.classList.add('stil-kanban-ghost');
        d.ghost.style.width = `${box.width}px`;
        document.body.append(d.ghost);
        d.card.classList.add('stil-kanban-dragging');
        d.card.setPointerCapture?.(event.pointerId);
    }
    event.preventDefault();
    d.ghost.style.left = `${event.clientX - d.offset[0]}px`;
    d.ghost.style.top = `${event.clientY - d.offset[1]}px`;
    markCardTarget(d, event.clientX, event.clientY);
}

function markCardTarget(d, x, y) {
    clearCardTarget(d);
    const board = d.card.closest('.stil-kanban');
    const column = [...board.querySelectorAll('[data-stil-column]')].find(c => {
        const box = c.getBoundingClientRect();
        return x >= box.left && x <= box.right;
    });
    if (!column || column.hasAttribute('data-stil-collapsed')) { d.target = null; return; }
    const cards = [...column.querySelectorAll('.stil-kanban-card[data-stil-item]')].filter(c => c !== d.card);
    const index = cards.findIndex(c => { const box = c.getBoundingClientRect(); return y < box.top + box.height / 2; });
    d.target = { column, index: index < 0 ? cards.length : index };
    if (index >= 0) cards[index].setAttribute('data-stil-drop-before', '');
    else column.setAttribute('data-stil-drop-end', '');
}

function clearCardTarget(d) {
    d.card.closest('.stil-kanban')?.querySelectorAll('[data-stil-drop-before], [data-stil-drop-end]')
        .forEach(e => { e.removeAttribute('data-stil-drop-before'); e.removeAttribute('data-stil-drop-end'); });
}

function onCardPointerUp(event) {
    const d = cardDrag;
    if (!d || event.pointerId !== d.pointerId) return;
    cardDrag = null;
    if (!d.ghost) return;
    finishCardDrag(d);
    if (event.type === 'pointerup' && d.target) {
        d.card.dispatchEvent(new CustomEvent('stilcardmove', {
            bubbles: true,
            detail: { item: d.card.dataset.stilItem, column: d.target.column.dataset.stilColumn, index: d.target.index },
        }));
    }
}

function finishCardDrag(d) {
    clearCardTarget(d);
    d.ghost.remove();
    d.card.classList.remove('stil-kanban-dragging');
}

function cancelCardDrag(event) {
    if (!cardDrag || event.key !== 'Escape') return;
    if (cardDrag.ghost) finishCardDrag(cardDrag);
    cardDrag = null;
}

// Chat and comment composers: Enter (or Ctrl+Enter) sends the form the textarea is in.
function sendFromComposer(event) {
    const field = event.target;
    if (!(field instanceof HTMLTextAreaElement) || !field.dataset.stilSend || event.key !== 'Enter' || event.isComposing) return false;
    const modifier = event.ctrlKey || event.metaKey;
    const plain = !modifier && !event.shiftKey && !event.altKey;
    if (!(modifier || field.dataset.stilSend === 'enter' && plain)) return false;
    event.preventDefault();
    field.form?.requestSubmit();
    return true;
}

function onToggle(event) {
    if (event.target instanceof HTMLElement && event.target.matches('.stil-popover')) {
        onPopoverToggle(event);
        return;
    }
    const dropdown = event.target;
    if (!(dropdown instanceof HTMLDetailsElement) || !dropdown.matches('.stil-dropdown')) return;
    const menu = dropdown.querySelector(':scope > .stil-dropdown-menu');
    if (!dropdown.open) {
        if (menu) dropPanel(menu);
        return;
    }
    for (const other of openDropdowns()) {
        if (other !== dropdown) other.open = false;
    }
    if (menu) liftPanel(menu);
}

// Popovers (.stil-popover with the popover attribute) sit in the top layer, so they are placed
// next to their toggle (the element whose popovertarget names them) here, and again on scroll
// and resize while open. Below by default; data-stil-placement="top" puts it above. It flips when
// there is no room, aligns with the toggle's start edge, and stays inside the viewport.
const openPopovers = new Set();

function onPopoverToggle(event) {
    const popover = event.target;
    if (event.newState === 'open') {
        openPopovers.add(popover);
        placePopover(popover);
    } else {
        openPopovers.delete(popover);
    }
}

function placePopover(popover) {
    const toggle = document.querySelector(`[popovertarget="${CSS.escape(popover.id)}"]`);
    if (toggle) placeNextTo(popover, toggle, { gap: 6, top: popover.dataset.stilPlacement === 'top' });
}

// Places a panel in the top layer under its anchor (or above it with top), flipping when there is
// no room. It lines up with the anchor's start edge, or its end edge with end, and stays inside
// the viewport while the anchor is visible; otherwise it scrolls away with it.
function placeNextTo(panel, anchorElement, { gap = 4, top: wantsTop = false, end = false } = {}) {
    const margin = 8;
    const anchor = anchorElement.getBoundingClientRect();
    const size = panel.getBoundingClientRect();
    const roomBelow = innerHeight - anchor.bottom - gap - margin;
    const roomAbove = anchor.top - gap - margin;
    const top = (wantsTop ? roomAbove >= size.height || roomAbove > roomBelow : roomBelow < size.height && roomAbove > roomBelow)
        ? anchor.top - gap - size.height
        : anchor.bottom + gap;
    const rtl = getComputedStyle(anchorElement).direction === 'rtl';
    let left = rtl !== end ? anchor.right - size.width : anchor.left;
    left = Math.min(Math.max(margin, left), innerWidth - size.width - margin);
    const visible = anchor.bottom > 0 && anchor.top < innerHeight;
    panel.style.top = `${visible ? Math.max(margin, top) : top}px`;
    panel.style.left = `${Math.max(margin, left)}px`;
}

// Dropdown menus and combobox lists are positioned inside their component by the CSS. When they
// open, they move to the top layer (popover="manual": Stil closes them, not the browser) and are
// placed next to the component; the DOM stays the same, so clicks and keys work as before.
const liftedPanels = new Set();

function liftPanel(panel) {
    if (!('showPopover' in panel) || !panel.isConnected) return;
    if (!panel.hasAttribute('popover')) panel.setAttribute('popover', 'manual');
    if (!panel.matches(':popover-open')) panel.showPopover();
    liftedPanels.add(panel);
    placePanel(panel);
}

function dropPanel(panel) {
    liftedPanels.delete(panel);
    if (panel.matches?.(':popover-open')) panel.hidePopover();
}

function placePanel(panel) {
    const combobox = panel.closest('.stil-combobox');
    if (combobox) {
        // As wide as the field, as it is without the top layer.
        panel.style.width = `${combobox.getBoundingClientRect().width}px`;
        placeNextTo(panel, combobox);
    } else {
        placeNextTo(panel, panel.parentElement, { end: panel.classList.contains('stil-dropdown-menu-end') });
    }
}

function onPopupMutations(mutations) {
    for (const mutation of mutations) {
        if (mutation.type === 'attributes') {
            const popup = mutation.target;
            if (!popup.matches('.stil-combobox-popup')) continue;
            if (popup.hidden) dropPanel(popup); else liftPanel(popup);
        } else {
            for (const node of mutation.addedNodes) {
                if (!(node instanceof Element)) continue;
                for (const popup of [node, ...node.querySelectorAll('.stil-combobox-popup')]) {
                    if (popup.matches('.stil-combobox-popup:not([hidden])')) liftPanel(popup);
                }
            }
        }
    }
}

function placeOpenPopovers() {
    for (const popover of openPopovers) {
        if (popover.matches(':popover-open')) placePopover(popover);
        else openPopovers.delete(popover);
    }
    for (const panel of liftedPanels) {
        if (panel.isConnected && panel.matches(':popover-open')) placePanel(panel);
        else liftedPanels.delete(panel);
    }
}

// Masked inputs: data-stil-mask="0000 00 00-0000" formats the value while typing. 0 is a digit,
// A a letter, * any character; everything else is a literal that is filled in. Runs in the
// capture phase, so a framework that reads the value on input sees the formatted one.
function onMaskInput(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.dataset.stilMask) return;
    const mask = input.dataset.stilMask;
    const caretFromEnd = input.value.length - (input.selectionEnd ?? input.value.length);
    const formatted = applyMask(mask, input.value, event.inputType?.startsWith('delete') === true);
    if (formatted === input.value) return;
    input.value = formatted;
    const caret = Math.max(0, formatted.length - caretFromEnd);
    input.setSelectionRange(caret, caret);
}

export function applyMask(mask, value, deleting = false) {
    const accepts = (slot, char) =>
        slot === '0' ? /\d/.test(char) : slot === 'A' ? /\p{L}/u.test(char) : slot === '*' ? char.trim() !== '' : false;
    const chars = [...value].filter(char => [...mask].some(slot => accepts(slot, char)) || false);
    let result = '';
    let next = 0;
    for (const slot of mask) {
        if (next >= chars.length) break;
        if ('0A*'.includes(slot)) {
            while (next < chars.length && !accepts(slot, chars[next])) next++;
            if (next >= chars.length) break;
            result += chars[next++];
        } else {
            result += slot;
        }
    }
    // While deleting, do not add a literal back at the end.
    if (deleting) {
        while (result.length > 0 && !'0A*'.includes(mask[result.length - 1])) result = result.slice(0, -1);
    }
    return result;
}


// Splitters (.stil-splitter > .stil-splitter-handle[role="separator"]): drag the handle, or use
// the arrow keys (5% a step), Home and End (the limits) and Enter (collapse to the minimum and
// back). The first pane's size, in percent, is --stil-splitter-size on the splitter and
// aria-valuenow on the handle; a bubbling stilsplit event (detail.size) reports each change.
let split = null;

function splitterOf(handle) {
    const splitter = handle.closest('.stil-splitter');
    return {
        splitter,
        vertical: splitter.classList.contains('stil-splitter-vertical'),
        min: Number(handle.getAttribute('aria-valuemin') ?? 0),
        max: Number(handle.getAttribute('aria-valuemax') ?? 100),
    };
}

function setSplit(handle, size, report) {
    const { splitter, min, max } = splitterOf(handle);
    const value = Math.round(Math.min(max, Math.max(min, size)) * 10) / 10;
    splitter.style.setProperty('--stil-splitter-size', String(value));
    handle.setAttribute('aria-valuenow', String(value));
    if (report) handle.dispatchEvent(new CustomEvent('stilsplit', { bubbles: true, detail: { size: value } }));
}

function onSplitterPointerDown(event) {
    const handle = event.target instanceof Element ? event.target.closest('.stil-splitter-handle') : null;
    if (!handle || event.button !== 0) return;
    event.preventDefault();
    handle.setPointerCapture(event.pointerId);
    handle.setAttribute('data-dragging', '');
    split = { handle, pointerId: event.pointerId };
}

function onSplitterPointerMove(event) {
    if (!split || event.pointerId !== split.pointerId) return;
    const { splitter, vertical } = splitterOf(split.handle);
    const rect = splitter.getBoundingClientRect();
    const rtl = !vertical && getComputedStyle(splitter).direction === 'rtl';
    const size = vertical
        ? (event.clientY - rect.top) / rect.height * 100
        : (rtl ? rect.right - event.clientX : event.clientX - rect.left) / rect.width * 100;
    setSplit(split.handle, size, false);
}

function onSplitterPointerUp(event) {
    if (!split || event.pointerId !== split.pointerId) return;
    const { handle } = split;
    split = null;
    handle.removeAttribute('data-dragging');
    setSplit(handle, Number(handle.getAttribute('aria-valuenow')), true);
}

function resizeSplitter(event) {
    const handle = event.target instanceof Element ? event.target.closest('.stil-splitter-handle') : null;
    if (!handle || event.target !== handle || event.altKey || event.ctrlKey || event.metaKey) return false;
    const { vertical, min, max } = splitterOf(handle);
    const current = Number(handle.getAttribute('aria-valuenow') ?? 50);
    const key = vertical ? event.key : logicalArrow(event.key, handle);
    const step = 5;
    let size;
    switch (key) {
        case vertical ? 'ArrowUp' : 'ArrowLeft': size = current - step; break;
        case vertical ? 'ArrowDown' : 'ArrowRight': size = current + step; break;
        case 'Home': size = min; break;
        case 'End': size = max; break;
        case 'Enter':
            if (current > min) {
                handle.dataset.stilRestore = String(current);
                size = min;
            } else {
                size = Number(handle.dataset.stilRestore ?? (min + max) / 2);
            }
            break;
        default: return false;
    }
    event.preventDefault();
    setSplit(handle, size, true);
    return true;
}

// Carousels (.stil-carousel): .stil-carousel-prev and .stil-carousel-next move one slide, the
// buttons in .stil-carousel-dots go to their slide, and the dot of the shown slide gets
// aria-current. A bubbling stilslide event (detail.index) reports the shown slide.
// data-stil-autoplay="6000" moves on every 6 seconds, except while the pointer or focus is in the
// carousel, while its .stil-carousel-pause button is pressed, and under reduced motion.
function slidesOf(carousel) {
    const track = carousel.querySelector('.stil-carousel-track');
    return { track, slides: track ? [...track.querySelectorAll(':scope > .stil-carousel-slide')] : [] };
}

function shownSlide(carousel) {
    const { track } = slidesOf(carousel);
    return track && track.clientWidth > 0 ? Math.round(Math.abs(track.scrollLeft) / track.clientWidth) : 0;
}

function showSlide(carousel, index) {
    const { track, slides } = slidesOf(carousel);
    if (slides.length === 0) return;
    const target = (index + slides.length) % slides.length;
    const rtl = getComputedStyle(track).direction === 'rtl';
    track.scrollTo({ left: (rtl ? -1 : 1) * target * track.clientWidth });
    carousel.stilShownAt = Date.now();
    // Not every browser fires scroll events during a smooth scroll; check once it has ended.
    setTimeout(() => updateCarousel(carousel), 600);
}

function onCarouselClick(event) {
    const target = event.target instanceof Element ? event.target : null;
    const carousel = target?.closest('.stil-carousel');
    if (!carousel) return;
    if (target.closest('.stil-carousel-prev')) showSlide(carousel, shownSlide(carousel) - 1);
    else if (target.closest('.stil-carousel-next')) showSlide(carousel, shownSlide(carousel) + 1);
    else if (target.closest('.stil-carousel-pause')) {
        const pause = target.closest('.stil-carousel-pause');
        pause.setAttribute('aria-pressed', String(pause.getAttribute('aria-pressed') !== 'true'));
    } else {
        const dot = target.closest('.stil-carousel-dots > button');
        if (dot) showSlide(carousel, [...dot.parentElement.children].indexOf(dot));
    }
}

function onCarouselScroll(event) {
    const track = event.target;
    if (!(track instanceof Element) || !track.matches('.stil-carousel-track')) return;
    updateCarousel(track.closest('.stil-carousel'));
}

// Marks the dot of the shown slide and reports it, once per change.
function updateCarousel(carousel) {
    if (!carousel) return;
    const index = shownSlide(carousel);
    if (carousel.stilIndex === index) return;
    carousel.stilIndex = index;
    carousel.querySelectorAll('.stil-carousel-dots > button').forEach((dot, i) =>
        dot.setAttribute('aria-current', String(i === index)));
    carousel.dispatchEvent(new CustomEvent('stilslide', { bubbles: true, detail: { index } }));
}

function advanceCarousels() {
    if (document.hidden || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    for (const carousel of document.querySelectorAll('.stil-carousel[data-stil-autoplay]')) {
        const delay = Number(carousel.dataset.stilAutoplay) || 6000;
        const paused = carousel.matches(':hover, :focus-within')
            || carousel.querySelector('.stil-carousel-pause[aria-pressed="true"]');
        carousel.stilShownAt ??= Date.now();
        if (paused) carousel.stilShownAt = Date.now();
        else if (Date.now() - carousel.stilShownAt >= delay) showSlide(carousel, shownSlide(carousel) + 1);
    }
}

// Image cropper (.stil-cropper-stage with a .stil-cropper-image): the image covers the stage and
// can be moved (drag, arrow keys), zoomed (wheel, + and -, setCropperZoom) and turned
// (rotateCropper). cropImage draws the framed part to a data URL.
const croppers = new WeakMap();

export function initCropper(stage) {
    const image = stage?.querySelector('.stil-cropper-image');
    if (!image) return;
    const start = () => {
        croppers.set(stage, { image, x: 0, y: 0, zoom: 1, rotation: 0 });
        drawCropper(stage);
    };
    if (image.complete && image.naturalWidth > 0) start();
    else image.addEventListener('load', start, { once: true });
    if (stage.stilCropperBound) return;
    stage.stilCropperBound = true;

    let pan = null;
    stage.addEventListener('pointerdown', event => {
        const state = croppers.get(stage);
        if (!state || event.button !== 0) return;
        stage.setPointerCapture(event.pointerId);
        pan = { id: event.pointerId, x: event.clientX - state.x, y: event.clientY - state.y };
    });
    stage.addEventListener('pointermove', event => {
        const state = croppers.get(stage);
        if (!pan || !state || event.pointerId !== pan.id) return;
        state.x = event.clientX - pan.x;
        state.y = event.clientY - pan.y;
        drawCropper(stage);
    });
    stage.addEventListener('pointerup', () => { pan = null; });
    stage.addEventListener('pointercancel', () => { pan = null; });
    stage.addEventListener('wheel', event => {
        const state = croppers.get(stage);
        if (!state) return;
        event.preventDefault();
        setCropperZoom(stage, state.zoom * (event.deltaY < 0 ? 1.1 : 1 / 1.1), true);
    }, { passive: false });
    stage.addEventListener('keydown', event => {
        const state = croppers.get(stage);
        if (!state) return;
        const step = event.shiftKey ? 40 : 10;
        const moves = { ArrowLeft: [step, 0], ArrowRight: [-step, 0], ArrowUp: [0, step], ArrowDown: [0, -step] };
        if (moves[event.key]) {
            state.x += moves[event.key][0];
            state.y += moves[event.key][1];
            drawCropper(stage);
        } else if (event.key === '+' || event.key === '=') {
            setCropperZoom(stage, state.zoom * 1.1, true);
        } else if (event.key === '-') {
            setCropperZoom(stage, state.zoom / 1.1, true);
        } else {
            return;
        }
        event.preventDefault();
    });
}

// The scale at which the (turned) image just covers the stage.
function coverScale(stage, state) {
    const { naturalWidth: w, naturalHeight: h } = state.image;
    const turned = state.rotation % 180 !== 0;
    return Math.max(stage.clientWidth / (turned ? h : w), stage.clientHeight / (turned ? w : h));
}

function drawCropper(stage) {
    const state = croppers.get(stage);
    if (!state) return;
    const scale = coverScale(stage, state) * state.zoom;
    const turned = state.rotation % 180 !== 0;
    const width = (turned ? state.image.naturalHeight : state.image.naturalWidth) * scale;
    const height = (turned ? state.image.naturalWidth : state.image.naturalHeight) * scale;
    // Keep the frame covered: the image may not move further than its overhang.
    const limitX = Math.max(0, (width - stage.clientWidth) / 2);
    const limitY = Math.max(0, (height - stage.clientHeight) / 2);
    state.x = Math.min(limitX, Math.max(-limitX, state.x));
    state.y = Math.min(limitY, Math.max(-limitY, state.y));
    state.image.style.transform =
        `translate(-50%, -50%) translate(${state.x}px, ${state.y}px) rotate(${state.rotation}deg) scale(${scale})`;
}

export function setCropperZoom(stage, zoom, report = false) {
    const state = croppers.get(stage);
    if (!state) return;
    state.zoom = Math.min(4, Math.max(1, zoom));
    drawCropper(stage);
    if (report) stage.dispatchEvent(new CustomEvent('stilcropzoom', { bubbles: true, detail: { zoom: state.zoom } }));
}

export function rotateCropper(stage, degrees = 90) {
    const state = croppers.get(stage);
    if (!state) return;
    state.rotation = (state.rotation + degrees + 360) % 360;
    drawCropper(stage);
}

export function cropImage(stage, width, height, type = 'image/png', quality = 0.92) {
    const state = croppers.get(stage);
    if (!state) return null;
    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    const context = canvas.getContext('2d');
    const k = width / stage.clientWidth;
    const scale = coverScale(stage, state) * state.zoom * k;
    context.translate(width / 2 + state.x * k, height / 2 + state.y * k);
    context.rotate(state.rotation * Math.PI / 180);
    context.scale(scale, scale);
    context.drawImage(state.image, -state.image.naturalWidth / 2, -state.image.naturalHeight / 2);
    return canvas.toDataURL(type, quality);
}

// Camera: a live preview in a <video>, and a still from it.
export async function startCamera(video, facingMode = 'user') {
    if (!video || !navigator.mediaDevices?.getUserMedia) return false;
    stopCamera(video);
    try {
        video.srcObject = await navigator.mediaDevices.getUserMedia({ video: { facingMode }, audio: false });
        await video.play();
        return true;
    } catch {
        return false;
    }
}

export function captureCamera(video, type = 'image/jpeg', quality = 0.9) {
    if (!video?.videoWidth) return null;
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d').drawImage(video, 0, 0);
    return canvas.toDataURL(type, quality);
}

export function videoSize(video) {
    return [video?.videoWidth ?? 0, video?.videoHeight ?? 0];
}

export function stopCamera(video) {
    video?.srcObject?.getTracks?.().forEach(track => track.stop());
    if (video) video.srcObject = null;
}

// Scanner: reads codes from the camera. It uses the browser's BarcodeDetector (Chromium on
// Android, ChromeOS and macOS) unless the page registers a decoder of its own, for example one
// built on a library, for the other browsers. A decoder takes the <video> and returns a promise
// of the codes in the current frame: [{ rawValue: "…", format: "qr_code" }]. Stil ships none.
let scanDecoder = null;

export function registerScanDecoder(decode) {
    scanDecoder = typeof decode === 'function' ? decode : null;
}

const scanners = new WeakMap();

// Returns "scanning", "no-camera" or "no-decoder". Each code found calls the .NET method
// OnScanned(value, format) once; the same code again only after the cooldown.
export async function startScanner(video, dotnet, { facingMode = 'environment', formats = null, cooldown = 1500, sound = true } = {}) {
    stopScanner(video);
    let decode = scanDecoder;
    if (!decode && 'BarcodeDetector' in globalThis) {
        const supported = await BarcodeDetector.getSupportedFormats();
        const wanted = formats?.length ? formats.filter(f => supported.includes(f)) : supported;
        if (wanted.length) {
            const detector = new BarcodeDetector({ formats: wanted });
            decode = source => detector.detect(source);
        }
    }
    if (!decode) return 'no-decoder';
    if (!await startCamera(video, facingMode)) return 'no-camera';

    const state = { stopped: false, last: '', lastTime: 0 };
    scanners.set(video, state);
    const tick = async () => {
        if (state.stopped) return;
        try {
            if (video.readyState >= 2) {
                for (const code of await decode(video) ?? []) {
                    if (formats?.length && !formats.includes(code.format)) continue;
                    const now = performance.now();
                    if (code.rawValue === state.last && now - state.lastTime < cooldown) continue;
                    state.last = code.rawValue;
                    state.lastTime = now;
                    if (sound) beep();
                    navigator.vibrate?.(40);
                    await dotnet.invokeMethodAsync('OnScanned', code.rawValue, code.format ?? null);
                    break;
                }
            }
        } catch {
            // A frame that cannot be read; try the next one.
        }
        if (!state.stopped) state.timer = setTimeout(tick, 120);
    };
    tick();
    return 'scanning';
}

export function stopScanner(video) {
    const state = video && scanners.get(video);
    if (state) {
        state.stopped = true;
        clearTimeout(state.timer);
        scanners.delete(video);
    }
    stopCamera(video);
}

// The flashlight of the camera in use, where the device has one. Returns whether it worked.
export async function setTorch(video, on) {
    const track = video?.srcObject?.getVideoTracks?.()[0];
    if (!track?.getCapabilities?.().torch) return false;
    try {
        await track.applyConstraints({ advanced: [{ torch: on }] });
        return true;
    } catch {
        return false;
    }
}

export function hasTorch(video) {
    return video?.srcObject?.getVideoTracks?.()[0]?.getCapabilities?.().torch === true;
}

let audio = null;

function beep() {
    try {
        audio ??= new AudioContext();
        const oscillator = audio.createOscillator();
        const gain = audio.createGain();
        oscillator.frequency.value = 1320;
        gain.gain.setValueAtTime(0.08, audio.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.0001, audio.currentTime + 0.12);
        oscillator.connect(gain).connect(audio.destination);
        oscillator.start();
        oscillator.stop(audio.currentTime + 0.12);
    } catch {
        // No audio: the scan still counts.
    }
}

// Signature pads: strokes drawn on a <canvas> with pointer events, as wide as the pen presses
// (1 to 4 pixels; a mouse presses 0.5). Each finished stroke goes to the .NET method
// OnStroke(points, width, height), with points as [x, y, pressure, x, y, pressure, …] in CSS
// pixels of the canvas. drawSignature redraws the strokes, scaled to the canvas's current size.
const pads = new WeakMap();

export function initSignaturePad(canvas, dotnet) {
    if (!canvas || pads.has(canvas)) return;
    const state = { strokes: [], width: 0, height: 0, current: null, last: null };
    pads.set(canvas, state);

    const point = event => {
        const box = canvas.getBoundingClientRect();
        const round = n => Math.round(n * 10) / 10;
        return [round(event.clientX - box.left), round(event.clientY - box.top), round(event.pressure || 0.5)];
    };

    canvas.addEventListener('pointerdown', event => {
        if (canvas.dataset.stilDisabled !== undefined || event.pointerType === 'mouse' && event.button !== 0) return;
        event.preventDefault();
        canvas.setPointerCapture(event.pointerId);
        state.current = point(event);
        state.last = state.current;
        strokeSegment(canvas, state.last, state.last);
    });
    canvas.addEventListener('pointermove', event => {
        if (!state.current) return;
        for (const e of event.getCoalescedEvents?.() ?? [event]) {
            const next = point(e);
            strokeSegment(canvas, state.last, next);
            state.current.push(...next);
            state.last = next;
        }
    });
    const end = () => {
        if (!state.current) return;
        const stroke = state.current;
        state.current = null;
        dotnet.invokeMethodAsync('OnStroke', stroke, canvas.clientWidth, canvas.clientHeight);
    };
    canvas.addEventListener('pointerup', end);
    canvas.addEventListener('pointercancel', end);

    // Keep the bitmap as sharp as the screen and redraw when the pad changes size.
    new ResizeObserver(() => redrawSignature(canvas)).observe(canvas);
}

export function drawSignature(canvas, strokes, width, height) {
    const state = canvas && pads.get(canvas);
    if (!state) return;
    Object.assign(state, { strokes: strokes ?? [], width, height });
    redrawSignature(canvas);
}

function redrawSignature(canvas) {
    const state = pads.get(canvas);
    const ratio = devicePixelRatio || 1;
    canvas.width = Math.round(canvas.clientWidth * ratio);
    canvas.height = Math.round(canvas.clientHeight * ratio);
    const scale = state.width ? canvas.clientWidth / state.width : 1;
    for (const stroke of state.strokes) {
        for (let i = 0; i < stroke.length; i += 3) {
            const from = i ? stroke.slice(i - 3, i) : stroke.slice(0, 3);
            strokeSegment(canvas, from.map((v, j) => j < 2 ? v * scale : v), stroke.slice(i, i + 3).map((v, j) => j < 2 ? v * scale : v));
        }
    }
}

function strokeSegment(canvas, [x1, y1], [x2, y2, pressure], context = canvas.getContext('2d'), ratio = canvas.width / (canvas.clientWidth || 1)) {
    context.lineCap = 'round';
    context.lineJoin = 'round';
    context.strokeStyle = getComputedStyle(canvas).color;
    context.lineWidth = (1 + 3 * pressure) * ratio;
    context.beginPath();
    context.moveTo(x1 * ratio, y1 * ratio);
    context.lineTo(x2 * ratio, y2 * ratio);
    context.stroke();
}

// The signature as a PNG data URL at twice its size: the strokes, or a typed name in the given
// font, in the given color on a transparent background.
export function signaturePng(strokes, typedName, width, height, color = '#000', font = 'cursive') {
    const canvas = document.createElement('canvas');
    canvas.width = Math.round(width * 2);
    canvas.height = Math.round(height * 2);
    const context = canvas.getContext('2d');
    if (typedName) {
        context.fillStyle = color;
        context.textAlign = 'center';
        context.font = `${Math.round(height * 0.72)}px ${font}`;
        context.fillText(typedName, width, height * 1.24, width * 1.9);
    }
    context.lineCap = 'round';
    context.lineJoin = 'round';
    context.strokeStyle = color;
    for (const stroke of strokes ?? []) {
        for (let i = 0; i < stroke.length; i += 3) {
            const [x1, y1] = i ? stroke.slice(i - 3, i) : stroke;
            context.lineWidth = (1 + 3 * stroke[i + 2]) * 2;
            context.beginPath();
            context.moveTo(x1 * 2, y1 * 2);
            context.lineTo(stroke[i] * 2, stroke[i + 1] * 2);
            context.stroke();
        }
    }
    return canvas.toDataURL('image/png');
}

// Scan inputs: a handheld USB or Bluetooth scanner types the code as fast keystrokes and ends
// with Enter. On Enter the input calls the .NET method OnEnter(value, fromScanner): fromScanner
// is true when the keys came at most maxInterval ms apart on average. Enter never submits the
// form here. OnEnter returns true to clear the input.
const scanInputs = new WeakMap();

export function initScanInput(input, dotnet, maxInterval = 50) {
    if (!input || scanInputs.has(input)) return;
    const state = { times: [] };
    scanInputs.set(input, state);
    input.addEventListener('keydown', async event => {
        const now = performance.now();
        if (event.key === 'Enter') {
            event.preventDefault();
            const times = state.times;
            const value = input.value;
            state.times = [];
            if (!value) return;
            const count = times.length;
            const fromScanner = count >= 3 && count >= value.length && (times[count - 1] - times[0]) / (count - 1) <= maxInterval;
            if (await dotnet.invokeMethodAsync('OnEnter', value, fromScanner)) input.value = '';
            return;
        }
        if (event.key.length !== 1 || event.ctrlKey || event.metaKey || event.altKey) return;
        // A pause starts a new burst.
        if (state.times.length && now - state.times[state.times.length - 1] > 300) state.times = [];
        state.times.push(now);
    });
}

// Prints the document in an iframe, such as a PDF from the same origin. Returns false when the
// browser does not allow it (another origin), so the caller can open the file instead.
// Saves text as a file, for example a CSV export.
export function downloadFile(name, type, text) {
    const url = URL.createObjectURL(new Blob([text], { type }));
    const link = Object.assign(document.createElement('a'), { href: url, download: name });
    document.body.append(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

// Whether the browser shows PDFs itself (false on most phones).
export function pdfViewerEnabled() {
    return navigator.pdfViewerEnabled !== false;
}

export function printFrame(frame) {
    try {
        frame.contentWindow.focus();
        frame.contentWindow.print();
        return true;
    } catch {
        return false;
    }
}

// Data grid column layout. A .stil-data-grid-resize handle in a header resizes its column by
// dragging and with the arrow keys (10px, Shift for 50px) within data-stil-min and data-stil-max,
// and fires a bubbling stilcolumnresize event (detail.key, detail.width in px). A header with
// data-stil-reorder is dragged to another header to move its column, which fires stilcolumnmove
// (detail.key, detail.before: the key of the column it goes before, or null for the end).
let column = null;

function headerWidth(th, width) {
    const min = Number(th.dataset.stilMin ?? 40);
    const max = Number(th.dataset.stilMax ?? 2000);
    return Math.round(Math.min(max, Math.max(min, width)));
}

function setHeaderWidth(th, width, report) {
    th.style.width = `${width}px`;
    th.style.minWidth = `${width}px`;
    th.querySelector('.stil-data-grid-resize')?.setAttribute('aria-valuenow', String(width));
    refreezeColumns(th.closest('table'));
    if (report) th.dispatchEvent(new CustomEvent('stilcolumnresize', { bubbles: true, detail: { key: th.dataset.stilColumn, width } }));
}

function onColumnPointerDown(event) {
    const target = event.target instanceof Element ? event.target : null;
    if (!target || event.button !== 0) return;
    const handle = target.closest('.stil-data-grid-resize');
    const th = target.closest('th[data-stil-column]');
    if (!th) return;
    if (handle) {
        event.preventDefault();
        handle.setPointerCapture(event.pointerId);
        handle.setAttribute('data-dragging', '');
        column = { mode: 'resize', th, handle, x: event.clientX, width: th.getBoundingClientRect().width, rtl: getComputedStyle(th).direction === 'rtl' };
    } else if (th.hasAttribute('data-stil-reorder') && !target.closest('input, select, .stil-data-grid-resize')) {
        column = { mode: 'pending', th, x: event.clientX, pointerId: event.pointerId };
    }
}

function onColumnPointerMove(event) {
    if (!column) return;
    if (column.mode === 'resize') {
        const delta = (event.clientX - column.x) * (column.rtl ? -1 : 1);
        setHeaderWidth(column.th, headerWidth(column.th, column.width + delta), false);
        return;
    }
    if (column.mode === 'pending' && Math.abs(event.clientX - column.x) < 6) return;
    if (column.mode === 'pending') {
        column.mode = 'move';
        column.th.setAttribute('data-stil-dragging', '');
    }
    const headers = [...column.th.parentElement.querySelectorAll(':scope > th[data-stil-reorder]')];
    headers.forEach(th => th.removeAttribute('data-stil-drop'));
    const over = headers.find(th => {
        const r = th.getBoundingClientRect();
        return event.clientX >= r.left && event.clientX <= r.right;
    });
    if (!over || over === column.th) {
        column.target = null;
        return;
    }
    const r = over.getBoundingClientRect();
    const rtl = getComputedStyle(over).direction === 'rtl';
    const firstHalf = event.clientX < r.left + r.width / 2;
    const before = rtl ? !firstHalf : firstHalf;
    over.setAttribute('data-stil-drop', before ? 'before' : 'after');
    column.target = { th: over, before };
}

function onColumnPointerUp() {
    if (!column) return;
    const { mode, th, handle, target } = column;
    column = null;
    if (mode === 'resize') {
        handle.removeAttribute('data-dragging');
        setHeaderWidth(th, headerWidth(th, th.getBoundingClientRect().width), true);
        return;
    }
    if (mode !== 'move') return;
    th.removeAttribute('data-stil-dragging');
    th.parentElement.querySelectorAll('[data-stil-drop]').forEach(header => header.removeAttribute('data-stil-drop'));
    // The header was dragged, not clicked: do not sort.
    const swallow = click => { click.stopPropagation(); click.preventDefault(); };
    window.addEventListener('click', swallow, { capture: true, once: true });
    setTimeout(() => window.removeEventListener('click', swallow, { capture: true }), 0);
    if (!target) return;
    const headers = [...th.parentElement.querySelectorAll(':scope > th[data-stil-column]')];
    const before = target.before ? target.th : headers[headers.indexOf(target.th) + 1] ?? null;
    if (before === th) return;
    th.dispatchEvent(new CustomEvent('stilcolumnmove', {
        bubbles: true,
        detail: { key: th.dataset.stilColumn, before: before?.dataset.stilColumn ?? null },
    }));
}

function resizeColumn(event) {
    const handle = event.target instanceof Element ? event.target.closest('.stil-data-grid-resize') : null;
    if (!handle || event.target !== handle) return false;
    const th = handle.closest('th');
    const width = th.getBoundingClientRect().width;
    const step = event.shiftKey ? 50 : 10;
    const key = logicalArrow(event.key, th);
    const next = key === 'ArrowLeft' ? width - step
        : key === 'ArrowRight' ? width + step
        : key === 'Home' ? 0
        : key === 'End' ? Number.MAX_SAFE_INTEGER
        : null;
    if (next === null) return false;
    event.preventDefault();
    setHeaderWidth(th, headerWidth(th, next), true);
    return true;
}

// Frozen columns: cells with data-stil-frozen="start" or "end" stick to that side; their offsets
// are the widths of the frozen columns before them. Kept up to date while the table resizes,
// together with the resize handles' aria-valuenow.
export function freezeColumns(table) {
    if (!table) return;
    measureColumns(table);
    refreezeColumns(table);
    if (!table.stilFrozenObserver && typeof ResizeObserver !== 'undefined') {
        table.stilFrozenObserver = new ResizeObserver(() => {
            measureColumns(table);
            refreezeColumns(table);
        });
        table.stilFrozenObserver.observe(table);
    }
}

// Gives each resize handle its column's width and limits, as a separator that takes focus needs.
function measureColumns(table) {
    for (const handle of table.querySelectorAll('.stil-data-grid-resize')) {
        const th = handle.closest('th');
        handle.setAttribute('aria-valuenow', String(Math.round(th.getBoundingClientRect().width)));
        handle.setAttribute('aria-valuemin', th.dataset.stilMin ?? '40');
        handle.setAttribute('aria-valuemax', th.dataset.stilMax ?? '2000');
    }
}

function refreezeColumns(table) {
    const header = table?.tHead?.rows[0];
    if (!header || !table.querySelector('[data-stil-frozen]')) return;
    const cells = [...header.cells];
    const offsets = new Map();
    let start = 0;
    cells.forEach((cell, i) => {
        if (cell.dataset.stilFrozen !== 'start') return;
        offsets.set(i, { side: 'start', offset: start });
        start += cell.getBoundingClientRect().width;
    });
    let end = 0;
    [...cells].reverse().forEach((cell, r) => {
        if (cell.dataset.stilFrozen !== 'end') return;
        offsets.set(cells.length - 1 - r, { side: 'end', offset: end });
        end += cell.getBoundingClientRect().width;
    });
    const lastStart = Math.max(-1, ...[...offsets].filter(([, o]) => o.side === 'start').map(([i]) => i));
    const firstEnd = Math.min(cells.length, ...[...offsets].filter(([, o]) => o.side === 'end').map(([i]) => i));
    for (const row of table.rows) {
        if (row.cells.length !== cells.length) continue;
        [...row.cells].forEach((cell, i) => {
            const frozen = offsets.get(i);
            if (!frozen) {
                // A cell that moved out of a frozen place keeps nothing of it.
                cell.style.insetInlineStart = cell.style.insetInlineEnd = '';
                cell.removeAttribute('data-stil-frozen-edge');
                return;
            }
            cell.style.insetInlineStart = frozen.side === 'start' ? `${frozen.offset}px` : '';
            cell.style.insetInlineEnd = frozen.side === 'end' ? `${frozen.offset}px` : '';
            if (i === lastStart) cell.setAttribute('data-stil-frozen-edge', 'start');
            else if (i === firstEnd) cell.setAttribute('data-stil-frozen-edge', 'end');
            else cell.removeAttribute('data-stil-frozen-edge');
        });
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

// Command palette: Ctrl+K (Cmd+K on a Mac) opens it from anywhere on the page. Returns an object
// whose dispose() removes the shortcut.
export function bindCommandKey(dotnet, key = 'k') {
    const handler = event => {
        if ((event.ctrlKey || event.metaKey) && !event.altKey && !event.shiftKey && event.key.toLowerCase() === key) {
            event.preventDefault();
            dotnet.invokeMethodAsync('OpenFromKey');
        }
    };
    document.addEventListener('keydown', handler);
    return { dispose: () => document.removeEventListener('keydown', handler) };
}

// Small values in local storage. Private windows and blocked storage give null and keep nothing.
export function readStorage(key) {
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

export function writeStorage(key, value) {
    try {
        localStorage.setItem(key, value);
    } catch {
        // Not kept; the page works without it.
    }
}

// Focuses the first field of the row or cell being edited in a data grid (StilDataGrid EditMode).
export function focusEditor(table) {
    table?.querySelector('[data-stil-editor] :is(input, select, textarea)')?.focus();
}

// Focuses the row in the tab order of a tree grid (StilTreeGrid), after the arrow keys moved it.
export function focusRow(table) {
    table?.querySelector('tbody > tr[aria-level][tabindex="0"]')?.focus();
}
