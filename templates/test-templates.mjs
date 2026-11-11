#!/usr/bin/env node
// Tests the dotnet new templates in templates/Stil.Templates.
//
// 1. Packs src/Stil.Blazor and the extra packages (Stil.Charts, Stil.Markdown, Stil.RichText)
//    into a local feed in a temp folder.
// 2. Packs the template package and installs it into an isolated template hive
//    (--debug:custom-hive). Nothing is installed into your own template list.
// 3. Creates an app for each option combination below, with a NuGet.config that uses the local
//    feed, nuget.org and a temp global packages folder.
// 4. Checks the generated files and runs `dotnet build -warnaserror` on each app.
//
// Usage: node templates/test-templates.mjs [--keep] [--only <text>]
//   --keep         keep the temp folder (its path is printed) to look at the generated apps.
//   --only <text>  only the cases whose name contains the text, for example --only Feature.
//
// Needs the .NET 10 SDK and Node.js 18 or later. Exits with a non-zero code on failure.

import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const keep = process.argv.includes('--keep');
const only = process.argv.includes('--only') ? process.argv[process.argv.indexOf('--only') + 1] : null;
const tmp = mkdtempSync(join(tmpdir(), 'stil-templates-'));
const feed = join(tmp, 'feed');
const templatePackages = join(tmp, 'templates');
const hive = join(tmp, 'hive');
const apps = join(tmp, 'apps');

// Builds outside the repo's bin/obj folders (so a build running at the same time is not
// disturbed), and without build servers that would keep files in the temp folder locked.
const isolatedBuild = ['-nodeReuse:false', '-p:UseSharedCompilation=false'];

// Each case: the app name, the template options and checks on the generated files.
const cases = [
    {
        name: 'Contoso.Default',
        options: [],
        expect: {
            'Program.cs': ['options.Font = StilFonts.Lexend;', 'AddInteractiveServerComponents', 'AddSingleton<IOrderService, InMemoryOrderService>()'],
            'Components/App.razor': ['<StilHead />', '<Routes @rendermode="InteractiveServer" />'],
            'Components/Layout/MainLayout.razor': ['StilThemeMode.Auto', '<StilDialogHost />', '<StilToastHost />', 'CollapseBelow'],
            'Components/Pages/Home.razor': ['<StilDataGrid', '<StilStatCard', 'Filterable'],
            'Components/Pages/NewOrder.razor': ['<EditForm', 'IStilToastService'],
            'Components/Pages/NotFound.razor': ['@page "/not-found"'],
            'Data/OrderService.cs': ['namespace Contoso.Default.Data;', 'interface IOrderService'],
            '.gitignore': ['[Oo]bj/'],
        },
        absent: ['Components/Pages/Home.Empty.razor'],
    },
    {
        name: 'Empty-App',
        options: ['--IncludeSampleContent', 'false'],
        expect: {
            'Components/Pages/Home.razor': ['Hello, world!'],
            'Components/Layout/MainLayout.razor': ['@L["Nav.Home"]</StilNavLink>', 'BrandText="Empty-App"'],
        },
        absent: ['Data', 'Components/Pages/NewOrder.razor', 'Components/Pages/Home.Empty.razor'],
        forbidden: { 'Program.cs': ['IOrderService'], 'Components/_Imports.razor': ['.Data'] },
    },
    {
        name: 'StaticApp',
        options: ['--Interactivity', 'None', '--Font', 'system'],
        expect: {
            'Program.cs': ['builder.Services.AddRazorComponents();', 'app.MapRazorComponents<App>();'],
            'Components/App.razor': ['<Routes />', '<HeadOutlet />'],
            'Components/Layout/MainLayout.razor': ['@formname="color-mode"', '<AntiforgeryToken />'],
            'Components/Pages/Home.razor': ['PageSize="0"', 'Sortable="false"'],
            'Components/Pages/NewOrder.razor': ['[SupplyParameterFromForm]', 'FormName="new-order"'],
        },
        forbidden: {
            'Program.cs': ['Interactive', '= StilFonts.Lexend'],
            'Components/App.razor': ['InteractiveServer'],
            'Components/Layout/MainLayout.razor': ['StilDialogHost', 'blazor-error-ui'],
            'wwwroot/app.css': ['blazor-error-ui'],
        },
    },
    {
        name: 'ThemedApp',
        options: ['--Primary', '#7048e8', '--ColorMode', 'dark', '--Culture', 'sv', '--Font', 'br-cobane'],
        expect: {
            'Program.cs': ['options.Theme = new() { Primary = "#7048e8" }', 'options.Font = StilFonts.BrCobane;'],
            'Components/Layout/MainLayout.razor': ['StilThemeMode.Dark', 'StilLanguageSelector'],
            'AppCultures.cs': ['public const string Default = "sv";'],
            'Locales/sv.json': ['Ny beställning'],
        },
    },
    {
        name: 'StaticEmptyApp',
        options: ['--Interactivity', 'None', '--IncludeSampleContent', 'false', '--ColorMode', 'light', '--GoogleFont', 'Open Sans'],
        expect: {
            'Components/Layout/MainLayout.razor': ['StilThemeMode.Light', '@formname="color-mode"'],
            'Program.cs': ['options.Font = StilFont.Google("Open Sans");'],
        },
        absent: ['Data'],
    },
];

