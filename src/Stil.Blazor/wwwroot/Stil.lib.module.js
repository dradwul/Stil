// Blazor JavaScript initializer: loaded automatically by blazor.web.js (and blazor.server.js /
// blazor.webassembly.js). It loads stil.js, which closes dropdowns on an outside click or Escape
// and handles data-stil-dismiss, also on statically rendered pages. It also registers the
// "stiltoggle" event, so components such as StilDropdown learn the real open state of a
// <details> element, whoever opened or closed it, and the calendar events that stil.js fires:
// "stilcalendaredge" (the keyboard left the month) and "stilweekedit" (an event was dragged).
const load = () => import('./stil.js');

function registerEvents(blazor) {
    blazor?.registerCustomEventType?.('stilcolumnresize', { createEventArgs: event => ({ key: event.detail?.key ?? '', width: event.detail?.width ?? 0 }) });
    blazor?.registerCustomEventType?.('stilcolumnmove', { createEventArgs: event => ({ key: event.detail?.key ?? '', before: event.detail?.before ?? null }) });
    blazor?.registerCustomEventType?.('stilcardmove', { createEventArgs: event => ({ item: event.detail?.item ?? '', column: event.detail?.column ?? '', index: event.detail?.index ?? 0 }) });
    blazor?.registerCustomEventType?.('stilsplit', { createEventArgs: event => ({ size: event.detail?.size ?? 0 }) });
    blazor?.registerCustomEventType?.('stilslide', { createEventArgs: event => ({ index: event.detail?.index ?? 0 }) });
    blazor?.registerCustomEventType?.('stilcropzoom', { createEventArgs: event => ({ zoom: event.detail?.zoom ?? 1 }) });
    blazor?.registerCustomEventType?.('stiltoggle', {
        browserEventName: 'toggle',
        // <details> has open; a popover reports its new state.
        createEventArgs: event => ({ open: event.newState ? event.newState === 'open' : event.target.open === true }),
    });
    blazor?.registerCustomEventType?.('stilcalendaredge', {
        createEventArgs: event => ({ key: event.detail?.key ?? '', date: event.detail?.cell?.dataset.date ?? null }),
    });
    blazor?.registerCustomEventType?.('stilweekedit', {
        createEventArgs: event => ({
            action: event.detail?.action ?? '',
            item: event.detail?.item?.dataset.stilItem ?? null,
            day: event.detail?.day ?? 0,
            start: event.detail?.start ?? 0,
            end: event.detail?.end ?? 0,
        }),
    });
}

export function afterWebStarted(blazor) {
    registerEvents(blazor);
    return load();
}

export function afterStarted(blazor) {
    registerEvents(blazor);
    return load();
}
