# Stil visual and accessibility tests

Playwright tests for the plain-HTML samples (`samples/Stil.Html`) and the
Blazor demo (`samples/Stil.Demo`):

- **Visual regression** (`@visual`): a screenshot of every component section of
  the showcase and the demo gallery, the admin and orders pages, and open
  dialogs, menus and listboxes, in light and dark mode, on a desktop and a
  phone viewport. A few sections are also checked in other brand palettes.
- **Accessibility** (`@a11y`): axe-core checks (WCAG 2.1 A and AA) of the same
  pages in light and dark mode.

## Run

Needs Node.js 20 or later and the .NET 10 SDK. Locally the tests use the
installed Microsoft Edge, so Playwright does not download a browser.

```shell
cd tests/Stil.Visual
npm ci
npm test              # everything
npm run test:a11y     # only the axe checks
npm run test:visual   # only the screenshots
npm run report        # open the HTML report of the last run
```

Playwright starts both servers itself: `static-server.mjs` for the HTML
samples (port 5181) and `dotnet run` for the demo (port 5100). If one is
already running, it is reused. The demo runs with invariant globalization, so
numbers and dates look the same on every machine.

## Baselines

The screenshots are compared with the baselines in
`tests/__screenshots__/{platform}/{project}/`. They are made on Windows with
Edge, and fonts and anti-aliasing differ between operating systems, so CI only
runs the axe checks (with Playwright's Chromium on Linux).

After an intended visual change, look at the differences in the report, then
update the baselines and commit them with the change:

```shell
npm run test:update
```

Web fonts from Google Fonts are blocked during the tests, so the pages render
with the fallback font and do not depend on the network. Spinners are frozen
and text carets hidden (`screenshot.css`).