// Each feature of --Features: its page, its route in the Features menu, and the package it adds.
const features = [
    { id: 'charts', page: 'Charts', route: 'charts', package: 'Stil.Charts', text: '<StilLineChart' },
    { id: 'markdown', page: 'Notes', route: 'notes', package: 'Stil.Markdown', text: '<StilMarkdownEditor' },
    { id: 'richtext', page: 'ProductText', route: 'product-text', package: 'Stil.RichText', text: '<StilRichTextEditor' },
    { id: 'datagrid', page: 'Sales', route: 'sales', text: 'ShowExport' },
    { id: 'calendar', page: 'Planner', route: 'planner', text: '<StilWeekPlanner' },
    { id: 'kanban', page: 'Board', route: 'board', text: '<StilKanban' },
    { id: 'gantt', page: 'Project', route: 'project', text: '<StilGantt' },
    { id: 'schedule', page: 'Bookings', route: 'bookings', text: '<StilResourcePlanner' },
    { id: 'chat', page: 'Chat', route: 'chat', text: '<StilComments' },
    { id: 'activity', page: 'Activity', route: 'activity', text: '<StilActivityLog' },
    { id: 'scanner', page: 'Scan', route: 'scan', text: '<StilScanInput' },
    { id: 'signature', page: 'Sign', route: 'sign', text: '<StilSignaturePad' },
    { id: 'commands' },
    { id: 'notifications', page: 'Notifications', route: 'notifications', text: 'keep: true' },
];
const packages = features.filter(f => f.package).map(f => f.package);
const pageOf = f => `Components/Pages/Features/${f.page}.razor`;

// Every feature on its own, with nothing of the others.
for (const feature of features) {
    const others = features.filter(f => f !== feature);
    cases.push({
        name: `Feature.${feature.id}`,
        options: ['--Features', feature.id],
        expect: {
            ...(feature.page ? { [pageOf(feature)]: [feature.text] } : {}),
            'Components/Layout/MainLayout.razor': feature.page
                ? [`<StilDropdownItem Href="${feature.route}">`]
                : ['<AppCommands />'],
            ...(feature.package ? { [`Feature.${feature.id}.csproj`]: [`Include="${feature.package}"`] } : {}),
            ...(feature.id === 'charts' ? { 'Components/App.razor': ['stil-charts.css'] } : {}),
            ...(feature.id === 'notifications' ? { 'Components/Layout/MainLayout.razor': ['<StilNotificationCenter />'] } : {}),
        },
        forbidden: {
            [`Feature.${feature.id}.csproj`]: packages.filter(p => p !== feature.package),
            'Components/Layout/MainLayout.razor': feature.page ? ['AppCommands', 'StilNotificationCenter'].filter(t => !(feature.id === 'notifications' && t === 'StilNotificationCenter')) : ['StilDropdown'],
            'Components/App.razor': feature.id === 'charts' ? [] : ['stil-charts.css'],
        },
        absent: [
            ...others.filter(f => f.page).map(pageOf),
            ...(feature.id === 'commands' ? [] : ['Components/Layout/AppCommands.razor']),
        ],
    });
}

