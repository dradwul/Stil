// Blazor JavaScript initializer: loaded automatically by blazor.web.js (and blazor.server.js /
// blazor.webassembly.js). It loads stil.js, which closes dropdowns on an outside click or Escape
// and handles data-stil-dismiss, also on statically rendered pages.
const load = () => import('./stil.js');

export function afterWebStarted() {
    return load();
}

export function afterStarted() {
    return load();
}
