// Showcase-only: switches Stil's color mode and brand palette on the element marked
// with [data-theme-target], and remembers the choice in this browser.
(() => {
    const palettes = {
        teal: {},
        purple: { '--stil-color-primary': '#7048e8', '--stil-color-secondary': '#d0bfff' },
        forest: { '--stil-color-primary': '#2b8a3e', '--stil-color-secondary': '#b2f2bb' },
        sunset: { '--stil-color-primary': '#e8590c', '--stil-color-secondary': '#ffd8a8' },
    };
    const tokenNames = [...new Set(Object.values(palettes).flatMap(Object.keys))];

    const read = key => { try { return localStorage.getItem(key); } catch { return null; } };
    const write = (key, value) => { try { localStorage.setItem(key, value); } catch { } };

    function apply(mode, palette) {
        const target = document.querySelector('[data-theme-target]');
        if (!target) return;
        target.dataset.stilTheme = mode;
        tokenNames.forEach(name => target.style.removeProperty(name));
        Object.entries(palettes[palette] ?? {}).forEach(([name, value]) => target.style.setProperty(name, value));
        document.querySelectorAll('[data-theme-mode]').forEach(select => select.value = mode);
        document.querySelectorAll('[data-theme-palette]').forEach(select => select.value = palette);
    }

    let mode = read('stil-showcase-mode') ?? 'light';
    let palette = read('stil-showcase-palette') ?? 'teal';

    document.addEventListener('change', event => {
        if (event.target.matches('[data-theme-mode]')) mode = event.target.value;
        else if (event.target.matches('[data-theme-palette]')) palette = event.target.value;
        else return;
        write('stil-showcase-mode', mode);
        write('stil-showcase-palette', palette);
        apply(mode, palette);
    });

    apply(mode, palette);
})();