// All features, with server interactivity and without it.
const allPages = Object.fromEntries(features.filter(f => f.page).map(f => [pageOf(f), [f.text]]));
cases.push({
    name: 'AllFeatures',
    options: ['--Features', 'all', '--Culture', 'sv'],
    expect: {
        ...allPages,
        'AllFeatures.csproj': packages.map(p => `Include="${p}"`),
        'Components/Layout/MainLayout.razor': ['<AppCommands />', '<StilNotificationCenter />', ...features.filter(f => f.page).map(f => `Href="${f.route}"`)],
        'Components/Layout/AppCommands.razor': ['Href = "orders/new"', ...features.filter(f => f.page).map(f => `Href = "${f.route}"`)],
        'Locales/sv.json': ['"Features": "Funktioner"', '"Board": "Tavla"'],
    },
    forbidden: { [pageOf(features[0])]: ['@rendermode'] },
});
cases.push({
    name: 'StaticAllFeatures',
    options: ['--Features', 'all', '--Interactivity', 'None', '--IncludeSampleContent', 'false'],
    expect: {
        [pageOf(features[0])]: ['@rendermode InteractiveServer'],
        'Program.cs': ['AddInteractiveServerComponents', 'AddInteractiveServerRenderMode'],
        'Components/Layout/MainLayout.razor': ['<AppCommands @rendermode="InteractiveServer" />',
            '<StilNotificationCenter @rendermode="InteractiveServer" />', '<StilToastHost @rendermode="InteractiveServer" />'],
        'Components/Layout/AppCommands.razor': ['L["Nav.Home"]'],
    },
    forbidden: { 'Components/Layout/AppCommands.razor': ['orders/new'] },
});

// Each starter: its pages and data, the packages it needs, and what it must not have.
const starterPages = {
    admin: ['Components/Pages/Admin/Orders.razor', 'Components/Pages/Admin/History.razor', 'Components/Pages/NewOrder.razor'],
    dashboard: ['Components/Pages/Dashboard/Overview.razor', 'Components/Pages/Dashboard/Products.razor'],
    monitoring: ['Components/Pages/Monitoring/Overview.razor', 'Components/Pages/Monitoring/Events.razor'],
    finance: ['Components/Pages/Finance/Overview.razor', 'Components/Pages/Finance/Transactions.razor'],
    planner: ['Components/Pages/Planner/Overview.razor', 'Components/Pages/Features/Planner.razor', 'Components/Pages/Features/Bookings.razor',
        'Components/Pages/Features/Board.razor', 'Components/Pages/Features/Project.razor'],
};
const starters = [
    { id: 'admin', data: { 'Data/OrderService.cs': ['void Update(Order order)', 'OrderEvent'] }, packages: [],
        layout: ['Href="history"', '<AppCommands />', '<StilNotificationCenter />'] },
    { id: 'dashboard', data: { 'Data/SalesService.cs': ['interface ISalesService'] }, packages: ['Stil.Charts'],
        layout: ['Href="products"'] },
    { id: 'monitoring', data: { 'Data/MonitoringService.cs': ['interface IMonitoringService'] }, packages: ['Stil.Charts'],
        layout: ['Href="events"', '<StilNotificationCenter />'] },
    { id: 'finance', data: { 'Data/FinanceService.cs': ['interface IFinanceService'] }, packages: ['Stil.Charts', 'Stil.Markdown'],
        layout: ['Href="transactions"'] },
    { id: 'planner', data: {}, packages: [], layout: ['Href="planner"', 'Href="project"', '<AppCommands />'] },
];
const allStarterPages = Object.values(starterPages).flat();
for (const starter of starters) {
    for (const interactive of [true, false]) {
        const name = `Starter.${starter.id}${interactive ? '' : '.NoInteractivity'}`;
        const pages = starterPages[starter.id];
        cases.push({
            name,
            options: ['--Starter', starter.id, ...(interactive ? [] : ['--Interactivity', 'None'])],
            expect: {
                ...Object.fromEntries(pages.filter(p => p !== 'Components/Pages/NewOrder.razor').map(p => [p, ['@page']])),
                ...starter.data,
                [`${name}.csproj`]: starter.packages.map(p => `Include="${p}"`),
                'Components/Layout/MainLayout.razor': starter.layout.map(t => interactive ? t : t.replace(' />', ' @rendermode="InteractiveServer" />')),
                ...(interactive ? {} : { 'Program.cs': ['AddInteractiveServerRenderMode'] }),
            },
            forbidden: {
                [`${name}.csproj`]: packages.filter(p => !starter.packages.includes(p)),
                // The planner's pages are in the navbar, not in a Features menu.
                'Components/Layout/MainLayout.razor': ['StilDropdown', 'Nav.Dashboard'],
            },
            absent: [
                'Components/Pages/Home.razor',
                ...allStarterPages.filter(p => !pages.includes(p)),
                ...(starter.id === 'admin' ? [] : ['Data/Order.cs', 'Data/OrderService.cs']),
            ],
        });
    }
}

