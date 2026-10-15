// Blazor JavaScript initializer: loaded automatically by blazor.web.js (and blazor.server.js /
// blazor.webassembly.js). It loads stil.js, which closes dropdowns on an outside click or Escape
// and handles data-stil-dismiss, also on statically rendered pages. It also registers the
// "stiltoggle" event, so components such as StilDropdown learn the real open state of a
// <details> element, whoever opened or closed it.
const load = () => import('./stil.js');

function registerEvents(blazor) {
    blazor?.registerCustomEventType?.('stiltoggle', {
        browserEventName: 'toggle',
        createEventArgs: event => ({ open: event.target.open === true }),
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
