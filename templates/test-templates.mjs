#!/usr/bin/env node
// Tests the dotnet new templates in templates/Stil.Templates.
//
// 1. Packs src/Stil.Blazor into a local feed in a temp folder.
// 2. Packs the template package and installs it into an isolated template hive
//    (--debug:custom-hive). Nothing is installed into your own template list.
// 3. Creates an app for each option combination below, with a NuGet.config that uses the local
//    feed, nuget.org and a temp global packages folder.
// 4. Checks the generated files and runs `dotnet build -warnaserror` on each app.
//
// Usage: node templates/test-templates.mjs [--keep]
//   --keep  keep the temp folder (its path is printed) to look at the generated apps.
//
// Needs the .NET 10 SDK and Node.js 18 or later. Exits with a non-zero code on failure.

import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const keep = process.argv.includes('--keep');
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
            'Program.cs': ['options.Font = StilFonts.BrShape;', 'AddInteractiveServerComponents', 'AddSingleton<OrderStore>'],
            'Components/App.razor': ['<StilHead />', '<Routes @rendermode="InteractiveServer" />'],
            'Components/Layout/MainLayout.razor': ['StilThemeMode.Auto', '<StilDialogHost />', '<StilToastHost />', 'CollapseBelow'],
            'Components/Pages/Home.razor': ['<StilDataGrid', '<StilStatCard', 'Filterable'],
            'Components/Pages/NewOrder.razor': ['<EditForm', 'IStilToastService'],
            'Components/Pages/NotFound.razor': ['@page "/not-found"'],
            'Data/OrderStore.cs': ['namespace Contoso.Default.Data;'],
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
        forbidden: { 'Program.cs': ['OrderStore'], 'Components/_Imports.razor': ['.Data'] },
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
            'Program.cs': ['Interactive', 'StilFonts.BrShape'],
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

    run('dotnet', ['pack', 'src/Stil.Blazor', '-c', 'Release', '-o', feed, '--artifacts-path', join(tmp, 'artifacts'), ...isolatedBuild], { retries: 3 });
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

    for (const testCase of cases) {
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
console.log(`\nAll ${cases.length} template apps built with no warnings.`);