// A starter with an extra feature: the feature is in the Features menu, the starter's pages are not.
cases.push({
    name: 'Starter.planner.charts',
    options: ['--Starter', 'planner', '--Features', 'charts', 'kanban', '--Culture', 'sv'],
    expect: {
        'Components/Layout/MainLayout.razor': ['<StilDropdownItem Href="charts">'],
        'Locales/sv.json': ['"Overview": "Översikt"', '"Charts": "Diagram"'],
    },
    forbidden: { 'Components/Layout/MainLayout.razor': ['<StilDropdownItem Href="board">'] },
});

// Text that must never be left in a generated app: template directives and placeholders.
const leftovers = ['#if', '#endif', '##else', 'STIL_PRIMARY_COLOR', 'STIL_GOOGLE_FONT', 'STIL_PACKAGE_VERSION', 'localhost:5500', 'localhost:5501', 'StilApp', 'My Stil App'];

function sleep(ms) {
    Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms);
}

function run(command, args, { cwd = repo, retries = 0 } = {}) {
    for (let attempt = 0; ; attempt++) {
        console.log(`\n> ${command} ${args.join(' ')}`);
        const result = spawnSync(command, args, {
            cwd,
            stdio: 'inherit',
            env: { ...process.env, DOTNET_CLI_TELEMETRY_OPTOUT: '1', DOTNET_NOLOGO: '1', DOTNET_SKIP_FIRST_TIME_EXPERIENCE: '1' },
        });
        if (result.status === 0) {
            return;
        }
        if (attempt < retries) {
            // Another build of the same project may hold a file lock; wait and try again.
            console.log(`Failed (exit code ${result.status}); trying again in 15 seconds.`);
            sleep(15_000);
            continue;
        }
        throw new Error(`'${command} ${args.join(' ')}' failed with exit code ${result.status ?? result.error}.`);
    }
}

function packageVersion(folder, id) {
    const pattern = new RegExp(`^${id.replace('.', '\\.')}\\.(\\d.*)\\.nupkg$`, 'i');
    for (const file of readdirSync(folder)) {
        const match = pattern.exec(file);
        if (match) {
            return { file: join(folder, file), version: match[1] };
        }
    }
    throw new Error(`No ${id} package in ${folder}.`);
}

function filesIn(folder) {
    return readdirSync(folder, { recursive: true })
        .map(file => join(folder, file))
        .filter(file => statSync(file).isFile() && !/[\\/](bin|obj)[\\/]/.test(relative(folder, file).replace(/^/, '/')));
}

function check(testCase, folder) {
    const errors = [];
    const read = file => readFileSync(join(folder, file), 'utf8');

    if (!existsSync(join(folder, `${testCase.name}.csproj`))) {
        errors.push(`${testCase.name}.csproj is missing.`);
    }
    for (const [file, texts] of Object.entries(testCase.expect ?? {})) {
        if (!existsSync(join(folder, file))) {
            errors.push(`${file} is missing.`);
            continue;
        }
        for (const text of texts.filter(text => !read(file).includes(text))) {
            errors.push(`${file} does not contain '${text}'.`);
        }
    }
    for (const [file, texts] of Object.entries(testCase.forbidden ?? {})) {
        for (const text of texts.filter(text => existsSync(join(folder, file)) && read(file).includes(text))) {
            errors.push(`${file} contains '${text}'.`);
        }
    }
    for (const path of (testCase.absent ?? []).filter(path => existsSync(join(folder, path)))) {
        errors.push(`${path} should not exist.`);
    }
    for (const file of filesIn(folder)) {
        const content = readFileSync(file, 'utf8');
        for (const text of leftovers.filter(text => content.includes(text))) {
            errors.push(`${relative(folder, file)} contains '${text}'.`);
        }
    }
    return errors;
}

const failures = [];
try {
    console.log(`Working folder: ${tmp}`);

    for (const project of ['src/Stil.Blazor', ...packages.map(p => `src/${p}`)]) {
        run('dotnet', ['pack', project, '-c', 'Release', '-o', feed, '--artifacts-path', join(tmp, 'artifacts'), ...isolatedBuild], { retries: 3 });
    }
    const stil = packageVersion(feed, 'Stil');

    run('dotnet', ['pack', 'templates/Stil.Templates', '-c', 'Release', '-o', templatePackages, '--artifacts-path', join(tmp, 'artifacts'), ...isolatedBuild]);
    const templates = packageVersion(templatePackages, 'Stil.Templates');

    run('dotnet', ['new', 'install', templates.file, '--debug:custom-hive', hive]);

    mkdirSync(apps, { recursive: true });
    writeFileSync(join(apps, 'NuGet.config'), `<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="${join(tmp, 'packages')}" />
  </config>
  <packageSources>
    <clear />
    <add key="local" value="${feed}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
`);

    for (const testCase of cases.filter(c => !only || c.name.includes(only))) {
        const folder = join(apps, testCase.name);
        try {
            run('dotnet', ['new', 'stil-app', '-n', testCase.name, '-o', folder, '--no-restore',
                '--StilVersion', stil.version, ...testCase.options, '--debug:custom-hive', hive]);

            const errors = check(testCase, folder);
            if (errors.length > 0) {
                throw new Error(`Unexpected files:\n  ${errors.join('\n  ')}`);
            }

            run('dotnet', ['build', folder, '-warnaserror', ...isolatedBuild]);
            console.log(`\nPASS ${testCase.name} ${testCase.options.join(' ')}`);
        } catch (error) {
            console.error(`\nFAIL ${testCase.name} ${testCase.options.join(' ')}\n${error.message}`);
            failures.push(testCase.name);
        }
    }
} catch (error) {
    console.error(`\n${error.message}`);
    failures.push('setup');
} finally {
    if (keep) {
        console.log(`\nKept ${tmp}`);
    } else {
        rmSync(tmp, { recursive: true, force: true, maxRetries: 5, retryDelay: 1000 });
    }
}

if (failures.length > 0) {
    console.error(`\n${failures.length} failed: ${failures.join(', ')}`);
    process.exit(1);
}
console.log(`\nAll ${cases.filter(c => !only || c.name.includes(only)).length} template apps built with no warnings.`);
